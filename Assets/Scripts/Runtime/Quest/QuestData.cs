using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QUEST_DATA", menuName = "Custom/Quests/Quest")]
public class QuestData : ScriptableObject
{
    public enum ID
    {
        PARCEL = 0,
        COFFEE = 1,
        BAG_OF_FRUITS = 2
    }
    public enum SERIES
    {
        NONE = 0,
        DELIVERY,
        SHOPPING,
    }
    public enum TYPE
    {
        NONE = 0,
        COLLECT = 1,
        PURCHASE = 2
    }
    public enum STATE
    {
        PENDING = 0,
        ACTIVE = 1,
        COMPLETE = 2
    }

    public ID id;
    public string title;
    public SERIES series;
    public TYPE type;
    public STATE state;
    public int count;
    public int quota;
    public bool isRepeatable;

    private void OnEnable()
    {
        count = 0;
    }
    public string GetSeries()
    {
        switch (series)
        {
            case SERIES.DELIVERY:
                return "Delivery";

            case SERIES.SHOPPING:
                return "Shopping";
        }

        switch (type)
        {
            case TYPE.COLLECT:
                return "Collect";

            case TYPE.PURCHASE:
                return "Purchase";
        }

        return "PLACEHOLDER";
    }
    private void OnValidate()
    {
        if (count < 0) count = 0;
        if (quota < 0) quota = 0;
    }
}