//   MetricDebug.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
#if DEBUG
using UnityEngine;
namespace AT_Utils
{
    public class MetricDebug : PartModule
    {
        static readonly Color mesh_color = new Color{r=0, g=1, b=0, a=0.2f};

        Bounds bounds;
        Metric metric;
        Transform refT;
        MeshRenderer metric_renderer;
        MeshFilter metric_mesh;

        public override void OnAwake()
        {
            base.OnAwake();
            var obj = new GameObject("ContentHullMesh", typeof(MeshFilter), typeof(MeshRenderer));
            obj.transform.SetParent(gameObject.transform);
            metric_mesh = obj.GetComponent<MeshFilter>();
            metric_renderer = obj.GetComponent<MeshRenderer>();
            metric_renderer.material = Utils.no_z_material;
            metric_renderer.material.color = mesh_color;
            metric_renderer.enabled = true;
            obj.SetActive(false);
        }

        public void Update()
        {
            
            if(vessel != null)
            {
                if(part != vessel.rootPart)
                    return;
                metric = new Metric(vessel, true);
                refT = vessel.vesselTransform;
                bounds = vessel.Bounds(refT);
            }
            else
            {
                metric = new Metric(part, true);
                refT = part.partTransform;
            }
            metric_mesh.gameObject.SetActive(false);
            var mesh = metric.hull_mesh;
            if(mesh != null)
            {
                metric_mesh.mesh = mesh;
                metric_mesh.transform.position = refT.position;
                metric_mesh.transform.rotation = refT.rotation;
                metric_mesh.gameObject.SetActive(true);
            }
        }

        void OnRenderObject()
        {
            if(refT == null) return;
            if(vessel == null || part == vessel.rootPart)
            {
                Utils.GLVec(refT.position, Vector3.up, Color.green);
                Utils.GLVec(refT.position, Vector3.right, Color.red);
                Utils.GLVec(refT.position, Vector3.forward, Color.blue);

                Utils.GLVec(refT.position, refT.up, Color.yellow);
                Utils.GLVec(refT.position, refT.right, Color.magenta);
                Utils.GLVec(refT.position, refT.forward, Color.cyan);
            }
            if(!bounds.size.IsZero())
                Utils.GLDrawBounds(bounds, refT, Color.white);
        }
    }
}
#endif
