using TMPro;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    [Header("User Interface (UI)")]
    [SerializeField] private TextMeshProUGUI tooltip_TMP;
    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private RectTransform backgroundRectTransform;
    [SerializeField] private RectTransform canvasRectTransform;

    private Camera uiCam;

    private void Awake()
    {
        uiCam = Camera.main;
        ShowTooltip("Random tooltip text");
    }
    private void Update()
    {
        Vector2 localPoint = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCam, out localPoint);

        transform.localPosition = localPoint;
        Vector2 anchoredPosition = rectTransform.anchoredPosition;
        if (anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
        {
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
        }
        if (anchoredPosition.y - backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
        {
            anchoredPosition.y = canvasRectTransform.rect.height + backgroundRectTransform.rect.height;
        }

        rectTransform.anchoredPosition = anchoredPosition;
    }
    private void ShowTooltip(string tooltipString)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        tooltip_TMP.text = tooltipString;
        float textPaddingSize = 4f;
        Vector2 backgroundSize = new(tooltip_TMP.preferredWidth + textPaddingSize * 2f, 
                tooltip_TMP.preferredHeight + textPaddingSize * 2f);
        backgroundRectTransform.sizeDelta = backgroundSize;
    }
    private void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
