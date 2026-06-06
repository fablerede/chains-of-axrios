using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyNameplate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image hpBarFill;
    [SerializeField] private GameObject hpBarContainer;

    private EnemyData data;
    private EnemyEntity entity;
    private Camera mainCamera;

    private Color conRed = new Color(1f, 0.2f, 0.2f);
    private Color conYellow = new Color(1f, 0.85f, 0f);
    private Color conWhite = Color.white;
    private Color conBlue = new Color(0.3f, 0.5f, 1f);
    private Color conGreen = new Color(0.2f, 0.85f, 0.2f);

    public void Initialize(EnemyData enemyData, EnemyEntity enemyEntity,
        float startHP, float startMaxHP)
    {
        // Force canvas rect to valid size before anything renders
        var rect = GetComponent<RectTransform>();
        if (rect != null && (rect.sizeDelta.x == 0 || rect.sizeDelta.y == 0))
            rect.sizeDelta = new Vector2(120, 40);

        data = enemyData;
        entity = enemyEntity;
        mainCamera = Camera.main;

        if (nameText != null) nameText.text = data.enemyName;
        if (levelText != null) levelText.text = data.level.ToString();
        if (hpBarContainer != null) hpBarContainer.SetActive(false);

        UpdateConColor();
        gameObject.SetActive(false);
    }

    private void UpdateConColor()
    {
        int playerLevel = 1;
        if (GameManager.Instance?.SelectedCharacter != null)
            playerLevel = GameManager.Instance.SelectedCharacter.level;

        int diff = data.level - playerLevel;
        Color conColor = diff >= 4 ? conRed :
                         diff >= 2 ? conYellow :
                         diff >= -1 ? conWhite :
                         diff >= -3 ? conBlue : conGreen;

        if (nameText != null) nameText.color = conColor;
        if (levelText != null) levelText.color = conColor;
    }

    public void UpdateHP(float current, float max)
    {
        if (hpBarFill != null)
            hpBarFill.fillAmount = current / max;
        if (hpBarContainer != null)
            hpBarContainer.SetActive(current < max);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null) return;
        transform.LookAt(transform.position + mainCamera.transform.rotation
            * Vector3.forward, mainCamera.transform.rotation * Vector3.up);

        // Ensure canvas has camera
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
            canvas.worldCamera = mainCamera;
    }
}