using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NodePointsGenerator))]
public class NodePointGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        NodePointsGenerator nodeGen = (NodePointsGenerator)target;

        DrawDefaultInspector();

        if(GUILayout.Button ("Generate NodePoints"))
            nodeGen.GenerateNodePointsAndPaths();
    }
}