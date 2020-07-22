using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AT_Utils
{
    public static partial class Utils
    {
        public static Mesh CreateBoundsMesh(Bounds bounds)
        {
            var corners = BoundCorners(bounds);
            var mesh = new Mesh
            {
                vertices = new[]
                {
                    corners[0], //left-bottom-back
                    corners[1], //left-bottom-front
                    corners[2], //left-top-back
                    corners[3], //left-top-front
                    corners[4], //right-bottom-back
                    corners[5], //right-bottom-front
                    corners[6], //right-top-back
                    corners[7], //right-top-front
                },
                // @formatter:off
                triangles = new[]
                {
                    0, 1, 2, 2, 1, 3, //left
                    3, 1, 7, 7, 1, 5, //front
                    5, 4, 7, 7, 4, 6, //right
                    6, 4, 2, 2, 4, 0, //back
                    2, 6, 3, 3, 6, 7, //top
                    0, 4, 1, 1, 4, 5, //bottom
                }
                // @formatter:on
            };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static GameObject CreateMeshGO(
            string name,
            Mesh mesh,
            Color color,
            Transform parent = null,
            bool isActive = true
        )
        {
            var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            if(parent != null)
            {
                obj.transform.SetParent(parent);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
            }
            else
            {
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;
            }
            var meshFilter = obj.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            var renderer = obj.GetComponent<MeshRenderer>();
            renderer.material = no_z_material;
            renderer.material.color = color;
            renderer.enabled = true;
            obj.SetActive(isActive);
            return obj;
        }

#if DEBUG
        private class AutoDestructor : MonoBehaviour
        {
            public float timeout = 1;

            private void Start() => StartCoroutine(CallbackUtil.DelayedCallback(timeout, Destroy, gameObject));
        }

        public static GameObject GrateMeshGOTemp(string name, Mesh mesh, Color color, float timeout, Transform parent)
        {
            var obj = CreateMeshGO(name, mesh, color, parent);
            var destructor = obj.AddComponent<AutoDestructor>();
            destructor.timeout = timeout;
            return obj;
        }
#endif
    }
}
