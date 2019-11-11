//   SpatialSensor.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri
using UnityEngine;

namespace AT_Utils
{
    public class SpatialSensor : MonoBehaviour
    {
        Vessel vessel;
        RealTimer check_timer = new RealTimer();

        public Callback<Part> on_trigger;

        public bool Empty => !check_timer.Started || check_timer.TimePassed;

        public static SpatialSensor AddToGO(GameObject obj, Vessel vsl, float check_rate = 1, Callback<Part> callback = null)
        {
            var sensor = obj.AddComponent<SpatialSensor>();
            sensor.vessel = vsl;
            sensor.check_timer.Period = check_rate;
            sensor.on_trigger = callback;
            return sensor;
        }

        public static SpatialSensor AddToMesh(MeshFilter meshFilter, Vessel vsl, float check_rate = 1, Callback<Part> callback = null)
        {
            meshFilter.AddCollider(true);
            return AddToGO(meshFilter.gameObject, vsl, check_rate, callback);
        }

        public static SpatialSensor AddToCollider(Collider collider, Vessel vsl, float check_rate = 1, Callback<Part> callback = null)
        {
            if(!collider.isTrigger)
            {
                Collider sensor_collider = null;
                if(collider is BoxCollider b)
                {
                    var new_collider = collider.gameObject.AddComponent<BoxCollider>();
                    new_collider.center = b.center;
                    new_collider.size = b.size;
                    sensor_collider = new_collider;
                }
                else if(collider is SphereCollider s)
                {
                    var new_collider = collider.gameObject.AddComponent<SphereCollider>();
                    new_collider.center = s.center;
                    new_collider.radius = s.radius;
                    sensor_collider = new_collider;
                }
                else if(collider is MeshCollider m)
                {
                    var new_collider = collider.gameObject.AddComponent<MeshCollider>();
                    new_collider.sharedMesh = m.sharedMesh;
                    new_collider.convex = true;
                }
                if(sensor_collider != null)
                {
                    sensor_collider.isTrigger = true;
                    sensor_collider.enabled = true;
                }
            }
            return AddToGO(collider.gameObject, vsl, check_rate, callback);
        }

        void OnTriggerStay(Collider col)
        {
            if(col != null && col.attachedRigidbody != null &&
               (!check_timer.Started
                || check_timer.Remaining < check_timer.Period / 2))
            {
                if(col.CompareTag("Untagged"))
                {
                    var p = col.attachedRigidbody.GetComponent<Part>();
                    if(p != null && p.vessel != null && p.vessel != vessel)
                    {
                        check_timer.Restart();
                        on_trigger?.Invoke(p);
                    }
                }
            }
        }
    }
}
