using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class MapGenerator : MonoBehaviour 
{
    [SerializeField]
	private MapView _mapView;
	
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

	[Tooltip("Modifica los valores de altura originales para que sean mas o menos pronunciados en ciertos rangos de altura")]
	public AnimationCurve TerrainHeightCurve;

	public bool AutoUpdate;
	private int _currentSeed;

	public UnityEvent OnGenerate = new UnityEvent();


	/// <summary>
	/// Funcion principal que genera el mapa y llama a su visualizacion
	/// </summary>
	[ExecuteAlways]
	public void GenerateMap(bool newSeed = false)
	{
		if(newSeed)
			_currentSeed = System.DateTime.Now.Millisecond;

        float[,] noiseMap = Noise.GenerateNoiseMap(MapWidth, MapHeight, _currentSeed, NoiseScale, Octaves, Persistance, Lacunarity);

		_mapView.DrawMap(noiseMap, HeightMultiplier, TerrainHeightCurve);

		OnGenerate.Invoke();
	}

	public bool IsHeightInRange(float realHeight, Vector2 heightRangeProp)
    {
		Vector2 realHeightRange = heightRangeProp * HeightMultiplier + new Vector2(_mapView.transform.position.y, _mapView.transform.position.y);
		return (realHeight > realHeightRange.x && realHeight < realHeightRange.y);
    }

    private void OnValidate()
    {
		if(!_mapView)
			_mapView = FindAnyObjectByType<MapView>();
        MapWidth = Mathf.Abs (MapWidth);
        MapHeight = Mathf.Abs (MapHeight);
    }
}
