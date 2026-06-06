using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to a close button to destroy a target root GameObject when clicked.
/// </summary>
public class CloseButtonUI : MonoBehaviour
{
    [SerializeField] private GameObject targetToDestroy;

    private void Start()
    {
        GetComponent<Button>()?.onClick.AddListener(() =>
        {
            if (targetToDestroy != null)
                Destroy(targetToDestroy);
        });
    }
}