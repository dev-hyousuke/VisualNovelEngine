using System.Collections.Generic;
using _MAIN.Scripts.Core.Characters;
using _MAIN.Scripts.Core.Dialogue.DataContainers;
using _MAIN.Scripts.Core.Dialogue.Managers;
using _MAIN.Scripts.Core.ScriptableObjects;
using UnityEngine;

namespace _MAIN.Scripts.Core.Dialogue
{
    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }
        
        [SerializeField] private CanvasGroup mainCanvas;
        [SerializeField] private DialogueSystemConfigurationSO config;
        public DialogueSystemConfigurationSO Config => config;
        
        public DialogueContainer dialogueContainer = new();
        public DialogueContinuePrompt dialogueContinuePrompt;
        public ConversationManager ConversationManager { get; private set; }
        private TextArchitect _textArchitect;
        private AutoReader _autoReader;
        private CanvasGroupController _cgController;

        public bool IsRunningConversation => ConversationManager.IsRunning;
        private bool _isInitialized;

        public delegate void DialogueSystemEvent();
        public event DialogueSystemEvent OnUserPromptNextEvent;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Initialize();
            }
            else
                DestroyImmediate(gameObject);
        }

        private void Initialize()
        {
            if (_isInitialized)
                return;

            _textArchitect = new TextArchitect(dialogueContainer.dialogueText);
            ConversationManager = new ConversationManager(_textArchitect);
            
            _cgController = new CanvasGroupController(this, mainCanvas);
            dialogueContainer.Initialize();    
            
            if (TryGetComponent(out _autoReader))
                _autoReader.Initialize(ConversationManager);
        }
        
        public void ApplySpeakerDataToDialogueContainer(string speakerName)
        {
            var character = CharacterManager.Instance.GetCharacter(speakerName);
            var characterConfigData = character != null ? character.Config : CharacterManager.Instance.GetCharacterConfig(speakerName);

            ApplySpeakerDataToDialogueContainer(characterConfigData);
        }

        public void ApplySpeakerDataToDialogueContainer(CharacterConfigData configuration)
        {
            dialogueContainer.SetDialogueColor(configuration.dialogueColor);
            dialogueContainer.SetDialogueFont(configuration.dialogueFont);
            var fontSize = config.defaultDialogueFontSize * configuration.dialogueFontScale;
            dialogueContainer.SetDialogueFontSize(fontSize);
            
            dialogueContainer.nameContainer.SetNameColor(configuration.nameColor);
            dialogueContainer.nameContainer.SetNameFont(configuration.nameFont);
            fontSize = config.defaultNameFontSize * configuration.nameFontScale;
            dialogueContainer.nameContainer.SetNameFontSize(fontSize);
        }

        public void ShowSpeakerName(string speakerName = "")
        {
            if (speakerName.ToLower() != "narrator")
                dialogueContainer.nameContainer.Show(speakerName);
            else
                HideSpeakerName();
        }

        public void HideSpeakerName() => dialogueContainer.nameContainer.Hide();

        public Coroutine Say(string speaker, string dialogue)
        {
            var conversation = new List<string>() { $"{speaker} \"{dialogue}\"" };
            return Say(conversation);
        }

        public Coroutine Say(List<string> lines)
        {
            var conversation = new Conversation(lines);
            return ConversationManager.StartConversation(conversation);
        }

        public Coroutine Say(Conversation conversation)
        {
            return ConversationManager.StartConversation(conversation);
        }

        public void OnUserPromptNext()
        {
            OnUserPromptNextEvent?.Invoke();
            
            if(_autoReader != null && _autoReader.isOn)
                _autoReader.Disable();
        }

        public void OnSystemPromptNext()
        {
            OnUserPromptNextEvent?.Invoke();
        }
        
        public Coroutine Show(float speed = 1f, bool immediate = false) => _cgController.Show(speed, immediate);

        public Coroutine Hide(float speed = 1f, bool immediate = false) => _cgController.Hide(speed, immediate);
    }
}
