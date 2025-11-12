using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    private QuestData data;

    [Header("Quest Components (UI)")]
    [SerializeField] private List<Image> m_Graphics;
    [SerializeField] private List<TextMeshProUGUI> m_Texts;

    [SerializeField] private Color m_CompletedColor;
    public TextMeshProUGUI m_QuestTitle;
    public Image m_QuestStatus;
    public Image m_QuestBackground;

    private IQuest IQBehaviour;
    private string m_QuestName;
    private bool IsQuestAdded;

    public QuestData Get()
    {
        return data;
    }
    public void Set(QuestData data)
    {
        this.data = data;
        m_QuestName = data.title.Replace("[X]", data.quota.ToString());
        m_QuestTitle.text = string.Format(m_QuestName + " ({0}/{1})", data.count, data.quota);
    }
    public void UpdateUI() => m_QuestTitle.text = string.Format(m_QuestName + " ({0}/{1})", data.count, data.quota);
    public IQuest GetBehaviour()
    {
        TryGetComponent(out IQBehaviour);
        return IQBehaviour;
    }
    public void DOTweenFadeRoutine(float duration = 0.5f)
    {
        IsQuestAdded = !IsQuestAdded;

        if (IsQuestAdded)
        {
            for (int i = 0; i < m_Graphics.Count; i++)
                m_Graphics[i].DOFade(1f, duration);

            for (int i = 0; i < m_Texts.Count; i++)
                m_Texts[i].DOFade(1f, duration);

            return;
        }

        for (int i = 0; i < m_Graphics.Count; i++)
            m_Graphics[i].DOFade(0f, duration);

        for (int i = 0; i < m_Texts.Count; i++)
            m_Texts[i].DOFade(0f, duration);

        m_QuestStatus.DOFade(0f, duration);
        m_QuestBackground.DOFade(0f, duration);
    }
}

public interface IQuest
{
    public void Set(QuestData data);
    public void QuestActiveEvent();
    public void QuestProgressEvent(int i);
    public void QuestCompletedEvent();
}