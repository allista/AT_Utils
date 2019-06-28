//   PartSpaceManager.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using UnityEngine;

namespace AT_Utils
{
    public class SpawnSpaceManager : ConfigNodeObject
    {
        protected Part part;
        protected Vessel vessel => part.vessel;

        [Persistent] public string SpawnSpace = string.Empty;
        [Persistent] public Vector3 SpawnOffset = Vector3.zero;
        [Persistent] public string SpawnTransform = string.Empty;
        protected Transform spawn_transform, spawn_transform_rotated;

        SpatialSensor Sensor;
        public bool SpawnSpaceEmpty => Sensor == null || Sensor.Empty;

        #region AutoRotation
        static readonly Quaternion xrot = Quaternion.Euler(90, 0, 0);
        static readonly Quaternion yrot = Quaternion.Euler(0, 90, 0);
        static readonly Quaternion zrot = Quaternion.Euler(0, 0, 90);
        static readonly Quaternion[,] swaps =
        {
            {Quaternion.identity,     zrot,                     yrot},
            {zrot.Inverse(),         Quaternion.identity,     xrot},
            {yrot.Inverse(),         xrot.Inverse(),         Quaternion.identity}
        };

        class SortedVector3
        {
            public float a, b, c;
            public uint i0, i1, i2;

            public uint this[int idx]
            {
                get
                {
                    if(idx == 0) return i0;
                    if(idx == 1) return i1;
                    if(idx == 2) return i2;
                    throw new System.ArgumentOutOfRangeException(nameof(idx));
                }
            }

            void add(float d, uint i)
            {
                if(d > a)
                {
                    b = a;
                    c = b;
                    a = d;
                    i2 = i1;
                    i1 = i0;
                    i0 = i;
                }
                else if(d > b)
                {
                    c = b;
                    b = d;
                    i2 = i1;
                    i1 = i;
                }
                else
                {
                    c = d;
                    i2 = i;
                }
            }

            public SortedVector3(Vector3 vec)
            {
                a = vec.x;
                b = c = float.NegativeInfinity;
                add(vec.y, 1);
                add(vec.z, 2);
            }
        }
        #endregion

        public MeshFilter Space { get; protected set; }
        public Metric SpaceMetric { get; protected set; }
        SortedVector3 spawn_space_sorted_size;
        public virtual bool Valid => !SpaceMetric.Empty && spawn_transform != null;

        public override void Load(ConfigNode node)
        {
            base.Load(node);
            //deprecated config conversion
            if(string.IsNullOrEmpty(SpawnSpace))
                node.TryGetValue("HangarSpace", ref SpawnSpace);
        }

        public void SetMetric(Metric metric) => SpaceMetric = metric;

        public void UpdateMetric()
        {
            if(!string.IsNullOrEmpty(SpawnSpace))
            {
                SpaceMetric = new Metric(part, SpawnSpace, true);
                spawn_space_sorted_size = new SortedVector3(SpaceMetric.size);
            }
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
                var launch_empty = new GameObject("__SPAWN_TRANSFORM");
                var parent = Space != null ? Space.transform : part.FindModelTransform("model");
                launch_empty.transform.SetParent(parent, false);
                spawn_transform = launch_empty.transform;
                spawn_transform.localPosition = Vector3.zero;
                spawn_transform.localRotation = Quaternion.identity;
            }
            var rot_empty = new GameObject("__SPAWN_TRANSFORM_ROTATED");
            rot_empty.transform.SetParent(spawn_transform, false);
            spawn_transform_rotated = rot_empty.transform;
            spawn_transform_rotated.localPosition = Vector3.zero;
            spawn_transform_rotated.localRotation = Quaternion.identity;
        }

        public void SetupSensor()
        {
            if(Space != null && Sensor == null)
                Sensor = SpatialSensor.AddToMesh(Space, vessel);
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

        public Transform GetSpawnTransform(Quaternion? rotation = null)
        {
            Vector3 spawn_offset;
            return GetSpawnTransform(Vector3.zero, out spawn_offset, rotation);
        }

        public Transform GetSpawnTransform(Bounds bounds, out Vector3 spawn_offset, Quaternion? rotation = null) =>
        GetSpawnTransform(bounds.size, out spawn_offset, rotation);

        public Transform GetSpawnTransform(Metric metric, out Vector3 spawn_offset, Quaternion? rotation = null) =>
        GetSpawnTransform(metric.size, out spawn_offset, rotation);

        public Transform GetSpawnTransform(Vector3 size, out Vector3 spawn_offset, Quaternion? rotation = null)
        {
            spawn_offset = Vector3.zero;
            if(!size.IsZero())
            {
                if(rotation != null)
                {
                    var localRotation = (Quaternion)rotation;
                    spawn_transform_rotated.localPosition = Vector3.zero;
                    spawn_transform_rotated.localRotation = localRotation;
                    if(!SpawnOffset.IsZero())
                    {
                        var sizeRot = (localRotation * size).AbsComponents();
                        spawn_offset = localRotation.Inverse() * Vector3.Scale(sizeRot / 2, SpawnOffset);
                    }
                    return spawn_transform_rotated;
                }
                if(!SpawnOffset.IsZero())
                    spawn_offset = Vector3.Scale(size / 2, SpawnOffset);
            }
            return spawn_transform;
        }

        public Quaternion GetOptimalRotation(Vector3 size)
        {
            var v_size = new SortedVector3(size);
            var r1 = swaps[spawn_space_sorted_size.i0, v_size.i0];
            var i2 = spawn_space_sorted_size.i0 == v_size.i1 ? 2 : 1;
            var r2 = swaps[spawn_space_sorted_size[i2], v_size[i2]];
            return  GetSpawnRotation(part.partTransform.rotation * r2 * r1);
        }

        public Quaternion GetSpawnRotation(Quaternion rotation) =>
        spawn_transform.rotation.Inverse() * rotation;

        public Quaternion GetSpawnRotation(Transform transform) =>
        GetSpawnRotation(transform.rotation);

        public bool MetricFits(Metric metric, Quaternion? rotation = null)
        {
            Vector3 spawn_offset;
            var T = GetSpawnTransform(metric, out spawn_offset, rotation);
            return MetricFits(metric, T, spawn_offset);
        }
    }
}

