using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management {
    [CreateAssetMenu(menuName = "Characters", fileName = "New Character")]
    public class CharacterObject : ScriptableObject {
        public Texture2D art;
        [TextArea(2, 10)]
        public string bio;
        public int thinkDepth = 1;
        public int aggression = 1; //Best to keep this at 1 or higher
        [Range(0, 100)] public int mistakeLikeliness = 0;

        [Header("Stress Response")]
        [Range(0, 4)] public int stressRate = 0;


        public bool thinkDepthChangesWithStress = false;
        public bool invertThinkDepthStressRelationship = false;
        public bool mistakesChangeWithStress = false;
        public bool invertMistakeStressRelationship = false;

    }
}

