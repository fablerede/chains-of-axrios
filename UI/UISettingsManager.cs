using UnityEngine;
using UnityEngine.UI;

public class UISettingsManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("UI Elements")]
    [SerializeField] private VitalsUI vitalsUI;
    [SerializeField] private TargetFrame targetFrame;

    [Header("Toggles")]
    [SerializeField] private Toggle vitalsLockToggle;
    [SerializeField] private Toggle targetFrameLockToggle;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    private void Start()
    {
        settingsPanel.SetActive(false);
        vitalsLockToggle.onValueChanged.AddListener(OnVitalsLockChanged);
        closeButton.onClick.AddListener(ClosePanel);
        vitalsLockToggle.isOn = vitalsUI.IsLocked;
        targetFrameLockToggle.onValueChanged.AddListener(OnTargetFrameLockChanged);
        targetFrameLockToggle.isOn = targetFrame.IsLocked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf)
                ClosePanel();
            else
                OpenPanel();
        }
    }

    private void OpenPanel()
    {
        settingsPanel.SetActive(true);
        vitalsLockToggle.isOn = vitalsUI.IsLocked;
    }

    private void ClosePanel()
    {
        settingsPanel.SetActive(false);
    }

    private void OnVitalsLockChanged(bool isLocked)
    {
        if (vitalsUI != null)
            vitalsUI.IsLocked = isLocked;
    }
    private void OnTargetFrameLockChanged(bool isLocked)
    {
        if (targetFrame != null)
            targetFrame.IsLocked = isLocked;
    }
}