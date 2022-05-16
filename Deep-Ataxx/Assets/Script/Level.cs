using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    [CreateAssetMenu(menuName = "Game Level", fileName = "New Level")]
    public class Level : ScriptableObject
    {
        public float padding = 1.27f;
        public int columns = 7;
        public int rows = 7;
        public TileObject tile;
        public PieceComponent piece;
        public Vector2Int[] redPositions, greenPositions, blockPositions;
    }
}

