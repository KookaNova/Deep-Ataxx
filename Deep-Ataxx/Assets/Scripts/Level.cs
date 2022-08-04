using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    [CreateAssetMenu(menuName = "Levels", fileName = "New Level")]
    public class Level : ScriptableObject
    {
        public string levelName;
        public float padding = 1.27f;
        public int columns = 7;
        public int rows = 7;
        public bool redFirst = true;
        public bool isFlagged = false;
        public TileObject tile;
        public PieceComponent piece;
        public Vector2Int[] p1_Positions, p2_Positions, block_Positions;
    }
}

