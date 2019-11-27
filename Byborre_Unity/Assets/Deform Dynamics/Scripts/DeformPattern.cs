using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeformDynamics;

using System;
using System.Runtime.InteropServices;

public class DeformPattern : MonoBehaviour
{
	private string shapeName;

    [SerializeField]
    [Range(10, 64000)]
    public uint numTriangles = 1000;
    private float triangleSize;

	[SerializeField, HideInInspector]
	public float oldNumTriangles;

	[HideInInspector]
	public bool triangulated;

	[SerializeField, HideInInspector]
	private Vector2[] _originalPoints;

	[SerializeField, HideInInspector]
	private Vector2[] _holes;

	[SerializeField, HideInInspector]
	private List<int> _holePointCounts;

	[SerializeField, HideInInspector]
	public Vector2[][] triangulationContourPoints;

	[SerializeField, HideInInspector]
	public uint lastSelectedStartVertex = 0;

	[SerializeField, HideInInspector]
	public uint lastSelectedEndVertex = 1;

	[SerializeField, HideInInspector]
	public bool triangulationContourRetrieved = false;

	[HideInInspector]
	public bool triangulationDirty = false;

	[SerializeField, HideInInspector]
	private Vector3 oldScale;

    private float ComputeShapeArea(List<Vector2> points)
    {
        float area = 0;
        int j = points.Count - 1;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[j];

            area += (b.x + a.x) * (b.y - a.y);

            j = i;
        }
        
        return -(area / 2);
    }

	private void UpdatePatternContourPoints()
	{
		if (_originalPoints == null) return;

		triangulationContourPoints = new Vector2[_holePointCounts == null ? 1 : _holePointCounts.Count + 1][];

		int numPoints;

		List<Vector2> points = new List<Vector2>();
        
		// Use the localScale to produce uniform size triangles
		for (int i = 0; i < _originalPoints.Length; i++)
		{
			points.Add(_originalPoints[i] * transform.localScale);
		}

        float shapeArea = ComputeShapeArea(points);
        float totalHoleArea = 0;

        if (_holes != null && _holePointCounts != null)
        {
            var processedPoints = 0;

            for (var i = 0; i < _holePointCounts.Count; i++)
            {
                var holePoints = new Vector2[_holePointCounts[i]];
                Array.Copy(_holes, processedPoints, holePoints, 0, _holePointCounts[i]);

                List<Vector2> holePointsList = new List<Vector2>(holePoints);

                totalHoleArea += ComputeShapeArea(holePointsList);
            }            
        }

        shapeArea -= totalHoleArea;

        if (shapeArea <= 0) return;

        triangleSize = (shapeArea / numTriangles) * 1.5f;

        var result = DeformPlugin.DXF.SimplifyShape(points.ToArray(), points.Count, triangleSize, out numPoints);

		if (result == -1)
		{
			triangulationContourRetrieved = false;
			return;
		}

		triangulationContourPoints[0] = new Vector2[numPoints];
		var contourPointsHandle = GCHandle.Alloc(triangulationContourPoints[0], GCHandleType.Pinned);
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

				result = DeformPlugin.DXF.SimplifyShape(holePoints, holePoints.Length, triangleSize, out numPoints);

				if (result == -1)
				{
					processedPoints += holePoints.Length;
					continue;
				}

				triangulationContourPoints[i + 1] = new Vector2[numPoints];
				contourPointsHandle = GCHandle.Alloc(triangulationContourPoints[i + 1], GCHandleType.Pinned);
				contourPointsPtr = contourPointsHandle.AddrOfPinnedObject();

                DeformPlugin.DXF.RetrieveSimplifiedShape(contourPointsPtr);

				contourPointsHandle.Free();

				processedPoints += holePoints.Length;
			}
		}

		triangulationContourRetrieved = true;
	}

	public void Triangulate()
	{
        DeformPlugin.InitializePlugin();

		UpdatePatternContourPoints();

		if (!triangulationContourRetrieved)
		{
            Debug.LogWarning(name + " was to small to be triangulated using the current triangle area.");
			return;
		}

		var numInputVertices = 0;
		var numSubShapes = 0;

		var pointList = new List<Vector2>();
		var subShapePointCountsList = new List<int>();

		foreach (var shapePoints in triangulationContourPoints)
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

		int numVertices, numOutputTriangles;

        DeformPlugin.DXF.TriangulateShape(pointList.ToArray(), numInputVertices, numSubShapes, subShapePointCountsList.ToArray(), triangleSize,
						 25, out numVertices, out numOutputTriangles);

		// Add check if numVertices is above 65535

		var meshVertices = new Vector3[numVertices];
		var meshVerticesHandle = GCHandle.Alloc(meshVertices, GCHandleType.Pinned);
		var meshVerticesPtr = meshVerticesHandle.AddrOfPinnedObject();

		var meshTriangles = new int[numOutputTriangles];
		var meshTrianglesHandle = GCHandle.Alloc(meshTriangles, GCHandleType.Pinned);
		var meshTrianglesPtr = meshTrianglesHandle.AddrOfPinnedObject();

        DeformPlugin.DXF.RetrieveTriangulatedShape(meshVerticesPtr, meshTrianglesPtr);

		meshVerticesHandle.Free();
		meshTrianglesHandle.Free();

		// Rescale the result to be in the correct scale
		// The counterpart to this is located in UpdateContourPoints()
		for (int i = 0; i < meshVertices.Length; i++)
		{
			meshVertices[i].x /= transform.localScale.x;
			meshVertices[i].y /= transform.localScale.y;
			meshVertices[i].z /= transform.localScale.z;
		}

		GetComponent<MeshFilter>().sharedMesh = new Mesh();

		GetComponent<MeshFilter>().sharedMesh.vertices = meshVertices;
		GetComponent<MeshFilter>().sharedMesh.triangles = meshTriangles;
		GetComponent<MeshFilter>().sharedMesh.RecalculateBounds();
		GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();

		UpdatePatternContourPoints();
		
		// These values are set so that we can detect wheter or not the pattern needs to be
		// retriangulated
		triangulated = true;
		oldScale = transform.localScale;
		triangulationDirty = false;
		oldNumTriangles = numTriangles;
	}

	public void SetPoints(Vector2[] vertices)
	{
		_originalPoints = vertices;
	}

	public void SetName(string n)
	{
		shapeName = n;
	}

	public string GetName()
	{
		return shapeName;
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

	private void DrawPattern()
	{
		if (Application.isPlaying) return;

		if (_originalPoints == null) return;

		Gizmos.color = Color.magenta;

		for (int i = 0; i < _originalPoints.Length; i++)
		{
			if (i < _originalPoints.Length - 1)
			{
				Vector3 p0 = new Vector3(_originalPoints[i].x, _originalPoints[i].y, 0);
				Vector3 p1 = new Vector3(_originalPoints[i + 1].x, _originalPoints[i + 1].y, 0);
                
				Gizmos.DrawLine(transform.TransformPoint(p0), transform.TransformPoint(p1));

            } else if (i == _originalPoints.Length - 1)
			{
				// Make the last line go from the last point to the first (0)
				Vector3 p0 = new Vector3(_originalPoints[i].x, _originalPoints[i].y, 0);
				Vector3 p1 = new Vector3(_originalPoints[0].x, _originalPoints[0].y, 0);
                
                Gizmos.DrawLine(transform.TransformPoint(p0), transform.TransformPoint(p1));
            }
		}
	}

#if UNITY_EDITOR

	private void OnDrawGizmos()
	{
		if (transform.hasChanged && GetComponent<MeshFilter>().sharedMesh != null)
		{
			if(oldScale != transform.localScale)
			{
				triangulationDirty = true;
			}

			if (oldScale == transform.localScale)
			{
				triangulationDirty = false;
			}

			transform.hasChanged = false;
		}

		// Only draw the pattern if the mesh has not yet been triangulated
		if (GetComponent<MeshFilter>().sharedMesh == null)
		{
			DrawPattern();
		}
	}
#endif
}
