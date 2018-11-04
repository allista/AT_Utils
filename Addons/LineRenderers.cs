//   SimpleLineRenderer.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

namespace AT_Utils
{
    public abstract class LineRendererBase
    {
        public int mapMask = 24;
        public int flightMask = 1;
        public string name { get; private set; } = "line";

        protected float width = 5f;
        protected Material _material;
        protected Vector3d[] points;
        protected List<Vector3d> _points = new List<Vector3d>();
        protected Color32[] colors;
        protected List<Color32> _colors = new List<Color32>();
        protected Color color = Color.gray;

        public abstract bool isActive { get; set; }

        public int numPoints => points != null ? points.Length : -1;

        public virtual int layerMask
        {
            get { return MapView.MapIsEnabled ? mapMask : flightMask; }
            set
            {
                if(MapView.MapIsEnabled)
                    mapMask = value;
                else
                    flightMask = value;
            }
        }

        public virtual float Width
        {
            get { return width; }
            set { width = value; }
        }

        public virtual Material material
        {
            get { return _material; }
            set { _material = value ?? Utils.no_z_material; }
        }

        protected LineRendererBase(string name, float width,
                                   int mapMask, int flightMask,
                                   Material material = null)
        {
            this.name = name;
            this.width = width;
            this.mapMask = mapMask;
            this.flightMask = flightMask;
            _material = material ?? Utils.no_z_material;
        }

        ~LineRendererBase()
        {
            destroy_line();
        }

        protected abstract bool create_line();
        protected abstract void destroy_line();
        protected abstract void update();

        public virtual void SetPoints(Vector3d[] points)
        {
            _points.Clear();
            this.points = points;
            if(points == null || points.Length == 0)
                Reset();
            else if(create_line())
                update();
        }

        public virtual void SetPoints(Vector3d[] points, Color color)
        {
            this.color = color;
            colors = null;
            SetPoints(points);
        }

        public virtual void SetPoints(Vector3d[] points, Color32[] colors)
        {
            this.colors = colors;
            SetPoints(points);
        }

        public virtual void AddPoint(Vector3d point)
        {
            _points.Add(point);
        }

        public virtual void AddPoint(Vector3d point, Color color)
        {
            _points.Add(point);
            _colors.Add(color);
        }

        public virtual void Draw()
        {
            points = _points.ToArray();
            _points.Clear();
            colors = _colors.ToArray();
            _colors.Clear();
            if(points.Length > 0 && create_line())
                update();
        }

        public virtual void Reset()
        {
            _points.Clear();
            _colors.Clear();
            points = null;
            colors = null;
            destroy_line();
        }
    }

    public class UnityLineRenderer : LineRendererBase
    {
        public bool AutoWidthMultiplier = true;
        float wmult = AT_UtilsGlobals.Instance.LineWidthMult;

        LineRenderer renderer;
        Gradient gradient;

        public override float Width
        {
            get { return width; }
            set
            {
                width = value;
                if(renderer != null)
                {
                    renderer.startWidth = width;
                    renderer.endWidth = width;
                }
            }
        }

        public float WidthMultiplier
        {
            get { return wmult; }
            set
            {
                wmult = value;
                if(!AutoWidthMultiplier && renderer != null)
                    renderer.widthMultiplier = wmult;
            }
        }

        public override Material material
        {
            get { return _material; }
            set
            {
                base.material = value;
                if(renderer != null)
                    renderer.material = _material;
            }
        }

        public override bool isActive
        {
            get { return renderer != null && renderer.enabled; }
            set { if(renderer != null) renderer.enabled = value; }
        }

        public UnityLineRenderer(string name, float width = 5f,
                                  int mapMask = 24, int flightMask = 1,
                                  Material material = null)
            : base(name, width, mapMask, flightMask, material)
        { }

        protected override void destroy_line()
        {
            if(renderer != null)
            {
                renderer.enabled = false;
                UnityEngine.Object.DestroyImmediate(renderer.gameObject);
                renderer = null;
            }
        }

        protected override bool create_line()
        {
            if(renderer == null)
            {
                destroy_line();
                var gameObject = new GameObject(name, typeof(LineRenderer));
                renderer = gameObject.GetComponent<LineRenderer>();
                renderer.useWorldSpace = true;
                renderer.material = _material ?? Utils.no_z_material;
                renderer.startWidth = width;
                renderer.endWidth = width;
                renderer.widthMultiplier = wmult;
            }
            return renderer != null;
        }

        void width_mult_from_camera(float dist)
        {
            renderer.widthMultiplier = Utils.ClampL(wmult * 1e-3f * dist,
                                                    AT_UtilsGlobals.Instance.MinLineWidthMult);
        }

        protected override void update()
        {
            var count = points.Length;
            renderer.enabled = true;
            renderer.positionCount = count;
            if(MapView.MapIsEnabled)
            {
                if(AutoWidthMultiplier)
                {
                    var cam = PlanetariumCamera.fetch;
                    if(cam != null)
                        width_mult_from_camera(cam.Distance);
                }
                var scaled = new List<Vector3>(new Vector3[count]);
                ScaledSpace.LocalToScaledSpace(points, scaled);
                renderer.gameObject.layer = mapMask;
                renderer.SetPositions(scaled.ToArray());
            }
            else
            {
                if(AutoWidthMultiplier)
                {
                    var cam = FlightCamera.fetch;
                    if(cam != null)
                        width_mult_from_camera(cam.Distance);
                }
                var v3points = new Vector3[count];
                for(int i = 0; i < count; i++)
                    v3points[i] = points[i];
                renderer.gameObject.layer = flightMask;
                renderer.SetPositions(v3points);
            }
            update_colors();
        }

        void update_colors()
        {
            if(gradient != null)
                renderer.colorGradient = gradient;
            else if(colors != null)
            {
                gradient = new Gradient();
                var count = Math.Min(colors.Length, 8);
                var step = count < colors.Length ? (float)colors.Length / count : 1;
                var ckeys = new GradientColorKey[count];
                for(int i = 0; i < count; i++)
                    ckeys[i] = new GradientColorKey { color = colors[Mathf.RoundToInt(i * step)], time = (float)i / count };
                gradient.SetKeys(ckeys, new[]{
                    new GradientAlphaKey{alpha=1, time=0},
                    new GradientAlphaKey{alpha=1, time=1}
                });
                renderer.colorGradient = gradient;
            }
            else
            {
                renderer.startColor = color;
                renderer.endColor = color;
            }
        }

        public override void SetPoints(Vector3d[] points, Color color)
        {
            gradient = null;
            base.SetPoints(points, color);
        }

        public override void SetPoints(Vector3d[] points, Color32[] colors)
        {
            gradient = null;
            base.SetPoints(points, colors);
        }

        public void SetGradient(Gradient gradient)
        {
            this.gradient = gradient;
        }

        public override void Reset()
        {
            gradient = null;
            base.Reset();
        }
    }

    public class VectrosityLineRenderer : LineRendererBase
    {
        VectorLine line;

        bool line3D = true;
        public bool Line3D
        {
            get { return line3D; }
            set { line3D = value; Reset(); }
        }

        public override bool isActive
        {
            get { return line != null && line.active; }
            set { if(line != null) line.active = value; }
        }

        public VectrosityLineRenderer(string name, float width = 5f,
                                      int mapMask = 24, int flightMask = 1,
                                      Material material = null,
                                      bool line3D = true)
            : base(name, width, mapMask, flightMask, material)
        {
            this.line3D = line3D;
        }

        protected override void destroy_line()
        {
            if(line != null)
            {
                line.active = false;
                VectorLine.Destroy(ref line);
                line = null;
            }
        }

        protected override bool create_line()
        {
            if(line == null || points.Length != line.points3.Count)
            {
                destroy_line();
                scaled_space = MapView.MapIsEnabled;
#pragma warning disable IDE0017 // Simplify object initialization
                line = new VectorLine(name, new List<Vector3>(points.Length), width,
                                      LineType.Continuous, Joins.Fill);
#pragma warning restore IDE0017 // Simplify object initialization
                line.texture = _material.mainTexture;
                line.material = _material;
                line.continuousTexture = true;
                line.ContinuousTextureOffset = 0;
                line.smoothColor = true;
                line.UpdateImmediate = true;
                line.capLength = 0;
                line.layer = scaled_space ? mapMask : flightMask;
            }
            return line != null;
        }

        bool scaled_space;
        protected override void update()
        {
            line.active = true;
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
            if(line3D)
                line.Draw3D();
            else
                line.Draw();
        }

        void update_colors()
        {
            if(colors != null && colors.Length > 0)
            {
                if(points != null)
                {
                    var ncolors = colors.Length;
                    var needed = points.Length - 1;
                    if(ncolors > needed)
                        Array.Resize(ref colors, needed);
                    else if(ncolors < needed)
                    {
                        Utils.Log("VectorsityLineRenderer[{}] Number of colors should be >= " +
                              "points.Length-1. Expected {} or more got {}",
                              name, points.Length - 1, colors.Length);
                        colors = null;
                    }
                }
                Utils.Log("points {}, colors {}", points.Length, colors.Length);//debug
                line.SetColors(new List<Color32>(colors));
            }
            else
                line.SetColor(color);
        }
    }
}

