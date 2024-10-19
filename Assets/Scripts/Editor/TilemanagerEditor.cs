using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileManager))]
public class TilemanagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TileManager generator = (TileManager) target;
        if (GUILayout.Button("Generate map"))
        {
            generator.GenerateMap();
        }
    }

    private void OnInspectorUpdate() 
    {
        TileManager generator = (TileManager) target;
        if (generator.mapSize < 2)
            generator.mapSize = 2;
    }
}
