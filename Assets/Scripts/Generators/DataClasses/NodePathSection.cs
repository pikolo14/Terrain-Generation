using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodePathSection
{
    public Vector3 P1, P2;
    public Vector3 M1, M2;

    public List<NodePathSection> Subsections = new List<NodePathSection>();

    public NodePathSection(Vector3 p1, Vector3 p2, Vector3 m1, Vector3 m2, int sectionCount, float maxAngleSubsectionVariation, float customSubsectionsTangentMultiplier, int recursivity)
    {
        P1 = p1;
        P2 = p2;
        M1 = m1;
        M2 = m2;

        if(recursivity > 0)
            SplitSections(sectionCount, maxAngleSubsectionVariation, customSubsectionsTangentMultiplier, recursivity);
    }

    /// <summary>
    /// Divide la curva definifida por los puntos y modificadores en secciones mas pequeñas de la misma longitud. Estas subsecciones se pueden seguir subdividiendo sucesivamente hasta el número de niveles indicados por la recursividad
    /// </summary>
    /// <param name="sectionsCount">Número de secciones en las que se dividira la curva actual en este nivel</param>
    /// <param name="recursivity">Número de veces consecutivas que se subdividira cada subseccion recursivamente (subsecciones de subsecciones de subsecciones, etc.)</param>
    public void SplitSections(int sectionsCount, float maxAngleSubsectionVariation, float customSubsectionsTangentMultiplier, int recursivity)
    {
        float timeIncrement = 1f/sectionsCount;
        float time = timeIncrement;

        //La distancia de los modificadores sera menor cuanto más secciones haya en un nivel
        float sectionCountWeight = 1f / sectionsCount;
        //Media de distancia de modificadores a extremos de la seccion padre
        float averageParentTangent = (Vector3.Distance(P1,M1) + Vector3.Distance(P2,M2))/2f;
        //Cuanto más bajo sea el nivel de recursividad, menor influencia tendrán sus secciones, acortando su tangente inversamente proporcional
        //TODO: Quitar este hardcodeo
        //TODO: Revisar como afecta este multiplicador en los sitios que se aplica (angulo y longitud de tangente)
        float maxRecursivity = 4f;
        float recursivityLevelMultiplier = (recursivity+1f)/maxRecursivity;

        Vector3 sectionP1 = P1;
        Vector3 sectionM1 = P1 + (M1 - P1)*sectionCountWeight*customSubsectionsTangentMultiplier;

        recursivity--;

        //Obtener los puntos intermedios de la curva
        for (int i = 0; i < sectionsCount; i++, time += timeIncrement)
        {
            //Secciones de primera a penúltima
            if(i != sectionsCount - 1)
            {
                Vector3 tangent;
                //Obtenemos el punto de la curva a cierta distancia y su tangente en dicho punto 
                Vector3 sectionP2 = CurveGeneration.GetCurvePointAtTime(P1, P2, M1, M2, time, out tangent);
                //Hacemos que la tangente tenga una longitud similar a las tangentes de los extremos de la seccion padre
                tangent = tangent.normalized * averageParentTangent;
                //Rotamos la tangente aleatoriamente dentro de un rango para obtener mas curvas en su recorrido (menor rango a menor nivel recursividad) y multiplicamos por un parametro custom para hacer sus tangentes mas o menos notorias
                tangent = NodePath.GetRandomVectorInAngle3D(tangent, maxAngleSubsectionVariation*recursivityLevelMultiplier) * customSubsectionsTangentMultiplier;
                //Reducimos esta longitud de manera inversamente proporcional al número de secciones de este nivel y porporcionalmente al nivel de recursividad
                tangent *= sectionCountWeight*recursivityLevelMultiplier;

                Vector3 sectionM2 = sectionP2 - tangent;
                //Debug.DrawLine(sectionP2, sectionM2, Color.blue, 3);

                //Añadimos la subseccion a la lista de este nivel y llamamos al siguiente nivel de recursividad
                Subsections.Add(new NodePathSection(sectionP1, sectionP2, sectionM1, sectionM2, sectionsCount, maxAngleSubsectionVariation, customSubsectionsTangentMultiplier, recursivity));

                //Preparamos el punto y modificador iniciales para la siguiente seccion
                sectionP1 = sectionP2;
                sectionM1 = sectionP2 + tangent;
            }
            //Última sección
            else
            {
                Vector3 sectionM2 = P2 + (M2 - P2) * customSubsectionsTangentMultiplier * sectionCountWeight*recursivityLevelMultiplier;
                //Coge P2 y M2 de esta sección padre
                Subsections.Add(new NodePathSection(sectionP1, P2, sectionM1, sectionM2, sectionsCount, maxAngleSubsectionVariation, customSubsectionsTangentMultiplier, recursivity));
            }
        }
    }

    /// <summary>
    /// Dibuja cada subseccion de esta seccion recursivamente hasta el ultimo nivel de recursividad
    /// </summary>
    /// <param name="pointsPerSection"></param>
    public void GetSectionPointsRecursively(ref List<Vector3> points, int pointsPerSection)
    {
        //Si no estamos en el nivel mas bajo de recursividad seguimos bajando
        if (Subsections != null && Subsections.Count > 1)
        {
            foreach(var section in Subsections)
            {
                section.GetSectionPointsRecursively(ref points, pointsPerSection);
            }
        }
        //Obtenemos los puntos de cada seccion de curva en el nivel mas bajo de recursividad (INCEPTION)
        else
        {
            var sectionPoints = CurveGeneration.GetBezierCurveSection(P1, P2, M1, M2, pointsPerSection);
            points.AddRange(sectionPoints.SubArray(1, sectionPoints.Length-1));
        }
    }
}
