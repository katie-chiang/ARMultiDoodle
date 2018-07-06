using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BezierMaster.MeshesCreating
{
    public class CreateLine : CreateMeshBase
    {

        public CreateLine(BezierSpline spline) { this.spline = spline; }

        public override Mesh CreateMesh()
        {
            SetVertices();
            SetTriangles();

            var mesh = new Mesh();
            mesh.name = "LineMesh";

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uv;

            //mesh.RecalculateNormals();
            //mesh.RecalculateBounds();

            return mesh;
        }

        void SetVertices()
        {
            int index = 0;

            int vertexCount = !twoSided ? lenghtSegmentsCount * widhtSegmentsCount : lenghtSegmentsCount * widhtSegmentsCount * 2;

            normals = new Vector3[vertexCount];
            vertices = new Vector3[vertexCount];
            uv = new Vector2[vertexCount];

            for (int i = 0; i < lenghtSegmentsCount; i++)
            {
                float t = (i) / (float)(lenghtSegmentsCount - 1);

                Vector3 p = spline.GetPoint(t);
                Vector3 perpendicular = spline.GetDirection(t);
                Vector3 perpendicularUp = perpendicular;

                perpendicular.Set(Mathf.Cos(Mathf.Deg2Rad * spline.GetRotationZ(t)) * perpendicular.z, Mathf.Sin(Mathf.Deg2Rad * spline.GetRotationZ(t)) * perpendicular.y, Mathf.Cos(Mathf.Deg2Rad * spline.GetRotationZ(t)) * -perpendicular.x);

                perpendicularUp = Vector3.Cross(perpendicularUp, perpendicular);


                perpendicular = perpendicular.normalized * radius1 * spline.GetScale(t).magnitude;

                p += perpendicular;

                for (int n = 0; n < widhtSegmentsCount; n++)
                {
                    float r = n / (float)(widhtSegmentsCount - 1);
                    normals[index] = perpendicularUp;
                    vertices[index] = p - perpendicular * r * 2;
                    uv[index] = textureOrientation ? new Vector2(t, r) : new Vector2(r, t);
                    index++;
                }
            }

            if (twoSided)
                for (int i = 0; i < lenghtSegmentsCount; i++)
                {
                    float t = (i) / (float)(lenghtSegmentsCount - 1);

                    Vector3 p = spline.GetPoint(t);
                    Vector3 perpendicular = spline.GetDirection(t);

                    Vector3 perpendicularUp = perpendicular;


                    perpendicular.Set(Mathf.Cos(Mathf.Deg2Rad * spline.GetRotationZ(t)) * perpendicular.z, Mathf.Sin(Mathf.Deg2Rad * spline.GetRotationZ(t)), Mathf.Cos(Mathf.Deg2Rad * spline.GetRotationZ(t)) * -perpendicular.x);
                    perpendicularUp = Vector3.Cross(perpendicularUp, perpendicular);
                    perpendicular = perpendicular.normalized * radius1 * spline.GetScale(t).magnitude;

                    p += perpendicular;

                    for (int n = 0; n < widhtSegmentsCount; n++)
                    {
                        float r = n / (float)(widhtSegmentsCount - 1);
                        normals[index] = -perpendicularUp;
                        vertices[index] = p - perpendicular * r * 2;
                        uv[index] = textureOrientation ? new Vector2(t, r) : new Vector2(r, t);
                        index++;
                    }
                }           
        }

        void SetTriangles()
        {
            int trianglesCount = (widhtSegmentsCount - 1) * 6 * (lenghtSegmentsCount - 1);
            int _index = 0;

            if (twoSided)
                trianglesCount *= 2;

            triangles = new int[trianglesCount];

            for (int i = 0; i < lenghtSegmentsCount - 1; i++)
            {
                for (int j = 0; j < widhtSegmentsCount - 1; j++)
                {
                    triangles[_index] = (widhtSegmentsCount) * i + j;
                    triangles[_index + 1] = (widhtSegmentsCount) * i + 1 + j;
                    triangles[_index + 2] = (widhtSegmentsCount) * (1 + i) + j;

                    triangles[_index + 3] = triangles[_index + 1];
                    triangles[_index + 4] = (widhtSegmentsCount) * (1 + i) + 1 + j;
                    triangles[_index + 5] = triangles[_index + 2];

                    _index += 6;
                }
            }

            int offset = lenghtSegmentsCount * widhtSegmentsCount;
            if (twoSided)
            {
                for (int i = 0; i < lenghtSegmentsCount - 1; i++)
                {
                    for (int j = 0; j < widhtSegmentsCount - 1; j++)
                    {
                        triangles[_index] = offset + (widhtSegmentsCount) * i + j;
                        triangles[_index + 2] = offset + (widhtSegmentsCount) * i + 1 + j;
                        triangles[_index + 1] = offset + (widhtSegmentsCount) * (1 + i) + j;

                        triangles[_index + 3] = triangles[_index + 1];
                        triangles[_index + 4] = offset + (widhtSegmentsCount) * (1 + i) + 1 + j;
                        triangles[_index + 5] = triangles[_index + 2];

                        _index += 6;
                    }
                }
            }
        }
    }
}