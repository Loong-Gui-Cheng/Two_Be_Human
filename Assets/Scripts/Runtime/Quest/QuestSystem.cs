using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class QuestSystem : MonoBehaviour
{
    [Header("Dataset")]
    [SerializeField] private List<QuestData> m_AvailableQuests;

    [Header("Quest (UI)")]
    [SerializeField] private RectTransform m_QuestParent;
    [SerializeField] private RectTransform m_ListParent;
    [SerializeField] private GameObject m_QuestPrefab;
    [SerializeField] private Color m_CompletedColor;
    [SerializeField] private List<QuestUI> m_Quests;

    [Header("Animation (UI)")]
    [SerializeField] private List<Image> m_Backgrounds;
    [SerializeField] private TextMeshProUGUI m_QuestSeriesTMP;
    [SerializeField] private TextMeshProUGUI m_MQuestSeriesTMP;
    [SerializeField] private TextMeshProUGUI m_QuestStarterTMP;
    [SerializeField] private RectTransform m_Line;
    [SerializeField] private Image m_MLine;

    public static System.Action<QuestData> onAssignQuest;
    public static System.Action<QuestData, int> onProgressQuest;
    public static System.Action<QuestData> onUpdateQuest;
    public static System.Action<QuestData> onCompleteQuest;

    private readonly Dictionary<QuestData.ID, QuestUI> m_QuestList = new();

    private bool IsQuestLineStarted;

    [Header("DEBUG ONLY (UI)")]
    public QuestData questDebug;


    private void OnEnable()
    {
        onAssignQuest += AssignQuest;
        onProgressQuest += ProgressQuest;
        onUpdateQuest += UpdateQuestUI;
        onCompleteQuest += CompleteQuest;
    }
    private void OnDisable()
    {
        onAssignQuest -= AssignQuest;
        onProgressQuest -= ProgressQuest;
        onUpdateQuest -= UpdateQuestUI;
        onCompleteQuest -= CompleteQuest;
    }

    public QuestData RandomQuest()
    {
        if (m_AvailableQuests.Count > 0)
        {
            int randQuest = Random.Range(0, m_AvailableQuests.Count);
            return m_AvailableQuests[randQuest];
        }
        return null;
    }
    private void AssignQuest(QuestData data)
    {
        if (data == null) return;
        if (m_Quests.Count >= 3) return;
        if (m_QuestList.ContainsKey(data.id)) return;
        //if (data.state == QuestData.STATE.ACTIVE) return;
        //if (data.state == QuestData.STATE.COMPLETE) return;

        GameObject go = Instantiate(m_QuestPrefab);
        if (!go.TryGetComponent(out QuestUI quest)) return;

        switch (data.type)
        {
            default:
                go.AddComponent<IQ_Debug>();
                break;
        }
        quest.Set(data);
        quest.GetBehaviour().Set(data);

        m_Quests.Add(quest);
        m_QuestList.Add(data.id, quest);
        quest.transform.SetParent(m_ListParent, false);

        // Update UI
        quest.GetBehaviour().QuestActiveEvent();

        if (!IsQuestLineStarted)
        {
            m_QuestSeriesTMP.text = quest.Get().GetSeries();
            m_MQuestSeriesTMP.text = m_QuestSeriesTMP.text;
            m_QuestStarterTMP.text = quest.m_QuestTitle.text;

            //AudioController.Instance.PlayUI(AudioController.SOUND_ID.QUEST_LINE_ACQUIRED);
            StartCoroutine(DOTweenQuestLineRoutine(quest));
        } 
        else quest.DOTweenFadeRoutine();
    }
    private void ProgressQuest(QuestData data, int i)
    {
        if (data == null) return;
        if (!m_QuestList.ContainsKey(data.id)) return;
        //if (data.state == QuestData.STATE.PENDING) return;
        //if (data.state == QuestData.STATE.COMPLETE) return;

        m_QuestList.TryGetValue(data.id, out QuestUI quest);
        if (quest == null) return;

        quest.GetBehaviour().QuestProgressEvent(i);
    }
    private void CompleteQuest(QuestData data)
    {
        if (data == null) return;
        if (!m_QuestList.ContainsKey(data.id)) return;
        //if (data.state == QuestData.STATE.PENDING) return;

        QuestUI quest = null;
        for (int i = m_Quests.Count - 1; i >= 0; i--)
        {
            if (m_Quests[i].Get().id == data.id)
            {
                quest = m_Quests[i];
                m_Quests.RemoveAt(i);
                m_QuestList.Remove(data.id);
                break;
            }
        }
        if (quest == null) return;

        // Update UI
        StartCoroutine(DOTweenRemoveQuestRoutine(quest));
    }



    public void AnimateQuestUI(QuestUI quest)
    {
        StartCoroutine(DOTweenQuestLineRoutine(quest));
    }
    private void UpdateQuestUI(QuestData data)
    {
        if (data == null) return;
        if (!m_QuestList.ContainsKey(data.id)) return;

        QuestUI quest = null;
        m_QuestList.TryGetValue(data.id, out quest);
        quest.UpdateUI();
    }



    private IEnumerator DOTweenRemoveQuestRoutine(QuestUI quest)
    {
        quest.m_QuestBackground.DOFade(0.8f, 0.3f);
        quest.m_QuestStatus.gameObject.SetActive(true);
        quest.m_QuestStatus.DOFade(1f, 0.3f);
        yield return new WaitForSeconds(0.3f);

        quest.m_QuestTitle.DOColor(m_CompletedColor, 0.5f);
        //AudioController.Instance.PlayUI(AudioController.SOUND_ID.DING);
        yield return new WaitForSeconds(0.8f);

        quest.DOTweenFadeRoutine();
        yield return new WaitForSeconds(0.5f);

        quest.transform.SetParent(null);
        Destroy(quest.gameObject);

        if (m_QuestList.Count <= 0)
        {
            m_QuestSeriesTMP.text = "Quest Completed!";
            m_QuestStarterTMP.text = "You have completed all your tasks!";
            //AudioController.Instance.PlayUI(AudioController.SOUND_ID.QUEST_LINE_COMPLETE);
            StartCoroutine(DOTweenQuestLineRoutine(null));
        }
        yield break;
    }
    private IEnumerator DOTweenQuestLineRoutine(QuestUI quest)
    {
        IsQuestLineStarted = !IsQuestLineStarted;

        if (!IsQuestLineStarted)
        {
            m_MQuestSeriesTMP.DOFade(0f, 0.3f);
            m_MLine.DOFade(0f, 0.3f);
        }

        for (int i = 0; i < m_Backgrounds.Count; i++)
            m_Backgrounds[i].DOFade(1f, 0.5f);

        m_QuestSeriesTMP.rectTransform.DOLocalMoveY(2f, 0.5f);
        m_Line.DOLocalMoveY(0f, 0.5f);
        m_QuestStarterTMP.rectTransform.DOLocalMoveY(-2f, 0.5f);
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < m_Backgrounds.Count; i++)
            m_Backgrounds[i].DOFade(0.01f, 0.5f);

        m_QuestSeriesTMP.rectTransform.DOLocalMoveY(60f, 0.5f);
        m_Line.DOLocalMoveY(55f, 0.5f);
        m_QuestStarterTMP.rectTransform.DOLocalMoveY(-60f, 0.5f);
        yield return new WaitForSeconds(1f);

        if (IsQuestLineStarted)
        {
            m_MQuestSeriesTMP.DOFade(1f, 0.3f);
            m_MLine.DOFade(1f, 0.3f);
            yield return new WaitForSeconds(0.3f);

            if (quest != null)
                quest.DOTweenFadeRoutine(0.5f);
        }
        yield break;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(QuestSystem))]
public class QuestSystemEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        QuestSystem questSystem = (QuestSystem)target;

        // Executes whenever values in inspector changes.
        if (DrawDefaultInspector())
        {
        }

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Runtime Controls", EditorStyles.boldLabel, GUILayout.MaxWidth(200f));
        if (GUILayout.Button("Assign Quest", GUILayout.Width(200f), GUILayout.Height(30f)))
            QuestSystem.onAssignQuest?.Invoke(questSystem.questDebug);

        if (GUILayout.Button("Progress Quest", GUILayout.Width(200f), GUILayout.Height(30f)))
            QuestSystem.onProgressQuest?.Invoke(questSystem.questDebug, 1);

        if (GUILayout.Button("Remove Quest", GUILayout.Width(200f), GUILayout.Height(30f)))
            QuestSystem.onCompleteQuest?.Invoke(questSystem.questDebug);

        if (GUILayout.Button("Animate Quest Line", GUILayout.Width(200f), GUILayout.Height(30f)))
            questSystem.AnimateQuestUI(null);

        GUILayout.EndVertical();
    }
}
#endif