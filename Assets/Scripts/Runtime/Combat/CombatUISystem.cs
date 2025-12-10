using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CombatUISystem : MonoBehaviour
{
    [Header("Logic Reference")]
    [SerializeField] private CombatSystem combatSystem;

    [Header("User Interface (UI)")]
    [SerializeField] private Canvas actionUICanvas;
    [SerializeField] private RectTransform memberUIGroup;
    [SerializeField] private GameObject memberUIPrefab;
    [SerializeField] private TextMeshProUGUI turn_TMP;

    [Header("Battle UI Animation")]
    [SerializeField] private RectTransform battleUI;
    [SerializeField] private Canvas battleCanvas;
    [SerializeField, CE_ReadOnly] private bool IsBattleUIAnimating;
    [SerializeField, CE_ReadOnly] private bool IsBattleUIDisplaying;

    [Header("Start Turn UI Animation")]
    [SerializeField] private Canvas startTurnUICanvas;
    [SerializeField] private RectTransform startTurnBGTransform;
    [SerializeField] private TextMeshProUGUI startTurn_TMP;

    [Header("Clash Turn UI Animation")]
    [SerializeField] private ClashUI clashUI;

    [Header("Result UI Animation")]
    [SerializeField] private Canvas resultUICanvas;
    [SerializeField] private Image resultBG;
    [SerializeField] private TextMeshProUGUI result_TMP;

    [Header("Action Canvas")]
    [SerializeField] private Canvas attackCanvas;
    [SerializeField] private Canvas itemsCanvas;
    [SerializeField] private Button attack_Button;
    [SerializeField] private Button items_Button;
    [SerializeField] private Button escape_Button;

    [Header("Action List")]
    [SerializeField] private RectTransform attackGroup;
    //[SerializeField] private RectTransform itemGroup;
    [SerializeField] private GameObject skillPrefab;
    //[SerializeField] private GameObject itemPrefab;

    [SerializeField] private List<SkillButtonUI> skills;
    [SerializeField] private List<GameObject> items;

    [Header("Tracker")]
    [SerializeField, CE_ReadOnly] private ActionSlot currentSlot;
    [SerializeField, CE_ReadOnly] private Character currentCharacter;
    [SerializeField, CE_ReadOnly] private SkillButtonUI currentSkill;

    [SerializeField, CE_ReadOnly] private List<ActionSlot> playerActions;
    [SerializeField, CE_ReadOnly] private List<ActionSlot> enemyActions;

    private readonly List<MemberUI> memberUIs = new();
    private List<ActionSlot> characterSlots = new();
    private List<ActionSlot> enemySlots = new();


    private const float DURATION_BATTLE_START = 1f;
    private const float DURATION_BATTLE_UI = 0.3f;
    private const float DURATION_TURN_UI = 3f;

    private readonly WaitForSeconds YIELD_BATTLE_START = new(DURATION_BATTLE_START);
    private readonly WaitForSeconds YIELD_BATTLE_UI = new(DURATION_BATTLE_UI);
    private readonly WaitForSeconds YIELD_TURN_UI = new(DURATION_TURN_UI);

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentCharacter != null || currentSkill != null)
            {
                ActionSlot slot = CheckPointerOverSlot(ref characterSlots);
                // If UI selected is an ally slot
                if (slot != null)
                {
                    return;
                }

                slot = CheckPointerOverSlot(ref enemySlots);
                // If UI selected is an enemy slot
                if (slot != null)
                {
                    LockTarget(slot);
                    //Debug.Log("YOU PRESSED ON ENEMY!");
                    return;
                }

                // If UI selected is anything else other than battle ui
                if (!IsPointerOverUIElement(battleUI))
                {
                    Unselect();
                    //Debug.Log("YOU PRESSED OUT!");
                }
            }
        }
    }



    public void InitialiseUI(ref List<Character> characters)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            GameObject memberUIGO = Instantiate(memberUIPrefab, memberUIGroup);
            if (memberUIGO.TryGetComponent(out MemberUI memberUI))
            {
                memberUI.InitialiseUI(characters[i].GetData(), characters[i]);
                memberUIs.Add(memberUI);
            }
        }

        for (int i = 0; i < memberUIs.Count; i++)
            memberUIs[i].UpdateUI();
    }
    public void ReceiveActionUI(ref List<Character> characters, ref List<Enemy> enemies) 
    {
        characterSlots.Clear();
        enemySlots.Clear();

        for (int i = 0; i < characters.Count;i++)
        {
            Character character = characters[i];
            for (int j = 0; j < character.actions.Count; j++)
            {
                ActionSlot actionSlot = character.actions[j];
                characterSlots.Add(actionSlot);
            }
        }
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            for (int j = 0; j < enemy.actions.Count; j++)
            {
                ActionSlot actionSlot = enemy.actions[j];
                enemySlots.Add(actionSlot);
            }
        }
    }
    private void ResetCanvasUI()
    {
        if (currentSlot != null) currentSlot.highlight.enabled = false;
        if (currentSkill != null) currentSkill.frame.color = Color.white;

        currentSlot = null;
        currentCharacter = null;
        currentSkill = null;

        for (int i = skills.Count - 1; i >= 0; i--)
        {
            Destroy(skills[i].gameObject);
            skills.RemoveAt(i);
        }
        for (int i = items.Count - 1; i >= 0; i--)
        {
            Destroy(items[i]);
            items.RemoveAt(i);
        }
    }
    public void UpdateTurnCount(int turn)
    {
        turn_TMP.text = string.Format("Turns: {0}", turn);
    }
    public void ToggleUI()
    {
        actionUICanvas.enabled = !actionUICanvas.enabled;
    }


    public void InitiateCombat()
    {
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.INITIATE_BATTLE);
        Unselect();
        Unselect();
        combatSystem.TurnProcess(ref playerActions, ref enemyActions);
    }
    public void AddEnemyAction(ActionSlot action) => enemyActions.Add(action);



    public void DisplayCharacter(ActionSlot actionSlotUI)
    {
        if (actionSlotUI == null) return;
        if (IsBattleUIAnimating) return;

        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);

        ResetCanvasUI();

        // Set temporary references to character selected.
        currentSlot = actionSlotUI;
        currentCharacter = actionSlotUI.character;
        CharacterData characterData = currentCharacter.GetData();

        // Add in all current character skills to Attack UI.
        for (int i = 0; i < characterData.skills.Count; i++)
        {
            GameObject skillGO = Instantiate(skillPrefab, attackGroup);
            SkillData skillData = characterData.skills[i];

            if (skillGO.TryGetComponent(out SkillButtonUI skill))
            {
                skill.SetupSkill(skillData);
                skill.button.onClick.AddListener(() => { SelectSkill(skill); });
                skills.Add(skill);
            }
        }

        clashUI.UpdateUI(actionSlotUI, actionSlotUI.targetSlot);

        // Show Battle UI (If it is hidden)
        if (!IsBattleUIDisplaying)
        {
            StartCoroutine(DOTweenBattleUI());
            clashUI.AnimateClashUI();
        }

        currentSlot.highlight.enabled = true;
    }
    private void Unselect()
    {
        if (IsBattleUIAnimating) return;

        // Unselect skill first
        if (currentSkill != null)
        {
            currentSkill.frame.color = Color.white;
            currentSkill = null;
            return;
        }

        // Unselect slot next
        if (currentSlot != null)
        {
            currentSlot.highlight.enabled = false;
            StartCoroutine(DOTweenBattleUI());
            clashUI.AnimateClashUI();
        }
    }
    public void UnequipSkill()
    {
        if (currentSlot == null) return;
        if (currentSlot.targetSlot == null) return;

        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);

        currentSlot.ResetAction();
        int index = FindActionIndex(currentSlot.GetID());
        if (index >= 0) playerActions.RemoveAt(index);

        // ***************************
        // ERROR CLASH UI (UNEQUIP)
        clashUI.UpdateUI(currentSlot, currentSlot.targetSlot);
    }
    private void SelectSkill(SkillButtonUI skill)
    {
        if (currentSkill != null)
        {
           currentSkill.frame.color = Color.white;
        }

        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);
        currentSkill = skill;
        skill.frame.color = Color.red;
    }
    public void LockTarget(ActionSlot enemySlot)
    {
        if (currentSlot == null) return;
        if (currentSkill == null) return;

        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);

        currentSlot.ResetAction();
        currentSlot.SetAction(currentSkill.data, enemySlot);

        int index = FindActionIndex(currentSlot.GetID());
        if (index < 0) playerActions.Add(currentSlot);

        Unselect();
        clashUI.UpdateUI(currentSlot, currentSlot.targetSlot);
    }
    


    public void DisplayAttacks()
    {
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);
        attackCanvas.enabled = true;
        itemsCanvas.enabled = false;
    }
    public void DisplayItems()
    {
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);
        attackCanvas.enabled = false;
        itemsCanvas.enabled = true;
    }




    // DOTween Animations
    #region Animation Functions
    private IEnumerator DOTweenBattleUI()
    {
        if (IsBattleUIAnimating) yield break;

        IsBattleUIAnimating = true;
        IsBattleUIDisplaying = !IsBattleUIDisplaying;

        if (IsBattleUIDisplaying)
        {
            battleCanvas.enabled = true;
            battleUI.DOMoveY(0f, DURATION_BATTLE_UI);
        }

        else battleUI.DOMoveY(-600f, DURATION_BATTLE_UI);
        yield return YIELD_BATTLE_UI;

        if (!IsBattleUIDisplaying)
        {
            battleCanvas.enabled = false;
            ResetCanvasUI();
        }

        IsBattleUIAnimating = false;
        yield break;
    }

    public void AnimateTurnUI(int turnCounter) => StartCoroutine(DOTweenTurnUI(turnCounter));
    private IEnumerator DOTweenTurnUI(int turnCounter)
    {
        startTurnUICanvas.enabled = true;
        startTurn_TMP.text = string.Format("TURN {0}", turnCounter);

        float duration = DURATION_TURN_UI / 2f;


        startTurnBGTransform.DOSizeDelta(new Vector2(startTurnBGTransform.sizeDelta.x, 300f), duration);
        startTurn_TMP.rectTransform.DOScale(Vector3.one, duration);
        startTurn_TMP.DOFade(1f, duration);
        yield return new WaitForSeconds(duration);


        startTurnBGTransform.DOSizeDelta(new Vector2(startTurnBGTransform.sizeDelta.x, 0f), duration);
        startTurn_TMP.rectTransform.DOScale(Vector3.zero, duration);
        startTurn_TMP.DOFade(0f, duration);
        yield return new WaitForSeconds(duration);

        startTurnUICanvas.enabled = false;
        yield break;
    }

    public void AnimateResultUI(bool IsVictory) => StartCoroutine(DOTweenResultUI(IsVictory));
    private IEnumerator DOTweenResultUI(bool IsVictory)
    {
        resultUICanvas.enabled = true;
        resultBG.DOFade(1f, 0.5f);

        if (IsVictory)
        {
            result_TMP.color = Color.yellow;
            result_TMP.text = "Victory";
        }
        else
        {
            result_TMP.color = Color.red;
            result_TMP.text = "Defeat";
        }

        yield return new WaitForSeconds(0.3f);
        result_TMP.enabled = true;
        result_TMP.rectTransform.DOScale(new Vector3(1f, 1f, 1f), 0.2f);
        yield return new WaitForSeconds(0.2f);
        result_TMP.rectTransform.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.5f, 5, 1f);

        yield return new WaitForSeconds(1f);
        if (TryGetComponent(out EFT_SceneTransition sceneTransition))
        {
            sceneTransition.EnterScene();
        }

        yield break;
    }

    public WaitForSeconds GetBattleStartDuration()
    {
        return YIELD_BATTLE_START;
    }
    public WaitForSeconds GetTurnUIDuration()
    {
        return YIELD_TURN_UI;
    }
    #endregion


    // Utility
    #region UI Utility Functions
    private int FindActionIndex(string id)
    {
        for (int i = 0; i < playerActions.Count; i++)
        {
            ActionSlot playerAction = playerActions[i];
            string actionCurrent = playerAction.GetID();

            if (string.Compare(id, actionCurrent, System.StringComparison.Ordinal) == 0)
                return i;
        }
        return -1;
    }
    private ActionSlot CheckPointerOverSlot(ref List<ActionSlot> actionSlots)
    {
        for (int i = 0; i < actionSlots.Count; i++)
        {
            ActionSlot slot = actionSlots[i];
            if (slot.frame.rectTransform == null) continue;

            if (IsPointerOverUIElement(slot.frame.rectTransform))
                return slot;
        }

        return null;
    }
    private bool IsPointerOverUIElement(RectTransform target)
    {
        PointerEventData pointerData = new(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.transform.IsChildOf(target))
                return true; // clicked inside
        }
        return false; // clicked outside
    }
    private bool DoesActionSlotMatchCurrent(ActionSlot actionSlot)
    {
        if (currentSlot == null) return false;

        string actionNew = actionSlot.GetID();
        string actionCurrent = currentSlot.GetID();

        if (string.Compare(actionNew, actionCurrent, System.StringComparison.Ordinal) == 0) return true;

        return false;
    }
    private void ParseJSON(ActionSlot actionSlot)
    {
        string id_data = actionSlot.GetID();
        string[] parts = id_data.Split(new[] { '<', '>' }, System.StringSplitOptions.RemoveEmptyEntries);

        string entity_name = string.Empty;
        bool is_player = false;
        int entity_id = 0;
        int count_id = 0;
        int action_id = 0;
        int speed = 0;

        foreach (string part in parts)
        {
            string[] pair = part.Split('=');
            string key = pair[0];
            string value = pair[1];

            switch (key)
            {
                case "entity_name": entity_name = value; break;
                case "is_player": is_player = bool.Parse(value); break;
                case "id": entity_id = int.Parse(value); break;
                case "count_id": count_id = int.Parse(value); break;
                case "action_id": action_id = int.Parse(value); break;
                case "speed": speed = int.Parse(value); break;
            }
        }
    }
    #endregion
}