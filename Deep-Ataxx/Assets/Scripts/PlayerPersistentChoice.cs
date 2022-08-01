using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    public class PlayerPersistentChoice : ScriptableObject
    {
        public enum Mode{
            story,
            twoPlayer,
            arcadeClassic,
            arcadeUntimed
        }
        public Mode selectedMode = Mode.twoPlayer;
        public Level selectedLevel;
        public CharacterObject opponent;
        public bool enableAI = false;
        public bool invertFirstTurn = false;
        public int playerTurn = 1;

        //It might seem wasteful to store these here, but it's convenient for selecting levels from within levels in arcade mode.
        [HideInInspector] public Level[] levelList;
        [HideInInspector] public CharacterObject[]  characterList;

        public void SelectRandomLevel(){
            var randInt = Random.Range(0, levelList.Length - 1);
            selectedLevel = levelList[randInt];
        }
        public void SelectRandomOpponent(){
            var randInt = Random.Range(0, characterList.Length - 1);
            opponent = characterList[randInt];
        }
    }

}

