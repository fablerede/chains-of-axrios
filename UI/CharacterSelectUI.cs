using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private Button createButton;
    [SerializeField] private Button enterWorldButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Transform characterListContent;

    [Header("Character Entry Prefab")]
    [SerializeField] private GameObject characterEntryPrefab;

    [Header("Create Panel")]
    [SerializeField] private GameObject createCharacterPanel;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_Dropdown factionDropdown;
    [SerializeField] private TMP_Dropdown classDropdown;
    [SerializeField] private Button confirmCreateButton;
    [SerializeField] private Button cancelCreateButton;

    private CharacterData selectedCharacter;
    private List<CharacterData> loadedCharacters = new List<CharacterData>();

    private Dictionary<string, string[]> factionClasses = new Dictionary<string, string[]>
    {
        { "Royals",  new[] { "Paladin", "Apothecary", "Mercenary" } },
        { "Rangers", new[] { "Pathfinder", "Druid", "Beastmaster" } },
        { "Nomads",  new[] { "Warlord", "Merchant", "Oracle" } }
    };

    private async void Start()
    {
        createButton.onClick.AddListener(OnCreateClicked);
        enterWorldButton.onClick.AddListener(OnEnterWorldClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
        backButton.onClick.AddListener(OnBackClicked);
        confirmCreateButton.onClick.AddListener(OnConfirmCreate);
        cancelCreateButton.onClick.AddListener(OnCancelCreate);
        factionDropdown.onValueChanged.AddListener(OnFactionChanged);

        createCharacterPanel.SetActive(false);
        enterWorldButton.interactable = false;
        deleteButton.interactable = false;

        SetupFactionDropdown();
        await RefreshCharacterList();
    }

    private void SetupFactionDropdown()
    {
        factionDropdown.ClearOptions();
        factionDropdown.AddOptions(new List<string> { "Royals", "Rangers", "Nomads" });
        OnFactionChanged(0);
    }

    private void OnFactionChanged(int index)
    {
        string faction = factionDropdown.options[index].text;
        classDropdown.ClearOptions();
        if (factionClasses.ContainsKey(faction))
            classDropdown.AddOptions(new List<string>(factionClasses[faction]));
    }

    private async Task RefreshCharacterList()
    {
        foreach (Transform child in characterListContent)
            Destroy(child.gameObject);

        loadedCharacters.Clear();
        var characters = await GameManager.Instance.CharacterDataService.LoadAllCharacters();

        foreach (var character in characters)
        {
            loadedCharacters.Add(character);
            if (characterEntryPrefab != null)
            {
                var entry = Instantiate(characterEntryPrefab, characterListContent);
                var label = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = $"{character.characterName}\n<size=14>{character.faction} — {character.className} — Lv.{character.level}</size>";

                var btn = entry.GetComponent<Button>();
                var capturedChar = character;
                if (btn != null)
                    btn.onClick.AddListener(() => OnCharacterEntryClicked(capturedChar));
            }
        }
    }

    private void OnCharacterEntryClicked(CharacterData character)
    {
        selectedCharacter = character;
        enterWorldButton.interactable = true;
        deleteButton.interactable = true;
    }

    private void OnCreateClicked()
    {
        createCharacterPanel.SetActive(true);
        nameInputField.text = "";
    }

    private async void OnConfirmCreate()
    {
        string charName = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(charName)) return;

        string faction = factionDropdown.options[factionDropdown.value].text;
        string className = classDropdown.options[classDropdown.value].text;

        var newCharacter = CharacterData.CreateNew(charName, faction, className);
        await GameManager.Instance.CharacterDataService.SaveCharacter(newCharacter);

        createCharacterPanel.SetActive(false);
        await RefreshCharacterList();
    }

    private void OnCancelCreate()
    {
        createCharacterPanel.SetActive(false);
    }

    private async void OnDeleteClicked()
    {
        if (selectedCharacter == null) return;
        await GameManager.Instance.CharacterDataService.DeleteCharacter(selectedCharacter.characterId);
        selectedCharacter = null;
        enterWorldButton.interactable = false;
        deleteButton.interactable = false;
        await RefreshCharacterList();
    }

    private void OnEnterWorldClicked()
    {
        if (selectedCharacter == null) return;
        GameManager.Instance.SetSelectedCharacter(selectedCharacter);
        GameManager.Instance.LoadGameWorld();
    }

    private void OnBackClicked()
    {
        GameManager.Instance.LoadTitleScreen();
    }
}