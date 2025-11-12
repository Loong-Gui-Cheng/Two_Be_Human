using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EFT_QuestSystem : MonoBehaviour, IEffector
{
    private enum ID
    {
        PROGRESS_QUEST = 0,
        COMPLETE_QUEST = 1,
        ASSIGN_QUEST = 2
    }

    [Header("IEffector Settings")]
    [SerializeField] private ID functionID;
    [SerializeField] private QuestData target;

    [ConditionalHide("onProgress", true)]
    [SerializeField] private int quota;

    // Inspector View Settings
    [HideInInspector] public bool onProgress;

    public void IEffectorExecute()
    {
        switch (functionID)
        {
            case ID.PROGRESS_QUEST:
                QuestSystem.onProgressQuest?.Invoke(target, quota);
                break;

            case ID.COMPLETE_QUEST:
                QuestSystem.onCompleteQuest?.Invoke(target);
                break;
        }
    }
    public void IEffectorExit()
    {
        throw new System.NotImplementedException();
    }


    private void OnValidate()
    {
        onProgress = false;

        switch (functionID)
        {
            case ID.PROGRESS_QUEST:
                onProgress = true;
                break;
        }
    }
}
