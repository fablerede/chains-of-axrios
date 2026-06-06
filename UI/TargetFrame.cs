using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetFrame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI targetName;
    [SerializeField] private TextMeshProUGUI targetLevel;
    [SerializeField] private Image hpBarFill;

    private EnemyEntity currentTarget;
    private RectTransform rectTransform;
    private bool isDragging = false;
    private Vector2 dragOffset;

    private Color conRed = new Color(1f, 0.2f, 0.2f);
    private Color conYellow = new Color(1f, 0.85f, 0f);
    private Color conWhite = Color.white;
    private Color conBlue = new Color(0.3f, 0.5f, 1f);
    private Color conGreen = new Color(0.2f, 0.85f, 0.2f);

    public bool IsLocked { get; set; } = true;
    private string SaveKey => "TargetFrame_Position";

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        LoadPosition();
        ClearTarget();
    }

    private void Update()
    {
        // Update HP bar
        if (currentTarget != null)
        {
            if (currentTarget.IsDead)
            {
                ClearTarget();
                return;
            }
            if (hpBarFill != null)
                hpBarFill.fillAmount = currentTarget.CurrentHP / currentTarget.MaxHP;
        }

        // Dragging
        if (!IsLocked)
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, mousePos, null, out localPoint);
            bool mouseOver = rectTransform.rect.Contains(localPoint);

            if (mouseOver && Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                dragOffset = rectTransform.anchoredPosition - mousePos;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (isDragging)
                {
                    isDragging = false;
                    SavePosition();
                }
            }

            if (isDragging)
                rectTransform.anchoredPosition = mousePos + dragOffset;
        }
        else
        {
            isDragging = false;
        }
    }

    public void SetTarget(EnemyEntity enemy)
    {
        currentTarget = enemy;

        if (enemy == null)
        {
            ClearTarget();
            return;
        }

        if (targetName != null)
            targetName.text = enemy.EnemyName;

        int playerLevel = 1;
        if (GameManager.Instance?.SelectedCharacter != null)
            playerLevel = GameManager.Instance.SelectedCharacter.level;

        int diff = enemy.data.level - playerLevel;
        Color conColor = diff >= 4 ? conRed :
                         diff >= 2 ? conYellow :
                         diff >= -1 ? conWhite :
                         diff >= -3 ? conBlue : conGreen;

        if (targetLevel != null)
        {
            targetLevel.text = enemy.data.level.ToString();
            targetLevel.color = conColor;
        }

        if (targetName != null)
            targetName.color = conColor;

        if (hpBarFill != null)
            hpBarFill.fillAmount = enemy.CurrentHP / enemy.MaxHP;
    }

    public void ClearTarget()
    {
        currentTarget = null;
        if (targetName != null)
        {
            targetName.text = "No Target";
            targetName.color = Color.white;
        }
        if (targetLevel != null)
        {
            targetLevel.text = "--";
            targetLevel.color = Color.white;
        }
        if (hpBarFill != null)
            hpBarFill.fillAmount = 0f;
    }

    private void SavePosition()
    {
        PlayerPrefs.SetFloat(SaveKey + "_X", rectTransform.anchoredPosition.x);
        PlayerPrefs.SetFloat(SaveKey + "_Y", rectTransform.anchoredPosition.y);
        PlayerPrefs.Save();
    }

    private void LoadPosition()
    {
        if (PlayerPrefs.HasKey(SaveKey + "_X"))
        {
            float x = PlayerPrefs.GetFloat(SaveKey + "_X");
            float y = PlayerPrefs.GetFloat(SaveKey + "_Y");
            rectTransform.anchoredPosition = new Vector2(x, y);
        }
    }
}