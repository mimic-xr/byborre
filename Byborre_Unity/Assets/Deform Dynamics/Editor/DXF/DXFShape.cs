#if UNITY_EDITOR

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using DeformDynamics;

public class DXFShape
{
    private Vector2[] _originalPoints;
    private Vector2[] _holes;
    private List<int> _holePointCounts;
    //private int _layer;
    private string name;

    public Vector2[][] TriangulationContourPoints;
    public uint LastSelectedStartVertex = 0;
    public uint LastSelectedEndVertex = 1;

    public bool triangulationContourRetrieved = false;
    
    public void UpdateContourPoints(float triangleArea)
    {
        if (_originalPoints == null) return;

        if (TriangulationContourPoints == null)
        {
            TriangulationContourPoints = new Vector2[_holePointCounts == null ? 1 : _holePointCounts.Count + 1][];
        }

        int numPoints;

        var result = DeformPlugin.DXF.SimplifyShape(_originalPoints, _originalPoints.Length, triangleArea, out numPoints);

        if (result == -1)
        {
            triangulationContourRetrieved = false;
            return;
        }

        TriangulationContourPoints[0] = new Vector2[numPoints];
        var contourPointsHandle = GCHandle.Alloc(TriangulationContourPoints[0], GCHandleType.Pinned);
        var contourPointsPtr = contourPointsHandle.AddrOfPinnedObject();

        DeformPlugin.DXF.RetrieveSimplifiedShape(contourPointsPtr);

        contourPointsHandle.Free();

        if (_holes != null && _holePointCounts != null)
        {
            var processedPoints = 0;

            for (var i = 0; i < _holePointCounts.Count; i++)
            {
                var holePoints = new Vector2[_holePointCounts[i]];
                Array.Copy(_holes, processedPoints, holePoints, 0, _holePointCounts[i]);

                result = DeformPlugin.DXF.SimplifyShape(holePoints, holePoints.Length, triangleArea, out numPoints);

                if (result == -1)
                {
                    processedPoints += holePoints.Length;
                    continue;
                }

                TriangulationContourPoints[i + 1] = new Vector2[numPoints];
                contourPointsHandle = GCHandle.Alloc(TriangulationContourPoints[i + 1], GCHandleType.Pinned);
                contourPointsPtr = contourPointsHandle.AddrOfPinnedObject();

                DeformPlugin.DXF.RetrieveSimplifiedShape(contourPointsPtr);

                contourPointsHandle.Free();

                processedPoints += holePoints.Length;
            }
        }

        triangulationContourRetrieved = true;
    }

    public void Triangulate(float triangleArea, Mesh mesh, float scaleFactor)
    {

		if (!triangulationContourRetrieved)
        {
            //Debug.LogWarning(name + " was to small to be triangulated using the current triangle area.");
            return;
        }

        var numInputVertices = 0;
        var numSubShapes = 0;
        
        var pointList = new List<Vector2>();
        var subShapePointCountsList = new List<int>();

        foreach (var shapePoints in TriangulationContourPoints)
        {
            if (shapePoints == null || shapePoints.Length < 3) continue;

            var processedPoints = 0;
            var duplicated = false;

            if (numSubShapes > 0)
            {
                // Check if this shape is a duplicate
                for (var i = 0; i < numSubShapes; i++)
                {
                    if (subShapePointCountsList[i] == shapePoints.Length &&
                        pointList[processedPoints] == shapePoints[0] &&
                        pointList[processedPoints + subShapePointCountsList[i] - 1] == shapePoints[shapePoints.Length - 1])
                    {
                        duplicated = true;
                        break;
                    }

                    processedPoints += subShapePointCountsList[i];
                }
            }

            if (duplicated) continue;
            
            numInputVertices += shapePoints.Length;
            numSubShapes++;
            pointList.AddRange(shapePoints);
            subShapePointCountsList.Add(shapePoints.Length);
        }

        int numVertices, numTriangles;

        DeformPlugin.DXF.TriangulateShape(pointList.ToArray(), numInputVertices, numSubShapes, subShapePointCountsList.ToArray(), triangleArea, 
                         25, out numVertices, out numTriangles);

        var meshVertices = new Vector3[numVertices];
        var meshVerticesHandle = GCHandle.Alloc(meshVertices, GCHandleType.Pinned);
        var meshVerticesPtr = meshVerticesHandle.AddrOfPinnedObject();

        var meshTriangles = new int[numTriangles];
        var meshTrianglesHandle = GCHandle.Alloc(meshTriangles, GCHandleType.Pinned);
        var meshTrianglesPtr = meshTrianglesHandle.AddrOfPinnedObject();

        DeformPlugin.DXF.RetrieveTriangulatedShape(meshVerticesPtr, meshTrianglesPtr);

        meshVerticesHandle.Free();
        meshTrianglesHandle.Free();

        if(scaleFactor != 1)
        {
            for (int i = 0; i < meshVertices.Length; i++)
            {
                meshVertices[i] *= scaleFactor;
            }
        }        

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    public void SetPoints(Vector2[] vertices)
    {
        _originalPoints = vertices;
    }

    public void SetLayer(int layer)
    {
        //_layer = layer;
    }

    public void AddHole(Vector2[] holePoints)
    {
        if (_holes == null)
        {
            _holes = new Vector2[holePoints.Length];
            _holePointCounts = new List<int>();
        }
        else
        {
            Array.Resize(ref _holes, _holes.Length + holePoints.Length);
        }

        Array.Copy(holePoints, 0, _holes, _holes.Length - holePoints.Length, holePoints.Length);
        _holePointCounts.Add(holePoints.Length);
    }

    public void SetName(string n)
    {
        name = n;
    }

    public String GetName()
    {
        return name;
    }
}
#endif