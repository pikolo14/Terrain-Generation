using Habrador_Computational_Geometry;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DelaunayTriangulation
{
    public static HashSet<HalfEdge2> GetDelaunayMeshShorterEdges(List<Vector2> points, float maxLengthProp)
    {
        var triangleData = GetDelaunayTriangleData2(points);
        float averageLength = triangleData.edges.GetAverageEdgeLength();
        float maxEdgeLength = averageLength * (1+maxLengthProp);
        RemoveLongerEdges(ref triangleData, maxEdgeLength);
        return triangleData.edges;
    }

    private static HalfEdgeData2 GetDelaunayTriangleData2(List<Vector2> points)
    {
        //Hull
        List<MyVector2> hullPoints_2d = points.Select(point => point.ToMyVector2()).ToList();

        //Normalize to range 0-1
        //We should use all points, including the constraints because the hole may be outside of the random points
        List<MyVector2> allPoints = new List<MyVector2>();

        //allPoints.AddRange(randomPoints_2d);

        allPoints.AddRange(hullPoints_2d);

        Normalizer2 normalizer = new Normalizer2(allPoints);

        List<MyVector2> hullPoints_2d_normalized = normalizer.Normalize(hullPoints_2d);

        HashSet<List<MyVector2>> allHolePoints_2d_normalized = new HashSet<List<MyVector2>>();



        // Generate the triangulation

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        timer.Start();

        //Algorithm 1. Delaunay by triangulate all points with some bad algorithm and then flip edges until we get a delaunay triangulation 
        //HalfEdgeData2 triangleData_normalized = _Delaunay.FlippingEdges(hullPoints_2d_normalized.ToHashSet(), new HalfEdgeData2());


        //Algorithm 2. Delaunay by inserting point-by-point while flipping edges after inserting a single point 
        HalfEdgeData2 triangleData_normalized = _Delaunay.PointByPoint(hullPoints_2d_normalized.ToHashSet(), new HalfEdgeData2());

        ////Algorithm 3. Constrained delaunay
        //HalfEdgeData2 triangleData_normalized = _Delaunay.ConstrainedBySloan(null, hullPoints_2d_normalized, allHolePoints_2d_normalized, shouldRemoveTriangles: true, new HalfEdgeData2());

        timer.Stop();

        Debug.Log($"Generated a delaunay triangulation in {timer.ElapsedMilliseconds / 1000f} seconds");


        //UnNormalize
        HalfEdgeData2 triangleData = normalizer.UnNormalize(triangleData_normalized);

        return triangleData;
    }

    private static float GetAverageEdgeLength(this HashSet<HalfEdge2> edges)
    {
        float sum = 0;

        foreach(var edge in edges)
        {
            sum+=edge.Length();
        }

        return sum / edges.Count();
    }

    private static void RemoveLongerEdges(ref HalfEdgeData2 triangleData, float maxLength)
    {
        List<HalfEdge2> edges = new List<HalfEdge2>();
        foreach(var edge in triangleData.edges)
        {
            if (edge.Length() > maxLength)
                edges.Add(edge);
        }

        triangleData.edges.RemoveWhere(edge => edge.Length() > maxLength);
    }

    private static Mesh GetMeshFromHalfEdgeData2(HalfEdgeData2 triangleData)
    {
        //From half-edge to triangle
        HashSet<Triangle2> triangles_2d = _TransformBetweenDataStructures.HalfEdge2ToTriangle2(triangleData);

        //From triangulation to mesh

        //Make sure the triangles have the correct orientation
        triangles_2d = HelpMethods.OrientTrianglesClockwise(triangles_2d);

        //From 2d to 3d
        HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();

        foreach (Triangle2 t in triangles_2d)
        {
            triangles_3d.Add(new Triangle3(t.p1.ToMyVector3_Yis3D(), t.p2.ToMyVector3_Yis3D(), t.p3.ToMyVector3_Yis3D()));
        }

        return _TransformBetweenDataStructures.Triangle3ToCompressedMesh(triangles_3d);
    }

    public static float Length(this HalfEdge2 edge)
    {
        return MyVector2.Distance(edge.prevEdge.v.position, edge.v.position);
    }
}
