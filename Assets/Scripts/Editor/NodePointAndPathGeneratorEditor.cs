using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NodePointAndPathGenerator))]
public class NodePointAndPathGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NodePointAndPathGenerator nodeGen = (NodePointAndPathGenerator)target;

        DrawDefaultInspector();

        if(GUILayout.Button ("Generate all"))
            nodeGen.GenerateNodePointsAndPaths();

        if (GUILayout.Button("Remove all"))
            nodeGen.RemoveAll();
    }
}