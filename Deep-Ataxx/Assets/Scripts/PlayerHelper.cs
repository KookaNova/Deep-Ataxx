using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cox.Infection.Management{
    public class PlayerHelper : MonoBehaviour
    {
        public PieceComponent selectedPiece;
        public TileObject hoveredTile;
        public bool isSinglePlayer;
        [FormerlySerializedAs("singleTurn")]
        public int controlledPlayerId = 0;

        public void AssignPlayerId(int playerId){
            controlledPlayerId = playerId;
        }

    }
}

