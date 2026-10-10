using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    public class OpponentBehaviour : MonoBehaviour {
        GameManager gm;
        CharacterObject opponent;

        public int moveTurn = 0;
        private int depthLevel = 0;
        private BoardState boardState;

        private void Awake() {
            gm = FindAnyObjectByType<GameManager>();
            opponent = gm.data.opponent;
        }
    }
       
}