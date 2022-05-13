using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    [CreateAssetMenu(menuName = "Game Rules", fileName = "New Rules")]
    public class Rules : ScriptableObject
    {
        public float padding = 1.25f;
        public int columns = 7;
        public int rows = 7;
        public TileObject tile;
        public PieceComponent piece;
        public Vector2Int[] redStartTiles, blueStartTiles;
    }
}

