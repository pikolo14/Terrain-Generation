using UnityEngine;
using System.Collections;

public static class Noise {

	/// <summary>
	/// Devuelve una matriz de valores entre 0 a 1 siguiendo la función
	/// </summary>
	/// <param name="mapWidth"></param>
	/// <param name="mapHeight"></param>
	/// <param name="scale"> Escala el tamaño del ruido. A menor escala, mas se parecerá a la estática de la television</param>
	/// <returns></returns>
	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale) {
		float[,] noiseMap = new float[mapWidth,mapHeight];

		if (scale <= 0) {
			scale = 0.0001f;
		}

		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				float sampleX = x / scale;
				float sampleY = y / scale;

				float perlinValue = Mathf.PerlinNoise (sampleX, sampleY);
				noiseMap [x, y] = perlinValue;
			}
		}

		return noiseMap;
	}

}
