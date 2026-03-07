using UnityEditor;
using UnityEngine;
using Crossatro.Grid;

[CustomEditor(typeof(CrosswordDatabase))]

public class CrosswordDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CrosswordDatabase db = (CrosswordDatabase)target;

        EditorGUILayout.Space(10);

        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        if (GUILayout.Button("Bake JSON to Database", GUILayout.Height(40)))
        {
            db.BakeDatabase();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.HelpBox("Click on this button every time the database is updated.", MessageType.Info);
    }
}