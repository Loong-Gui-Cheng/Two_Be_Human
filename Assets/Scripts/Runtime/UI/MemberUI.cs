using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemberUI : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField, CE_ReadOnly] private CharacterData characterData;
    [SerializeField] private Character character;

    [Header("User Interface (UI)")]
    [SerializeField] private TextMeshProUGUI name_TMP;
    [SerializeField] private TextMeshProUGUI speed_TMP;
    [SerializeField] private TextMeshProUGUI health_TMP;
    [SerializeField] private Slider health_slider;

    [Header("State")]
    [SerializeField, CE_ReadOnly] private bool IsAnimating;

    public void LoadUI()
    {
        if (characterData == null) return;
        name_TMP.text = string.Format("{0}", characterData.Name);
        speed_TMP.text = string.Format("{0}", 0);
        health_TMP.text = string.Format("{0}", characterData.HP);

        health_slider.minValue = 0;
        health_slider.maxValue = characterData.MaxHP;
    }
    public void UpdateUI()
    {
        if (characterData == null) return;
        speed_TMP.text = string.Format("{0}", character.GetSPD());
        health_TMP.text = string.Format("{0}", characterData.HP);
        health_slider.value = characterData.HP;
    }
    public bool CheckAnimationStatus()
    {
        return IsAnimating;
    }
    public IEnumerator HealthRoutine(int beforeHP, int afterHP)
    {
        IsAnimating = true;
        float differenceHP = afterHP - beforeHP;

        while (differenceHP > 0)
        {
            differenceHP -= Time.deltaTime;
            health_slider.value -= Time.deltaTime;
            yield return null;
        }
        health_slider.value = characterData.HP;

        IsAnimating = false;
        yield break;
    }
}
