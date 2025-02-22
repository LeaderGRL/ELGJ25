using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tiles.Scripts
{
    
    public class TilesGroup : MonoBehaviour
    {
        [field: SerializeField]
        public List<Tile> Tiles { get; private set; } = new List<Tile>();
    }
}
