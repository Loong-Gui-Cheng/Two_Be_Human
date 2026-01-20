using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DIALOGUE_CONDITIONS", menuName = "Custom/Dialogues/Conditional")]
public class DialogueConditional : ScriptableObject
{
    #region Condition Data
    public enum CONDITION_ID
    {
        NONE = 0,
        QUEST_1,
        QUEST_2,
        QUEST_3,
        QUEST_4
    }
    #endregion

    public string ref_GO_Target1;
    public string ref_GO_Target2;

    // Condition ID
    public CONDITION_ID ID;

    public bool Evaluate()
    {
        switch (ID)
        {
            case CONDITION_ID.QUEST_1:
                return ConditionQuest1();

            case CONDITION_ID.QUEST_2:
                return ConditionQuest1();
        }
        return false;
    }

    public bool ConditionQuest1()
    {
        // Evaluate something here.
        return false;
    }
}