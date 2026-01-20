using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DIALOGUE_DATA", menuName = "Custom/Dialogues/Dialogue")]
public class DialogueData : ScriptableObject
{
    // Cases
    // 1. Dialogue Ends without option (Check List.Length)
    // 2. Dialogue "Ends" with options (Add Buttons -> Apply Listeners)
    // 3. Dialogue unlock on special predicates [Unlock X] ()

    // Prequesites
    // 1. True or False Statement (Flexible)
    // 2. Extendable variable conditional types (Predicates)
    // 3. Evaluated on DialogueSystem conditional check (run-time)

    [Header("ID")]
    public string branchName;

    [Header("Enter/Exit Behaviour")]
    [Tooltip("A GameObject name implementing IDialogueBinding. Executes scripts On Enter.")]
    public string refIDB_GONameStart; // A specific GameObject with a script implementing IDialogueBind.
    [Tooltip("A GameObject name implementing IDialogueBinding. Executes scripts On Exit.")]
    public string refIDB_GONameEnd; // A specific GameObject with a script implementing IDialogueBind.
    [Tooltip("Enables/Disable Auto-Play Mode On Enter.")]
    public bool onEnterAutoPlay;

    [Header("Skip (OnScript)")]
    [Tooltip("A list of GameObject names implementing IDialogueBinding. Executes only if player skips through dialogue. (Applicable only for <script> tag.)")]
    public List<string> refIDB_GONameOnScript;

    [Header("Content")]
    public List<Dialogue> dialogues;
    public Branch nextBranch;

    [Header("Choice")]
    public List<Branch> options;

    private void OnValidate()
    {
        if (nextBranch.data != null)
            nextBranch.name = nextBranch.data.branchName;
        
        for (int i = 0; i < dialogues.Count; i++)
        {
            Branch eventBranch = dialogues[i].eventBranch;
            if (eventBranch.data == null) continue;
            eventBranch.name = eventBranch.data.branchName;
        }
    }
}

[System.Serializable]
public class Dialogue
{
    public DialogueNPCData NPC;
    public AudioClip clip;
    [TextArea(3, 5)] public string message;
    public Branch eventBranch;
}

[System.Serializable]
public class Branch
{
    public string name;
    [Tooltip("A branch to swap to upon fulfilling criteria.")]
    public DialogueData data;
    [Tooltip("A set of criteria that triggers swapping when fulfiled.")]
    public DialogueConditional condition;

    public bool IsValid()
    {
        return name != string.Empty && data != null; 
    }
    public bool IsConditionFulfilled()
    {
        if (condition != null)
            return condition.Evaluate();

        return true;
    }
}

/// <summary>
/// A custom script to execute DBind events on run-time, using a GO name as reference in DialogueData.
/// </summary>
public interface IDialogueBind
{
    void IDialogueExecute();
}