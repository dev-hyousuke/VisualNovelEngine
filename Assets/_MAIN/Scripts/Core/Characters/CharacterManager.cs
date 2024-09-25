using System.Collections.Generic;
using _MAIN.Scripts.Core.Characters.Types;
using _MAIN.Scripts.Core.Dialogue;
using _MAIN.Scripts.Core.ScriptableObjects;
using _MAIN.Scripts.Enums;
using UnityEngine;

namespace _MAIN.Scripts.Core.Characters
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }
        private Dictionary<string, Character> _characters = new();

        private CharacterConfigSO Config => DialogueSystem.Instance.Config.characterConfigurationAsset;
        
        private const string CharacterCastingID = " as ";
        private const string CharacterNameID = "<charname>";
        public string CharacterRootPathFormat => $"Characters/{CharacterNameID}";
        public string CharacterPrefabNameFormat => $"Character - [{CharacterNameID}]";
        public string CharacterPrefabPathFormat => $"{CharacterRootPathFormat}/{CharacterPrefabNameFormat}";


        [SerializeField] private RectTransform characterPanel = null;
        public RectTransform CharacterPanel => characterPanel;
        private void Awake()
        {
            Instance = this;
        }

        public CharacterConfigData GetCharacterConfig(string characterName)
        {
            return Config.GetConfig(characterName);
        }

        public Character GetCharacter(string characterName, bool createIfDoesNotExist = false)
        {
            if (_characters.ContainsKey(characterName.ToLower()))
                return _characters[characterName.ToLower()];
            
            if (createIfDoesNotExist)
                return CreateCharacter(characterName);

            return null;
        }

        public Character CreateCharacter(string characterName)
        {
            if (_characters.ContainsKey(characterName.ToLower()))
            {
                Debug.LogWarning($"A Character called '{characterName}' already exists. Did not create the character.");
                return null;
            }

            var info = GetCharacterInfo(characterName);

            var character = CreateCharacterFromInfo(info);

            _characters.Add(characterName.ToLower(), character);

            return character;
        }

        private CharacterInfo GetCharacterInfo(string characterName)
        {
            CharacterInfo result = new CharacterInfo();

            string[] nameData = characterName.Split(CharacterCastingID, System.StringSplitOptions.RemoveEmptyEntries);
            result.Name = nameData[0];
            result.CastingName = nameData.Length > 1 ? nameData[1] : result.Name;

            result.Config = Config.GetConfig(result.CastingName);

            result.Prefab = GetPrefabForCharacter(result.CastingName);
            
            result.RootCharacterFolder = FormatCharacterPath(CharacterRootPathFormat, result.CastingName);

            return result;
        }
        
        private GameObject GetPrefabForCharacter(string characterName)
        {
            string prefabPath = FormatCharacterPath(CharacterPrefabPathFormat, characterName);
            return Resources.Load<GameObject>(prefabPath);
        }

        private Character CreateCharacterFromInfo(CharacterInfo info)
        {
            return info.Config.characterType switch
            {
                ECharacterType.Text => new CharacterText(info.Name, info.Config),
                ECharacterType.Sprite or ECharacterType.SpriteSheet => new CharacterSprite(info.Name, info.Config, info.Prefab, info.RootCharacterFolder),
                ECharacterType.Live2D => new CharacterLive2D(info.Name, info.Config, info.Prefab, info.RootCharacterFolder),
                ECharacterType.Model3D => new CharacterModel3D(info.Name, info.Config, info.Prefab, info.RootCharacterFolder),
                _ => null
            };
        }
        
        public string FormatCharacterPath(string path, string characterName) => path.Replace(CharacterNameID, characterName);

        private class CharacterInfo
        {
            public string Name = "";
            public string CastingName;
            public string RootCharacterFolder;
            public CharacterConfigData Config;
            public GameObject Prefab;
        }
    }
}