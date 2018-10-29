//   SimpleLineRenderer.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vectrosity;
namespace AT_Utils
{
    public class SimpleLineRenderer : MonoBehaviour
    {
        VectorLine line;
        Vector3d[] points;

        public float alpha = 1f;
        public Color color = Color.gray;
        protected int layerMask = 31;

        protected virtual Color line_color => color.A(alpha);

        public bool isActive => line != null && line.active;

        protected virtual void create_line()
        {
            if(points.Length > 0)
            {
                line = new VectorLine(name + " line", new List<Vector3>(points.Length), 5f, LineType.Continuous);
                line.texture = MapView.OrbitLinesMaterial.mainTexture;
                line.material = MapView.OrbitLinesMaterial;
                line.continuousTexture = true;
                line.color = line_color;
                line.rectTransform.gameObject.layer = layerMask;
                line.UpdateImmediate = true;
            }
            else if(line != null)
                line.active = false;
        }

        protected virtual void update()
        {
            if(MapView.MapIsEnabled)
                ScaledSpace.LocalToScaledSpace(points, line.points3);
            else
            {
                for(int i = 0, pointsLength = points.Length; i < pointsLength; i++)
                {
                    line.points3[i] = points[i];
                    line.drawEnd = points.Length - 1;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if(line != null)
                VectorLine.Destroy(ref line);
        }

        public virtual void SetPoints(IList<Vector3d> points, Color color, float alpha = 1f)
        {
            this.points = points.ToArray();
            this.color = color;
            this.alpha = alpha;
            create_line();
        }

        public virtual void Reset()
        {
            points = null;
            create_line();
        }

        protected virtual void LateUpdate()
        {
            if(isActive)
            {
                update();
                line.Draw();
            }
        }
    }
}
