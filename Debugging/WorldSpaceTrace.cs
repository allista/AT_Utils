//   WorldSpaceTrace.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri

using System;
using UnityEngine;

namespace AT_Utils
{
    public class WorldSpaceTrace : MonoBehaviour
    {
        public float Delay = 5f;
        static readonly Color COLOR = new Color { r = 1, g = 0, b = 1, a = 0.25f };

        void Start()
        {
            this.Log("Starting WorldSpaceTrace for {}s at {}", Delay, transform.position);
            StartCoroutine(CallbackUtil.DelayedCallback
                (Delay, () =>
                {
                    if(gameObject != null) Destroy(gameObject);
                }));
        }

        public static UnityEngine.Object Create(Vector3 position, Transform T = null, float size = 1, float delay = -1, Color? color = null)
        {
            var obj = new GameObject("WorldSpaceTrace_"+Guid.NewGuid().ToString(), 
                                     typeof(MeshFilter), typeof(MeshRenderer), typeof(WorldSpaceTrace));
            //parent the object (if there's a Transform) and set its position/rotation
            if(T != null)
            {
                obj.transform.SetParent(T, false);
                obj.transform.localRotation = Quaternion.identity;
            }
            else
                obj.transform.rotation = Quaternion.identity;
            obj.transform.position = position;
            // setup trace delay
            if(delay > 0)
            {
                var trace = obj.GetComponent<WorldSpaceTrace>();
                trace.Delay = delay;
            }
            // setup the mesh
            var mesh_filter = obj.GetComponent<MeshFilter>();
            var mesh = mesh_filter.mesh;
            mesh.vertices = Utils.BoundCorners(new Bounds(Vector3.zero, new Vector3(size, size, size)));
            mesh.triangles = Utils.BoundTriangles();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            // setup renderer
            var renderer = obj.GetComponent<MeshRenderer>();
            renderer.material = Utils.no_z_material;
            renderer.material.color = color ?? COLOR;
            renderer.enabled = true;
            // activate and return
            obj.SetActive(true);
            return obj;
        }
    }
}
