using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet;
using FishNet.Transporting;

public class TitleScreenUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button quitButton;

    [Header("Join Panel")]
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private Button confirmJoinButton;
    [SerializeField] private Button cancelJoinButton;

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        if (confirmJoinButton != null)
            confirmJoinButton.onClick.AddListener(OnConfirmJoin);
        if (cancelJoinButton != null)
            cancelJoinButton.onClick.AddListener(OnCancelJoin);

        if (joinPanel != null)
            joinPanel.SetActive(false);
    }

    private void OnHostClicked()
    {
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
        GameManager.Instance.LoadCharacterSelect();
    }

    private void OnJoinClicked()
    {
        if (joinPanel != null)
            joinPanel.SetActive(true);
    }

    private void OnConfirmJoin()
    {
        string ip = ipInputField != null ? ipInputField.text : "localhost";
        if (string.IsNullOrEmpty(ip)) ip = "localhost";

        InstanceFinder.ClientManager.StartConnection(ip);
        GameManager.Instance.LoadCharacterSelect();
    }

    private void OnCancelJoin()
    {
        if (joinPanel != null)
            joinPanel.SetActive(false);
    }

    private void OnQuitClicked()
    {
        GameManager.Instance.QuitGame();
    }
}