using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

	public int MapWidth = 100;
	public int MapHeight = 100;
	public float NoiseScale = 10;
	public float HeightMultiplier = 1;

	[Range(1,10)]
	public int Octaves = 3;
	[Range(1,10)]
	public float Lacunarity = 2;
	[Range(0, 1)]
	public float Persistance = 0.5f;

	public bool AutoUpdate;
	public bool AutoGenerateSeed = true;
	private int _currentSeed;


	/// <summary>
	/// Funcion principal que genera el mapa y llama a su visualizacion
	/// </summary>
	[ExecuteAlways]
	public void GenerateMap()
	{
		if(AutoGenerateSeed)
			_currentSeed = System.DateTime.Now.Millisecond;

        float[,] noiseMap = Noise.GenerateNoiseMap(MapWidth, MapHeight, _currentSeed, NoiseScale, Octaves, Persistance, Lacunarity);

		MapView display = FindObjectOfType<MapView>();
		display.DrawMap(noiseMap, HeightMultiplier);
	}

    private void OnValidate()
    {
        MapWidth = Mathf.Abs (MapWidth);
        MapHeight = Mathf.Abs (MapHeight);
    }
}
