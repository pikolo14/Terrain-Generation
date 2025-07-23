using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class MapView : MonoBehaviour 
{
    public enum DrawMode
    {
        Noise,
        Color
    }

    [SerializeField]
    public DrawMode Mode { get => _mode; set => _mode = value; }
    private static DrawMode _mode = DrawMode.Color;

    public Renderer TextureRender;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;
    public Transform ChunkParent;

    [Header("Color")]
    [Tooltip("Gradiente de color en funcion de la altura")]
	public Gradient HeightGradient;

    [Header("Texture settings")]
    [Tooltip("Número de pixeles de textura que equivaldrá cada quad del terreno en un eje")]
    public int PixelsPerQuad = 2;
    [SerializeField]
    FilterMode ImageFilter = FilterMode.Bilinear;

    [SerializeField][HideInInspector]
	public UnityEvent OnViewParametersChanged = new UnityEvent();

    [SerializeField]
    public Texture2D MeshTexture;


    /// <summary>
    /// Dibuja la malla, aplica su textura previamente creada y genera su collider
    /// </summary>
    /// <param name="meshData"></param>
    public void DrawFinalMesh(MeshData meshData)
    {
        MeshFilter.sharedMesh = meshData.GetMesh();
        RefreshMapTexture();
        GenerateCollider(meshData);
    }

    /// <summary>
    /// Vuelve a aplicar la textura almacenada en los parámetros de la clase (solo para modo sin chunks)
    /// </summary>
    public void RefreshMapTexture()
    {
        MeshRenderer.material.mainTexture = MeshTexture;
    }

    public void DrawChunks(List<TerrainChunk> chunks)
    {
        //Vaciar chunks previos
        while(ChunkParent.childCount > 0)
        {
            DestroyImmediate(ChunkParent.GetChild(ChunkParent.childCount-1).gameObject);
        }

        //Generar marlla con textura de cada chunk
        foreach (var chunk in chunks)
        {
            GameObject meshGO = new GameObject("Chunk " + chunk.Coordinates);
            meshGO.transform.parent = ChunkParent;
            var renderer = meshGO.AddComponent<MeshRenderer>();
            MeshFilter filter = meshGO.AddComponent<MeshFilter>();
            filter.sharedMesh = chunk.Mesh.GetMesh();
            renderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
            renderer.sharedMaterial.mainTexture = chunk.Texture;
        }
    }


    #region COLLIDER GENERATION

    /// <summary>
    /// Genera el collider del terreno con la malla indicada
    /// </summary>
    /// <param name="meshData"></param>
    public void GenerateCollider(MeshData meshData)
    {
        MeshCollider collider = MeshRenderer.gameObject.GetComponent<MeshCollider>();
        if(!collider)
            collider = MeshRenderer.gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = meshData.GetMesh();
    }

    #endregion


    #region TEXTURE GENERATION

    public void PrepareTerrainTexture(float[,] heightMap)
    {
        Color[] colorArray = GetColorArray(heightMap);
        MeshTexture = TextureUtils.GetTextureFromColorArray(colorArray, heightMap.GetLength(0), heightMap.GetLength(1), ImageFilter);
    }

    public void PrepareTerrainTexture(MeshData meshData, Vector2Int mapSize, float maxHeight)
    {
        Color[] colorArray = GetColorArray(meshData, mapSize, maxHeight, PixelsPerQuad);
        MeshTexture = TextureUtils.GetTextureFromColorArray(colorArray, (mapSize.x-1)*PixelsPerQuad, (mapSize.y-1)*PixelsPerQuad, ImageFilter);
    }

    public void PrepareTerrainChunkTexture(List<TerrainChunk> chunks, float maxHeight)
    {
        foreach(var chunk in chunks)
        {
            Color[] colorArray = GetColorArray(chunk.Mesh, chunk.Size, maxHeight, PixelsPerQuad);
            chunk.Texture = TextureUtils.GetTextureFromColorArray(colorArray, (chunk.Size.x) * PixelsPerQuad, (chunk.Size.y) * PixelsPerQuad, ImageFilter);
        }
    }

    /// <summary> 
    /// Devuelve el array con los colores necesario para pasarselo a una textura
    /// </summary>
    /// <param name="heightMap"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    private Color[] GetColorArray(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        Color[] colorArray = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (_mode == DrawMode.Noise)
                    colorArray[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
                else
                    colorArray[y * width + x] = HeightGradient.Evaluate(heightMap[x, y]);
            }
        }

        return colorArray;
    }

    private Color[] GetColorArray(MeshData data, Vector2Int mapSize, float maxHeight, int pixelsPerQuad)
    {
        Color[] colorArray = new Color[(mapSize.x) * (mapSize.y) * pixelsPerQuad*pixelsPerQuad];
        Vector2Int imageSize = new Vector2Int((mapSize.x) * pixelsPerQuad, (mapSize.y) * pixelsPerQuad);

        for (int y = 0; y < imageSize.y; y++)
        {
            for (int x = 0; x < imageSize.x; x++)
            {
                int pixelIndex = y * imageSize.x + x;
                int quadX = x/pixelsPerQuad;
                int quadY = y/pixelsPerQuad;
                float margin = 1f / pixelsPerQuad / 2f;
                Vector2 quadUV = new Vector2(x % pixelsPerQuad, y % pixelsPerQuad) / pixelsPerQuad + new Vector2(margin,margin);

                //TODO:Fix vertices que se tienen en cuenta para hacer interpolacion

                float h00 = data.Vertices[(quadY*(mapSize.x+1)) + quadX].y;
                float h10 = data.Vertices[(quadY*(mapSize.x+1)) + quadX+1].y;
                float h01 = data.Vertices[(quadY+1)*(mapSize.x+1) + quadX].y;
                float h11 = data.Vertices[(quadY+1)*(mapSize.x+1) + quadX+1].y;
                float height = Mathf.Lerp(Mathf.Lerp(h00, h10, quadUV.x), Mathf.Lerp(h01, h11, quadUV.x), quadUV.y)/maxHeight;
                
                if (_mode == DrawMode.Noise)
                    colorArray[pixelIndex] = Color.Lerp(Color.black, Color.white, height);
                else
                    colorArray[pixelIndex] = HeightGradient.Evaluate(height);
            }
        }

        return colorArray;
    }

    /// <summary>
    /// Coloca la textura introducida en el renderer
    /// </summary>
    /// <param name="texture"></param>
    public void DrawTexture(Texture2D texture)
	{
        TextureRender.sharedMaterial.mainTexture = texture;
        TextureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    #endregion


    #region EDITOR

    private void OnValidate()
    {
        OnViewParametersChanged?.Invoke();
    }

    #endregion
}