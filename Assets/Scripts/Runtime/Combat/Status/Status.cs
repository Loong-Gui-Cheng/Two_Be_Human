using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Status : MonoBehaviour
{
    [Header("Data Reference")]
    public StatusData data;
    [SerializeField, CE_ReadOnly] private GameObject statusEffectGO;
    public IStatusEffect effect;

    [Header("Runtime (Stats)")]
    public int potency;
    public int count;

    [Header("User Interface (UI)")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI potency_TMP;
    [SerializeField] private TextMeshProUGUI count_TMP;

    public void SetData(StatusData input, int potency, int count)
    {
        // Can only be initialised once.
        if (data != null) return;

        data = input;
        this.potency = potency;
        this.count = count;

        icon.sprite = data.icon;
        statusEffectGO = Instantiate(data.effectPrefab, transform);

        if (statusEffectGO.TryGetComponent(out IStatusEffect Ieffect))
            effect = Ieffect;

        UpdateUI();
    }
    public StatusData GetData()
    {
        return data;
    }
    public void UpdateUI()
    {
        potency_TMP.text = string.Format("{0}", potency);
        count_TMP.text = string.Format("{0}", count);
    }
}

public interface IStatusEffect
{
    void OnEnter(Status statusParent, CombatEntity entity, List<Character> onFieldCharacters, List<Enemy> onFieldEnemies);
    void OnHit(CombatEntity entity);
    void OnExit(Status statusParent, CombatEntity entity);
}
