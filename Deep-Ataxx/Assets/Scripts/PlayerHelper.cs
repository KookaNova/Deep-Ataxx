using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    public class PlayerHelper : MonoBehaviour
    {
        public PieceComponent selectedPiece;
        public TileObject hoveredTile;
        public bool isSinglePlayer;
        public int singleTurn = 0;

        public void AssignSingleTurn(int turn){
            singleTurn = turn;

        }

    }
}

