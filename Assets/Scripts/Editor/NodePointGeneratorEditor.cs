using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NodePointsAndPathsGenerator))]
public class NodePointGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NodePointsAndPathsGenerator nodeGen = (NodePointsAndPathsGenerator)target;

        DrawDefaultInspector();

        if(GUILayout.Button ("Generate NodePoints"))
            nodeGen.GenerateNodePointsAndPaths();
    }
}