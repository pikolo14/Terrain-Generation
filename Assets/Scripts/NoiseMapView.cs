using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine.Events;

public class NoiseMapView : MonoBehaviour 
{
	public Renderer TextureRender;
	public Gradient HeightGradient;

	public UnityEvent OnViewParametersChanged = new UnityEvent();

	/// <summary>
	/// Imprimir el mapa pasado en una textura
	/// </summary>
	/// <param name="noiseMap"></param>
	public void DrawNoiseMap(float[,] noiseMap) {
		int width = noiseMap.GetLength (0);
		int height = noiseMap.GetLength (1);

		Texture2D texture = new Texture2D (width, height);

		Color[] colourMap = new Color[width * height];
		for (int y = 0; y < height; y++) 
		{
			for (int x = 0; x < width; x++) 
			{
				//colourMap [y * width + x] = Color.Lerp (Color.black, Color.white, noiseMap [x, y]);
				colourMap [y * width + x] = HeightGradient.Evaluate(noiseMap[x,y]);
			}
		}
		texture.SetPixels (colourMap);
		texture.Apply ();

		TextureRender.sharedMaterial.mainTexture = texture;
		TextureRender.transform.localScale = new Vector3 (width, 1, height);
    }

    private void OnValidate()
    {
		OnViewParametersChanged?.Invoke();
    }
}