using UnityEngine;

namespace _MAIN.Scripts.Core.Characters.Types
{
    public class CharacterModel3D : Character
    {
        public CharacterModel3D(string name, CharacterConfigData config) : base(name, config)
        {
            Debug.Log($"Created 3D Character: '{name}'");
        }
    }
}