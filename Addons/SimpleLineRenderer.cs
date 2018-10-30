//   SimpleLineRenderer.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
namespace AT_Utils
{
    public class SimpleLineRenderer
    {
        protected string name = "line";
        protected float width = 5f;

        VectorLine line;
        Vector3d[] points;
        List<Color32> colors;
        Color color = Color.gray;

        public int NumPoints => points != null ? points.Length : -1;

        public bool isActive
        {
            get { return line != null && line.active; }
            set { if(line != null) line.active = value; }
        }

        public SimpleLineRenderer(string name, float width = 5f)
        {
            this.name = name;
            this.width = width;
        }

        ~SimpleLineRenderer()
        {
            destroy_line();
        }

        protected virtual void destroy_line()
        {
            if(line != null)
            {
                line.active = false;
                VectorLine.Destroy(ref line);
                line = null;
            }
        }

        protected virtual bool create_line()
        {
            if(line == null || points.Length != line.points3.Count)
            {
                destroy_line();
#pragma warning disable IDE0017 // Simplify object initialization
                line = new VectorLine(name, new List<Vector3>(points.Length), width,
                                      LineType.Continuous, Joins.Fill);
#pragma warning restore IDE0017 // Simplify object initialization
                line.texture = MapView.OrbitLinesMaterial.mainTexture;
                line.material = MapView.OrbitLinesMaterial;
                line.continuousTexture = true;
                line.ContinuousTextureOffset = 0;
                line.smoothColor = true;
                line.UpdateImmediate = true;
                scaled_space = MapView.MapIsEnabled;
                line.rectTransform.gameObject.layer = scaled_space? 31 : 1;
            }
            return line != null;
        }

        bool scaled_space;
        protected virtual void update()
        {
            line.rectTransform.position = Vector3.zero;
            if(MapView.MapIsEnabled)
            {
                if(!scaled_space)
                {
                    destroy_line();
                    create_line();
                    scaled_space = true;
                }
                ScaledSpace.LocalToScaledSpace(points, line.points3);
            }
            else
            {
                if(scaled_space)
                {
                    destroy_line();
                    create_line();
                    scaled_space = false;
                }
                for(int i = 0, pointsLength = points.Length; i < pointsLength; i++)
                    line.points3[i] = points[i];
            }
            update_colors();
            line.Draw3D();
        }

        void update_colors()
        {
            if(colors != null)
                line.SetColors(colors);
            else
                line.SetColor(color);
        }

        public virtual void SetPoints(Vector3d[] points)
        {
            this.points = points;
            if(points == null || points.Length == 0)
                Reset();
            else if(create_line())
            {
                update();
                line.active = true;
            }
        }

        public virtual void SetPoints(Vector3d[] points, Color color)
        {
            this.color = color;
            colors = null;
            SetPoints(points);
        }

        public virtual void SetPoints(Vector3d[] points, List<Color32> colors)
        {
            this.colors = null;
            if(points != null && colors != null)
            {
                if(colors.Count == points.Length - 1)
                    this.colors = colors;
                else
                    Utils.Log("SimpleLineRenderer[{}] Number of colors should be equal " +
                              "to points.Length-1. Expected {} got {}",
                              name, points.Length - 1, colors.Count);
            }
            SetPoints(points);
        }

        public virtual void Reset()
        {
            points = null;
            colors = null;
            destroy_line();
        }
    }
}

