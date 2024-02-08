using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

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
        RefreshTexture();
        GenerateCollider(meshData);
    }

    /// <summary>
    /// Vuelve a aplicar la textura almacenada en los parámetros de la clase
    /// </summary>
    public void RefreshTexture()
    {
        MeshRenderer.material.mainTexture = MeshTexture;
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
        //mapSize -= new Vector2Int(1, 1);

        Color[] colorArray = new Color[(mapSize.x) * (mapSize.y) * pixelsPerQuad*pixelsPerQuad];

        //for (int y = 0; y < mapSize.y; y++)
        //{
        //    for (int x = 0; x < mapSize.x; x++)
        //    {
        //        int quadIndex = y * mapSize.x + x;
        //        float quadProportionalHeight = data.Vertices[quadIndex].y/maxHeight;

        //        for(int py = 0; py< pixelsPerQuad; py++)
        //        {
        //            for(int px = 0; px< pixelsPerQuad; px++)
        //            {
        //                int pixelIndex = quadIndex * pixelsPerQuad*pixelsPerQuad + py * pixelsPerQuad + px;

        //                //TODO: Lerp colores dentro del quad

        //                if (_mode == DrawMode.Noise)
        //                    colorArray[pixelIndex] = Color.Lerp(Color.black, Color.white, quadProportionalHeight);
        //                else
        //                    colorArray[pixelIndex] = HeightGradient.Evaluate(quadProportionalHeight);
        //            }
        //        }
        //    }
        //}

        Vector2Int imageSize = new Vector2Int((mapSize.x-1) * pixelsPerQuad, (mapSize.y-1) * pixelsPerQuad);

        for (int y = 0; y < imageSize.y; y++)
        {
            for (int x = 0; x < imageSize.x; x++)
            {
                int pixelIndex = y * imageSize.x + x;
                int quadX = x/pixelsPerQuad;
                int quadY = y/pixelsPerQuad;
                float margin = 1f / pixelsPerQuad / 2f;
                Vector2 quadUV = new Vector2(x%pixelsPerQuad, y%pixelsPerQuad)/pixelsPerQuad + new Vector2(margin,margin);

                float height;
                float h00 = data.Vertices[quadY*mapSize.x + quadX].y;
                float h10 = data.Vertices[quadY*mapSize.x + quadX+1].y;
                float h01 = data.Vertices[(quadY+1)*mapSize.x + quadX].y;
                float h11 = data.Vertices[(quadY+1)*mapSize.x + quadX+1].y;

                height = Mathf.Lerp(Mathf.Lerp(h00, h10, quadUV.x), Mathf.Lerp(h01, h11, quadUV.x), quadUV.y)/maxHeight;
                
                if (_mode == DrawMode.Noise)
                    colorArray[pixelIndex] = Color.Lerp(Color.black, Color.white, height);
                else
                    colorArray[pixelIndex] = HeightGradient.Evaluate(height);
            }
        }

        return colorArray;
    }

    /// <summary>
    /// Coloca la textura introducida en el en render
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