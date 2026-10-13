using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cox.Infection.Management{
    [CreateAssetMenu(menuName = "Characters", fileName = "New Character")]
    public class CharacterObject : ScriptableObject
    {
        public Texture2D art;
        [TextArea(2,10)]
        public string bio;
        public int thinkDepth = 1;
        public int aggression = 1; //Best to keep this at 1 or higher
        public float stress = 0; //This may be best hidden.
    }
}

