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
    private DrawMode _mode = DrawMode.Color;

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


    #region TEXTURE GENERATION

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
            chunk.UpdateMaterial();
        }
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

    #endregion


    #region EDITOR

    private void OnValidate()
    {
        OnViewParametersChanged?.Invoke();
    }

    #endregion
}