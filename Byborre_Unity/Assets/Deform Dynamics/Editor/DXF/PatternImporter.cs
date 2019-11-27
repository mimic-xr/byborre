#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

[ScriptedImporter(1, "dxf2", 100)]
public class PatternImporter : ScriptedImporter
{
    //public UnitOfMeasurement unitOfMeasurements;

    //public float fractionOfMeter = 1.0f;

    //public float triangleMultiplier = 1.0f;

    //private float scaleFactor = 1.0f;
    //private DeformPattern[] patterns;

    private int numShapes, numPoints;

    private List<List<Vector2>> dxf_shapes;
    private List<Vector2> dxf_shapes_flattened;
    private List<int> dxf_shape_layers;
    private List<Vector2> dxf_shape_positions;
    private List<int> dxf_point_counts;
    private List<List<Vector2>> dxf_bounding_boxes;
    private List<int> dxf_holes;
    private List<int> dxf_hole_owners;

	private Vector2[] points;
	private int[] pointCounts;
	private int[] layers;
	private int[] holes;
	private int[] holeOwners;
	private Vector2[] positions;

	//private const float PATTERN_SIZE_FACTOR = 0.2f;

	// The number of shapes with the holes incorporated
	private int numActualShapes;
    
	public override void OnImportAsset(AssetImportContext ctx)
	{
        ProcessDXF(ctx.assetPath);

        points = dxf_shapes_flattened.ToArray();
        pointCounts = dxf_point_counts.ToArray();
        layers = dxf_shape_layers.ToArray();
        holes = dxf_holes.ToArray();
        holeOwners = dxf_hole_owners.ToArray();
        positions = dxf_shape_positions.ToArray();

		char[] slashes = { '\\', '/' };

		string filename = ctx.assetPath.Substring(ctx.assetPath.LastIndexOfAny(slashes) + 1);
		filename = filename.Substring(0, filename.Length - ".dxf2".Length);

		var root = new GameObject(filename);

		var processedPoints = 0;

		for (int i = 0; i < numShapes; i++)
		{
			GameObject g = new GameObject();
			g.AddComponent<MeshFilter>();
			g.AddComponent<MeshRenderer>();

			DeformPattern pattern = g.AddComponent<DeformPattern>();

			if (holes[i] == 0) // Not a hole, create shape
			{
				CreateShape(i, pattern, processedPoints);
			}
			else
			{
				AddHoleToShape(i, holeOwners[i], pattern, processedPoints);
			}

			processedPoints += pointCounts[i];

			g.name = filename + " " + pattern.GetName();
			g.transform.position = positions[i];
            
			ctx.AddObjectToAsset("object " + i, g);
		}

		ctx.AddObjectToAsset("root", root);
		ctx.SetMainObject(root);
	}

    void GetShapeBoundingBox(List<Vector2> shape_points, out Vector2 bb_min, out Vector2 bb_max)
    {
        float min_x = Mathf.Infinity;
        float min_y = Mathf.Infinity;

        float max_x = -Mathf.Infinity;
        float max_y = -Mathf.Infinity;

        for (int i = 0; i < shape_points.Count; i++)
        {
            float x = shape_points[i].x;
            float y = shape_points[i].y;

            if (x < min_x)
            {
                min_x = x;
            }

            if (x > max_x)
            {
                max_x = x;
            }

            if (y < min_y)
            {
                min_y = y;
            }

            if (y > max_y)
            {
                max_y = y;
            }
        }

        bb_min = new Vector2(min_x, min_y);
        bb_max = new Vector2(max_x, max_y);
    }

    int FindHoleOwner(int hole_index)
    {
        Vector2 hole_bb_min = dxf_bounding_boxes[hole_index][0];
        Vector2 hole_bb_max = dxf_bounding_boxes[hole_index][1];

        int num_objects = dxf_shapes.Count;

        for (int i = 0; i < num_objects; i++)
        {
            if (i == hole_index) continue;

            Vector2 shape_bb_min = dxf_bounding_boxes[i][0];
            Vector2 shape_bb_max = dxf_bounding_boxes[i][1];

            bool within_bb = hole_bb_min.x > shape_bb_min.x &&
                             hole_bb_min.y > shape_bb_min.y &&
                             hole_bb_max.x < shape_bb_max.x &&
                             hole_bb_max.y < shape_bb_max.y;

            if (within_bb) return i;
        }

        return -1;
    }

    private void GetDXFProperties(string path, out int numPolylines, out List<int> vertexCounts)
    {
        vertexCounts = new List<int>();

        string[] lines = File.ReadAllLines(path);

        int polylineCount = 0;
        int vertexCount = 0;

        foreach (string line in lines)
        {
            if (line == "POLYLINE")
            {
                polylineCount++;
                vertexCount = 0;
            }
            else if (line == "VERTEX")
            {
                vertexCount++;
            }
            else if (line == "SEQEND")
            {
                vertexCounts.Add(vertexCount);
            }
        }

        numPolylines = polylineCount;
    }

    private void ProcessDXF(string path)
    {
        dxf_shapes = new List<List<Vector2>>();
        dxf_shapes_flattened = new List<Vector2>();
        dxf_shape_layers = new List<int>();
        dxf_point_counts = new List<int>();

        dxf_holes = new List<int>();
        dxf_hole_owners = new List<int>();

        dxf_shape_positions = new List<Vector2>();
        dxf_bounding_boxes = new List<List<Vector2>>();

        string[] lines = File.ReadAllLines(path);

        bool reading_polyline = false;
        bool reading_vertex = false;

        int num_polylines_in_file;
        List<int> vertex_counts_in_file;

        GetDXFProperties(path, out num_polylines_in_file, out vertex_counts_in_file);

        int current_polyline = 0;
        int current_valid_polyline = 0;

        List<Vector2> polyline_points = new List<Vector2>();

        int layer = 0;
        float x_value = 0;
        float y_value = 0;

        Vector2 bb_min = new Vector2(1000000000, 1000000000);
        Vector2 bb_max = new Vector2(-1000000000, -1000000000);

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = Regex.Replace(lines[i], @"\s+", "");

            if (lines[i] == "POLYLINE")
            {
                if (vertex_counts_in_file[current_polyline] < 3) // If not a polygon, continue
                { 
                    current_polyline++;
                    continue;
                }

                polyline_points.Clear();

                reading_polyline = true;
            }
            else if (reading_polyline && lines[i] == "8") // Polygon layer
            {
                i++;
                layer = int.Parse(lines[i]);
            }
            else if (reading_polyline && lines[i] == "VERTEX")
            {
                reading_vertex = true;
            }
            else if (reading_vertex && lines[i] == "10") // X value
            {
                i++;
                x_value = float.Parse(lines[i], CultureInfo.InvariantCulture);
            }
            else if (reading_vertex && lines[i] == "20") // Y value
            {
                i++;
                y_value = float.Parse(lines[i], CultureInfo.InvariantCulture);

                Vector2 v = new Vector2(x_value, y_value);

                polyline_points.Add(v);

                if (v.x < bb_min.x && v.y < bb_min.y)
                {
                    bb_min = v;
                }

                if (v.x > bb_max.x && v.y > bb_max.x)
                {
                    bb_max = v;
                }

                reading_vertex = false;
            }
            else if (reading_polyline && lines[i] == "SEQEND")
            {
                Vector2 min = Vector2.zero;
                Vector2 max = Vector2.zero;

                GetShapeBoundingBox(polyline_points, out min, out max);

                List<Vector2> bounding_box = new List<Vector2>();
                bounding_box.Add(min);
                bounding_box.Add(max);

                dxf_bounding_boxes.Add(bounding_box);

                dxf_holes.Add(0);
                dxf_hole_owners.Add(0);

                // Detect hole by checking if first and last point are the same	
                float x = polyline_points[polyline_points.Count - 1].x;
                float y = polyline_points[polyline_points.Count - 1].y;

                if (x == polyline_points[0].x && y == polyline_points[0].y)
                {
                    polyline_points.RemoveAt(polyline_points.Count - 1);

                    dxf_hole_owners[current_valid_polyline] = FindHoleOwner(current_valid_polyline);

                    if (dxf_hole_owners[current_valid_polyline] != -1) // Mark this shape as a hole iff hole owner was found
                    {
                        dxf_holes[current_valid_polyline] = 1;
                    }
                }

                dxf_shapes.Add(new List<Vector2>(polyline_points));

                dxf_shape_layers.Add(layer);
                dxf_point_counts.Add(polyline_points.Count);

                reading_polyline = false;

                current_valid_polyline++;
                current_polyline++;
            }
        }

        numShapes = dxf_shapes.Count;

        for (int i = 0; i < numShapes; i++)
        {
            dxf_shape_positions.Add(Vector2.zero);
        }
        
        for (int i = 0; i < numShapes; i++)
        {
            if (dxf_holes[i] == 0) // If this is not a hole, store shape position
            {
                dxf_shape_positions[i] = dxf_bounding_boxes[i][0] + 
                    (dxf_bounding_boxes[i][1] - dxf_bounding_boxes[i][0]) * 0.5f;
            }

            for (int j = 0; j < dxf_shapes[i].Count; j++)
            {
                Vector2 pos = dxf_holes[i] == 0 ? dxf_shape_positions[i] : dxf_shape_positions[dxf_hole_owners[i]];

                dxf_shapes_flattened.Add(dxf_shapes[i][j] - pos);
            }
        }

        numPoints = dxf_shapes_flattened.Count;
    }

	private void CreateShape(int index, DeformPattern p, int processedPoints)
	{
		if (pointCounts[index] < 3) return;

		p.SetName("[" + layers[index] + "] " + numActualShapes);

		var shapePoints = new Vector2[pointCounts[index]];
		Array.Copy(points, processedPoints, shapePoints, 0, pointCounts[index]);

		p.SetPoints(shapePoints);

		numActualShapes++;
	}

	private void AddHoleToShape(int holeIndex, int holeOwnerIndex, DeformPattern p, int processedPoints)
	{
		if (pointCounts[holeIndex] < 3) return;

		if (p == null)
		{
			CreateShape(holeOwnerIndex, p, processedPoints);
		}

		var holePoints = new Vector2[pointCounts[holeIndex]];
		Array.Copy(points, processedPoints, holePoints, 0, pointCounts[holeIndex]);

		p.AddHole(holePoints);
	}
}
#endif