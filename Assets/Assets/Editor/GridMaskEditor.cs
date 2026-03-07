#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Crossatro.Grid;

/// <summary>
/// Visual editor for GridMask ScriptableObjects.
/// Replaces the text row fields with a clickable colored grid.
///
/// Cell colors:
///   White  = letter slot (.)
///   Black  = wall (#)
///   Red    = heart (H)
/// </summary>
[CustomEditor(typeof(GridMask))]
public class GridMaskEditor : Editor
{
    // Cell rendering
    private const float CELL_SIZE = 28f;
    private const float CELL_GAP = 2f;
    private const float TOOLBAR_HEIGHT = 24f;

    // Colors
    private static readonly Color COLOR_WHITE = new Color(0.95f, 0.95f, 0.95f);
    private static readonly Color COLOR_BLACK = new Color(0.15f, 0.15f, 0.15f);
    private static readonly Color COLOR_HEART = new Color(0.9f, 0.25f, 0.25f);
    private static readonly Color COLOR_BORDER = new Color(0.3f, 0.3f, 0.3f);

    // Cached grid state
    private char[,] _grid;
    private int _width;
    private int _height;
    private bool _initialized;

    // Size editing
    private int _newWidth;
    private int _newHeight;

    public override void OnInspectorGUI()
    {
        GridMask mask = (GridMask)target;

        // Initialize from mask data
        if (!_initialized)
        {
            LoadFromMask(mask);
            _initialized = true;
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Grid Mask Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Size controls
        DrawSizeControls(mask);
        EditorGUILayout.Space(8);

        // Draw the visual grid
        DrawGrid();
        EditorGUILayout.Space(8);

        // Legend
        DrawLegend();
        EditorGUILayout.Space(5);

        // Stats
        DrawStats();
        EditorGUILayout.Space(5);

        // Action buttons
        DrawActions(mask);
    }

    // ============================================================
    // Size Controls
    // ============================================================

    private void DrawSizeControls(GridMask mask)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Width", GUILayout.Width(40));
        _newWidth = EditorGUILayout.IntField(_newWidth, GUILayout.Width(40));
        _newWidth = Mathf.Clamp(_newWidth, 3, 25);

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Height", GUILayout.Width(42));
        _newHeight = EditorGUILayout.IntField(_newHeight, GUILayout.Width(40));
        _newHeight = Mathf.Clamp(_newHeight, 3, 25);

        GUILayout.Space(10);

        if (GUILayout.Button("Resize", GUILayout.Width(60)))
        {
            ResizeGrid(_newWidth, _newHeight);
            SaveToMask(mask);
        }

        if (GUILayout.Button("Clear", GUILayout.Width(50)))
        {
            FillGrid('.');
            SaveToMask(mask);
        }

        EditorGUILayout.EndHorizontal();
    }

    // ============================================================
    // Grid Rendering
    // ============================================================

    private void DrawGrid()
    {
        if (_grid == null || _width == 0 || _height == 0) return;

        float totalWidth = _width * (CELL_SIZE + CELL_GAP) - CELL_GAP;
        float totalHeight = _height * (CELL_SIZE + CELL_GAP) - CELL_GAP;

        // Reserve layout space
        Rect gridArea = GUILayoutUtility.GetRect(
            totalWidth + 4, totalHeight + 4);

        // Center the grid
        float startX = gridArea.x + (gridArea.width - totalWidth) * 0.5f;
        float startY = gridArea.y + 2;

        // Draw each cell
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                float cellX = startX + x * (CELL_SIZE + CELL_GAP);
                float cellY = startY + y * (CELL_SIZE + CELL_GAP);
                Rect cellRect = new Rect(cellX, cellY, CELL_SIZE, CELL_SIZE);

                char cellValue = _grid[x, y];
                Color cellColor = GetCellColor(cellValue);

                // Draw cell background
                EditorGUI.DrawRect(cellRect, cellColor);

                // Draw border
                DrawBorder(cellRect, COLOR_BORDER, 1);

                // Draw heart symbol
                if (cellValue == 'H')
                {
                    GUIStyle heartStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 16,
                        normal = { textColor = Color.white }
                    };
                    GUI.Label(cellRect, "H", heartStyle);
                }

                // Draw coordinate hint for top-left corner
                if (x == 0 && y == 0)
                {
                    GUIStyle coordStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.UpperLeft,
                        fontSize = 8,
                        normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
                    };
                    GUI.Label(cellRect, "0,0", coordStyle);
                }

                // Handle clicks
                HandleCellClick(cellRect, x, y);
            }
        }
    }

    private void HandleCellClick(Rect cellRect, int x, int y)
    {
        Event e = Event.current;
        if (e.type != EventType.MouseDown) return;
        if (!cellRect.Contains(e.mousePosition)) return;

        GridMask mask = (GridMask)target;

        if (e.button == 0) // Left click: cycle forward
        {
            _grid[x, y] = CycleForward(_grid[x, y]);
        }
        else if (e.button == 1) // Right click: cycle backward
        {
            _grid[x, y] = CycleBackward(_grid[x, y]);
        }

        // Ensure only one heart exists
        if (_grid[x, y] == 'H')
            ClearOtherHearts(x, y);

        SaveToMask(mask);
        e.Use();
        Repaint();
    }

    // ============================================================
    // Legend & Stats
    // ============================================================

    private void DrawLegend()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        DrawLegendItem(COLOR_WHITE, "White (letter)");
        GUILayout.Space(10);
        DrawLegendItem(COLOR_BLACK, "Black (wall)");
        GUILayout.Space(10);
        DrawLegendItem(COLOR_HEART, "Heart H");
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Click=cycle  Right-click=reverse",
            EditorStyles.miniLabel, GUILayout.Width(200));

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLegendItem(Color color, string label)
    {
        Rect colorRect = GUILayoutUtility.GetRect(14, 14, GUILayout.Width(14));
        EditorGUI.DrawRect(colorRect, color);
        DrawBorder(colorRect, COLOR_BORDER, 1);
        EditorGUILayout.LabelField(label, EditorStyles.miniLabel,
            GUILayout.Width(80));
    }

    private void DrawStats()
    {
        if (_grid == null) return;

        int whiteCount = 0, blackCount = 0, heartCount = 0;
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                switch (_grid[x, y])
                {
                    case '.': whiteCount++; break;
                    case '#': blackCount++; break;
                    case 'H': heartCount++; break;
                }
            }

        int total = _width * _height;
        float blackRatio = total > 0 ? blackCount / (float)total : 0;

        EditorGUILayout.HelpBox(
            $"{_width}×{_height} grid  |  " +
            $"{whiteCount} white  |  {blackCount} black ({blackRatio:P0})  |  " +
            $"{heartCount} heart",
            MessageType.Info);
    }

    // ============================================================
    // Actions
    // ============================================================

    private void DrawActions(GridMask mask)
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Fill White", GUILayout.Height(28)))
        {
            FillGrid('.');
            SaveToMask(mask);
        }

        if (GUILayout.Button("Fill Black Border", GUILayout.Height(28)))
        {
            FillBorder('#');
            SaveToMask(mask);
        }

        if (GUILayout.Button("Place Heart Center", GUILayout.Height(28)))
        {
            ClearAllHearts();
            _grid[_width / 2, _height / 2] = 'H';
            SaveToMask(mask);
        }

        EditorGUILayout.EndHorizontal();
    }

    // ============================================================
    // Grid Operations
    // ============================================================

    private void LoadFromMask(GridMask mask)
    {
        string[] rows = mask.GetRows();

        if (rows == null || rows.Length == 0)
        {
            _width = 9;
            _height = 9;
            _grid = new char[_width, _height];
            FillGrid('.');
        }
        else
        {
            _height = rows.Length;
            _width = 0;
            foreach (var r in rows)
                if (r != null && r.Length > _width)
                    _width = r.Length;

            if (_width == 0) _width = 9;

            _grid = new char[_width, _height];

            for (int y = 0; y < _height; y++)
            {
                string row = y < rows.Length ? rows[y] : "";
                for (int x = 0; x < _width; x++)
                {
                    char c = x < row.Length ? row[x] : '#';
                    _grid[x, y] = (c == '.' || c == '#' || c == 'H') ? c : '#';
                }
            }
        }

        _newWidth = _width;
        _newHeight = _height;
    }

    private void SaveToMask(GridMask mask)
    {
        string[] rows = new string[_height];
        for (int y = 0; y < _height; y++)
        {
            char[] chars = new char[_width];
            for (int x = 0; x < _width; x++)
                chars[x] = _grid[x, y];
            rows[y] = new string(chars);
        }

        Undo.RecordObject(mask, "Edit Grid Mask");
        mask.SetRows(rows);
        EditorUtility.SetDirty(mask);
    }

    private void ResizeGrid(int newWidth, int newHeight)
    {
        char[,] newGrid = new char[newWidth, newHeight];
        for (int y = 0; y < newHeight; y++)
            for (int x = 0; x < newWidth; x++)
                newGrid[x, y] = '.';

        // Copy existing data
        int copyW = Mathf.Min(_width, newWidth);
        int copyH = Mathf.Min(_height, newHeight);
        for (int y = 0; y < copyH; y++)
            for (int x = 0; x < copyW; x++)
                newGrid[x, y] = _grid[x, y];

        _grid = newGrid;
        _width = newWidth;
        _height = newHeight;
        _newWidth = newWidth;
        _newHeight = newHeight;
    }

    private void FillGrid(char value)
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                _grid[x, y] = value;
    }

    private void FillBorder(char value)
    {
        for (int x = 0; x < _width; x++)
        {
            _grid[x, 0] = value;
            _grid[x, _height - 1] = value;
        }
        for (int y = 0; y < _height; y++)
        {
            _grid[0, y] = value;
            _grid[_width - 1, y] = value;
        }
    }

    private void ClearOtherHearts(int keepX, int keepY)
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                if (_grid[x, y] == 'H' && (x != keepX || y != keepY))
                    _grid[x, y] = '.';
    }

    private void ClearAllHearts()
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                if (_grid[x, y] == 'H')
                    _grid[x, y] = '.';
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static Color GetCellColor(char c)
    {
        switch (c)
        {
            case '.': return COLOR_WHITE;
            case '#': return COLOR_BLACK;
            case 'H': return COLOR_HEART;
            default: return COLOR_BLACK;
        }
    }

    private static char CycleForward(char c)
    {
        switch (c)
        {
            case '.': return '#';
            case '#': return 'H';
            case 'H': return '.';
            default: return '.';
        }
    }

    private static char CycleBackward(char c)
    {
        switch (c)
        {
            case '.': return 'H';
            case 'H': return '#';
            case '#': return '.';
            default: return '.';
        }
    }

    private static void DrawBorder(Rect rect, Color color, float width)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, width), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - width, rect.width, width), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, width, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - width, rect.y, width, rect.height), color);
    }
}
#endif