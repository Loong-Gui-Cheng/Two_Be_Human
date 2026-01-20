using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************************
Written by: Loong Gui Cheng
Description: This class triggers/activates the dialogue system based on conditions met (if there are any).
This can be done via collision (Tag checking) / button press.

Class relation:
Sends over Dialogue NPC ID to Dialogue System to activate Dialogue Branch set on an Dialogue Object.

NOTE: My apologies if some of the code here are confusing. 
*********************************************************************************/

public class EFT_Dialogue : MonoBehaviour, IEffector
{
    [Header("Voice Target")]
    [SerializeField] private DialogueNPCData.DIALOGUE_NPC_ID ID;

    [Header("Settings")]
    [Tooltip("Start Dialogue When Triggered")]
    [SerializeField] private bool onStart = true;
    [Tooltip("Set new dialogue branch to NPC")]
    [SerializeField] private bool onSetNewDialogue;

    [ConditionalHide("onStart", true), Tooltip("Delay before trigger fires")]
    [SerializeField] private float delayStart = 0f;
    [ConditionalHide("onSetNewDialogue", true)]
    public DialogueData newBranch;

    // Conditions before dialogue can be triggered.
    private readonly List<IDialogueCondition> conditions = new();
    private bool isActive = true;

    private void Awake() => GetComponents(conditions);
    private void OnEnable()
    {
        if (onStart && delayStart > 0f)
        {
            isActive = false;
            StartCoroutine(DelayRoutine());
        }
    }

    public void IEffectorExecute()
    {
        if (!isActive) return;
        if (!enabled) return;
        if (SceneTransitionManager.Instance != null)
        {
            if (SceneTransitionManager.Instance.IsCurrentlyLoading())
                return;
        }
        if (!AreConditionsMet()) return;

        SetDialogue();
    }

    public void IEffectorExit()
    {
        // Dialogue Trigger By Tag
        if (!isActive) return;
        if (!enabled) return;

        ClearDialogue();
    }

    public void SetDialogue()
    {
        if (DialogueSystem.Instance == null) return;
        if (DialogueSystem.Instance.IsCurrentlyRunning()) return;

        DialogueSystem.Instance.SetDialogueTarget(ID, newBranch, onStart, !onStart);
    }

    private void ClearDialogue()
    {
        if (DialogueSystem.Instance == null) return;
        if (DialogueSystem.Instance.IsCurrentlyRunning()) return;

        //DialogueSystem.Instance.ClearDialogueTarget();
    }

    private bool AreConditionsMet()
    {
        if (conditions == null) return true;
        for (int i = 0; i < conditions.Count; i++)
        {
            if (conditions[i] == null) continue;
            if (!conditions[i].Execute()) return false;
        }
        return true;
    }
    private IEnumerator DelayRoutine()
    {
        isActive = false;
        yield return new WaitForSeconds(delayStart);
        isActive = true;
    }
}

public interface IDialogueCondition
{
    bool Execute();
}