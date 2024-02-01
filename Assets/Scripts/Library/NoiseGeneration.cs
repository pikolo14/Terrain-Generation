using UnityEngine;
using System.Collections;


public static class NoiseGeneration

{
	private const int _maxRandomSeed = 100000;

	/// <summary>
	/// Devuelve una matriz de valores entre 0 a 1 siguiendo la función
	/// </summary>
	/// <param name="mapX"></param>
	/// <param name="mapY"></param>
	/// <param name="scale">Escala el tamaño del ruido. A menor escala, mas se parecerá a la estática de la television</param>
	/// <param name="octaves">Número de ondas o mapas de perlin que se van a superponer para obtener un resultado mas realista</param>
	/// <param name="persistance">Valor ente 0 y 1 que controla la diferencia de amplitud ente octavas. A mayor persistencia, mayor cambio de amplitud entre octavas</param>
	/// <param name="lacunarity">Valor mayor que 1 que controla el incremento de frecuencia entre octavas. A mayor lacunarity, mayor cambio de frecuencia ente octavas</param>
	/// <returns></returns>
	public static float[,] GenerateNoiseMap(int mapX, int mapY, int seed, float scale, int octaves, float persistance, float lacunarity) 
	{
		float[,] noiseMap = new float[mapX,mapY];
		float maxHeight = float.MinValue;
		float minHeight = float.MaxValue;
        System.Random randomGenerator = new System.Random(seed);
		Vector2[] octaveOffsets = new Vector2[octaves];

		//Obtenemos el offset inicial de las coordenadas con la mitad de X e Y. Asi la generación partirá del centro de la textura y no de la esquina 
		Vector2 initOffset = new Vector2(mapX,mapY)/2f;

		//Evitar escalas negativas
		if (scale <= 0)
			scale = 0.0001f;

		//Cambiar offset de cada octava aleatoriamente
		for(int octaveId = 0; octaveId < octaves; octaveId++)
		{
			float offsetX = randomGenerator.Next(-_maxRandomSeed, _maxRandomSeed);
			float offsetY = randomGenerator.Next(-_maxRandomSeed, _maxRandomSeed);
			octaveOffsets[octaveId] = new Vector2(offsetX, offsetY);
		}

		//Calcular valor de cada celda de la matriz del mapa
		for (int y = 0; y < mapY; y++) 
		{
			for (int x = 0; x < mapX; x++) 
			{
				//Multiplicador de la altura de los valores (hace mas ancha o estrecha la onda manteniendo la misma forma proporcional y frequencia). Conforme se avanza de octava se disminuye
				float currentAmplitude = 1;
				//Multiplicador de la longitud de la onda (hace que las curvas sean más estrechas manteniendo su altura). Conforme se avanza de octava se aumenta
				float currentFrequency = 1;
				//Suma de todos los valores obtenidos para cada octava en función de su amplitud y lacunarity
				float totalHeight = 0;

				//Obtenemos un valor de perlin para cada octava, lo modificamos en funcion de la octava que sea y realizamos el sumatorio de todas las octavas
				for (int octaveId = 0; octaveId < octaves; octaveId++)
				{
					//Coordenadas que se van a introducir en la funcion de Perlin. Perlin ya tiene unos valores randomizados que va a devolver en funcion de las coordenadas, pero podemos distorsionar estos valores escalando en X o Y. De esta manera estas coordenadas se modificaran previamente en funcion de la escala y frecuencia.
					float sampleX = (x-initOffset.x) /scale * currentFrequency + octaveOffsets[octaveId].x;
					float sampleY = (y-initOffset.y) /scale * currentFrequency + octaveOffsets[octaveId].y;

					//Obtenemos el valor de perlin y lo modificamos según la octava que sea. Esperamos un valor ente -1 y 1
					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) *2 -1;
					//Sumamos el valor resultante de cada octava
					totalHeight += perlinValue * currentAmplitude;

					//A mayor octava, mas frecuente (piedras mas numerosas)
					currentFrequency *= lacunarity;
					//A mayor octava, menor amplitud (piedras mas pequeñas)
					currentAmplitude *= persistance;
				}

				noiseMap[x, y] = totalHeight;

				//Obtenemos el valor máximo y mínimo de altura en el mapa
				if (totalHeight > maxHeight)
					maxHeight = totalHeight;
				else if (totalHeight < minHeight)
					minHeight = totalHeight;
			}
		}

		//Normalizamos (valor de 0 a 1) todos los valores del mapa usando las alturas máxima y mínima
		for (int y = 0; y < mapY; y++)
		{
			for (int x = 0; x < mapX; x++)
			{
				noiseMap[x, y] = Mathf.InverseLerp(minHeight, maxHeight, noiseMap[x, y]);
			}
		}

		return noiseMap;
	}
}
