//   PartSpaceManager.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    public class SpawnSpaceManager : ConfigNodeObject
    {
        protected Part part;
        protected Vessel vessel => part.vessel;

        [Persistent] public string SpawnSpace = string.Empty;
        [Persistent] public bool AutoPositionVessel;
        [Persistent] public Vector3 SpawnOffset = Vector3.zero;
        [Persistent] public string SpawnTransform = string.Empty;
        protected Transform spawn_transform;

        SpawnSpaceSensor Sensor;
        public bool SpawnSpaceEmpty => Sensor == null || Sensor.SpawnSpaceEmpty;

        #region AutoRotation
        static readonly Quaternion xyrot = Quaternion.Euler(0, 0, 90);
        static readonly Quaternion xzrot = Quaternion.Euler(0, 90, 0);
        static readonly Quaternion yzrot = Quaternion.Euler(90, 0, 0);
        static readonly Quaternion[,] swaps =
        {
            {Quaternion.identity,     xyrot,                     xzrot},
            {xyrot.Inverse(),         Quaternion.identity,     yzrot},
            {xzrot.Inverse(),         yzrot.Inverse(),         Quaternion.identity}
        };

        static List<KeyValuePair<float, int>> sort_vector(Vector3 v)
        {
            var s = new List<KeyValuePair<float, int>>(3);
            s.Add(new KeyValuePair<float, int>(v[0], 0));
            s.Add(new KeyValuePair<float, int>(v[1], 1));
            s.Add(new KeyValuePair<float, int>(v[2], 2));
            s.Sort((x, y) => x.Key.CompareTo(y.Key));
            return s;
        }
        #endregion

        public MeshFilter Space { get; protected set; }
        public Metric SpaceMetric { get; protected set; }
        public virtual bool Valid => !SpaceMetric.Empty && spawn_transform != null;

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            //deprecated config conversion
            if(string.IsNullOrEmpty(SpawnSpace))
                node.TryGetValue("HangarSpace", ref SpawnSpace);
            if(AutoPositionVessel)
                SpawnOffset = Vector3.zero;
        }

        public void SetMetric(Metric metric) => SpaceMetric = metric;

        public void UpdateMetric()
        {
            if(!string.IsNullOrEmpty(SpawnSpace))
                SpaceMetric = new Metric(part, SpawnSpace, true);
        }

        public void Init(Part part)
        {
            this.part = part;
            if(!string.IsNullOrEmpty(SpawnSpace))
            {
                Space = part.FindModelComponent<MeshFilter>(SpawnSpace);
                if(Space != null) 
                {
                    flip_mesh_if_needed(Space);
                    Space.gameObject.layer = 2; // IgnoreRaycast
                    UpdateMetric();
                }
            }
            if(!string.IsNullOrEmpty(SpawnTransform))
                spawn_transform = part.FindModelTransform(SpawnTransform);
            if(spawn_transform == null)
            {
                var launch_empty = new GameObject("auto_spawn_empty");
                var parent = Space != null ? Space.transform : part.transform;
                launch_empty.transform.SetParent(parent, false);
                spawn_transform = launch_empty.transform;
            }
        }

        public void SetupSensor()
        {
            if(Space == null) return;
            Space.AddCollider(true);
            Sensor = Space.gameObject.AddComponent<SpawnSpaceSensor>();
            Sensor.Init(vessel);
        }

        protected void flip_mesh_if_needed(MeshFilter mesh_filter)
        {
            //check if the hangar space has its normals flipped iside; if not, flip them
            var flipped = false;
            var mesh = mesh_filter.sharedMesh;
            var tris = mesh.triangles;
            var verts = mesh.vertices;
            var center = mesh.bounds.center;
            for(int i = 0, len = tris.Length / 3; i < len; i++)
            {
                var j = i * 3;
                var p = new Plane(verts[tris[j]], verts[tris[j + 1]], verts[tris[j + 2]]);
                var outside = !p.GetSide(center);
                if(outside)
                {
                    var t = tris[j];
                    tris[j] = tris[j + 2];
                    tris[j + 2] = t;
                    flipped = true;
                }
            }
            if(flipped)
            {
                part.Log("The '{}' mesh is not flipped. Hangar space normals should be pointed INSIDE.", mesh_filter.name);
                mesh.triangles = tris;
                mesh.RecalculateNormals();
            }
        }

        public bool MetricFits(Metric metric, Transform position, Vector3 offset)
        {
            return Space != null ?
                metric.FitsAligned(position, Space.transform, Space.sharedMesh, offset) :
                metric.FitsAligned(position, part.partTransform, SpaceMetric, offset);
        }

        public Vector3 GetSpawnOffset(Metric metric) =>
        GetSpawnOffset(metric.bounds);

        public Vector3 GetSpawnOffset(Bounds local_bounds) =>
        SpawnOffset.IsZero() ? 
                   SpawnOffset :
                   Vector3.Scale(local_bounds.extents, SpawnOffset);

        public Vector3 GetSpawnOffset(Vector3 local_bounds_extents) =>
        SpawnOffset.IsZero() ? 
                   SpawnOffset :
                   Vector3.Scale(local_bounds_extents, SpawnOffset);

        public Transform GetSpawnTransform(Bounds bounds = default(Bounds)) =>
        GetSpawnTransform(bounds.size);

        public Transform GetSpawnTransform(Metric metric) =>
        GetSpawnTransform(metric.size);

        public Transform GetSpawnTransform(Vector3 size)
        {
            if(AutoPositionVessel && !size.IsZero())
            {
                var s_size = sort_vector(SpaceMetric.size);
                var v_size = sort_vector(size);
                var r1 = swaps[s_size[0].Value, v_size[0].Value];
                var i2 = s_size[0].Value == v_size[1].Value ? 2 : 1;
                var r2 = swaps[s_size[i2].Value, v_size[i2].Value];
                spawn_transform.localPosition = Vector3.zero;
                spawn_transform.localRotation = Quaternion.identity;
                spawn_transform.rotation = part.transform.rotation * r2 * r1;
            }
            return spawn_transform;
        }

        public bool MetricFits(Metric metric) =>
        MetricFits(metric, GetSpawnTransform(metric), GetSpawnOffset(metric));

        class SpawnSpaceSensor : MonoBehaviour
        {
            Vessel vessel;
            RealTimer spawn_space_check = new RealTimer();

            public bool SpawnSpaceEmpty => 
            !spawn_space_check.Started || spawn_space_check.TimePassed;

            public void Init(Vessel vsl) => vessel = vsl;

            void OnTriggerStay(Collider col)
            {
                if(col != null && col.attachedRigidbody != null &&
                   (!spawn_space_check.Started || 
                    spawn_space_check.Remaining < spawn_space_check.Period/2))
                {
                    if(col.CompareTag("Untagged"))
                    {
                        var p = col.attachedRigidbody.GetComponent<Part>();
                        if(p != null && p.vessel != null && p.vessel != vessel)
                            spawn_space_check.Restart();
                    }
                }
            }
        }
    }
}

