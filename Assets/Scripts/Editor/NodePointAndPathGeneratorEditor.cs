using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NodePathsGenerator))]
public class NodePointAndPathGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NodePathsGenerator nodeGen = (NodePathsGenerator)target;

        DrawDefaultInspector();

        if(GUILayout.Button ("Generate all"))
            nodeGen.GenerateNodePointsAndPaths();

        if (GUILayout.Button("Remove all"))
            nodeGen.RemoveAll();
    }
}