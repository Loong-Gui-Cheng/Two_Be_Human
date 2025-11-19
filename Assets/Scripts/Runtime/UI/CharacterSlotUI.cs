using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSlotUI : MonoBehaviour
{
    [Header("Data Reference")]
    [SerializeField, CE_ReadOnly] private CharacterData characterData;

    [Header("User Interface (UI)")]
    [SerializeField] private GameObject characterUIGroup;
    [SerializeField] private Image portrait_Image;
    [SerializeField] private TextMeshProUGUI name_TMP;
    [SerializeField] private TextMeshProUGUI level_TMP;
    [SerializeField] private TextMeshProUGUI atk_TMP;
    [SerializeField] private TextMeshProUGUI def_TMP;
    [SerializeField] private TextMeshProUGUI empty_TMP;

    public CharacterData GetData()
    {
        return characterData;
    }
    public void SetData(CharacterData data) => characterData = data;

    public void UpdateUI()
    {
        if (characterData != null)
        {
            characterUIGroup.gameObject.SetActive(true);
            empty_TMP.gameObject.SetActive(false);

            portrait_Image.sprite = characterData.portrait;
            name_TMP.text = string.Format("Name: {0}", characterData.Name);
            level_TMP.text = string.Format("Level: {0}", characterData.Level);
            atk_TMP.text = string.Format("ATK: {0}", characterData.ATK);
            def_TMP.text = string.Format("DEF: {0}", characterData.DEF);
        }
        else
        {
            characterUIGroup.gameObject.SetActive(false);
            empty_TMP.gameObject.SetActive(true);
        }
    }
}