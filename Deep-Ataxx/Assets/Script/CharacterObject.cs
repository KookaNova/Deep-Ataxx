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
    }
}

