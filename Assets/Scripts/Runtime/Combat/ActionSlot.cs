using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;


public class ActionSlot : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField, CE_ReadOnly] private string id;
    [SerializeField, TextArea(3, 5)] private string jsonData;
    public Enemy enemy;
    public Character character;

    [Header("Speed")]
    [SerializeField] private int SPD;
    [SerializeField] private TextMeshProUGUI speed_TMP;

    [Header("User Interface (UI)")]
    [SerializeField] private Sprite slot_background;
    [SerializeField] private Image icon;

    public Canvas canvas;
    public Image frame;
    public Light2D highlight;
    public Button button;

    [Header("Action")]
    public SkillData skillData;
    public ActionSlot targetSlot;

    public bool IsClashing;
    public List<ActionSlot> clashedBy;

    // WIP
    public IAction action;



    public void SetAction(SkillData skill, ActionSlot enemySlot)
    {
        icon.sprite = skill.icon;
        targetSlot = enemySlot;
        skillData = skill;

        if (enemySlot != null)
        {
            // When enemy sets their target, skip clash check logic.
            if (enemySlot.skillData == null) return;

            bool clashableAttack = skillData.type == SkillData.TYPE.CLASH && enemySlot.skillData.type == SkillData.TYPE.CLASH;

            bool Redirection = SPD > enemySlot.GetSPD() && clashableAttack;
            bool CounterDirection = id == enemySlot.targetSlot.GetID() && clashableAttack;

            // If this slot is faster than targeted slot and targeted slot is an attack, append to clash.
            if (Redirection)
            {
                Clash(enemySlot);
            }
            // If targeted slot (Target id) is same as this slot ID, and both are attack, append to clash.
            else if (CounterDirection)
            {
                Clash(enemySlot);
            }
            // Unopposed action.
            else
            {
                string log = string.Format("Player {0} unopposes Enemy {1}!", id, enemySlot.GetID());
                Debug.Log(log);
            }
        }
    }
    private void RemoveClashAction(string id)
    {
        // clashedBy - belong to "Enemy"
        for (int i = 0; i < clashedBy.Count; i++)
        {
            if (string.Compare(clashedBy[i].id, id, System.StringComparison.Ordinal) == 0)
            {
                clashedBy.RemoveAt(i);
                break;
            }
        }

        if (clashedBy.Count <= 0)
            IsClashing = false;
    } 
    private void Clash(ActionSlot enemySlot)
    {
        IsClashing = true;
        enemySlot.IsClashing = true;
        enemySlot.clashedBy.Insert(0, this);

        string targetDebug = string.Format("Enemy {0} Is Clashing -> Player {1}, with other clashable(s): ", enemySlot.GetID(), id);

        // Log clash order list
        if (enemySlot.clashedBy.Count > 1)
        {
            for (int i = 1; i < enemySlot.clashedBy.Count; i++)
            {
                string clashableID = string.Format("{0} | ", enemySlot.clashedBy[i].GetID());
                targetDebug = string.Concat(targetDebug, clashableID);
            }
        }
        Debug.Log(targetDebug);
    }
    public void ResetAction()
    {
        // Remove ally clash attack from enemy (if any)
        if (targetSlot != null) targetSlot.RemoveClashAction(id);

        // Reset UI
        icon.sprite = slot_background;
        IsClashing = false;
        targetSlot = null;
        action = null;
    }

    public void SetSPD(int speedValue)
    {
        SPD = speedValue;
        speed_TMP.text = string.Format("{0}", SPD);
    }
    public void SetID(Enemy input, int actionID)
    {
        enemy = input;
        EnemyData data = enemy.GetData();

        // 1 is_player
        // 2 id
        // 3 count_id
        // 4 action_id

        id = string.Format("{0}{1}{2}A{3}S{4}", 0, data.id, 1, actionID, SPD);
        jsonData = string.Format("<entity_name={0}><is_player={1}><id={2}><count_id={3}><action_id=A{4}><speed={5}>", data.Name, 0, data.id, 1, actionID, SPD);
    }
    public void SetID(Character input, int actionID)
    {
        character = input;
        CharacterData data = character.GetData();

        id = string.Format("{0}{1}{2}A{3}S{4}", 1, (int)data.id, 0, actionID, SPD);
        jsonData = string.Format("<entity_name={0}><is_player={1}><id={2}><count_id={3}><action_id=A{4}><speed={5}>", data.Name, 1, (int)data.id, 0, actionID, SPD);
    }
    public int GetSPD()
    {
        return SPD;
    }
    public string GetID()
    {
        return id;
    }
    public string GetJSON()
    {
        return jsonData;
    }
}

public interface IAction
{
    void Act(ActionSlot thisEntity, ActionSlot otherEntity);
}