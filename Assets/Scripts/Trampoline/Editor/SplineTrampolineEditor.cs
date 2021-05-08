using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineTrampolean))]
public class WaypointEditor : Editor
{
    private SerializedProperty _propWaypoints;
    private Vector3 _inspectorInputPos = new Vector3();
    private int _inspectorInputIndex = 0;
    Transform targetTransform;

    private void OnEnable()
    {
        SceneView.duringSceneGui += DuringSceneGUI;
        _propWaypoints = serializedObject.FindProperty("_waypoints");
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        using (new GUILayout.VerticalScope(EditorStyles.boldLabel))
        {
            EditorGUILayout.PropertyField(_propWaypoints);
            GUILayout.Space(10);

            using (new GUILayout.HorizontalScope(EditorStyles.boldLabel))
            {
                if (GUILayout.Button("Squash all Y to 0", GUILayout.Width(150)))
                {
                    for (int i = 0; i < _propWaypoints.arraySize; ++i)
                    {
                        Vector3 vec = _propWaypoints.GetArrayElementAtIndex(i).vector3Value;
                        vec.y = 0;
                        _propWaypoints.GetArrayElementAtIndex(i).vector3Value = vec;
                    }
                    serializedObject.ApplyModifiedProperties();
                }

                if (GUILayout.Button("Add Point", GUILayout.Width(100)))
                {
                    _propWaypoints.InsertArrayElementAtIndex(_inspectorInputIndex);
                    _propWaypoints.GetArrayElementAtIndex(_inspectorInputIndex).vector3Value = _inspectorInputPos;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            _inspectorInputPos = EditorGUILayout.Vector3Field("New pos", _inspectorInputPos);
            _inspectorInputIndex = EditorGUILayout.IntField("Index", _inspectorInputIndex);

            if (_inspectorInputIndex >= _propWaypoints.arraySize)
            {
                _inspectorInputIndex = _propWaypoints.arraySize;
            }
            if (_inspectorInputIndex < 0)
            {
                _inspectorInputIndex = 0;
            }
        }

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        bool shift = (Event.current.modifiers & EventModifiers.Shift) != 0;
        bool ctrl = (Event.current.modifiers & EventModifiers.Control) != 0;
        var targetObject = target as GameObject;
        for (int i = 0; i < _propWaypoints.arraySize; i++)
        {
            SerializedProperty prop = _propWaypoints.GetArrayElementAtIndex(i);
            var nextElement = Mathf.Clamp(i + 1, 0, _propWaypoints.arraySize - 1);
            if (shift) // Add points at mouse
            {
                Vector3 a = _propWaypoints.GetArrayElementAtIndex(i).vector3Value;
                Vector3 b = _propWaypoints.GetArrayElementAtIndex(nextElement).vector3Value;

                Vector3 betweenPoints = a + 0.5f * (b - a);
                Handles.color = Color.green;
                if (Handles.Button(betweenPoints, Quaternion.LookRotation(Camera.current.transform.forward, Camera.current.transform.up), 0.25f, 0.25f, Handles.SphereHandleCap))
                {
                    _propWaypoints.InsertArrayElementAtIndex(i + 1);
                    _propWaypoints.GetArrayElementAtIndex(i + 1).vector3Value = betweenPoints;
                }

            }
            else if (ctrl) // Delete points at mouse
            {
                Handles.color = Color.red;
                if (Handles.Button(prop.vector3Value, Quaternion.LookRotation(Camera.current.transform.forward, Camera.current.transform.up), 0.25f, 0.25f, Handles.SphereHandleCap))
                {
                    _propWaypoints.DeleteArrayElementAtIndex(i);
                    i--;
                    continue;
                }
            }

            else // Move point at mouse
            {
                prop.vector3Value = Handles.PositionHandle(prop.vector3Value, Quaternion.identity);
            }

            Handles.color = Color.white;
            Handles.SphereHandleCap(0, prop.vector3Value, Quaternion.identity, 0.05f, EventType.Repaint);
            Handles.DrawAAPolyLine(_propWaypoints.GetArrayElementAtIndex(i).vector3Value, _propWaypoints.GetArrayElementAtIndex(nextElement).vector3Value);
        }
        serializedObject.ApplyModifiedProperties();
        DrawInformationBox();
    }


    void DrawInformationBox()
    {
        Rect size = new Rect(0, 0, 220, 100);
        float sizeButton = 20;
        Handles.BeginGUI();

        GUI.BeginGroup(new Rect(Screen.width - size.width - 10, Screen.height - size.height - 50, size.width, size.height));
        GUI.Box(size, "Waypoint system");

        Rect rc = new Rect(5, sizeButton, size.width, sizeButton);
        GUI.Label(rc, "Shift Click to Add");
        rc.y += sizeButton;

        GUI.Label(rc, "Ctrl Click to Delete");
        rc.y += sizeButton;


        GUI.EndGroup();
        Handles.EndGUI();
    }
}

