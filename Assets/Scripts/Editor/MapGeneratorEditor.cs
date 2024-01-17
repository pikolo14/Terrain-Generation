using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor {

	public override void OnInspectorGUI() {
		MapGenerator mapGen = (MapGenerator)target;

		if (DrawDefaultInspector ()) {
			if (mapGen.AutoUpdate) {
				mapGen.GenerateMap ();
			}
		}

		if (GUILayout.Button ("Regenerate")) {
			mapGen.GenerateMap (false);
		}

        if (GUILayout.Button("Generate new seed"))
        {
            mapGen.GenerateMap(true);
        }
    }
}