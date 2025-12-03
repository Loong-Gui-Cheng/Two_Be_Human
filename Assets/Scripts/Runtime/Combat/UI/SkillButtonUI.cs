using UnityEngine;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour
{
    [Header("Skill")]
    public SkillData data;
    public Image frame;
    public Image icon;
    public Button button;

    public void SetupSkill(SkillData data)
    {
        this.data = data;
        icon.sprite = data.icon;
    }
}