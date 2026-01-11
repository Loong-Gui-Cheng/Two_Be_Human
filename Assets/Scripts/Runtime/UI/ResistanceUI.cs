using TMPro;
using UnityEditor.U2D.Animation;
using UnityEngine;

public class ResistanceUI : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField] private SkillData.RESISTANCE_TYPE type;
    [SerializeField, CE_ReadOnly] private CharacterData characterData;

    [Header("User Interface (UI)")]
    [SerializeField] private TextMeshProUGUI effectiveness_TMP;
    [SerializeField] private TextMeshProUGUI multiplier_TMP;

    public CharacterData GetData()
    {
        return characterData;
    }
    public void SetData(CharacterData input)
    {
        characterData = input;

        float resistMultiplier = 0f;
        switch (type)
        {
            case SkillData.RESISTANCE_TYPE.SLASH:
                resistMultiplier = characterData.slashResist;
                break;
            case SkillData.RESISTANCE_TYPE.PIERCE:
                resistMultiplier = characterData.pierceResist;
                break;
            case SkillData.RESISTANCE_TYPE.BLUNT:
                resistMultiplier = characterData.bluntResist;
                break;
            case SkillData.RESISTANCE_TYPE.MAGIC:
                resistMultiplier = characterData.magicResist;
                break;
        }

        if (resistMultiplier > 1f)
        {
            effectiveness_TMP.text = "Fatal";
            effectiveness_TMP.color = Color.red;
        }
        else if (resistMultiplier < 1f)
        {
            effectiveness_TMP.text = "Ineff.";
            effectiveness_TMP.color = Color.gray;
        }
        else
        {
            effectiveness_TMP.text = "Normal";
            effectiveness_TMP.color = Color.white;
        }

        multiplier_TMP.text = string.Format("[{0}x]", resistMultiplier);
    }
}
