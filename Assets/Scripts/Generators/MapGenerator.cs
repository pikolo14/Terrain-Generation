using UnityEngine;
using System;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour 
{
    [SerializeField]
	private MapView _mapView;
	[SerializeField]
	private NodePathsGenerator _pathsGenerator;
	[SerializeField]
	private PathCarving _pathCarving;

    [SerializeField][HideInInspector]
    public MeshData MeshData;
    public Vector2Int MapSize = new Vector2Int(100,100);
    public Vector2Int ChunkSize = new Vector2Int(100, 100);
	public float NoiseScale = 10;
	public float HeightMultiplier = 1;
    public Transform ChunksParent;
    private List<TerrainChunk> _terrainChunks = new List<TerrainChunk>();

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


	[ExecuteAlways]
    private void Awake()
    {
		PrepareComponents();
    }

    /// <summary>
    /// Genera el mapa de principio a fin, pasando por la generacion de malla con noise, generacion de textura
    /// </summary>
    [ExecuteInEditMode]
	public void GenerateCompleteMap(bool newSeed = false)
	{
        if (newSeed)
			_currentSeed = DateTime.Now.Millisecond;

        //Vaciar chunks previos
        for(int i = ChunksParent.childCount-1; i >= 0; i--)
        {
            if(!Application.isPlaying)
                DestroyImmediate(ChunksParent.GetChild(i).gameObject);
            else
                Destroy(ChunksParent.GetChild(i).gameObject);
        }

        //1. Generacion de malla a partir de noise y curva de resample de alturas
        float[,] noiseMap = NoiseGeneration.GenerateNoiseMap(MapSize.x+1, MapSize.y+1, _currentSeed, NoiseScale, Octaves, Persistance, Lacunarity);
        _terrainChunks = MeshGeneration.GenerateTerrainChunks(in noiseMap, ChunkSize, HeightMultiplier, TerrainHeightCurve, ChunksParent);
        //2. Generar paths
        _pathsGenerator.GenerateNodePointsAndPaths(MapSize, transform.position, HeightMultiplier, _currentSeed);
        ////3. Aplicar carving
        //if (_pathCarving && _pathCarving.enabled && _pathCarving.gameObject.activeInHierarchy)
        //{
        //    _pathCarving.CarvePaths(ref MeshData, MapSize, _pathsGenerator.NodePaths);
        //    _mapView.GenerateCollider(MeshData);
        //}
        //4. Generar textura por alturas
        _mapView.PrepareTerrainChunkTexture(_terrainChunks, HeightMultiplier);
	}


    #region UTILS

    /// <summary>
    /// Busca y referencia los componentes necesarios para la generacion de mapa
    /// </summary>
    private void PrepareComponents()
    {
        if (_pathsGenerator == null)
            _pathsGenerator = FindObjectOfType<NodePathsGenerator>();
        if (_mapView == null)
            _mapView = GetComponent<MapView>();
        if (_pathCarving == null)
            _pathCarving = FindObjectOfType<PathCarving>();
    }

    #endregion


    #region EDITOR

    private void OnValidate()
    {
        PrepareComponents();
        MapSize.x = Mathf.Abs (MapSize.x);
        MapSize.y = Mathf.Abs (MapSize.y);
    }

    #endregion	
}
