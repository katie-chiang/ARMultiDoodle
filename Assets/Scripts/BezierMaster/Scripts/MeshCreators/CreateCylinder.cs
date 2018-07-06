using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BezierMaster.MeshesCreating
{
    public class CreateCylinder : CreateMeshBase
    {
        public bool capStart = false;
        public bool capEnd = false;

        public CreateCylinder(BezierSpline spline)
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

            mesh.RecalculateBounds();

            //Debug.Log("reach");

            return mesh;
        }

        Vector3[] FixNormals(Vector3[] normals)
        {
            Vector3[] fixedNormals = normals;

            int n = fixedNormals.Length - 1;
            for (int i = 0; i <= widhtSegmentsCount; i++)
            {
                fixedNormals[n - widhtSegmentsCount + i] = fixedNormals[i];
            }

            return fixedNormals;
        }

        public static Vector3 RotateVector(Vector3 baseVector, Vector3 axis, float deg)
        {
            float angle = deg * Mathf.Deg2Rad;

            float x = axis.x;
            float y = axis.y;
            float z = axis.z;

            float c = Mathf.Cos(angle);
            float s = Mathf.Sin(angle);
            float c1 = 1 - c;

            float[,] rotationMatrix = new float[,] { { c + c1 * x * x, c1 * x * y - z * s, x * z * c1 + y * s },
                                                 { x * y * c1 + z * s, y * y * c1 + c, y * z * c1 - x * s },
                                                 { x * z * c1 - y * s, x * z * c1 + x * s, z * z * c1 + c } };

            Vector3 result = new Vector3(baseVector.x * rotationMatrix[0, 0] + baseVector.y * rotationMatrix[1, 0] + baseVector.z * rotationMatrix[2, 0],
                                         baseVector.x * rotationMatrix[0, 1] + baseVector.y * rotationMatrix[1, 1] + baseVector.z * rotationMatrix[2, 1],
                                         baseVector.x * rotationMatrix[0, 2] + baseVector.y * rotationMatrix[1, 2] + baseVector.z * rotationMatrix[2, 2]);

            return result;
        }

        private void SetVertices()
        {
            int index = 0;
            int verticesCount = lenghtSegmentsCount * (widhtSegmentsCount + 1);

            if (!spline.Loop)
            {
                if (capStart)
                    verticesCount += 1 + widhtSegmentsCount;
                if (capEnd)
                    verticesCount += 1 + widhtSegmentsCount;
            }

            vertices = new Vector3[verticesCount];
            normals = new Vector3[verticesCount];
            uv = new Vector2[verticesCount];

            //Debug.Log("length segment count: " + lenghtSegmentsCount);
            //Debug.Log("width count: " + WidthSegmentsCount);

            //generating base vertices
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

                    vertices[index] = result;
                    normals[index] = _normal;

                    //allign to selected orientation X/Y
                    Vector2 vertexUV = textureOrientation ? new Vector2(t, j / (float)(widhtSegmentsCount)) : new Vector2(j / (float)(widhtSegmentsCount), t);
                    uv[index] = vertexUV;

                    index++;
                }


            }

            //generating caps
            if (!spline.Loop)
                CreateCaps(index);


            if (spline.Loop)
            {
                int n = vertices.Length - 1;
                for (int i = 0; i <= widhtSegmentsCount; i++)
                {
                    vertices[n - widhtSegmentsCount + i] = vertices[i];
                }
            }

        }

        void CreateCaps(int index)
        {
            if (capStart)
            {
                Vector3 startPoint = spline.GetPoint(0);
                Vector3 normal = spline.GetDirection(0);

                vertices[index] = startPoint;
                normals[index] = -normal;
                uv[index++] = new Vector3(0.5f, 0.5f);

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

                    vertices[index] = result;
                    normals[index] = -spline.GetDirection(0);

                    float p = j / (float)(widhtSegmentsCount) * 360f * Mathf.Deg2Rad;
                    uv[index++] = new Vector2(Mathf.Cos(p) / 2 + 0.5f, Mathf.Sin(p) / 2 + 0.5f);
                }
            }
            if (capEnd)
            {
                Vector3 startPoint = vertices[index] = spline.GetPoint(1);
                Vector3 normal = spline.GetDirection(1);

                normals[index] = normal;
                uv[index++] = new Vector3(0.5f, 0.5f);

                float A = normal.x;
                float B = normal.y;
                float C = normal.z;

                Vector3 scale = spline.GetScale(1);
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

                    vertices[index] = result;
                    normals[index] = spline.GetDirection(1);

                    float p = j / (float)(widhtSegmentsCount) * 360f * Mathf.Deg2Rad;
                    uv[index++] = new Vector2(Mathf.Cos(p) / 2 + 0.5f, Mathf.Sin(p) / 2 + 0.5f);
                }
            }
        }

        private void SetTriangles()
        {
            int trianglesCount = (widhtSegmentsCount) * 6 * (lenghtSegmentsCount - 1);
            int _index = 0;

            if (!spline.Loop)
            {
                if (capStart)
                    trianglesCount += widhtSegmentsCount * 3;

                if (capEnd)
                    trianglesCount += widhtSegmentsCount * 3;
            }


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

            if (!spline.Loop)
            {
                int startIndex = lenghtSegmentsCount * (widhtSegmentsCount + 1);

                if (capStart)
                {
                    for (int i = 1; i <= widhtSegmentsCount; i++)
                    {
                        triangles[_index] = startIndex;
                        triangles[_index + 2] = startIndex + i;
                        triangles[_index + 1] = startIndex + i % widhtSegmentsCount + 1;
                        _index += 3;
                    }
                }
                if (capEnd)
                {
                    if (capStart)
                        startIndex += 1 + widhtSegmentsCount;

                    for (int i = 1; i <= widhtSegmentsCount; i++)
                    {
                        triangles[_index] = startIndex;
                        triangles[_index + 1] = startIndex + i;
                        triangles[_index + 2] = startIndex + i % widhtSegmentsCount + 1;
                        _index += 3;
                    }
                }
            }
        }
    }
}