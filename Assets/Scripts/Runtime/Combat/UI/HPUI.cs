using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPUI : MonoBehaviour
{
    [Header("User Interface (UI)")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Slider hp_Slider;
    [SerializeField] private TextMeshProUGUI hp_TMP;

    private const float DURATION_HP = 0.3f;
    private readonly WaitForSeconds YIELD_HP = new(DURATION_HP);


    public void SetupUI(Character character)
    {
        hp_Slider.maxValue = character.entity.MaxHP;
        hp_Slider.value = character.entity.HP;
        hp_Slider.minValue = 0;
        hp_TMP.text = string.Format("{0}", hp_Slider.value);
    }
    public void SetupUI(Enemy enemy)
    {
        hp_Slider.maxValue = enemy.entity.MaxHP;
        hp_Slider.value = enemy.entity.HP;
        hp_Slider.minValue = 0;
        hp_TMP.text = string.Format("{0}", hp_Slider.value);
    }
    public void ToggleUI() => canvas.enabled = !canvas.enabled;
    public void AnimateHPSlider(float origin, float end)
    {
        StartCoroutine(DOTweenHPSlider(origin, end));
    }
    private IEnumerator DOTweenHPSlider(float origin, float end)
    {
        float differenceHP = origin - end;
        float speed = differenceHP / DURATION_HP;

        while (differenceHP > 0 && hp_Slider.value > 0)
        {
            differenceHP -= Time.deltaTime * speed;
            hp_Slider.value -= Time.deltaTime * speed;
            hp_TMP.text = string.Format("{0}", Mathf.RoundToInt(hp_Slider.value));
            yield return null;
        }

        hp_Slider.value = end;
        hp_TMP.text = string.Format("{0}", Mathf.RoundToInt(end));
        if (hp_Slider.value <= 0.99f) hp_Slider.value = 0;

        yield break;
    }
    public WaitForSeconds GetHPDuration()
    {
        return YIELD_HP;
    }
}
