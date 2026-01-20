using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/*********************************************************************************
Written by: Loong Gui Cheng
Description: This class manages the dialogue system logic between NPCs / Player.
These include whenever a player talks to an NPC, interact with an object (Dialog panel shows up),
a event happening in a scene, or anything that requires panel with text display.

Class relation:
Manipulates Dialogue Data Scriptable Object class (Process Dialogue Text & Branching)
Manipulates Dialogue NPC Data Scriptable Object class (Display NPC name on Runtime)
Stores Dialogue Object script class as an ID for retrieving data (Mainly for voice-overs & SFX) [*m_Speakers]

Uses Dialogue Trigger script to retrieve NPC ID Object here & to activate dialogue.
Uses Dialogue Bind (DBind) scripts to activate custom script effects on Scene using a GameObject name as reference point in Dialogue Data.
Uses TextMeshProAnimated custom script to animate dialogue text & activate custom events using tags (Character Animation, Sound Effect, Pause) 

NOTE: My apologies if some of the code here are confusing. 
*********************************************************************************/

/// <summary>
/// Manages the dialogue logic between NPCs / Player.
/// </summary>
public class DialogueSystem : Singleton<DialogueSystem>
{
    [Header("Parented (UI)")]
    [SerializeField] private Transform OVRCenterAnchor;

    [Header("User Interface (UI)")]
    [SerializeField] private GameObject m_StartPanel;
    [SerializeField] private GameObject m_MainPanel;
    [SerializeField] private RectTransform m_ChoicePanel;
    [SerializeField] private TextMeshProUGUI m_SpeakerNameTMP;
    [SerializeField] private TextMeshProAnimated m_DialogueTMP;
    [SerializeField] private TextMeshProUGUI m_ContinueTMP;
    [SerializeField] private Image m_AutoPlayOff;

    [Header("Controllers")]
    [SerializeField] private Button m_SkipButton;
    public Toggle m_AutoplayToggle;
    [SerializeField] private List<Button> m_Choices;

    [Header("All NPCs")]
    [SerializeField] private List<DialogueObject> m_Speakers;
    private readonly Dictionary<DialogueNPCData.DIALOGUE_NPC_ID, DialogueObject> m_DialogueNPCs = new();

    // Data Field
    [Header("Privated Variables (Debug)")]
    [CE_ReadOnly, SerializeField] private DialogueObject currentTarget;
    [CE_ReadOnly, SerializeField] private DialogueNPCData currentNPC;
    [CE_ReadOnly, SerializeField] private DialogueData currentBranch;

    // Unit Tests
    private bool isRunning; // System is currently yapping
    private bool canProceed; // Can proceed to next dialogue
    private bool isChoice; // Is currently choosing an option.
    private bool isSwapBranch; // Is entering a different branch.
    private bool isDisappear; // Disable dialogue object when ending dialogue.
    private bool isSkipping; // Is currently skipping through dialogues.

    // Iterators
    [CE_ReadOnly, SerializeField] private int iSequence = -1; // Current Dialogue.
    [CE_ReadOnly, SerializeField] private int endSequence = -1; // End Dialogue.

    private readonly WaitForSeconds cooldownDuration = new(0.5f);
    private readonly WaitForSeconds autoDuration = new(2f);
    private int m_ContinueTextVisibleLines = -1;

    private void Start()
    {
        m_ContinueTextVisibleLines = m_ContinueTMP.maxVisibleLines;

        if (m_Speakers.Count <= 0)
        {
            Debug.LogWarning("Don't forget to put dialogue speakers!");
            return;
        }

        SetSpeakerReferences();
    }
    private void Update()
    {
        if (!m_StartPanel.activeSelf && !m_MainPanel.activeSelf) return;
        if (OVRCenterAnchor == null) return;

        // Centering the dialogue
        Vector3 position = OVRCenterAnchor.position;
        Vector3 forward = -Vector3.Normalize(OVRCenterAnchor.transform.forward);
        Vector3 rotation = OVRCenterAnchor.rotation.eulerAngles;

        transform.SetPositionAndRotation(new Vector3(position.x + forward.x * 0.3f, position.y - 0.25f, position.z + forward.z * 0.3f),
            Quaternion.Euler(rotation.x, rotation.y, 0));

    }


    #region Set/Clear Dialogue Target
    public void SetDialogueTarget(DialogueNPCData.DIALOGUE_NPC_ID id, DialogueData newBranch, bool startImmediately, bool displayStartButton)
    {
        // Unit Tests
        if (IsCurrentlyRunning()) return;

        m_DialogueNPCs.TryGetValue(id, out DialogueObject target);
        if (target == null) return;
        if (startImmediately && newBranch != null) target.dialogueBranch = newBranch;
        if (target.dialogueBranch == null && target.defaultBranch == null) return;

        // Store a temporary reference to the triggered object and its data
        currentTarget = target;

        if (currentTarget.dialogueBranch != null)
            currentBranch = currentTarget.dialogueBranch;
        else
            currentBranch = currentTarget.defaultBranch;

        // Start dialogue immediately on trigger
        if (startImmediately)
            StartDialogue();

        // Turn on dialogue button
        else if (displayStartButton)
        {
            m_StartPanel.SetActive(true);
        }

    }
    public void ClearDialogueTarget()
    {
        currentTarget = null;
        currentBranch = null;
        currentNPC = null;
        m_StartPanel.SetActive(false);
    }
    private void SetSpeakerReferences()
    {
        for (int i = m_Speakers.Count - 1; i >= 0; i--)
        {
            // If NPC is null, remove invalid item from list.
            DialogueObject npc = m_Speakers[i];
            if (npc == null)
            {
                m_Speakers.RemoveAt(i);
                continue;
            }

            // If NPC is already inside, remove any other duplicates of it.
            DialogueNPCData.DIALOGUE_NPC_ID ID = npc.data.ID;
            if (m_DialogueNPCs.ContainsKey(ID))
            {
                m_Speakers.RemoveAt(i);
                continue;
            }

            // NPC is valid, add it to list for 3D voice purposes.
            m_DialogueNPCs.Add(ID, npc);
        }
    }
    #endregion

    #region Main Function
    public void StartDialogue()
    {
        // If branch dialogues not set-up properly, exit early.
        if (currentBranch.dialogues == null || currentBranch.dialogues.Count <= 0)
        {
            EndDialogue();
            return;
        }

        // System is running.
        isRunning = true;
        canProceed = true;
        isChoice = false;
        isSwapBranch = false;
        isSkipping = false;

        // Display dialogue box and skip button.
        m_StartPanel.SetActive(false);
        m_MainPanel.SetActive(true);

        m_SkipButton.gameObject.SetActive(true);
        m_AutoplayToggle.gameObject.SetActive(true);

        // Freeze Player Position
        // Rotate Speaker towards Player

        // Set text-based events
        if (currentTarget != null)
        {
            m_DialogueTMP.onAction.AddListener((action) => { currentTarget.SetAction(action); });
            m_DialogueTMP.onScript.AddListener((IDB_GO_NAME) => { CheckCustomBindings(IDB_GO_NAME); });
            m_DialogueTMP.onAnimation.AddListener((stateName) => { currentTarget.Animate(stateName); });

            if (currentTarget.CanPlayACVoice())
                m_DialogueTMP.onTextReveal.AddListener((c) => { currentTarget.ReproduceSound(c); });
        }

        // Set iterators
        iSequence = 0;
        endSequence = currentBranch.dialogues.Count;

        // Launch custom binding scripts upon entering new branch.
        string IDialogueBindGOStart = currentBranch.refIDB_GONameStart;
        CheckCustomBindings(IDialogueBindGOStart);

        // Check if starting dialogue has auto on.
        if (currentBranch.onEnterAutoPlay)
            m_AutoplayToggle.isOn = true;

        NextDialogue();
    }
    public void SwapDialogue()
    {
        if (iSequence >= endSequence)
            iSequence = endSequence - 1;

        // If branch dialogues not set-up properly, exit early.
        if (currentBranch.dialogues == null || currentBranch.dialogues.Count <= 0)
        {
            EndDialogue();
            return;
        }

        isChoice = false;
        isSwapBranch = false;

        m_DialogueTMP.StopAnimating();

        // Set iterators
        iSequence = 0;
        endSequence = currentBranch.dialogues.Count;

        // Launch custom binding scripts upon entering new branch.
        string IDialogueBindGOStart = currentBranch.refIDB_GONameStart;
        CheckCustomBindings(IDialogueBindGOStart);

        if (isSkipping)
        {
            SkipDialogue();
            return;
        }

        if (currentBranch.onEnterAutoPlay)
            m_AutoplayToggle.isOn = true;

        NextDialogue();
    }
    public void NextDialogue()
    {
        if (!canProceed) return;
        if (isChoice) return;

        StopAllCoroutines();

        // Dialogue entered new branch.
        if (isSwapBranch)
        {
            SwapDialogue();
            return;
        }
        // Dialogue has ended.
        if (iSequence >= endSequence)
        {
            EndDialogue();
            return;
        }

        // Start next dialogue cooldown.
        StartCoroutine(DialogueRoutine());

        if (m_DialogueTMP.IsCurrentlyAnimating())
            m_DialogueTMP.StopAnimating();

        Dialogue dialogue = currentBranch.dialogues[iSequence];
        DialogueNPCData previousNPC = currentNPC;
        currentNPC = dialogue.NPC;

        // Check if previous npc is valid.
        if (previousNPC != null)
        {
            // Stop playing sound from previous NPC.
            DialogueObject previous = currentTarget;
            previous.StopAudio();

            if (currentNPC == null)
            {
                Debug.Assert(currentNPC == null);
                return;
            }

            if (previousNPC.ID != currentNPC.ID)
            {
                // Switch voice target to new NPC if found.
                if (m_DialogueNPCs.TryGetValue(currentNPC.ID, out DialogueObject newSpeaker))
                {
                    currentTarget = newSpeaker;
                    m_DialogueTMP.RemoveAllListeners();

                    // Set Text Based Events
                    m_DialogueTMP.onAction.AddListener((action) => { currentTarget.SetAction(action); });
                    m_DialogueTMP.onScript.AddListener((IDB_GO_NAME) => { CheckCustomBindings(IDB_GO_NAME); });
                    m_DialogueTMP.onAnimation.AddListener((stateName) => { currentTarget.Animate(stateName); });

                    if (currentTarget.CanPlayACVoice())
                        m_DialogueTMP.onTextReveal.AddListener((c) => { currentTarget.ReproduceSound(c); });
                }
            }
        }
        
        // Play voice-over clip, if any.
        if (dialogue.clip != null && AudioController.Instance != null)
            AudioController.Instance.Play3D(currentTarget.voiceSource, dialogue.clip);

        AudioController.Instance.PlayUI(AudioController.SOUND_ID.DIALOGUE_CLICK);

        // Set NPC UI
        SetNPCUI(currentNPC);
        m_DialogueTMP.text = dialogue.message;
        m_DialogueTMP.Animate();
        iSequence++;

        // Auto-Button
        if (m_AutoplayToggle.isOn)
            StartCoroutine(AutoCompleteRoutine());

        // If new branch is valid and condition is fulfilled, step into it.
        if (dialogue.eventBranch.IsValid() 
            && dialogue.eventBranch.IsConditionFulfilled())
        {
            currentBranch = dialogue.eventBranch.data;
            isSwapBranch = true;
            return;
        }

        // If dialogue is already at the end, check for any options.
        if (iSequence == endSequence)
        {
            // Hide skip button.
            m_SkipButton.gameObject.SetActive(false);
            m_AutoplayToggle.gameObject.SetActive(false);

            int noOfChoices = currentBranch.options.Count;
            if (noOfChoices <= 0) return;

            // User can pick a choice.
            isChoice = true;
            isSkipping = false;

            // Add functionalities
            for (int i = 0; i < noOfChoices; i++)
            {
                // Only supports up to 3 options at a time.
                if (i >= 3) break;
                Branch option = currentBranch.options[i];
                if (!option.IsValid()) continue;

                Button button = m_Choices[i];
                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();

                // Unit Tests
                if (label == null) return;
                if (!option.IsConditionFulfilled()) button.enabled = false;

                label.text = option.name;
                button.onClick.AddListener(() => { PickChoice(option.data); });
                button.gameObject.SetActive(true);
            }
        }
    }
    public void EndDialogue()
    {
        // Disable voice & sound from NPC if there are any.
        if (currentTarget != null)
            currentTarget.StopAudio();

        AudioController.Instance.PlayUI(AudioController.SOUND_ID.DIALOGUE_CLICK);

        // Launch custom binding scripts upon exiting branch.
        string IDialogueBindGOEnd = currentBranch.refIDB_GONameEnd;
        CheckCustomBindings(IDialogueBindGOEnd);

        Branch nextBranch = currentBranch.nextBranch;

        // Set Current Dialogue Object to new branch.
        currentTarget.dialogueBranch = null;

        if (nextBranch.IsValid() && nextBranch.IsConditionFulfilled())
            currentTarget.dialogueBranch = nextBranch.data;

        // System is not running anymore.
        isRunning = false;
        canProceed = false;
        isChoice = false;
        isSwapBranch = false;
        isSkipping = false;

        if (isDisappear)
        {
            if (currentTarget.TryGetComponent(out EFT_Dialogue DT))
                DT.enabled = false;

            isDisappear = false;
        }

        // Hide dialogue panel.
        m_MainPanel.SetActive(false);

        // Reset name and text field.
        m_DialogueTMP.StopAnimating();
        m_DialogueTMP.RemoveAllListeners();
        m_DialogueTMP.text = string.Empty;
        m_SpeakerNameTMP.text = "NULL";
        m_SpeakerNameTMP.color = Color.black;

        // Clear the Dialogue Object references.
        ClearDialogueTarget();
        m_AutoplayToggle.isOn = false;

        // Reset iterators.
        iSequence = -1;
        endSequence = -1;
    }
    #endregion

    #region Subsidary Dialogue Controls
    public void AutoCompleteDialogue()
    {
        StopCoroutine(AutoCompleteRoutine());

        if (m_AutoplayToggle.isOn)
        {
            StartCoroutine(AutoCompleteRoutine());
            m_AutoPlayOff.gameObject.SetActive(false);
        }
        else
        {
            m_AutoPlayOff.gameObject.SetActive(true);
        }
    }
    public void SkipDialogue()
    {
        // Unit Tests
        if (currentTarget == null) return;
        if (currentBranch == null) return;

        isSkipping = true;

        m_AutoplayToggle.isOn = false;
        StopAllCoroutines();

        // Skip dialogue until a conditional branch. 
        for (int i = iSequence; i < endSequence; i++, iSequence++)
        {
            List<string> refBinds = currentBranch.refIDB_GONameOnScript;

            if (refBinds.Count > 0 && i < refBinds.Count)
            {
                string refIDB_GOName = currentBranch.refIDB_GONameOnScript[i];
                CheckCustomBindings(refIDB_GOName);
            }

            Branch branch = currentBranch.dialogues[i].eventBranch;

            // If new branch is valid and condition is fulfilled, stop iterating.
            if (branch.IsValid() && branch.IsConditionFulfilled())
                break;
        }

        // Case 1: Reached and fulfilled conditional branch.
        if (iSequence < endSequence)
        {
            DialogueData eventBranchData = currentBranch.dialogues[iSequence].eventBranch.data;
            currentBranch = eventBranchData;
            SwapDialogue();
            return;
        }

        // Case 2: Reached the end of branch.
        if (currentBranch.options.Count > 0)
        {
            // Case 2.1 Ending dialogue has options.
            // Backtrack to last dialogue sequence. (Because iSequence == endSequence, which is invalid.)
            iSequence -= 1;
            NextDialogue();
            return;
        }

        // Case 2.2: Dialogue officially ends.
        EndDialogue();
    }
    private void PickChoice(DialogueData branch)
    {
        // Step into the new dialogue branch.
        // Re-enable skip button.
        m_SkipButton.gameObject.SetActive(true);
        m_AutoplayToggle.gameObject.SetActive(true);

        // Clear all choice buttons after choosing an option.
        for (int i = 0; i < m_Choices.Count; i++)
        {
            m_Choices[i].onClick.RemoveAllListeners();
            m_Choices[i].gameObject.SetActive(false);
            m_Choices[i].enabled = true;
        }
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);

        currentBranch = branch;
        SwapDialogue();
    }
    #endregion


    #region Helper Functions
    public DialogueObject GetNPCObject(DialogueNPCData.DIALOGUE_NPC_ID ID)
    {
        m_DialogueNPCs.TryGetValue(ID, out DialogueObject NPC);
        return NPC;
    }  
    public bool IsCurrentlyRunning() { return isRunning; }
    public void SetNPCUI(DialogueNPCData npc)
    {
        if (npc == null)
        {
            m_SpeakerNameTMP.text = string.Empty;
            m_SpeakerNameTMP.color = Color.black;
            return;
        }

        m_SpeakerNameTMP.text = npc.alias;
        m_SpeakerNameTMP.color = npc.aliasColor;
    }
    private void CheckCustomBindings(string IDialogueBindGO)
    {
        if (IDialogueBindGO == null) return;
        if (IDialogueBindGO.CompareTo(string.Empty) != 0)
        {
            GameObject binder = GameObject.Find(IDialogueBindGO);

            // Execute custom functions. (GO with IDialogueBind script interface).
            // (Eg. Opening shop menu after dialogue finish).
            if (binder != null)
            {
                IDialogueBind[] bindings = binder.GetComponents<IDialogueBind>();
                if (bindings != null && bindings.Length > 0)
                {
                    foreach (IDialogueBind bind in bindings)
                        bind.IDialogueExecute();
                }
            }
        }
    }

    private IEnumerator DialogueRoutine()
    {
        m_ContinueTMP.maxVisibleLines = 0;

        canProceed = false;
        yield return cooldownDuration;
        canProceed = true;

        while (m_DialogueTMP.IsCurrentlyAnimating())
            yield return null;

        m_ContinueTMP.maxVisibleLines = m_ContinueTextVisibleLines;
        yield break;
    }
    private IEnumerator AutoCompleteRoutine()
    {
        while (m_DialogueTMP.IsCurrentlyAnimating())
        {
            yield return null;
        }
        while (currentTarget.IsPlaying())
        {
            yield return null;
        }
        yield return autoDuration;

        if (!isChoice)
            NextDialogue();

        yield break;
    }
    // Mainly for debugging.
    public void SelectChoice(int i)
    {
        if (i < 0 || i >= 3) return;

        if (m_Choices[i].enabled)
            m_Choices[i].onClick?.Invoke();
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(DialogueSystem))]
public class DialogueSystemEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DialogueSystem dialogueSystem = (DialogueSystem)target;

        // Executes whenever values in inspector changes.
        if (DrawDefaultInspector())
        {
        }

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Runtime Controls", EditorStyles.boldLabel, GUILayout.MaxWidth(200f));
        if (GUILayout.Button("Next Dialogue", GUILayout.Width(200f), GUILayout.Height(30f)))
            dialogueSystem.NextDialogue();
        if (GUILayout.Button("Skip Dialogue", GUILayout.Width(200f), GUILayout.Height(30f)))
            dialogueSystem.SkipDialogue();
        if (GUILayout.Button("Enable Auto Dialogue", GUILayout.Width(200f), GUILayout.Height(30f)))
            dialogueSystem.m_AutoplayToggle.isOn = true;
        if (GUILayout.Button("Disable Auto Dialogue", GUILayout.Width(200f), GUILayout.Height(30f)))
            dialogueSystem.m_AutoplayToggle.isOn = false;
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Dialogue Choices", EditorStyles.boldLabel, GUILayout.MaxWidth(150f));
        if (GUILayout.Button("Option 1", GUILayout.Width(150f), GUILayout.Height(30f)))
            dialogueSystem.SelectChoice(0);
        if (GUILayout.Button("Option 2", GUILayout.Width(150f), GUILayout.Height(30f)))
            dialogueSystem.SelectChoice(1);
        if (GUILayout.Button("Option 3", GUILayout.Width(150f), GUILayout.Height(30f)))
            dialogueSystem.SelectChoice(2);
        GUILayout.EndVertical();
    }
}
#endif