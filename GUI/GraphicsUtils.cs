//
// GLUtils.cs
//
// Author:
//       Allis Tauri <allista@gmail.com>
//
// Copyright (c) 2016 Allis Tauri
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AT_Utils
{
	public class Triangle : IEnumerable<int>
	{
		readonly protected int i1, i2, i3;

		public Triangle(int i1, int i2, int i3) //indecies need to be clockwise
		{ this.i1 = i1; this.i2 = i2; this.i3 = i3; }

		public virtual IEnumerator<int> GetEnumerator()
		{
			yield return i1;
			yield return i2;
			yield return i3;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{ return GetEnumerator(); }
	}

	public class Quad : Triangle
	{
		readonly protected int i4;

		public Quad(int i1, int i2, int i3, int i4) //indecies need to be clockwise
			: base(i1, i2, i3) { this.i4 = i4; }

		public override IEnumerator<int> GetEnumerator ()
		{
			yield return i1;
			yield return i2;
			yield return i3;

			yield return i3;
			yield return i4;
			yield return i1;
		}
	}

	public class Basis
	{
		public readonly Vector3 x, y, z;
		public Basis(Vector3 x, Vector3 y, Vector3 z)
		{ this.x = x; this.y = y; this.z = z; }
	}

	public class MaterialWrapper
	{
		readonly string path;
		Material material;

		public MaterialWrapper(string material_path)
		{ path = material_path; }

		public Material This 
		{ 
			get 
			{ 
				if(material == null) 
					material = new Material(Shader.Find(path));
				return material;
			}
		}

		public Material New { get { return new Material(this); } }

		public static implicit operator Material(MaterialWrapper mw) { return mw.This; }
	}

	//adapted from MechJeb
	public static partial class Utils
	{
		static MaterialWrapper gl_material = new MaterialWrapper("Particles/Additive");
		static MaterialWrapper no_z_material = new MaterialWrapper("GUI/Text Shader");
		static MaterialWrapper diffuse_material = new MaterialWrapper("Diffuse");

		public static void DrawMesh(Vector3[] edges, IEnumerable<int> tris, Transform t, Color c = default(Color), Material mat = null)
		{
			//make a mesh
			var m = new Mesh();
			m.vertices  = edges;
			m.triangles = tris.ToArray();
			//recalculate normals and bounds
			m.RecalculateBounds();
			m.RecalculateNormals();
			//make own material
			if(mat == null) mat = no_z_material.New;
			mat.color = (c == default(Color))? Color.white : c;
			//draw mesh in the world space
			Graphics.DrawMesh(m, t.localToWorldMatrix, mat, 0);
		}

		public static void DrawMeshArrow(Vector3 ori, Vector3 dir, Transform T, Color c = default(Color))
		{
			float l = dir.magnitude;
			float w = l*0.02f;
			w = w > 0.05f ? 0.05f : (w < 0.01f ? 0.01f : w);
			Vector3 x = Mathf.Abs(Vector3.Dot(dir.normalized,Vector3.up)) < 0.9f ? 
				Vector3.Cross(dir, Vector3.up).normalized : Vector3.Cross(Vector3.forward, dir).normalized;
			Vector3 y = Vector3.Cross(x, dir).normalized*w; x *= w;
			var edges = new Vector3[5];
			edges[0] = ori+dir; 
			edges[1] = ori-x-y;
			edges[2] = ori-x+y;
			edges[3] = ori+x+y;
			edges[4] = ori+x-y;
			var tris = new List<int>();
			tris.AddRange(new Quad(1, 2, 3, 4));
			tris.AddRange(new Triangle(0, 1, 2));
			tris.AddRange(new Triangle(0, 2, 3));
			tris.AddRange(new Triangle(0, 3, 4));
			tris.AddRange(new Triangle(0, 4, 1));
			DrawMesh(edges, tris, T, c);
		}

		public static void DrawBounds(Bounds b, Transform T, Color c)
		{
			Vector3[] edges = Utils.BoundCorners(b);
			var tris = new List<int>();
			tris.AddRange(new Quad(0, 1, 3, 2));
			tris.AddRange(new Quad(0, 2, 6, 4));
			tris.AddRange(new Quad(0, 1, 5, 4));
			tris.AddRange(new Quad(1, 3, 7, 5));
			tris.AddRange(new Quad(2, 3, 7, 6));
			tris.AddRange(new Quad(6, 7, 5, 4));
			DrawMesh(edges, tris, T, c);
		}

		public static void DrawPoint(Vector3 point, Transform T, Color c = default(Color))
		{ DrawBounds(new Bounds(point, Vector3.one*0.1f), T, c); }

		public static void DrawHull(ConvexHull3D h, Transform T, Color c = default(Color))
		{
			var verts = new List<Vector3>(h.Faces.Count*3);
			var tris  = new List<int>(h.Faces.Count*3);
			foreach(Face f in h.Faces) 
			{
				verts.AddRange(f);
				tris.AddRange(new []{0+tris.Count, 1+tris.Count, 2+tris.Count});
			}
			DrawMesh(verts.ToArray(), tris, T, c, diffuse_material.New);
		}

		static Camera GLBeginWorld(out float far)
		{
			Camera camera;
			if(HighLogic.LoadedSceneIsEditor) camera = EditorLogic.fetch.editorCamera;
			else if(MapView.MapIsEnabled) camera = PlanetariumCamera.Camera;
			else camera = FlightCamera.fetch.mainCamera;
			far = camera.farClipPlane;
			camera.farClipPlane = far*100;
			GL.PushMatrix();
			gl_material.This.SetPass(0);
			GL.LoadProjectionMatrix(camera.projectionMatrix);
			GL.modelview = camera.worldToCameraMatrix;
			return camera;
		}

		public static void GLTriangleMap(Vector3d[] worldVertices, Color c)
		{
			float far;
			var camera = GLBeginWorld(out far);
			GL.Begin(GL.TRIANGLES);
			GL.Color(c);
			GL.Vertex(worldVertices[0]);
			GL.Vertex(worldVertices[1]);
			GL.Vertex(worldVertices[2]);
			GL.End();
			GL.PopMatrix();
			camera.farClipPlane = far;
		}

		public static void GLTriangleMap(Vector3[] worldVertices, Color c)
		{
			float far;
			var camera = GLBeginWorld(out far);
			GL.Begin(GL.TRIANGLES);
			GL.Color(c);
			GL.Vertex(worldVertices[0]);
			GL.Vertex(worldVertices[1]);
			GL.Vertex(worldVertices[2]);
			GL.End();
			GL.PopMatrix();
			camera.farClipPlane = far;
		}

		public static void GLLine(Vector3 ori, Vector3 end, Color c)
		{
			float far;
			var camera = GLBeginWorld(out far);
			if(MapView.MapIsEnabled)
			{
				ori = ScaledSpace.LocalToScaledSpace(ori);
				end = ScaledSpace.LocalToScaledSpace(end);
			}
			GL.Begin(GL.LINES);
			GL.Color(c);
			GL.Vertex(ori);
			GL.Vertex(end);
			GL.End();
			GL.PopMatrix();
			camera.farClipPlane = far;
		}

		public static void GLVec(Vector3 ori, Vector3 vec, Color c)
		{ GLLine(ori, ori+vec, c); }

		//		edges[0] = new Vector3(min.x, min.y, min.z); //left-bottom-back
		//		edges[1] = new Vector3(min.x, min.y, max.z); //left-bottom-front
		//		edges[2] = new Vector3(min.x, max.y, min.z); //left-top-back
		//		edges[3] = new Vector3(min.x, max.y, max.z); //left-top-front
		//		edges[4] = new Vector3(max.x, min.y, min.z); //right-bottom-back
		//		edges[5] = new Vector3(max.x, min.y, max.z); //right-bottom-front
		//		edges[6] = new Vector3(max.x, max.y, min.z); //right-top-back
		//		edges[7] = new Vector3(max.x, max.y, max.z); //right-top-front

		static void gl_line(Vector3 ori, Vector3 end)
		{ GL.Vertex(ori); GL.Vertex(end); }

		public static void GLBounds(Bounds b, Transform T, Color col)
		{
			var c = Utils.BoundCorners(b);
			for(int i = 0; i < 8; i++) 
			{
				c[i] = T.TransformPoint(c[i]);
				if(MapView.MapIsEnabled)
					c[i] = ScaledSpace.LocalToScaledSpace(c[i]);
			}
			float far;
			var camera = GLBeginWorld(out far);
			GL.Begin(GL.LINES);
			GL.Color(col);
			gl_line(c[0], c[1]);
			gl_line(c[1], c[5]);
			gl_line(c[5], c[4]);
			gl_line(c[4], c[0]);

			gl_line(c[2], c[3]);
			gl_line(c[3], c[7]);
			gl_line(c[7], c[6]);
			gl_line(c[6], c[2]);

			gl_line(c[2], c[0]);
			gl_line(c[3], c[1]);
			gl_line(c[7], c[5]);
			gl_line(c[6], c[4]);
			GL.End();
			GL.PopMatrix();
			camera.farClipPlane = far;
		}
	}
}

