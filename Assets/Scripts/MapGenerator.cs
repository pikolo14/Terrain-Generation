using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

	public bool autoUpdate;

	/// <summary>
	/// Funcion principal que genera el mapa y su visualizacion
	/// </summary>
	public void GenerateMap() {
		float[,] noiseMap = Noise.GenerateNoiseMap (mapWidth, mapHeight, noiseScale);

		NoiseMapView display = FindObjectOfType<NoiseMapView> ();
		display.DrawNoiseMap (noiseMap);
	}
	
}
