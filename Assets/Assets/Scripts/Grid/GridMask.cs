using Crossatro.Enemy;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Crossatro.Grid
{
    /// <summary>
    /// Define the shape of a crossword grid.
    /// </summary>
    [CreateAssetMenu(fileName = "GridMask", menuName = "Crossatro/Grid Mask")]
    public class GridMask: ScriptableObject
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("Grid Pattern")]
        [Tooltip("Eache string is one row. Use: # = black, . = white, H = heart")]
        [TextArea(3, 20)]
        [SerializeField] private string[] _rows;

        // ============================================================
        // Parsed data
        // ============================================================

        public enum CellType
        {
            Black, // Wall
            White, // Letter
            Heart, // Heart
        }

        private CellType[,] _grid;
        private int _width;
        private int _height;
        private Vector2 _heartPosition;
        private bool _isParsed;

        // ============================================================
        // Properties
        // ============================================================

        public int Width => EnsureParsed()._width;
        public int Height => EnsureParsed()._height;
        public Vector2 HeartPosition => EnsureParsed()._heartPosition;

        // ============================================================
        // API
        // ============================================================

        /// <summary>
        /// Get the cell type at grid coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public CellType GetCell(int x, int y)
        {
            EnsureParsed();
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return CellType.Black;
            return _grid[x, y];
        }

        /// <summary>
        /// Check if a cell is a letter slot.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsWhite(int x, int y)
        {
            return GetCell(x, y) == CellType.White;
        }

        /// <summary>
        /// Get all white cell positions.
        /// </summary>
        /// <returns></returns>
        public List<Vector2> GetAllWhitePosition()
        {
            EnsureParsed();
            var position = new List<Vector2>();

            for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                    if (_grid[x, y] == CellType.White)
                        position.Add(new Vector2(x, -y));

            return position;
        }

        /// <summary>
        /// Get the heart position.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetHeartPosition()
        {
            EnsureParsed();
            return new Vector2(_heartPosition.x, -_heartPosition.y);
        }

        /// <summary>
        /// Find all horizontal and vertical slots.
        /// A slot is a 2+ consecutive white cells.
        /// </summary>
        /// <returns></returns>
        public List<Slot> FindSlots()
        {
            EnsureParsed();
            var slots = new List<Slot>();
            int id = 0;

            // Horizontal slots
            for (int y = 0; y < _height; y++)
            {
                int runStart = -1;

                for (int x = 0; x <= _width; x++)
                {
                    bool isWhite = x < _width && _grid[x, y] == CellType.White;

                    if (isWhite && runStart < 0)
                        runStart = x;
                    else if (!isWhite && runStart >= 0)
                    {
                        int length = x - runStart;
                        if (length >= 2)
                        {
                            slots.Add(new Slot
                            {
                                Id = id++,
                                StartX = runStart,
                                StartY = y,
                                Length = length,
                                IsHorizontal = true,
                            });
                        }
                        runStart = -1;
                    }
                }
            }

            // Vertical slots
            for (int x = 0; x < _width; x++)
            {
                int runStart = -1;

                for (int y = 0; y <= _height; y++)
                {
                    bool isWhite = y < _height && _grid[x, y] == CellType.White;

                    if (isWhite && runStart < 0)
                    {
                        runStart = y;
                    }
                    else if (!isWhite && runStart >= 0)
                    {
                        int length = y - runStart;
                        if (length >= 2)
                        {
                            slots.Add(new Slot
                            {
                                Id = id++,
                                StartX = x,
                                StartY = runStart,
                                Length = length,
                                IsHorizontal = false
                            });
                        }
                        runStart = -1;
                    }
                }
            }

            return slots;
        }

        // ============================================================
        // Parsing
        // ============================================================

        /// <summary>
        /// Parse the text pattern into the internal grid.
        /// </summary>
        /// <returns></returns>
        private GridMask EnsureParsed()
        {
            if (_isParsed) return this;
            Parse();
            return this;
        }

        private void Parse()
        {
            _isParsed = true;
            
            if (_rows == null || _rows.Length == 0)
            {
                _width = 0;
                _height = 0;
                _grid = new CellType[0, 0];
                return;
            }

            _height = _rows.Length;
            _width = 0;

            // Find max width
            foreach (string row in _rows)
            {
                if (row != null && row.Length > _width)
                    _width = row.Length;   
            }

            _grid = new CellType[_width, _height];

            // Parse each cell
            for (int y  = 0; y < _height; y++)
            {
                string row = y < _height ? _rows[y] : "";

                for (int x = 0; x < _width; x++)
                {
                    char c = x < row.Length ? row[x] : '#';

                    switch (c)
                    {
                        case '.':
                            _grid[x, y] = CellType.White;
                            break;
                        case 'H':
                            _grid[x, y] = CellType.Heart;
                            _heartPosition = new Vector2(x, y);
                            break;
                        default:
                            _grid[x, y] = CellType.Black;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Force reparse.
        /// </summary>
        private void OnValidate()
        {
            _isParsed = false;
        }

        // ============================================================
        // Mask generation
        // ============================================================

        /// <summary>
        /// Set the mask pattern.
        /// </summary>
        /// <param name="rows"></param>
        public void SetRows(string[] rows)
        {
            _rows = rows;
            _isParsed = false;
        }

        /// <summary>
        /// Get the raw rows.
        /// </summary>
        /// <returns></returns>
        public string[] GetRows()
        {
            return _rows;
        }
    }

    /// <summary>
    /// A slot in the crossword grid => Consecutive 2+ white cells
    /// </summary>
    public class Slot
    {
        /// <summary>
        /// unique identifier for this slot.
        /// </summary>
        public int Id;

        /// <summary>
        /// Grid x of the first cell.
        /// </summary>
        public int StartX;

        /// <summary>
        /// Grid y of the first Cell
        /// </summary>
        public int StartY;

        /// <summary>
        /// Number of cells in this slot.
        /// </summary>
        public int Length;

        /// <summary>
        /// True = Horizontal
        /// False = Vertical
        /// </summary>
        public bool IsHorizontal;

        /// <summary>
        /// The word placed in this slot.
        /// </summary>
        public WordEntry PlacedWord;

        /// <summary>
        /// 
        /// </summary>
        public string PlacedString;
        
        /// <summary>
        /// Get the grid position of the nth cell in this slot.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public (int x, int y) GetCellPosition(int index)
        {
            if (IsHorizontal)
                return (StartX + index, StartY);
            
            return (StartX, StartY + index);
        }

        /// <summary>
        /// Get the grid position as a vector2
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector2 GetCellWorldPosition(int index)
        {
            var (x, y) = GetCellPosition(index);
            return new Vector2(x, -y);
        }

        public override string ToString()
        {
            string dir = IsHorizontal ? "H" : "V";
            string word = PlacedString ?? "???";
            return $"Slot#{Id} [{dir}] ({StartX}, {StartY}) len={Length} '{word}'";
        }
    }
}