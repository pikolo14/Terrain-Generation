using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Habrador_Computational_Geometry;
using static PointsGeneration;


public static class CurveGeneration
{
    public const int DefaultSectionSubdivisions = 20;

    /// <summary>
    /// Devuelve los puntos de una curva de bezier que comunica los puntos indicados, pero con una posicion aleatoria de sus handles o modificadores
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="maxRandomRadius">Radio de la esfera en la que se podrá colocar los puntos modificadores aleatorios. A mayor valor, mayor curva</param>
    /// <returns></returns>
    public static Vector3[] GetRandomBezierCurve(Vector3 p1, Vector3 p2, float maxRandomRadius, int subdivisions)
    {
        Vector3 m1 = GetRandomPointInCircle(p1, maxRandomRadius);
        Vector3 m2 = GetRandomPointInCircle(p2, maxRandomRadius);
        return GetBezierCurveSection(p1,p2,m1,m2,subdivisions);
    }

    public static Vector3[] GetBezierCurveSection(Vector3 p1, Vector3 p2, Vector3 m1, Vector3 m2, int subdivisions = 0)
    {
        if (subdivisions <= 0)
            subdivisions = DefaultSectionSubdivisions;

        Vector3[] points = BezierCubic_EqualSteps(p1,p2,m1,m2,subdivisions);
        return points;
    }

    private static Vector3[] BezierCubic_EqualSteps(Vector3 p1, Vector3 p2, Vector3 m1, Vector3 m2, int steps)
    {
        MyVector3 posA = p1.ToMyVector3();
        MyVector3 posB = p2.ToMyVector3();
        MyVector3 handleA = m1.ToMyVector3();
        MyVector3 handleB = m2.ToMyVector3();

        //Create a curve which is the data structure used in the following calculations
        BezierCubic bezierCubic = new BezierCubic(posA, posB, handleA, handleB);

        //Step 1. Calculate the length of the entire curve
        //This is needed so we know for how long we should walk each step
        float lengthNaive = InterpolationHelpMethods.GetLength_Naive(bezierCubic, steps: 20, tEnd: 1f);

        float lengthExact = InterpolationHelpMethods.GetLength_SimpsonsRule(bezierCubic, tStart: 0f, tEnd: 1f);

        //Debug.Log("Naive length: " + lengthNaive + " Exact length: " + lengthExact);


        //Step 2. Convert the t's to be percentage along the curve
        //Save the accurate t at each position on the curve
        List<float> accurateTs = new List<float>();

        //Important not to confuse this with the step size we use to iterate t
        //This step size is distance in m
        float curveLength = lengthNaive;

        float curveLength_stepSize = curveLength / (float)steps;

        float t_stepSize = 1f / (float)steps;

        float t = 0f;

        float distanceTravelled = 0f;

        for (int i = 0; i < steps + 1; i++)
        {
            //MyVector3 inaccuratePos = bezierCubic.GetPosition(t);

            //Calculate the t needed to get to this distance along the curve
            //Method 1
            //float accurateT = InterpolationHelpMethods.Find_t_FromDistance_Iterative(bezierCubic, distanceTravelled, length);
            //Method 2
            float accurateT = InterpolationHelpMethods.Find_t_FromDistance_Lookup(bezierCubic, distanceTravelled, accumulatedDistances: null);

            accurateTs.Add(accurateT);

            //Debug.Log(accurateT);


            //Test that the derivative calculations are working
            //float dEst = InterpolationHelpMethods.EstimateDerivative(bezierCubic, t);
            //float dAct = bezierCubic.ExactDerivative(t);

            //Debug.Log("Estimated derivative: " + dEst + " Actual derivative: " + dAct);

            //Debug.Log("Distance " + distanceTravelled);


            //Move on to next iteration
            distanceTravelled += curveLength_stepSize;

            t += t_stepSize;
        }


        //Step3. Use the new t's to get information from the curve

        //The interpolated positions
        Vector3[] actualPositions = new Vector3[accurateTs.Count];
        //Save the tangent at each position on the curve
        List<Vector3> tangents = new List<Vector3>();
        //Save the orientation, which includes the tangent
        //List<InterpolationTransform> orientations = new List<InterpolationTransform>();

        for (int i = 0; i < accurateTs.Count; i++)
        {
            float accurateT = accurateTs[i];

            //Position on the curve
            MyVector3 actualPos = bezierCubic.GetPosition(accurateT);
            actualPositions[i] = actualPos.ToVector3();

            //Tangent at each position
            MyVector3 tangentDir = BezierCubic.GetTangent(posA, posB, handleA, handleB, accurateT);
            tangents.Add(tangentDir.ToVector3());

            //Orientation, which includes both position and tangent
            //InterpolationTransform orientation = InterpolationTransform.GetTransform(bezierCubic, accurateT);

            //orientations.Add(orientation);
        }

        return actualPositions;



        //The orientation at each t position
        //MyVector3 startUpRef = MyVector3.Up;

        //List<InterpolationTransform> orientationsFrames = InterpolationTransform.GetTransforms_RotationMinimisingFrame(bezierCubic, accurateTs, startUpRef);


        ////Display stuff

        ////The curve which is split into steps
        ////DisplayInterpolation.DisplayCurve(actualPositions, useRandomColor: true);
        //DisplayInterpolation.DisplayCurve(actualPositions, Color.gray);

        ////The start and end values and the handle points
        //DisplayInterpolation.DisplayHandle(handleA.ToVector3(), posA.ToVector3());
        //DisplayInterpolation.DisplayHandle(handleB.ToVector3(), posB.ToVector3());

        ////The actual Bezier cubic for reference
        //DisplayInterpolation.DisplayCurve(bezierCubic, Color.black);
        ////Handles.DrawBezier(posA.ToVector3(), posB.ToVector3(), handleA.ToVector3(), handleB.ToVector3(), Color.black, EditorGUIUtility.whiteTexture, 1f);

        ////The tangents
        ////DisplayInterpolation.DisplayDirections(actualPositions, tangents, 1f, Color.red);

        ////The orientation
        ////DisplayInterpolation.DisplayOrientations(orientations, 1f);
        //DisplayInterpolation.DisplayOrientations(orientationsFrames, 1f);

        ////Extrude mesh along the curve
        ////InterpolationTransform testTrans = orientationsFrames[1];

        ////MyVector3 pos = testTrans.LocalToWorld(MyVector3.Up * 2f);
        ////MyVector3 pos = testTrans.LocalToWorld(MyVector3.Right * 2f);

        ////Gizmos.DrawSphere(pos.ToVector3(), 0.1f);

        ////DisplayInterpolation.DisplayExtrudedMesh(orientationsFrames, meshProfile);
    }
}
