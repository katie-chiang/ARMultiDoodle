using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BezierMaster.MeshesCreating
{
    public class CreateTube : CreateMeshBase
    {
        public bool capStart = false;
        public bool capEnd = false;

        public float radius2 = 10;

        int index = 0;

        public CreateTube(BezierSpline spline)
        {
            this.spline = spline;
        }

        public override Mesh CreateMesh()
        {
            SetVertices();
            SetTriangles();

            var mesh = new Mesh();
            mesh.name = "TubeMesh";

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.normals = normals;

            //mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }


        private void SetVertices()
        {
            int verticesCount = lenghtSegmentsCount * (widhtSegmentsCount + 1) * 2;

            if (!spline.Loop)
                verticesCount += widhtSegmentsCount * 4;

            vertices = new Vector3[verticesCount];
            uv = new Vector2[verticesCount];
            normals = new Vector3[verticesCount];

            //generating base vertices x1
            GenerateTubeVertices(radius1, true);

            //generating base vertices x2
            GenerateTubeVertices(radius2, false);

            //generating caps
            if (!spline.Loop)
                CreateCaps(index);

            if (spline.Loop)
            {
                int n1 = vertices.Length - 1;
                int n2 = (vertices.Length - 1) / 2;

                for (int i = 0; i <= widhtSegmentsCount; i++)
                {
                    vertices[n2 - widhtSegmentsCount + i] = vertices[i];
                    vertices[n1 - widhtSegmentsCount + i] = vertices[n2 + i + 1];
                }
            }

        }

        void GenerateTubeVertices(float radius, bool normalDir)
        {
            for (int i = 0; i < lenghtSegmentsCount; i++)
            {
                float t = (i) / (float)(lenghtSegmentsCount - 1);

                Vector3 startPoint = spline.GetPoint(t);
                Vector3 normal = spline.GetDirection(t);

                float A = normal.x;
                float B = normal.y;
                float C = normal.z;

                Vector3 scale = spline.GetScale(t);
                scale = new Vector3(scale.x - 1, scale.y - 1, scale.z - 1);

                for (int j = 0; j <= widhtSegmentsCount; j++)
                {
                    float n = (j / (float)(widhtSegmentsCount) * 360f + spline.GetRotationZ(t)) * Mathf.Deg2Rad;

                    float x = startPoint.x + radius / Mathf.Sqrt(A * A + C * C)
                        * (C * Mathf.Cos(n) - A * B * Mathf.Sin(n)
                        / Mathf.Sqrt(A * A + B * B + C * C));

                    float y = startPoint.y + radius * Mathf.Sqrt(A * A + C * C)
                        / Mathf.Sqrt(A * A + B * B + C * C) * Mathf.Sin(n);

                    float z = startPoint.z - radius / Mathf.Sqrt(A * A + C * C)
                        * (A * Mathf.Cos(n) + B * C * Mathf.Sin(n)
                        / Mathf.Sqrt(A * A + B * B + C * C));

                    Vector3 result = new Vector3(x, y, z);
                    Vector3 _normal = result - startPoint;
                    _normal = _normal.normalized;

                    Vector3 offset = new Vector3(_normal.x * scale.x, _normal.y * scale.y, _normal.z * scale.z);
                    offset = Vector3.ProjectOnPlane(offset, normal);

                    result += offset;

                    vertices[index] = result;
                    normals[index] = normalDir ? _normal : -_normal;

                    //allign to selected orientation X/Y
                    Vector2 vertexUV = textureOrientation ? new Vector2(t, j / (float)(widhtSegmentsCount)) : new Vector2(j / (float)(widhtSegmentsCount), t);
                    uv[index] = vertexUV;

                    index++;
                }
            }
        }

        void CreateCaps(int startIndex)
        {
            Vector3 startPoint = spline.GetPoint(0);
            Vector3 normal = spline.GetDirection(0);

            float A = normal.x;
            float B = normal.y;
            float C = normal.z;

            Vector3 scale = spline.GetScale(0);
            scale = new Vector3(scale.x - 1, scale.y - 1, scale.z - 1);

            for (int j = 0; j < widhtSegmentsCount; j++)
            {
                float n = (j / (float)(widhtSegmentsCount) * 360f + spline.GetRotationZ(0)) * Mathf.Deg2Rad;

                float x = startPoint.x + radius1 / Mathf.Sqrt(A * A + C * C)
                    * (C * Mathf.Cos(n) - A * B * Mathf.Sin(n)
                    / Mathf.Sqrt(A * A + B * B + C * C));

                float y = startPoint.y + radius1 * Mathf.Sqrt(A * A + C * C)
                    / Mathf.Sqrt(A * A + B * B + C * C) * Mathf.Sin(n);

                float z = startPoint.z - radius1 / Mathf.Sqrt(A * A + C * C)
                    * (A * Mathf.Cos(n) + B * C * Mathf.Sin(n)
                    / Mathf.Sqrt(A * A + B * B + C * C));

                Vector3 result = new Vector3(x, y, z);

                Vector3 _normal = result - startPoint;
                _normal = _normal.normalized;

                Vector3 offset = new Vector3(_normal.x * scale.x, _normal.y * scale.y, _normal.z * scale.z);
                offset = Vector3.ProjectOnPlane(offset, normal);

                result += offset;

                vertices[startIndex] = result;
                normals[startIndex] = -normal;

                float p = j / (float)(widhtSegmentsCount) * 360f * Mathf.Deg2Rad;
                uv[startIndex++] = new Vector2(Mathf.Cos(p) / 2 + 0.5f, Mathf.Sin(p) / 2 + 0.5f);
            }

            for (int j = 0; j < widhtSegmentsCount; j++)
            {
                float n = (j / (float)(widhtSegmentsCount) * 360f + spline.GetRotationZ(0)) * Mathf.Deg2Rad;

                float x = startPoint.x + radius2 / Mathf.Sqrt(A * A + C * C)
                    * (C * Mathf.Cos(n) - A * B * Mathf.Sin(n)
                    / Mathf.Sqrt(A * A + B * B + C * C));

                float y = startPoint.y + radius2 * Mathf.Sqrt(A * A + C * C)
                    / Mathf.Sqrt(A * A + B * B + C * C) * Mathf.Sin(n);

                float z = startPoint.z - radius2 / Mathf.Sqrt(A * A + C * C)
                    * (A * Mathf.Cos(n) + B * C * Mathf.Sin(n)
                    / Mathf.Sqrt(A * A + B * B + C * C));

                Vector3 result = new Vector3(x, y, z);

                Vector3 _normal = result - startPoint;
                _normal = _normal.normalized;

                Vector3 offset = new Vector3(_normal.x * scale.x, _normal.y * scale.y, _normal.z * scale.z);
                offset = Vector3.ProjectOnPlane(offset, normal);

                result += offset;

                vertices[startIndex] = result;
                normals[startIndex] = -normal;

                float p = j / (float)(widhtSegmentsCount) * 360f * Mathf.Deg2Rad;
                uv[startIndex++] = new Vector2(Mathf.Cos(p) / 2 * radius2 / radius1 + 0.5f, Mathf.Sin(p) / 2 * radius2 / radius1 + 0.5f);
            }

            startPoint = spline.GetPoint(1);
            normal = spline.GetDirection(1);

            A = normal.x;
            B = normal.y;
            C = normal.z;

            scale = spline.GetScale(1);
            scale = new Vector3(scale.x - 1, scale.y - 1, scale.z - 1);

            for (int j = 0; j < widhtSegmentsCount; j++)
            {
                float n = (j / (float)(widhtSegmentsCount) * 360f + spline.GetRotationZ(1)) * Mathf.Deg2Rad;

                float x = startPoint.x + radius1 / Mathf.Sqrt(A * A + C * C)
                    * (C * Mathf.Cos(n) - A * B * Mathf.Sin(n)
                    / Mathf.Sqrt(A * A + B * B + C * C));

                float y = startPoint.y + radius1 * Mathf.Sqrt(A * A + C * C)
                    / Mathf.Sqrt(A * A + B * B + C * C) * Mathf.Sin(n);

                float z = startPoint.z - radius1 / Mathf.Sqrt(A * A + C * C)
                    * (A * Mathf.Cos(n) + B * C * Mathf.Sin(n)
                    / Mathf.Sqrt(A * A + B * B + C * C));

                Vector3 result = new Vector3(x, y, z);

                Vector3 _normal = result - startPoint;
                _normal = _normal.normalized;

                Vector3 offset = new Vector3(_normal.x * scale.x, _normal.y * scale.y, _normal.z * scale.z);
                offset = Vector3.ProjectOnPlane(offset, normal);

                result += offset;

                vertices[startIndex] = result;
                normals[startIndex] = normal;

                float p = j / (float)(widhtSegmentsCount) * 360f * Mathf.Deg2Rad;
                uv[startIndex++] = new Vector2(Mathf.Cos(p) / 2 + 0.5f, Mathf.Sin(p) / 2 + 0.5f);
            }

            for (int j = 0; j < widhtSegmentsCount; j++)
            {
                float n = (j / (float)(widhtSegmentsCount) * 360f + spline.GetRotationZ(1)) * Mathf.Deg2Rad;

                float x = startPoint.x + radius2 / Mathf.Sqrt(A * A + C * C)
                    * (C * Mathf.Cos(n) - A * B * Mathf.Sin(n)
                    / Mathf.Sqrt(A * A + B * B + C * C));

                float y = startPoint.y + radius2 * Mathf.Sqrt(A * A + C * C)
                    / Mathf.Sqrt(A * A + B * B + C * C) * Mathf.Sin(n);

                float z = startPoint.z - radius2 / Mathf.Sqrt(A * A + C * C)
                    * (A * Mathf.Cos(n) + B * C * Mathf.Sin(n)
                    / Mathf.Sqrt(A * A + B * B + C * C));

                Vector3 result = new Vector3(x, y, z);

                Vector3 _normal = result - startPoint;
                _normal = _normal.normalized;

                Vector3 offset = new Vector3(_normal.x * scale.x, _normal.y * scale.y, _normal.z * scale.z);
                offset = Vector3.ProjectOnPlane(offset, normal);

                result += offset;

                vertices[startIndex] = result;
                normals[startIndex] = normal;

                float p = j / (float)(widhtSegmentsCount) * 360f * Mathf.Deg2Rad;
                uv[startIndex++] = new Vector2(Mathf.Cos(p) / 2 * radius2 / radius1 + 0.5f, Mathf.Sin(p) / 2 * radius2 / radius1 + 0.5f);
            }
        }

        private void SetTriangles()
        {
            int trianglesCount = (widhtSegmentsCount) * 6 * (lenghtSegmentsCount - 1) * 2;
            int _index = 0;

            if (!spline.Loop)
                trianglesCount += widhtSegmentsCount * 12;

            triangles = new int[trianglesCount];

            for (int i = 0; i < lenghtSegmentsCount - 1; i++)
            {
                for (int j = 0; j < widhtSegmentsCount; j++)
                {
                    triangles[_index] = (widhtSegmentsCount + 1) * i + j;
                    triangles[_index + 1] = (widhtSegmentsCount + 1) * i + (1 + j) % (widhtSegmentsCount + 1);
                    triangles[_index + 2] = (widhtSegmentsCount + 1) * (1 + i) + j;

                    triangles[_index + 3] = triangles[_index + 1];
                    triangles[_index + 4] = (widhtSegmentsCount + 1) * (1 + i) + (1 + j) % (widhtSegmentsCount + 1);
                    triangles[_index + 5] = triangles[_index + 2];

                    _index += 6;
                }
            }

            int offset = lenghtSegmentsCount * (widhtSegmentsCount + 1);
            

            for (int i = 0; i < lenghtSegmentsCount - 1; i++)
            {
                for (int j = 0; j < widhtSegmentsCount; j++)
                {
                    triangles[_index] = offset + (widhtSegmentsCount + 1) * i + j;
                    triangles[_index + 2] = offset + (widhtSegmentsCount + 1) * i + (1 + j) % (widhtSegmentsCount + 1);
                    triangles[_index + 1] = offset + (widhtSegmentsCount + 1) * (1 + i) + j;

                    triangles[_index + 3] = triangles[_index + 1];
                    triangles[_index + 4] = offset + (widhtSegmentsCount + 1) * (1 + i) + (1 + j) % (widhtSegmentsCount + 1);
                    triangles[_index + 5] = triangles[_index + 2];

                    _index += 6;
                }
            }

            if (!spline.Loop)
            {
                int startIndex = lenghtSegmentsCount * (widhtSegmentsCount + 1) * 2;

                for (int i = 0; i < widhtSegmentsCount; i++)
                {
                    triangles[_index] = startIndex + i;
                    triangles[_index + 1] = startIndex + widhtSegmentsCount + i;
                    triangles[_index + 2] = startIndex + widhtSegmentsCount + (1 + i) % (widhtSegmentsCount);

                    triangles[_index + 3] = triangles[_index];
                    triangles[_index + 4] = triangles[_index + 2];
                    triangles[_index + 5] = startIndex + (1 + i) % (widhtSegmentsCount);

                    _index += 6;
                }

                startIndex += widhtSegmentsCount * 2;

                for (int i = 0; i < widhtSegmentsCount; i++)
                {
                    triangles[_index] = startIndex + i;
                    triangles[_index + 2] = startIndex + widhtSegmentsCount + i;
                    triangles[_index + 1] = startIndex + widhtSegmentsCount + (1 + i) % (widhtSegmentsCount);

                    triangles[_index + 3] = triangles[_index];
                    triangles[_index + 5] = triangles[_index + 1];
                    triangles[_index + 4] = startIndex + (1 + i) % (widhtSegmentsCount);

                    _index += 6;
                }
            }

        }
    }
}