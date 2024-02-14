using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePathSection
{
    public Vector3 P1, P2;
    public Vector3 M1, M2;

    public List<NodePathSection> Subsections = new List<NodePathSection>();

    public NodePathSection(Vector3 p1, Vector3 p2, Vector3 m1, Vector3 m2, int sectionCount, int recursivity)
    {
        P1 = p1;
        P2 = p2;
        M1 = m1;
        M2 = m2;
        SplitSections(sectionCount, recursivity);
    }

    /// <summary>
    /// Divide la curva definifida por los puntos y modificadores en secciones mas pequeñas de la misma longitud. Estas subsecciones se pueden seguir subdividiendo sucesivamente hasta el número de niveles indicados por la recursividad
    /// </summary>
    /// <param name="sectionsCount">Número de secciones en las que se dividira la curva actual en este nivel</param>
    /// <param name="recursivity">Número de veces consecutivas que se subdividira cada subseccion recursivamente (subsecciones de subsecciones de subsecciones, etc.)</param>
    public void SplitSections(int sectionsCount, int recursivity)
    {
        float timeIncrement = 1f / sectionsCount;
        float time = 0;
        Vector3 sectionP1 = P1;
        Vector3 sectionM1 = M1 - P1;

        //Obtener los puntos intermedios de la curva
        for (int i = 0; i < sectionsCount; i++, time += timeIncrement)
        {
            Vector3 tangent;
            Vector3 sectionP2 = CurveGeneration.GetCurvePointAtTime(P1, P2, M1, M2, time, out tangent);
            Vector3 sectionM2 = sectionP2 - tangent;

            //TODO: Modificar la tangente para hacer mas enrevesada la curva

            //Añadimos la subseccion a la lista de este nivel y llamamos al siguiente nivel de recursividad
            Subsections.Add(new NodePathSection(sectionP1, sectionP2, sectionM1, sectionM2, sectionsCount, --recursivity));

            //Preparamos el punto y modificador iniciales para la siguiente seccion
            sectionP1 = sectionP2;
            sectionM1 = sectionP2 + tangent;
        }
    }

    /// <summary>
    /// Dibuja cada subseccion de esta seccion recursivamente hasta el ultimo nivel de recursividad
    /// </summary>
    /// <param name="pointsPerSection"></param>
    public void DrawSectionRecursively(int pointsPerSection)
    {
        if (Subsections != null && Subsections.Count > 1)
        {
            DrawSectionRecursively(pointsPerSection);
        }
        else
        {
            //TODO: Dibujar tramo de curva del fondo del nivel mas bajo de recursividad (INCEPTION)
        }
    }
}
