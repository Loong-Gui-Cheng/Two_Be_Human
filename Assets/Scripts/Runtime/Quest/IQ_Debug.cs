using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IQ_Debug : MonoBehaviour, IQuest
{
    [SerializeField] private QuestData data;

    public void QuestActiveEvent()
    {
        if (data == null) return;
        Debug.Log("Active event triggered");

        data.state = QuestData.STATE.ACTIVE;
    }

    public void QuestProgressEvent(int i)
    {
        if (data == null) return;
        Debug.Log("Progress event triggered");

        data.count += i;
        QuestSystem.onUpdateQuest?.Invoke(data);

        if (data.count >= data.quota)
            QuestCompletedEvent();
    }
    public void QuestCompletedEvent()
    {
        if (data == null) return;
        Debug.Log("Complete event triggered");

        QuestSystem.onCompleteQuest?.Invoke(data);

        if (data.isRepeatable)
        {
            data.state = QuestData.STATE.PENDING;
            data.count = 0;
        }
        else
            data.state = QuestData.STATE.COMPLETE;
    }

    public void Set(QuestData data) => this.data = data;
}
