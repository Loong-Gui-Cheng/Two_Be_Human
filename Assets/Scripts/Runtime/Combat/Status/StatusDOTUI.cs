using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusDOTUI : MonoBehaviour
{
    [Header("User Interface")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI potency_TMP;

    private float MAX_DURATION = 2f;

    public void SetUpUI(StatusData data, int potency)
    {
        icon.sprite = data.icon;
        potency_TMP.text = string.Format("{0}", potency);

        StartCoroutine(DOTweenUI());
    }
    private IEnumerator DOTweenUI()
    {
        icon.DOFade(0f, MAX_DURATION);
        potency_TMP.DOFade(0f, MAX_DURATION);

        transform.DOLocalMoveY(transform.position.y + 1f, MAX_DURATION);
        yield return new WaitForSeconds(MAX_DURATION);

        Destroy(gameObject);
        yield break;
    }
    private void OnDestroy()
    {
        icon.DOKill();
        potency_TMP.DOKill();
        transform.DOKill();
    }
}
