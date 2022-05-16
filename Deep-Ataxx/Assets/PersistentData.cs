using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Cox.Infection.Management{
    public class PersistentData : MonoBehaviour
    {
        public Level selectedLevel;

        private void Awake() {
            DontDestroyOnLoad(this);
        }

        private void OnDestroy() {
            Debug.Log("Well, it got destroyed anyway.");
        }
    }

}

