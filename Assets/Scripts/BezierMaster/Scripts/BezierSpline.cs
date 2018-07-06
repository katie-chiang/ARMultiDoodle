//
//Large part from catlikecoding.com Curves And Splines Tutorial
//Thanks to Jasper Flick 
//
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;


namespace BezierMaster
{
    public enum BezierControlPointMode
    {
        Free,
        Aligned,
        Mirrored
    }

    [Serializable]
    public class BezierSpline : MonoBehaviour
    {
        [SerializeField]
        private Vector3[] points;

        [SerializeField]
        private BezierControlPointMode[] modes;

        [SerializeField]
        public float[] zRotationAtPoint;

        [SerializeField]
        public Vector3[] scaleFactor3d;

        [SerializeField]
        private bool loop;
        private const int lenghtResolution = 10;

        public bool Loop
        {
            get
            {
                return loop;
            }
            set
            {

                loop = value;
                if (value == true)
                {
                    if (ControlPointCount <= 1)
                        AddCurve();

                    modes[modes.Length - 1] = modes[0];
                    zRotationAtPoint[zRotationAtPoint.Length - 1] = zRotationAtPoint[0];
                    scaleFactor3d[scaleFactor3d.Length - 1] = scaleFactor3d[0];
                    SetControlPoint(points.Length - 1, points[0]);
                }
            }
        }

        public void Reset()
        {
            points = new Vector3[] {

                Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero
            };
         

            modes = new BezierControlPointMode[] {
                BezierControlPointMode.Aligned,
                BezierControlPointMode.Aligned
            };

            zRotationAtPoint = new float[] { 0, 0 };

            scaleFactor3d = new Vector3[] { Vector3.one, Vector3.one };

            loop = false;
        }

        public void Reset(Vector3 touchPosition)
        {
            points = new Vector3[] {
                new Vector3(touchPosition.x, touchPosition.y, touchPosition.z),
                new Vector3(touchPosition.x, touchPosition.y, touchPosition.z),
                new Vector3(touchPosition.x, touchPosition.y, touchPosition.z),
                new Vector3(touchPosition.x, touchPosition.y, touchPosition.z)
            };
         

            modes = new BezierControlPointMode[] {
                BezierControlPointMode.Aligned,
                BezierControlPointMode.Aligned
            };

            zRotationAtPoint = new float[] { 0, 0 };

            scaleFactor3d = new Vector3[] { Vector3.one, Vector3.one };

            loop = false;
        }

        public int ControlPointCount
        {
            get
            {
                return points.Length;
            }
        }

        public Vector3 GetControlPoint(int index)
        {
            return points[index > points.Length - 1 ? points.Length - 1 : index];
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            if (index % 3 == 0)
            {
                Vector3 delta = point - points[index];
                if (loop)
                {
                    if (index == 0)
                    {
                        points[1] += delta;
                        points[points.Length - 2] += delta;
                        points[points.Length - 1] = point;
                    }
                    else if (index == points.Length - 1)
                    {
                        points[0] = point;
                        points[1] += delta;
                        points[index - 1] += delta;
                    }
                    else
                    {
                        points[index - 1] += delta;
                        points[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        points[index - 1] += delta;
                    }
                    if (index + 1 < points.Length)
                    {
                        points[index + 1] += delta;
                    }
                }
            }
            points[index] = point;
            EnforceMode(index);
        }

        public Vector3 GetPoint(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                i = (int)t;
                t -= i;
                i *= 3;
            }
            return BezierBase.GetPoint(
                points[i], points[i + 1], points[i + 2], points[i + 3], t);
        }



        public float GetRotationZ(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = zRotationAtPoint.Length - 1;

                return zRotationAtPoint[i];
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                i = (int)t;
                t -= i;
            }

            float rot = Mathf.Lerp(zRotationAtPoint[i], zRotationAtPoint[i + 1], t);
            return rot;
        }

        public Vector3 GetScale(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = scaleFactor3d.Length - 1;

                return scaleFactor3d[i];
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                i = (int)t;
                t -= i;
            }

            Vector3 scale = Vector3.Lerp(scaleFactor3d[i], scaleFactor3d[i + 1], t);
            return scale;
        }

        public Vector3 GetVelocity(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                i = (int)t;
                t -= i;
                i *= 3;
            }
            return BezierBase.GetFirstDerivative(
                points[i], points[i + 1], points[i + 2], points[i + 3], t);
        }

        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }

        public float GetCurveLenght()
        {
            float lenght = 0;
            Vector3 lastPoint = GetPoint(0);

            for (int i = 1; i < lenghtResolution; i++)
            {
                var next = GetPoint((float)i / lenghtResolution);
                lenght += Vector3.Magnitude(next - lastPoint);
                lastPoint = next;
            }

            return lenght;
        }

        public BezierControlPointMode GetControlPointMode(int index)
        {
            int ind = (index + 1) / 3;

            return modes[ind >= modes.Length ? 0 : ind];
        }

        public void SetControlPointMode(int index, BezierControlPointMode mode)
        {
            int modeIndex = (index + 1) / 3;
            modes[modeIndex] = mode;
            if (loop)
            {
                if (modeIndex == 0)
                {
                    modes[modes.Length - 1] = mode;
                }
                else if (modeIndex == modes.Length - 1)
                {
                    modes[0] = mode;
                }
            }
            EnforceMode(index);
        }

        private void EnforceMode(int index)
        {
            int modeIndex = (index + 1) / 3;
            BezierControlPointMode mode = modes[modeIndex];
            if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1))
            {
                return;
            }
            int middleIndex = modeIndex * 3;
            int fixedIndex, enforcedIndex;

            if (index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                if (fixedIndex < 0)
                {
                    fixedIndex = points.Length - 2;
                }
                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= points.Length)
                {
                    enforcedIndex = 1;
                }
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= points.Length)
                {
                    fixedIndex = 1;
                }
                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                {
                    enforcedIndex = points.Length - 2;
                }
            }

            Vector3 middle = points[middleIndex];
            Vector3 enforcedTangent = middle - points[fixedIndex];
            if (mode == BezierControlPointMode.Aligned)
            {
                enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
            }
            points[enforcedIndex] = middle + enforcedTangent;
        }
        
        public void AddCurve()
        {
            
            
            Vector3 point = points[points.Length - 1];
            Array.Resize(ref points, points.Length + 3);
            point.x += 20f;
            points[points.Length - 3] = point;
            point.x += 20f;
            point.z += 20f;
            points[points.Length - 2] = point;
            point.x += 20f;

            points[points.Length - 1] = point;

            Array.Resize(ref modes, modes.Length + 1);
            modes[modes.Length - 1] = modes[modes.Length - 2];
            EnforceMode(points.Length - 4);

            Array.Resize(ref zRotationAtPoint, modes.Length);
            Array.Resize(ref scaleFactor3d, modes.Length);
            scaleFactor3d[scaleFactor3d.Length - 1] = Vector3.one;

            if (loop)
            {
                points[points.Length - 1] = points[0];
                modes[modes.Length - 1] = modes[0];
                zRotationAtPoint[zRotationAtPoint.Length - 1] = zRotationAtPoint[0];
                scaleFactor3d[scaleFactor3d.Length - 1] = scaleFactor3d[0];
                EnforceMode(0);
            }
        }

        public void AddCurve(Vector3 touchPosition)
        {

            Vector3 point = touchPosition;

            Array.Resize(ref points, points.Length + 3);

            points[points.Length - 3] = point;

            points[points.Length - 2] = point;

            points[points.Length - 1] = point;

            Array.Resize(ref modes, modes.Length + 1);
            modes[modes.Length - 1] = modes[modes.Length - 2];
            EnforceMode(points.Length - 4);

            Array.Resize(ref zRotationAtPoint, modes.Length);
            Array.Resize(ref scaleFactor3d, modes.Length);
            scaleFactor3d[scaleFactor3d.Length - 1] = Vector3.one;

            //if (loop)
            //{
            //    points[points.Length - 1] = points[0];
            //    modes[modes.Length - 1] = modes[0];
            //    zRotationAtPoint[zRotationAtPoint.Length - 1] = zRotationAtPoint[0];
            //    scaleFactor3d[scaleFactor3d.Length - 1] = scaleFactor3d[0];
            //    EnforceMode(0);
            //}
        }

        public void RemoveCurve(int index)
        {

            if (CurveCount <= 1 || index < 0)
                return;

            if (index > 1 && index % 3 != 2)
                index -= 1 + index % 3;
            else if (index == 1)
                index--;


            Debug.Log(index);


            for (int i = index; i < points.Length - 3; i++)
            {
                int modeIndex = (i + 1) / 3;
                points[i] = points[i + 3];

                if (modeIndex < CurveCount)
                {
                    modes[modeIndex] = modes[modeIndex + 1];
                    zRotationAtPoint[modeIndex] = zRotationAtPoint[modeIndex + 1];
                }
            }


            Array.Resize(ref points, points.Length - 3);
            Array.Resize(ref modes, modes.Length - 1);
            Array.Resize(ref zRotationAtPoint, modes.Length);
            Array.Resize(ref scaleFactor3d, modes.Length);

            EnforceMode(points.Length - 4);

            if (loop)
            {
                points[points.Length - 1] = points[0];
                modes[modes.Length - 1] = modes[0];
                zRotationAtPoint[zRotationAtPoint.Length - 1] = zRotationAtPoint[0];
                scaleFactor3d[scaleFactor3d.Length - 1] = scaleFactor3d[0];
                EnforceMode(0);
            }

        }

        public int CurveCount
        {
            get
            {
                return (points.Length - 1) / 3;
            }
        }
    }
}