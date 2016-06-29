//
// GLUtils.cs
//
// Author:
//       Allis Tauri <allista@gmail.com>
//
// Copyright (c) 2016 Allis Tauri
//
using System;
using UnityEngine;

namespace AT_Utils
{
	//adapted from MechJeb
	public static class GLUtils
	{
		static Material _material;
		static Material material
		{
			get
			{
				if(_material == null) _material = new Material(Shader.Find("Particles/Additive"));
				return _material;
			}
		}

		static Camera GLBeginWorld(out float far)
		{
			var camera = MapView.MapIsEnabled? PlanetariumCamera.Camera : FlightCamera.fetch.mainCamera;
			far = camera.farClipPlane;
			camera.farClipPlane = far*100;
			GL.PushMatrix();
			material.SetPass(0);
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

		public static void GLBounds(Bounds b, Transform T, Color col)
		{
			var c = Utils.BoundCorners(b);
			for(int i = 0; i < 8; i++) c[i] = T.TransformPoint(c[i]);
			GLLine(c[0], c[1], col);
			GLLine(c[1], c[5], col);
			GLLine(c[5], c[4], col);
			GLLine(c[4], c[0], col);

			GLLine(c[2], c[3], col);
			GLLine(c[3], c[7], col);
			GLLine(c[7], c[6], col);
			GLLine(c[6], c[2], col);

			GLLine(c[2], c[0], col);
			GLLine(c[3], c[1], col);
			GLLine(c[7], c[5], col);
			GLLine(c[6], c[4], col);
		}
	}
}

