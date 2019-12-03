using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
[InitializeOnLoad]
public class DeformSewing : MonoBehaviour
{
    // Sewing modes
    private const int MODE_VERTEX = 0;
    private const int MODE_CONTOUR = 1;
    
    // Menu item name
    private const string MENU_NAME = "Deform/Sew";

    private static float contourBreakThreshold = 0.95f;

    public static float sphere_size = 0.05f;
    public static bool active = false;

    static string[] modes = { "Vertex mode", "Contour mode" };
    static GUIContent[] vertex_mode_strings = { new GUIContent(" A Start ", "A"),
                                                new GUIContent(" A End ", "A"),
                                                new GUIContent(" B Start ", "B"),
                                                new GUIContent(" B End ", "B") };

    static GUIContent[] contour_mode_strings = { new GUIContent("A", "A"),
                                                 new GUIContent("B", "B"), };
    static int mode = 0;
    static int old_mode = 0;
    
    static DeformBody from_body, to_body;
    
    static Vector3[] v;

    // Index of the particle that the mouse hovers over
    static int hover_index = -1;
    static int old_hover_index = -1;

    static int from_start = -1;
    static int from_end = -1;
    static int from_contour = -1;
    static bool from_forward = false;
    static List<int> from_points;

    static int to_start = -1;
    static int to_end = -1;
    static int to_contour = -1;
    static bool to_forward = false;
    static List<int> to_points;

    static bool shortest_path = true;

    static int vertex_mode_state = 0;
    static int contour_mode_state = 0;

    static Rect uirect;
    static GUIStyle boxStyle;
    static Vector2 oldMousePos;

    static bool shouldCreateSeam = false;
    static bool shouldReset = false;
    static bool shouldReverseA = false;
    static bool shouldReverseB = false;

    static Color seam_start_color = Color.green;
    static Color seam_end_color = Color.red;
    static Color from_seam_color = Color.Lerp(Color.yellow, Color.red, 0.25f);
    static Color to_seam_color = Color.Lerp(Color.red, Color.cyan, 0.75f);

    static DeformSewing()
    {
        from_points = new List<int>();
        to_points = new List<int>();

        SceneView.duringSceneGui += onSceneGUI =>
        {
            if (!active || Application.isPlaying)
            {
                Menu.SetChecked(MENU_NAME, false);
                return;
            }

            if (shouldReset)
            {
                from_start = -1;
                from_end = -1;
                
                to_start = -1;
                to_end = -1;
                
                vertex_mode_state = 0;
                contour_mode_state = 0;
            }

            if (shouldReverseA)
            {
                int temp = from_start;
                from_start = from_end;
                from_end = temp;
                from_forward = !from_forward;
                from_points.Reverse();

                shouldReverseA = false;
            }

            if (shouldReverseB)
            {
                int temp = to_start;
                to_start = to_end;
                to_end = temp;
                to_forward = !to_forward;
                to_points.Reverse();

                shouldReverseB = false;
            }

            if (shouldCreateSeam)
            {
                CreateSeam();
            }

            switch (mode)
            {
                case MODE_VERTEX: HandleVertexMode(); break;
                case MODE_CONTOUR: HandleContourMode(); break;
            }

            DrawHandles();

            Handles.BeginGUI();

            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
            
            if (Event.current.type == EventType.Repaint)
            {
                uirect = GUILayout.Window(0, uirect, DrawUIWindow, "Sewing editor");

                //10 and 28 are magic values, since Screen size is not exactly right.
                uirect.x = Screen.width / EditorGUIUtility.pixelsPerPoint - uirect.width - 10;
                uirect.y = Screen.height / EditorGUIUtility.pixelsPerPoint - uirect.height - 28;
            }

            GUILayout.Window(0, uirect, DrawUIWindow, "Sewing editor");
            Handles.EndGUI();
        };
    }

    static bool PointOccupied(DeformBody body, int contour, int index, bool from)
    {
        return from ? (body == from_body && contour == from_contour && (index == from_start || index == from_end || from_points.Contains(index))) :
                      (body == to_body && contour == to_contour && (index == to_start || index == to_end || to_points.Contains(index)));        
    }

    static void HandleVertexMode()
    {
        DeformBody body;
        int contour;

        if (vertex_mode_state < 2)
        {
            from_points.Clear();
        }

        if (vertex_mode_state < 4)
        {
            to_points.Clear();
        }

        if (vertex_mode_state == 0)
        {
            FindHoverPoint(FindObjectsOfType<DeformBody>(), out body, out contour, out hover_index);

            if (hover_index != -1 && hover_index != old_hover_index /*&& !PointOccupied(hover_index, false)*/)
            {
                from_body = body;
                from_contour = contour;
                from_start = hover_index;
            }
        }
        else if (vertex_mode_state == 1)
        {
            FindHoverPoint(from_body, from_contour, out hover_index);

            if (hover_index != -1 && hover_index != old_hover_index /*&& !PointOccupied(hover_index, false)*/)
            {
                from_end = hover_index;
            }

            if (from_start != from_end)
            {
                UpdateIntermediaryPoints(from_body, from_contour, from_start, from_end, true);
            }
        }
        else if (vertex_mode_state == 2)
        {
            FindHoverPoint(FindObjectsOfType<DeformBody>(), out body, out contour, out hover_index);

            int[] toContourPoints = GetContourPoints(to_body, to_contour);
            
            if (hover_index != -1 && hover_index != old_hover_index && !PointOccupied(body, contour, hover_index, true))
            {
                to_body = body;
                to_contour = contour;
                to_start = hover_index;
            }

            if (from_points.Count > toContourPoints.Length)
            {
                to_start = -1;
            }
        }
        else if (vertex_mode_state == 3)
        {
            FindHoverPoint(to_body, to_contour, out hover_index);

            if (hover_index != -1 && hover_index != old_hover_index && !PointOccupied(to_body, to_contour, hover_index, true))
            {
                v = to_body.GetVertices();

                int[] toContourPoints = GetContourPoints(to_body, to_contour);

                int forward_index = (to_start + from_points.Count + 1) % toContourPoints.Length;
                int backward_index = (to_start + toContourPoints.Length - (from_points.Count + 1)) % toContourPoints.Length;

                float forward_distance = (v[toContourPoints[hover_index]] - v[toContourPoints[forward_index]]).magnitude;
                float backward_distance = (v[toContourPoints[hover_index]] - v[toContourPoints[backward_index]]).magnitude;

                hover_index = forward_distance < backward_distance ? forward_index : backward_index;

                // Don't allow setting the hover_index to an already occupied point
                // This logic is only valid if the seam lines are on the same object and contour
                if (from_body == to_body && from_contour == to_contour && PointOccupied(to_body, to_contour, hover_index, true))
                {
                    hover_index = -1;
                }
                else
                {
                    to_end = hover_index;
                }
            }

            if (to_start != to_end)
            {
                UpdateIntermediaryPoints(to_body, to_contour, to_start, to_end, false);
            }
        }

        int guiControl = GUIUtility.GetControlID(FocusType.Passive);
        UpdateStates(guiControl);

        old_hover_index = hover_index;

        SceneView.RepaintAll();
    }

    static void HandleContourMode()
    {
        DeformBody body;
        int contour;

        // Always use shortest path in contour mode
        shortest_path = true;

        if (contour_mode_state == 0)
        {
            FindHoverPoint(FindObjectsOfType<DeformBody>(), out body, out contour, out hover_index);

            if (hover_index != -1 && hover_index != old_hover_index /*&& !PointOccupied(hover_index, false)*/)
            {
                from_body = body;
                from_contour = contour;

                FindEndpoints(body, contour);

                if (true/*!PointOccupied(from_start, false) && !PointOccupied(from_end, false)*/)
                {
                    from_points.Clear();
                    UpdateIntermediaryPoints(from_body, from_contour, from_start, from_end, true);                    
                }
                else
                {
                    //from_start = -1;
                    //from_end = -1;
                }

                if (from_start != -1 && from_end != -1)
                {
                    int[] fromContourPoints = GetContourPoints(from_body, from_contour);

                    // If the vector hover_index => from_end is shorter than hover_index => from_start, swap endpoints
                    if ((v[fromContourPoints[hover_index]] - v[fromContourPoints[from_end]]).magnitude <
                        (v[fromContourPoints[hover_index]] - v[fromContourPoints[from_start]]).magnitude)
                    {
                        int temp = from_start;
                        from_start = from_end;
                        from_end = temp;
                        from_points.Reverse();
                    }
                }
            }
        }
        else if (contour_mode_state == 1)
        {
            FindHoverPoint(FindObjectsOfType<DeformBody>(), out body, out contour, out hover_index);

            if (hover_index != -1 && hover_index != old_hover_index && !PointOccupied(body, contour, hover_index, true))
            {
                to_body = body;
                to_contour = contour;

                FindEndpoints(body, contour);

                if (!PointOccupied(body, contour, to_start, true) && !PointOccupied(body, contour, to_end, true))
                {
                    to_points.Clear();
                    UpdateIntermediaryPoints(to_body, to_contour, to_start, to_end, false);                    
                }
                else
                {
                    to_start = -1;
                    to_end = -1;
                }

                if (to_start != -1 && to_end != -1)
                {
                    int[] toContourPoints = GetContourPoints(to_body, to_contour);

                    // If the vector hover_index => from_end is shorter than hover_index => from_start, swap endpoints
                    if ((v[toContourPoints[hover_index]] - v[toContourPoints[to_end]]).magnitude <
                        (v[toContourPoints[hover_index]] - v[toContourPoints[to_start]]).magnitude)
                    {
                        int temp = to_start;
                        to_start = to_end;
                        to_end = temp;
                        to_points.Reverse();
                    }
                }
            }
        }

        int guiControl = GUIUtility.GetControlID(FocusType.Passive);
        UpdateStates(guiControl);

        old_hover_index = hover_index;

        SceneView.RepaintAll();
    }

    static void FindHoverPoint(DeformBody body, int contour, out int hoverIndex)
    {
        if (body == null)
        {
            hoverIndex = -1;
            return;
        }

        List<int> possibleIndices = new List<int>();

        v = body.GetVertices();

        if (v == null || body.contourPointsArray == null)
        {
            hoverIndex = -1;
            return;
        }

        int[] contourPoints = GetContourPoints(body, contour);

        for (int p = 0; p < contourPoints.Length; p++)
        {
            var mousePos = Event.current.mousePosition;

            var pos = body.transform.TransformPoint(v[contourPoints[p]]);
            var guiPos = HandleUtility.WorldToGUIPoint(pos);

            var sqrDistance = Vector2.SqrMagnitude(mousePos - guiPos);

            if (sqrDistance < 100)
            {
                possibleIndices.Add(p);
            }
        }        

        int closest = -1;
        float min_distance_to_camera = Mathf.Infinity;
        Vector3 camPos = Camera.current.transform.position;
        
        Vector3 closestPos = Vector3.zero;

        for (int i = 0; i < possibleIndices.Count; i++)
        {
            int[] points = GetContourPoints(body, contour);
            Vector3 pos = v[points[possibleIndices[i]]];
            float distance = (camPos - pos).magnitude;

            if (distance < min_distance_to_camera)
            {
                min_distance_to_camera = distance;
                closestPos = pos;
                closest = i;
            }
        }

        if (closest == -1)
        {
            hoverIndex = -1;
        }
        else
        {
            hoverIndex = possibleIndices[closest];
            SceneView.RepaintAll();
        }
    }

    static void FindHoverPoint(DeformBody[] bodies, out DeformBody hoverBody, out int hoverContour, out int hoverIndex)
    {
        List<DeformBody> possibleBodies = new List<DeformBody>();
        List<int> possibleContours = new List<int>();
        List<int> possibleIndices = new List<int>();

        foreach (DeformBody body in bodies)
        {
            // TODO: Handle bodies with simulation meshes
            if (body.simulationMesh) continue;

            v = body.GetVertices();

            if (v == null) continue;

            if (body.contourPointsArray == null || body.contourPointsArray.Length == 0)
            {
                if (!body.UpdateMeshContourPoints())
                {
                    continue;
                }
            }

            int numContours = body.contourPointCountsArray.Length;
            
            for (int c = 0; c < numContours; c++) // for each contour check its points
            {
                int index;

                FindHoverPoint(body, c, out index);

                if (index != -1)
                {
                    possibleBodies.Add(body);
                    possibleContours.Add(c);
                    possibleIndices.Add(index);
                }
            }
        }

        int closest = -1;
        float min_distance_to_camera = Mathf.Infinity;
        Vector3 camPos = Camera.current.transform.position;

        Vector3 closestPos = Vector3.zero;

        for (int i = 0; i < possibleIndices.Count; i++)
        {
            v = possibleBodies[i].GetVertices();

            int[] points = GetContourPoints(possibleBodies[i], possibleContours[i]);
            Vector3 pos = v[points[possibleIndices[i]]];
            float distance = (camPos - pos).magnitude;

            if (distance < min_distance_to_camera)
            {
                min_distance_to_camera = distance;
                closestPos = pos;
                closest = i;
            }
        }

        if (closest == -1)
        {
            hoverBody = null;
            hoverContour = -1;
            hoverIndex = -1;
        }
        else
        {
            hoverBody = possibleBodies[closest];
            hoverContour = possibleContours[closest];
            hoverIndex = possibleIndices[closest];
        }
    }

    static T[] Slice<T>(T[] source, int start, int end)
    {
        // Handles negative ends.
        if (end < 0)
        {
            end = source.Length + end;
        }
        int len = end - start;

        // Return new array.
        T[] res = new T[len];
        for (int i = 0; i < len; i++)
        {
            res[i] = source[i + start];
        }
        return res;
    }

    static int[] GetContourPoints(DeformBody body, int contour)
    {
        if (body == null || contour == -1)
        {
            return new int[0];
        }

        int numContours = body.contourPointCountsArray.Length;
        int numProcessed = 0;
        int numContourPoints = 0;

        for (int c = 0; c <= contour; c++) // for each contour check its points
        {
            numContourPoints = body.contourPointCountsArray[c];

            if (c == contour)
            {
                break;
            }

            numProcessed += numContourPoints;           
        }

        return Slice(body.contourPointsArray, numProcessed, numProcessed + numContourPoints);
    }

    static void UpdateIntermediaryPoints(DeformBody body, int contour, int start, int end, bool from)
    {
        if (start == -1 || end == -1) return;

        int[] contourPoints = GetContourPoints(body, contour);

        if (contourPoints.Length == 0) return;
        
        int index = start;
        
        // Count the number of points in both directions to find shortest path to end
        int num_forward = 0;
        int num_backward = 0;

        while (index != end)
        {
            index = (index + 1) % contourPoints.Length;
            num_forward++;
        }

        index = start;

        while (index != end)
        {
            index = (index + contourPoints.Length - 1) % contourPoints.Length;
            num_backward++;
        }

        bool forward = shortest_path ? (num_forward < num_backward) : (num_forward > num_backward);

        if (from)
        {
            from_forward = forward;
        }
        else
        {
            to_forward = forward;
        }

        index = start;

        if (forward)
        {
            index = (index + 1) % contourPoints.Length;
        }
        else
        {
            index = (index + contourPoints.Length - 1) % contourPoints.Length;
        }

        while (index != end)
        {
            if (from)
            {
                from_points.Add(index);
            }
            else 
            {
                to_points.Add(index);
            }

            // Is this code duplication necessary?
            if (forward)
            {
                index = (index + 1) % contourPoints.Length;
            }
            else
            {
                index = (index + contourPoints.Length - 1) % contourPoints.Length;
            }
        }
    }

    static void UpdateStates(int guiControl)
    {
        if (Event.current.button != 0)
        {
            return;
        }

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUIUtility.hotControl = guiControl;

            if (mode == MODE_VERTEX)
            {
                vertex_mode_state++;
                vertex_mode_state %= 5;
            }
            else if (mode == MODE_CONTOUR)
            {
                contour_mode_state++;
                contour_mode_state %= 3;
            }

            Event.current.Use();
            GUIUtility.hotControl = 0;
        }
    }

    static void FindEndpoints(DeformBody body, int contour)
    {
        int[] contourPoints = GetContourPoints(body, contour);

        Vector3[] v = body.GetVertices();

        int index = hover_index;
        int prev = (index + contourPoints.Length - 1) % contourPoints.Length;
        int next = (index + 1) % contourPoints.Length;

        while (next != hover_index) // Look for endpoint in forward direction
        {
            index = (index + 1) % contourPoints.Length;
            prev = (index + contourPoints.Length - 1) % contourPoints.Length;
            next = (index + 1) % contourPoints.Length;

            Vector3 a = v[contourPoints[prev]] - v[contourPoints[index]];
            Vector3 b = v[contourPoints[next]] - v[contourPoints[index]];

            float angle = Vector3.Dot(a, b) / (a.magnitude * b.magnitude);

            if (Mathf.Abs(angle) < contourBreakThreshold)
            {
                switch (contour_mode_state)
                {
                    case 0:
                        from_start = index;
                        break;
                    case 1:
                        to_start = index;
                        break;
                }

                break;
            }
        }

        // Reset indices
        index = hover_index;
        prev = (index + contourPoints.Length - 1) % contourPoints.Length;
        next = (index + 1) % contourPoints.Length;

        while (prev != hover_index) // Look for endpoint in backward direction
        {
            index = (index + contourPoints.Length - 1) % contourPoints.Length;
            prev = (index + contourPoints.Length - 1) % contourPoints.Length;
            next = (index + 1) % contourPoints.Length;

            Vector3 a = (v[contourPoints[prev]] - v[contourPoints[index]]).normalized;
            Vector3 b = (v[contourPoints[next]] - v[contourPoints[index]]).normalized;

            float angle = Vector3.Dot(a, b);

            if (Mathf.Abs(angle) < contourBreakThreshold)
            {
                to_body = body;

                switch (contour_mode_state)
                {
                    case 0:
                        from_end = index;
                        break;
                    case 1:
                        to_end = index;
                        break;
                }

                break;
            }
        }
    }

    static void DrawHandles()
    {
        int guiControl = GUIUtility.GetControlID(FocusType.Passive);

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        foreach (DeformBody body in FindObjectsOfType<DeformBody>())
        {
            // TODO: Handle bodies with simulation meshes
            if (body.simulationMesh) continue;

            v = body.GetVertices();

            if (v == null) continue;

            if (body.contourPointsArray == null || body.contourPointsArray.Length == 0)
            {
                if (!body.UpdateMeshContourPoints())
                {
                    continue;
                }
            }

            for (int i = 0; i < body.contourPointsArray.Length; i++)
            {
                Handles.color = Color.white;
                Vector3 pos = body.transform.TransformPoint(v[body.contourPointsArray[i]]);
                Handles.SphereHandleCap(guiControl, pos, Quaternion.identity, sphere_size, EventType.Repaint);
            }
        }

        // Draw from
        if (from_body && from_contour != -1 && from_start != -1)
        {
            v = from_body.GetVertices();
            
            if (from_body.contourPointsArray == null)
            {
                if (!from_body.UpdateMeshContourPoints())
                {
                    // TODO: Show error message?
                }
            }

            int[] fromContourPoints = GetContourPoints(from_body, from_contour);
            
            // Draw from_start
            Handles.color = Color.green;
            Vector3 from_start_pos = from_body.transform.TransformPoint(v[fromContourPoints[from_start]]);
            Handles.SphereHandleCap(guiControl, from_start_pos, Quaternion.identity, sphere_size, EventType.Repaint);

            if (from_end != -1)
            {
                // Draw FROM intermediary points
                if (mode == MODE_VERTEX) Handles.color = vertex_mode_state < 2 ? Color.Lerp(Color.white, from_seam_color, 0.6f) : from_seam_color;
                if (mode == MODE_CONTOUR) Handles.color = contour_mode_state < 1 ? Color.Lerp(Color.white, from_seam_color, 0.6f) : from_seam_color;

                foreach (int idx in from_points)
                {
                    Vector3 from_point_pos = from_body.transform.TransformPoint(v[fromContourPoints[idx]]);
                    Handles.SphereHandleCap(guiControl, from_point_pos, Quaternion.identity, sphere_size, EventType.Repaint);
                }

                // Draw from_end
                Handles.color = Color.red;
                Vector3 from_end_pos = from_body.transform.TransformPoint(v[fromContourPoints[from_end]]);
                Handles.SphereHandleCap(guiControl, from_end_pos, Quaternion.identity, sphere_size, EventType.Repaint);
            }
        }

        // Draw to
        if (to_body && to_contour != -1 && to_start != -1)
        {
            v = to_body.GetVertices();

            int[] toContourPoints = GetContourPoints(to_body, to_contour);

            // Draw to_start
            Handles.color = Color.green;
            Vector3 to_start_pos = to_body.transform.TransformPoint(v[toContourPoints[to_start]]);
            Handles.SphereHandleCap(guiControl, to_start_pos, Quaternion.identity, sphere_size, EventType.Repaint);

            if (to_end != -1)
            {
                // Draw TO intermediary points
                if (mode == MODE_VERTEX) Handles.color = vertex_mode_state < 4 ? Color.Lerp(Color.white, to_seam_color, 0.6f) : to_seam_color;
                if (mode == MODE_CONTOUR) Handles.color = contour_mode_state < 2 ? Color.Lerp(Color.white, to_seam_color, 0.6f) : to_seam_color;

                foreach (int idx in to_points)
                {
                    Vector3 to_point_pos = to_body.transform.TransformPoint(v[toContourPoints[idx]]);
                    Handles.SphereHandleCap(guiControl, to_point_pos, Quaternion.identity, sphere_size, EventType.Repaint);
                }

                // Draw to_end
                Handles.color = Color.red;
                Vector3 to_end_pos = to_body.transform.TransformPoint(v[toContourPoints[to_end]]);
                Handles.SphereHandleCap(guiControl, to_end_pos, Quaternion.identity, sphere_size, EventType.Repaint);
            }
        }
    }

    static void DrawUIWindow(int windowId)
    {
        GUILayout.BeginHorizontal();
        mode = GUILayout.Toolbar(mode, modes);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Sphere size");
        sphere_size = GUILayout.HorizontalSlider(sphere_size, 0.01f, 0.1f);
        GUILayout.EndHorizontal();

        if (mode == MODE_CONTOUR)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Contour break threshold");
            contourBreakThreshold = GUILayout.HorizontalSlider(contourBreakThreshold, 0.8f, 0.999f);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        if (mode == MODE_VERTEX)
        {
            vertex_mode_state = GUILayout.SelectionGrid(vertex_mode_state, vertex_mode_strings, 4);

            GUI.enabled = vertex_mode_state < 2;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Use shortest path");
            shortest_path = GUILayout.Toggle(shortest_path, "");
            GUILayout.EndHorizontal();
            GUI.enabled = true;


            GUILayout.Space(22);
        }
        else if (mode == MODE_CONTOUR)
        {
            contour_mode_state = GUILayout.SelectionGrid(contour_mode_state, contour_mode_strings, 2);

            GUILayout.BeginHorizontal();

            if (contour_mode_state < 1) GUI.enabled = false;
            shouldReverseA = GUILayout.Button("Reverse A");

            if (contour_mode_state < 2) GUI.enabled = false;
            shouldReverseB = GUILayout.Button("Reverse B");

            GUI.enabled = true;
            GUILayout.EndHorizontal();            
        }

        GUILayout.Space(20);

        // Disable "Create seam" button if endpoints haven't been set
        if ((mode == MODE_VERTEX && vertex_mode_state < 4) || (mode == MODE_CONTOUR && contour_mode_state < 2))
        {
            GUI.enabled = false;
        }

        GUILayout.BeginHorizontal();
        shouldCreateSeam = GUILayout.Button("Create seam");
        GUILayout.EndHorizontal();

        GUI.enabled = true;

        GUILayout.BeginHorizontal();
        shouldReset = GUILayout.Button("Reset");
        GUILayout.EndHorizontal();

        GUILayout.Space(18);
        
        GUILayout.BeginHorizontal();
        bool shouldExit = GUILayout.Button("Close sewing editor");
        GUILayout.EndHorizontal();

        if (shouldExit)
        {
            active = false;
        }

        if (mode != old_mode)
        {
            shouldReset = true;
            old_mode = mode;
        }
    }

    public static void CreateSeam()
    {
        var g = new GameObject("DeformSeam");
        var seam = g.AddComponent<DeformSeam>();

        Undo.RecordObject(seam, "Created seam");

		seam.bodyA = from_body;
        seam.contourA = from_contour;
        seam.bodyASeamBegin = from_start;
        seam.bodyASeamEnd = from_end;
        seam.bodyASewingDirection = from_forward;

        seam.bodyB = to_body;
        seam.contourB = to_contour;
        seam.bodyBSeamBegin = to_start;
        seam.bodyBSeamEnd = to_end;
        seam.bodyBSewingDirection = to_forward;

        seam.shortest_path = shortest_path;

        if (seam.seamIndices == null)
        {
            seam.seamIndices = new List<int>();
        }
        else
        {
            seam.seamIndices.Clear();
        }

        int[] fromContourPoints = GetContourPoints(from_body, from_contour);
        int[] toContourPoints = GetContourPoints(to_body, to_contour);

        seam.seamIndices.Add(fromContourPoints[from_start]);
        seam.seamIndices.Add(toContourPoints[to_start]);

        int i = 0;

        for (; i < Mathf.Min(from_points.Count, to_points.Count); i++)
        {
            seam.seamIndices.Add(fromContourPoints[from_points[i]]);
            seam.seamIndices.Add(toContourPoints[to_points[i]]);
        }

        if (i < from_points.Count)
        {
            seam.seamIndices.Add(fromContourPoints[from_points[i] % fromContourPoints.Length]);
        }
        else if (from_end >= 0 && from_end < fromContourPoints.Length)
        {
            seam.seamIndices.Add(fromContourPoints[from_end]);
        }        

        if (i < to_points.Count)
        {
            seam.seamIndices.Add(toContourPoints[to_points[i] % toContourPoints.Length]);
        }
        else if (to_end >= 0 && to_end < toContourPoints.Length)
        {
            seam.seamIndices.Add(toContourPoints[to_end]);
        }        

        seam.Create();

        Undo.RecordObject(seam, "Created seam");

        shouldReset = true;
        shouldCreateSeam = false;

    }

    public static void FillSeamIndices(DeformSeam seam)
    {
        from_points.Clear();
        to_points.Clear();

        UpdateIntermediaryPoints(seam.bodyA, seam.contourA, seam.bodyASeamBegin, seam.bodyASeamEnd, true);
        UpdateIntermediaryPoints(seam.bodyB, seam.contourB, seam.bodyBSeamBegin, seam.bodyBSeamEnd, false);

        if (seam.seamIndices == null)
        {
            seam.seamIndices = new List<int>();
        }
        else
        {
            seam.seamIndices.Clear();
        }

        int[] fromContourPoints = GetContourPoints(seam.bodyA, seam.contourA);
        int[] toContourPoints = GetContourPoints(seam.bodyB, seam.contourB);

        seam.seamIndices.Add(fromContourPoints[seam.bodyASeamBegin]);
        seam.seamIndices.Add(toContourPoints[seam.bodyBSeamBegin]);

        int i = 0;

        for (; i < Mathf.Min(from_points.Count, to_points.Count); i++)
        {
            seam.seamIndices.Add(fromContourPoints[from_points[i]]);
            seam.seamIndices.Add(toContourPoints[to_points[i]]);
        }

        if (i < from_points.Count)
        {
            seam.seamIndices.Add(fromContourPoints[from_points[i] % fromContourPoints.Length]);
        }
        else
        {
            seam.seamIndices.Add(fromContourPoints[seam.bodyASeamEnd]);
        }

        if (i < to_points.Count)
        {
            seam.seamIndices.Add(toContourPoints[to_points[i] % toContourPoints.Length]);
        }
        else
        {
            seam.seamIndices.Add(toContourPoints[seam.bodyBSeamEnd]);
        }
    }
    
    [MenuItem(MENU_NAME)]
    private static void SetSewingActive()
    {
        active = !active;

        // Set checkmark on menu item
        Menu.SetChecked(MENU_NAME, active);
        // Saving editor state
        EditorPrefs.SetBool(MENU_NAME, active);

        SceneView.RepaintAll();
    }
}
#endif