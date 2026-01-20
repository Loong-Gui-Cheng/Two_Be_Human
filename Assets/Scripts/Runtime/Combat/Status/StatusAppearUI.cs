using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusAppearUI : MonoBehaviour
{
    [Header("User Interface")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI count_TMP;
    [SerializeField] private TextMeshProUGUI name_TMP;

    private float MAX_DURATION = 2f;

    public void SetUpUI(StatusData data, int count)
    {
        icon.sprite = data.icon;
        count_TMP.text = string.Format("+{0}", count);
        name_TMP.text = string.Format("{0} Count", data.Name);

        StartCoroutine(DOTweenUI());
    }
    private IEnumerator DOTweenUI()
    {
        icon.DOFade(0f, MAX_DURATION);
        count_TMP.DOFade(0f, MAX_DURATION);
        name_TMP.DOFade(0f, MAX_DURATION);

        transform.DOLocalMoveY(150f, MAX_DURATION);
        yield return new WaitForSeconds(MAX_DURATION);

        Destroy(gameObject);
        yield break;
    }
    private void OnDestroy()
    {
        icon.DOKill();
        count_TMP.DOKill();
        name_TMP.DOKill();
        transform.DOKill();
    }
}
