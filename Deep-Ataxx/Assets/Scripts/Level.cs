using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    [CreateAssetMenu(menuName = "Levels", fileName = "New Level")]
    public class Level : ScriptableObject
    {
        public string levelName;
        public int columns = 7;
        public int rows = 7;
        public bool redFirst = true;
        public bool isFlagged = false;
        public TileObject tile;
        public PieceComponent piece;
        public Vector3Int[] piecePositions; //x and y represent positions, z represents piece state
    }
}

