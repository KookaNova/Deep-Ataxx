using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Cox.Infection.Management{
    public class PlayerPersistantChoice : ScriptableObject
    {
        public Level selectedLevel;
        public bool enableAI = false;
        public bool invertFirstTurn = false;
        public int playerTurn = 1;
    }

}

