using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tile))]
public class TileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        OnInspectorUpdate();
    }

    public void OnInspectorUpdate()
    {
        Tile tile = (Tile)target;
        tile.UpdateType();
    }
}
