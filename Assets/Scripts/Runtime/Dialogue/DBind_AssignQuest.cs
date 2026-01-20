using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBind_AssignQuest : MonoBehaviour, IDialogueBind
{
    [Header("IDialogueBind Settings")]
    [SerializeField] private bool IsSingle;

    [ConditionalHide("IsSingle", true, false)]
    [SerializeField] private QuestData assign;

    [Header("Other Settings")]
    [SerializeField] private List<QuestData> assignList;

    public void IDialogueExecute()
    {
        if (IsSingle && assign == null) return;

        if (IsSingle)
        {
            QuestSystem.onAssignQuest?.Invoke(assign);
            return;
        }

        if (assignList.Count <= 0) return;
        for (int i = 0; i < assignList.Count; i++)
        {
            if (assignList[i] == null) continue;
            QuestData quest = assignList[i];
            QuestSystem.onAssignQuest?.Invoke(quest);
        }
    }
}