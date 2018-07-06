using UnityEngine;
using UnityEditor;
using System;
using BezierMaster.MeshesCreating;

namespace BezierMaster
{
    [Flags]
    public enum EditorListOption
    {
        None = 0,
        ListSize = 1,
        ListLabel = 2,
        Default = ListSize | ListLabel
    }

    [CustomEditor(typeof(BezierMaster))]
    public class BezierMasterInspectorExt : Editor
    {
        private BezierMaster master;
        private BezierSpline spline;
        private Transform handleTransform;
        private Quaternion handleRotation;

        private const int lineSteps = 3;
        private const float directionScale = 0.05f;
        private const float handleSize = 0.1f;

        private int selectedIndex = -1;
        int count;
        int countTouch = 1;
        


        private void OnEnable()
        {
            master = target as BezierMaster;
            master.spline = master.GetComponent<BezierSpline>();

            if (spline == null)
                spline = master.spline;

            count = master.ObjectsCount;
        }



        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Welcome to Besier Master! Just create line and then \nchoose options below");
            GUILayout.Space(5);

            GUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            master.showCurveEditor = EditorGUILayout.Foldout(master.showCurveEditor, "Curve Editor");
            if (master.showCurveEditor)
                CurveEditor();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            master.showObjectsOptions = EditorGUILayout.Foldout(master.showObjectsOptions, "Objects Instatiating");
            if (master.showObjectsOptions)
            {
                master.usingOfSpline = (Using)EditorGUILayout.Popup((int)master.usingOfSpline, new string[] { "Objects", "Mesh", "None" });

                switch (master.usingOfSpline)
                {
                    case Using.Objects:
                        DestroyImmediate(master.meshGO);
                        ObjectsInstantiating();
                        break;
                    case Using.Mesh:
                        master.Clear(true);
                        MeshInstatiating();
                        break;

                    case Using.None:
                        DestroyImmediate(master.meshGO);
                        master.Clear(true);
                        break;
                }

                if (master.usingOfSpline != Using.None)
                {

                    GUILayout.BeginVertical("box");
                    GUILayout.Space(5);

                    master.autoUpdate = EditorGUILayout.Toggle("Auto Update", master.autoUpdate);
                    if (!master.autoUpdate)
                    {
                        if (GUILayout.Button("Update"))
                            master.UpdateMaster(true);
                    }
                    else
                        master.UpdateMaster(false);

                    GUILayout.Space(5);


                    if (GUILayout.Button("Detach Objects"))
                    {
                        Undo.RecordObject(master, "Detach Objects");
                        EditorUtility.SetDirty(master);
                        master.DetachObjects();
                        master.usingOfSpline = Using.None;
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();


            GUILayout.BeginVertical("box");
            master.showAnimationOptions = EditorGUILayout.Foldout(master.showAnimationOptions, "Animation");
            if (master.showAnimationOptions)
            {
                Animation();
            }
            GUILayout.EndVertical();

        }

        private void CurveEditor()
        {
            if (spline == null)
                spline = master.spline;

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Add Curve"))
            {
                Undo.RecordObject(spline, "Add Curve");
                spline.AddCurve();
                EditorUtility.SetDirty(spline);
            }
            if (GUILayout.Button("Remove Curve"))
            {
                Undo.RecordObject(spline, "Remove Curve");
                spline.RemoveCurve(selectedIndex);
                selectedIndex = -1;
                EditorUtility.SetDirty(spline);
            }
            if (GUILayout.Button("Reset"))
            {
                Undo.RecordObject(spline, "Reset");
                spline.Reset();
                master.Reset();
                selectedIndex = -1;
                EditorUtility.SetDirty(spline);
            }
            //Debug.Log("begin vertical");
            GUILayout.BeginVertical("box");
            GUILayout.Space(5);
            bool loop = EditorGUILayout.Toggle("Loop", spline.Loop);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Toggle Loop");
                EditorUtility.SetDirty(spline);
                spline.Loop = loop;
            }

            GUILayout.Label("    Curve Lenght: " + spline.GetCurveLenght());
            GUILayout.Space(5);
            GUILayout.EndVertical();

            if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount){
                DrawSelectedPointInspector();
                Debug.Log("Draw selected point");
            }
            else
            {
                //Debug.Log("else");
                GUILayout.BeginVertical("box");
                GUILayout.Space(5);
                GUILayout.Label("   Select point to edit!");
                GUILayout.Space(5);
                GUILayout.EndVertical();
            }
        }

        private void DrawSelectedPointInspector()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Selected Point: " + selectedIndex);
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                spline.SetControlPoint(selectedIndex, point);

            }
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            BezierControlPointMode mode = (BezierControlPointMode)
                 EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Change Point Mode");
                spline.SetControlPointMode(selectedIndex, mode);
                EditorUtility.SetDirty(spline);
            }

            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            float zRotation = EditorGUILayout.FloatField("Z Rotation At Point", spline.zRotationAtPoint[(selectedIndex + 1) / 3]);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Variable change");
                int index = (selectedIndex + 1) / 3;
                EditorUtility.SetDirty(spline);
                spline.zRotationAtPoint[index] = zRotation;

                if (spline.Loop && (index == spline.zRotationAtPoint.Length - 1 || index == 0))
                    spline.zRotationAtPoint[index] = spline.zRotationAtPoint[0] = zRotation;

            }
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            Vector3 scale = EditorGUILayout.Vector3Field("Scale Factor", spline.scaleFactor3d[(selectedIndex + 1) / 3]);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Variable change");
                int index = (selectedIndex + 1) / 3;
                EditorUtility.SetDirty(spline);
                spline.scaleFactor3d[index] = scale;

                if (spline.Loop && (index == spline.zRotationAtPoint.Length - 1 || index == 0))
                    spline.scaleFactor3d[index] = spline.scaleFactor3d[0] = scale;

            }


            GUILayout.Space(5);
            GUILayout.EndVertical();

        }

        private void ObjectsInstantiating()
        {
            GUILayout.BeginVertical("box");
            serializedObject.Update();
            ShowElements(serializedObject.FindProperty("objectsPrefabs"));
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(3);

            EditorGUI.BeginChangeCheck();
            count = EditorGUILayout.IntField("Objects Count", master.ObjectsCount);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.ObjectsCount = count;
            }

            GUILayout.Space(5);
            master.showRotationsOptions = EditorGUILayout.Foldout(master.showRotationsOptions, "Adjust Rotation");
            if (master.showRotationsOptions)
            {
                EditorGUI.indentLevel += 1;
                GUILayout.Space(5);
                EditorGUI.BeginChangeCheck();
                bool applyRotationX = EditorGUILayout.Toggle("Apply Rotation X", master.ApplyRotationX);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(master, "Variable change");
                    EditorUtility.SetDirty(master);
                    master.ApplyRotationX = applyRotationX;
                }

                EditorGUI.BeginChangeCheck();
                bool applyRotationY = EditorGUILayout.Toggle("Apply Rotation Y", master.ApplyRotationY);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(master, "Variable change");
                    EditorUtility.SetDirty(master);
                    master.ApplyRotationY = applyRotationY;
                }

                EditorGUI.BeginChangeCheck();
                bool applyRotationZ = EditorGUILayout.Toggle("Apply Rotation Z", master.ApplyRotationZ);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(master, "Variable change");
                    EditorUtility.SetDirty(master);
                    master.ApplyRotationZ = applyRotationZ;
                }

                GUILayout.Space(5);

                EditorGUI.BeginChangeCheck();
                Vector3 rotation = EditorGUILayout.Vector3Field("Rotation", master.addRotation.eulerAngles);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(master, "Variable change");
                    EditorUtility.SetDirty(master);
                    master.addRotation = Quaternion.Euler(rotation);
                }
                EditorGUI.indentLevel -= 1;
            }
            GUILayout.Space(5);
            master.showObjRandomisationOptions = EditorGUILayout.Foldout(master.showObjRandomisationOptions, "Randomise");
            if (master.showObjRandomisationOptions)
            {
                EditorGUI.indentLevel += 1;

                EditorGUI.BeginChangeCheck();
                bool randomise = EditorGUILayout.Toggle("Randomise", master.randomise);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(master, "Variable change");
                    EditorUtility.SetDirty(master);
                    master.randomise = randomise;
                    master.UpdateMaster(true);
                }

                EditorGUI.BeginChangeCheck();
                Vector3 MaxOffset = EditorGUILayout.Vector3Field("Max Possition Offset", master.offsetRandomMaximum);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(master, "Variable change");
                    EditorUtility.SetDirty(master);
                    master.offsetRandomMaximum = MaxOffset;

                    if (master.autoUpdate)
                        master.UpdateMaster(true);

                }

                EditorGUI.BeginChangeCheck();
                Vector3 MaxRotation = EditorGUILayout.Vector3Field("Max Rotation Offset", master.rotationRandomMaximum);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(master, "Variable change");
                    EditorUtility.SetDirty(master);
                    master.rotationRandomMaximum = MaxRotation;

                    if (master.autoUpdate)
                        master.UpdateMaster(true);
                }
                EditorGUI.BeginChangeCheck();
                Vector3 MaxScale = EditorGUILayout.Vector3Field("Max Scale Offset", master.scaleRandomMaximum);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(master, "Variable change");
                    EditorUtility.SetDirty(master);
                    master.scaleRandomMaximum = MaxScale;
                    if (master.autoUpdate)
                        master.UpdateMaster(true);
                }

                EditorGUI.indentLevel -= 1;
            }
            GUILayout.Space(5);

            GUILayout.EndVertical();
        }

        private void MeshInstatiating()
        {
            GUILayout.BeginVertical("box");

            GUILayout.Space(5);
            master.meshType = (MeshType)EditorGUILayout.Popup("Mesh Type", (int)master.meshType, new string[] { "Line", "Cylinder", "Tube" });


            switch (master.meshType)
            {
                case MeshType.Line:
                    CreateLineMesh();
                    break;

                case MeshType.Cylinder:
                    CreateCylinderMesh();
                    break;

                case MeshType.Tube:
                    CreateTubeMesh();
                    break;
            }

            GUILayout.Space(5);
            GUILayout.Space(5);

            master.showObjRandomisationOptions = EditorGUILayout.Foldout(master.showObjRandomisationOptions, "Randomise");
            master.randomise = master.showObjRandomisationOptions;
            if (master.showObjRandomisationOptions)
            {
                EditorGUI.indentLevel += 1;




                EditorGUI.BeginChangeCheck();
                Vector3 MaxOffset = EditorGUILayout.Vector3Field("Max Possition Offset", master.offsetRandomMaximum);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(master, "Variable change");
                    EditorUtility.SetDirty(master);
                    master.offsetRandomMaximum = MaxOffset;

                    if (master.autoUpdate)
                        master.UpdateMaster(true);

                }

                EditorGUI.BeginChangeCheck();
                Vector3 MaxRotation = EditorGUILayout.Vector3Field("Max Rotation Offset", master.rotationRandomMaximum);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(master, "Variable change");
                    EditorUtility.SetDirty(master);
                    master.rotationRandomMaximum = MaxRotation;

                    if (master.autoUpdate)
                        master.UpdateMaster(true);
                }
                EditorGUI.BeginChangeCheck();
                Vector3 MaxScale = EditorGUILayout.Vector3Field("Max Scale Offset", master.scaleRandomMaximum);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(master, "Variable change");
                    EditorUtility.SetDirty(master);
                    master.scaleRandomMaximum = MaxScale;
                    if (master.autoUpdate)
                        master.UpdateMaster(true);
                }


                EditorGUI.indentLevel -= 1;
            }
            GUILayout.Space(5);

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GUILayout.Label("Vertex Count: " + master.verticesCount);
            GUILayout.Label("Triangles Count: " + master.trianglesCount);
            GUILayout.EndVertical();
        }

        private void CreateLineMesh()
        {
            var meshCreator = new CreateLine(spline);
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            float widht = EditorGUILayout.FloatField("Width", master.radius1);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.radius1 = widht;
            }

            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            int lenghtSegmentsCount = EditorGUILayout.IntField("Lenght Segments", master.LenghtSegmentsCount);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.LenghtSegmentsCount = lenghtSegmentsCount;
            }

            EditorGUI.BeginChangeCheck();
            int widthSegmentsCount = EditorGUILayout.IntField("Width Segments", master.WidthSegmentsCount);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.WidthSegmentsCount = widthSegmentsCount;
            }

            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            bool twoSided = EditorGUILayout.Toggle("Two Sided", master.twoSided);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.twoSided = twoSided;
            }

            EditorGUI.BeginChangeCheck();
            bool textureOrientation = EditorGUILayout.Toggle("Texture Orientation X/Y", master.textureOrientation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.textureOrientation = textureOrientation;
            }

            meshCreator.LenghtSegmentsCount = master.LenghtSegmentsCount;
            meshCreator.WidthSegmentsCount = master.WidthSegmentsCount;
            meshCreator.radius1 = master.radius1;
            meshCreator.twoSided = master.twoSided;
            meshCreator.textureOrientation = textureOrientation;

            master.meshCreator = meshCreator;
        }

        private void CreateCylinderMesh()
        {
            var meshCreator = new CreateCylinder(spline);
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            float radius = EditorGUILayout.FloatField("Radius", master.radius1);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.radius1 = radius;
            }

            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();

            int lenghtSegmentsCount = EditorGUILayout.IntField("Lenght Segments", master.LenghtSegmentsCount);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.LenghtSegmentsCount = lenghtSegmentsCount;
            }

            EditorGUI.BeginChangeCheck();
            int widthSegmentsCount = EditorGUILayout.IntField("Width Segments", master.WidthSegmentsCount);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.WidthSegmentsCount = widthSegmentsCount;
            }

            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            bool capStart = EditorGUILayout.Toggle("Cap Start", master.capStart);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.capStart = capStart;
            }

            EditorGUI.BeginChangeCheck();
            bool capEnd = EditorGUILayout.Toggle("Cap End", master.capEnd);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.capEnd = capEnd;
            }

            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            bool textureOrientation = EditorGUILayout.Toggle("Texture Orientation X/Y", master.textureOrientation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.textureOrientation = textureOrientation;
            }

            meshCreator.LenghtSegmentsCount = master.LenghtSegmentsCount;
            meshCreator.WidthSegmentsCount = master.WidthSegmentsCount;
            meshCreator.radius1 = master.radius1;
            meshCreator.textureOrientation = textureOrientation;
            meshCreator.capStart = master.capStart;
            meshCreator.capEnd = master.capEnd;

            master.meshCreator = meshCreator;
        }

        private void CreateTubeMesh()
        {
            var meshCreator = new CreateTube(spline);
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            float radius = EditorGUILayout.FloatField("Radius 1", master.radius1);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.radius1 = radius;
            }

            EditorGUI.BeginChangeCheck();
            float radius2 = EditorGUILayout.FloatField("Radius 2", master.radius2);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.radius2 = radius2;
            }

            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();

            int lenghtSegmentsCount = EditorGUILayout.IntField("Lenght Segments", master.LenghtSegmentsCount);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.LenghtSegmentsCount = lenghtSegmentsCount;
            }

            EditorGUI.BeginChangeCheck();
            int widthSegmentsCount = EditorGUILayout.IntField("Width Segments", master.WidthSegmentsCount);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.WidthSegmentsCount = widthSegmentsCount;
            }


            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            bool textureOrientation = EditorGUILayout.Toggle("Texture Orientation X/Y", master.textureOrientation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(master, "Variable change");
                EditorUtility.SetDirty(master);
                master.textureOrientation = textureOrientation;
            }

            meshCreator.LenghtSegmentsCount = master.LenghtSegmentsCount;
            meshCreator.WidthSegmentsCount = master.WidthSegmentsCount;
            meshCreator.radius1 = master.radius1;
            meshCreator.radius2 = master.radius2;
            meshCreator.textureOrientation = textureOrientation;


            master.meshCreator = meshCreator;
        }

        private void Animation()
        {
            GUILayout.Label("  You can animate some script variables in Animation window! \n  Vertex animation coming soon!");
        }

        private static void ShowElements(SerializedProperty list)
        {
            EditorGUILayout.PropertyField(list);

            if (list.arraySize == 0)
                list.arraySize = 1;

            if (list.GetArrayElementAtIndex(0) == null)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label("Add prefab!");

                GUILayout.Space(20);
                GUILayout.EndVertical();
            }

            if (list.isExpanded)
            {
                EditorGUI.indentLevel += 1;


                for (int i = 0; i < list.arraySize; i++)
                    EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent("Object " + (i + 1)));




                EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
                EditorGUI.indentLevel -= 1;
            }
            else
            {
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(0), GUIContent.none);
            }
        }

        private void OnSceneGUI()
        {
            if (spline == null)
                spline = master.spline;

            handleTransform = spline.transform;

            handleRotation = Tools.pivotRotation == PivotRotation.Local ?
                handleTransform.rotation : Quaternion.identity;

            Vector3 p0 = ShowPoint(0);
            for (int i = 1; i < spline.ControlPointCount; i += 3)
            {
                Vector3 p1 = ShowPoint(i);
                Vector3 p2 = ShowPoint(i + 1);
                Vector3 p3 = ShowPoint(i + 2);

                Handles.color = modeColors[(int)spline.GetControlPointMode(i)];
                Handles.DrawLine(p0, p1);
                Handles.color = modeColors[(int)spline.GetControlPointMode(i + 2)];
                Handles.DrawLine(p2, p3);
                Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
                p0 = p3;
            }

            
        }

        private static Color[] modeColors = {
        Color.white,
        Color.yellow,
        Color.cyan
    };

        private Vector3 ShowPoint(int index)
        {
            Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
            float size = HandleUtility.GetHandleSize(point);

            if (index % 3 == 0)
                size *= (handleSize * 1.5f);
            else
                size *= handleSize;

            EditorGUI.BeginChangeCheck();

            Handles.color = modeColors[(int)spline.GetControlPointMode(index)];



            if (Handles.Button(point, handleRotation, size, size, Handles.SphereHandleCap))
            {
                selectedIndex = index;

            }

            if (index == selectedIndex)
            {
                point = Handles.DoPositionHandle(point, handleRotation);
                if (EditorGUI.EndChangeCheck())
                {


                    Undo.RecordObject(spline, "Move Point");
                    EditorUtility.SetDirty(spline);
                    spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));


                }
                Repaint();
            }
            return point;
        }

        private void ShowVelocity()
        {
            Handles.color = Color.green;
            Vector3 point = master.transform.TransformPoint(spline.GetPoint(0f));
            Handles.DrawLine(point, point + spline.GetVelocity(0f) * directionScale);

            int ls = lineSteps * spline.ControlPointCount;
            for (int i = 1; i <= ls; i++)
            {
                point = master.transform.TransformPoint(spline.GetPoint(i / (float)ls));
                Handles.DrawLine(point, point + spline.GetVelocity(i / (float)ls) * directionScale);
            }
        }
    }
}
    
