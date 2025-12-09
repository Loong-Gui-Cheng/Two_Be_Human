using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Collections;
using UnityEngine.TextCore.Text;
using static SkillData;
using DG.Tweening;

public class CombatSystem : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private CombatUISystem combatUISystem;

    [Header("Data Input")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private List<EnemyData> enemyData;

    [Header("Prefabs")]
    [SerializeField] private Transform characterGroup;
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private Transform enemyGroup;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject actionDicePrefab;

    [Header("Tracker")]
    [SerializeField, CE_ReadOnly] private int turnCounter;
    [SerializeField, CE_ReadOnly] private int totalDice;
    [SerializeField, CE_ReadOnly] private List<Enemy> onFieldEnemies;
    private List<Character> characters = new();
    private readonly Queue<Enemy> enemies = new();

    [Header("Action Slots (Combat Start)")]
    private readonly List<ActionSlot> unopposedAttacks = new();
    private readonly List<ActionSlot> clashAttacks = new();
    private readonly List<ActionSlot> targetedAllies = new();
    [SerializeField, CE_ReadOnly] private List<ActionSlot> allAttacks = new();

    private const int MAX_DICE = 6;

    public void OnLoad(PlayerData playerData)
    {
        this.playerData = playerData;
    }
    private void Start()
    {
        if (playerData == null) return;

        turnCounter = 0;
        StartCoroutine(TurnAnimation(true));
    }

    private void InitialiseBattle()
    {
        for (int i = 0; i < playerData.combatCharacters.Count; i++)
        {
            GameObject characterGO = Instantiate(characterPrefab, characterGroup);
            if (characterGO.TryGetComponent(out Character character))
            {
                character.Initialise(playerData.combatCharacters[i]);
                characters.Add(character);
            }
        }
        for (int i = 0; i < enemyData.Count; i++)
        {
            GameObject enemyGO = Instantiate(enemyPrefab, enemyGroup);
            if (enemyGO.TryGetComponent(out Enemy enemy))
            {
                enemy.Initialise(enemyData[i]);
                enemies.Enqueue(enemy);
            }
        }
        for (int i = enemies.Count - 1; i >= 0 ; i--)
        {
            if (onFieldEnemies.Count >= 3) break;
            onFieldEnemies.Add(enemies.Dequeue());
        }

        totalDice = playerData.combatCharacters.Count;


        // Set-up user interface
        for (int i = 0; i < characters.Count; i++)
        {
            Character character = characters[i];
            character.hpUI.SetupUI(character);
        }
        for (int i = 0; i < onFieldEnemies.Count; i++)
        {
            Enemy enemy = onFieldEnemies[i];
            enemy.hpUI.SetupUI(enemy);
        }

        combatUISystem.InitialiseUI(ref characters);
    }
    private void TurnStart()
    {
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.FINGER_SNAP);

        ApplyDynamicFormation(CharacterToTransform(characters), 2f, new Vector2(-5f, 0.4f));
        ApplyDynamicFormation(EnemyToTransform(onFieldEnemies), 2f, new Vector2(5f, 0.4f), true);

        // Increment no. of player dices and turn.
        if (turnCounter > 1 && totalDice < MAX_DICE) totalDice += 1;

        // Add speed dices & randomize speed values.
        for (int i = 0; i < totalDice; i++)
        {
            int characterIndex = i % characters.Count;
            Character character = characters[characterIndex];
            GameObject diceSlot = Instantiate(actionDicePrefab, character.actionGroup);

            if (diceSlot.TryGetComponent(out ActionSlot actionSlot))
            {
                CharacterData data = character.GetData();
                actionSlot.SetSPD(Random.Range(data.minSPD, data.maxSPD + 1));
                actionSlot.SetID(character, i);

                actionSlot.button.onClick.AddListener(() => { combatUISystem.DisplayCharacter(actionSlot);});
                character.actions.Add(actionSlot);
                targetedAllies.Add(actionSlot);
            }
        }
        for (int i = 0; i < totalDice; i++)
        {
            int enemyIndex = i % onFieldEnemies.Count;
            Enemy enemy = onFieldEnemies[enemyIndex];
            GameObject diceSlot = Instantiate(actionDicePrefab, enemy.actionGroup);

            if (diceSlot.TryGetComponent(out ActionSlot actionSlot))
            {
                EnemyData data = enemy.GetData();
                actionSlot.SetSPD(Random.Range(data.minSPD, data.maxSPD + 1));
                actionSlot.SetID(enemy, i);

                enemy.actions.Add(actionSlot);
            }
        }



        // Update Combat UI System
        combatUISystem.ReceiveActionUI(ref characters, ref onFieldEnemies);
        combatUISystem.UpdateTurnCount(turnCounter);

        RandomEnemyAttack();
    }
    public void TurnProcess(ref List<ActionSlot> playerInput, ref List<ActionSlot> enemyInput)
    {
        // Sort all actions from highest to lowest speed.
        playerInput.Sort((a, b) => b.GetSPD().CompareTo(a.GetSPD()));
        enemyInput.Sort((a, b) => b.GetSPD().CompareTo(a.GetSPD()));

        // 1. Add enemy unopposed attacks, and clashable attacks.
        for (int i = 0; i < enemyInput.Count; i++)
        {
            // 1. No clashable attacks = unopposed attack
            ActionSlot enemySlot = enemyInput[i];
            List<ActionSlot> clashables = enemyInput[i].clashedBy;

            if (clashables == null || clashables.Count <= 0 && enemySlot.skillData != null)
            {
                unopposedAttacks.Add(enemyInput[i]);
                continue;
            }

            // 2. There are clashable attacks -> Pick the front element as clasher.
            // Remove the ally slot that clashes with enemy.
            ActionSlot allyClash = clashables[0];
            int allyIndex = FindActionIndex(allyClash.GetID(), ref playerInput);
            playerInput.RemoveAt(allyIndex);

            // 3. Remove all other clashable attacks from enemy, since it already has a clash.
            if (clashables.Count > 1)
            {
                for (int j = clashables.Count - 1; j >= 1; j--)
                {
                    ActionSlot unopposedClash = clashables[j];
                    unopposedClash.IsClashing = false;
                    clashables.RemoveAt(j);
                }
            }

            // 4. Append the faster clasher, for speed sorting later.
            if (allyClash.GetSPD() > enemySlot.GetSPD()) clashAttacks.Add(allyClash);
            else clashAttacks.Add(enemySlot);
        }

        // 2. Add ally unopposed attacks.
        for (int i = 0; i < playerInput.Count; i++)
        {
            if (playerInput[i].skillData != null)
                unopposedAttacks.Add(playerInput[i]);
        }


        // Merge both clashables and unopposed into one action list.
        // Sort both clashable attacks and unopposed attacks from highest to lowest speed.
        // Afterwards, use FIFO to pop the list.
        for (int i = 0; i < clashAttacks.Count; i++)
        {
            ActionSlot clashSlot = clashAttacks[i];
            allAttacks.Add(clashSlot);
        }
        for (int i = 0; i < unopposedAttacks.Count; i++)
        {
            ActionSlot unopposedSlot = unopposedAttacks[i];
            allAttacks.Add(unopposedSlot);
        }
        allAttacks.Sort((a, b) => b.GetSPD().CompareTo(a.GetSPD()));

        // Log all actions
        for (int i = 0; i < allAttacks.Count; i++)
        {
            ActionSlot action = allAttacks[i];
            string log;

            if (action.IsClashing)
                log = string.Format("Entity {0} clashed with Entity {1}!", action.GetID(), action.targetSlot.GetID());
            else
                log = string.Format("Entity {0} unopposes Entity {1}!", action.GetID(), action.targetSlot.GetID());

            Debug.Log(log);
        }


        // Hide Combat UI & Character Slot UI
        combatUISystem.ToggleUI();
        HideActionSlotUI();

        playerInput.Clear();
        enemyInput.Clear();

        // Start combat animation
        StartCoroutine(CombatRoutine());
    }
    public void TurnEnd()
    {
        targetedAllies.Clear();
        unopposedAttacks.Clear();
        clashAttacks.Clear();
        allAttacks.Clear();

        // Remove any dead characters and enemies from list.
        for (int i = characters.Count - 1; i >= 0; i--)
        {
            if (characters[i].entity.HP <= 0)
            {
                Destroy(characters[i].gameObject);
                characters.RemoveAt(i);
            }
        }
        for (int i = onFieldEnemies.Count - 1; i >= 0; i--)
        {
            if (onFieldEnemies[i].entity.HP <= 0)
            {
                Destroy(onFieldEnemies[i].gameObject);
                onFieldEnemies.RemoveAt(i);
            }
        }


        // Clear all existing characters and enemy action slots
        for (int i = 0; i < characters.Count; i++)
        {
            Character character = characters[i];

            for (int j = character.actions.Count - 1; j >= 0; j--)
                Destroy(character.actions[j].gameObject);

            character.actions.Clear();
        }
        for (int i = 0; i < onFieldEnemies.Count; i++)
        {
            Enemy enemy = onFieldEnemies[i];

            for (int j = enemy.actions.Count - 1; j >= 0; j--)
                Destroy(enemy.actions[j].gameObject);

            enemy.actions.Clear();
        }

        // If all characters dies => player lose.
        if (characters.Count <= 0)
        {
            StartCoroutine(LoseBattleAnimation());
            return;
        }


        // If all enemies dies => player wins.
        if (onFieldEnemies.Count <= 0 && enemies.Count <= 0)
        {
            StartCoroutine(WinBattleAnimation());
            return;
        }
 
        // Check if there are any back-up enemies. If so, bring them onto field.
        if (onFieldEnemies.Count < 3)
        {
            if (enemies.Count > 0)
                onFieldEnemies.Add(enemies.Dequeue());
        }

        StartCoroutine(TurnAnimation());
        Debug.Log("[Combat System] Turn End!");
    }


    // Enemy AI Functions
    #region Enemy AI
    private void RandomEnemyAttack()
    {
        int guaranteedATK = 1;

        // Loop through all enemies, and randomise skill selection for every action slot.
        for (int i = 0; i < onFieldEnemies.Count; i++)
        {
            Enemy enemy = onFieldEnemies[i];
            EnemyData data = enemy.GetData();

            for (int j = 0; j < enemy.actions.Count; j++)
            {
                ActionSlot slot = enemy.actions[j];
                if (guaranteedATK > 0)
                {
                    EnemyAIRandom(slot, data);
                    guaranteedATK--;
                    continue;
                }
                int randAttack = Random.Range(0, 100);
                if (randAttack <= 49)
                {
                    EnemyAIRandom(slot, data);
                }
            }
        }
    }
    private void EnemyAIRandom(ActionSlot chosenSlot, EnemyData data)
    {
        int randSkill = Random.Range(0, data.skills.Count);
        SkillData skill = data.skills[randSkill];

        int randTarget = Random.Range(0, targetedAllies.Count);
        chosenSlot.SetAction(skill, targetedAllies[randTarget]);

        combatUISystem.AddEnemyAction(chosenSlot);
    }
    #endregion

    // Formations
    #region Team Formation Functions
    private List<Transform> CharacterToTransform(List<Character> characters)
    {
        List<Transform> transforms = new();
        for (int i = 0; i < characters.Count; i++) transforms.Add(characters[i].transform);
        return transforms;
    }
    private List<Transform> EnemyToTransform(List<Enemy> activeEnemies)
    {
        List<Transform> transforms = new();
        for (int i = 0; i < activeEnemies.Count; i++) transforms.Add(activeEnemies[i].transform);
        return transforms;
    }
    private void ApplyDynamicFormation(List<Transform> entities, float spacing, Vector2 origin, bool bFlip = false)
    {
        float flip = -1f;       // Left Spacing
        if (bFlip) flip = 1f;    // Right Spacing

        bool IsEvenFormation = entities.Count % 2 == 0;

        if (IsEvenFormation) SetEvenFormation(ref entities, spacing, origin, flip);
        else SetOddFormation(ref entities, spacing, origin, flip);
    }
    private void SetEvenFormation(ref List<Transform> entities, float spacing, Vector2 origin, float flip)
    {
        int count = entities.Count;
        int cols = Mathf.FloorToInt(count / 2);

        for (int i = 0; i < count; i++)
        {
            bool IsOdd = !(i % 2 == 0);

            float yOffset =  spacing;
            float xOffset = Mathf.Ceil(i / 2) * spacing;

            // Every even index = flip y value
            float finalY = yOffset;
            if (IsOdd) finalY *= -1;

            // Don't apply offset on first column
            float initialOffset = 2f;
            if (i < 2) initialOffset = 0f;

            Vector3 position = new(origin.x + (xOffset * flip) + (initialOffset * flip), origin.y + finalY, 0f);
            entities[i].transform.position = position;
        }
    }
    private void SetOddFormation(ref List<Transform> entities, float spacing, Vector2 origin, float flip)
    {
        int count = entities.Count;
        int cols = Mathf.FloorToInt(count / 2);

        int membersSplit = entities.Count - 1;
        if (membersSplit != 0) membersSplit /= 2;

        // Leader (Entity)
        entities[0].transform.position = new(origin.x + (spacing * -flip * 0.5f), origin.y, 0f);

        // Arrow Formation
        if (membersSplit != 0)
        {
            for (int i = membersSplit; i < membersSplit + 1; i++)
            {
                float offset = i * spacing;

                Vector3 position = new(origin.x + (offset * flip), origin.y + offset, 0f);
                entities[i].transform.position = position;
            }
            for (int i = membersSplit + 1, j = 1; i < count; i++, j++)
            {
                float offset = j * spacing;

                Vector3 position = new(origin.x + (offset * flip), origin.y - offset, 0f);
                entities[i].transform.position = position;
            }
        }
    }
    #endregion

    // Animations
    #region Animation Functions
    public void TestAnimation()
    {
        StartCoroutine(LoseBattleAnimation());
    }
    private IEnumerator TurnAnimation(bool initialisation = false)
    {
        yield return combatUISystem.GetBattleStartDuration();
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.TURN_START);
        turnCounter += 1;

        combatUISystem.AnimateTurnUI(turnCounter);
        yield return combatUISystem.GetTurnUIDuration();

        if (initialisation) InitialiseBattle();

        TurnStart();
        combatUISystem.ToggleUI();
        yield break;
    }
    private IEnumerator WinBattleAnimation()
    {
        for (int i = 0; i < characters.Count; i++)
            characters[i].hpUI.ToggleUI();

        yield return new WaitForSeconds(1f);

        combatUISystem.AnimateResultUI(true);
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.ROUND_WIN);
        yield break;
    }
    private IEnumerator LoseBattleAnimation()
    {
        for (int i = 0; i < onFieldEnemies.Count; i++)
            onFieldEnemies[i].hpUI.ToggleUI();

        yield return new WaitForSeconds(1f);

        combatUISystem.AnimateResultUI(false);
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.ROUND_LOSE);
        yield break;
    }
    private IEnumerator CombatRoutine()
    {
        Character character = null;
        Enemy enemy = null;
        ActionSlot characterAction = null;
        ActionSlot enemyAction = null;

        //A: No actions remaining, end turn.
        if (allAttacks.Count <= 0)
        {
            TurnEnd();
            yield break;
        }

        //B: There are still actions remaining.
        ActionSlot action = allAttacks[0];
        allAttacks.RemoveAt(0);

        //B1. Set references for animation later
        AssignCharacterEnemyReference(action, ref character, ref enemy, ref characterAction, ref enemyAction);

        //B2. Check if action target is invalid.
        if (character.entity.HP <= 0 || enemy.entity.HP <= 0)
        {
            StartCoroutine(CombatRoutine());
            yield break;
        }


        //C1. Clash Attacks
        if (action.IsClashing)
        {
            character.coinUI.SetupUI(characterAction.skillData);
            enemy.coinUI.SetupUI(enemyAction.skillData);

            StartCoroutine(ClashAnimation(character, enemy, characterAction, enemyAction));
        }
        //C2. Unopposed Attacks
        else
        {
            // This action belongs to a player.
            if (action.character != null)
            {
                character.coinUI.SetupUI(characterAction.skillData);
                StartCoroutine(AttackAnimation(character.entity, enemy.entity, characterAction, characterAction.skillData.coins));
            }
            // This action belongs to an enemy.
            else
            {
                enemy.coinUI.SetupUI(enemyAction.skillData);
                StartCoroutine(AttackAnimation(enemy.entity, character.entity, enemyAction, enemyAction.skillData.coins));
            }
        }
        yield break;
    }
    private IEnumerator ClashAnimation(Character character, Enemy enemy, ActionSlot characterAction, ActionSlot enemyAction)
    {
        if (character == null || enemy == null) yield break;
        if (characterAction == null || enemyAction == null) yield break;

        //1.  Set-up coin flip variables
        SkillData characterSkill = characterAction.skillData;
        int characterCoins = character.coinUI.coins.Count;
        int cHeads = 0;
        int cTails = 0;

        SkillData enemySkill = enemyAction.skillData;
        int enemyCoins = enemy.coinUI.coins.Count;
        int eHeads = 0;
        int eTails = 0;

        int characterPower = characterSkill.baseCoinPower;
        int enemyPower = enemySkill.baseCoinPower;


        //2. Flip character and enemy coins, (80% heads 20% tails)
        for (int i = 0; i < characterCoins; i++)
        {
            int randAccuracy = Random.Range(0, 100);
            bool IsHeads = randAccuracy >= 79;

            if (IsHeads) cHeads++;
            else cTails++;
        }
        for (int i = 0; i < enemyCoins; i++)
        {
            int randAccuracy = Random.Range(0, 100);
            bool IsHeads = randAccuracy >= 79;

            if (IsHeads) eHeads++;
            else eTails++;
        }


        //3. Add up character and enemy coin power
        int cIncrement = characterSkill.incrementCoinPower * cHeads;
        characterPower += cIncrement;

        int eIncrement = enemySkill.incrementCoinPower * eHeads;
        enemyPower += eIncrement;

        character.coinUI.AnimateCoinTossUI(cHeads, cTails, characterSkill.baseCoinPower, characterSkill.incrementCoinPower);
        enemy.coinUI.AnimateCoinTossUI(eHeads, eTails, enemySkill.baseCoinPower, enemySkill.incrementCoinPower);

        yield return character.coinUI.GetCoinTossDuration();



        //4. Break the loser coin (if successful)
        character.entity.AnimateCharacter(CombatEntity.Animation_ID.ATTACK);
        enemy.entity.AnimateCharacter(CombatEntity.Animation_ID.ATTACK);

        if (characterPower > enemyPower)
        {
            character.entity.SpawnVFX(CombatEntity.VFX_ID.PARRY);
            AudioController.Instance.PlayUI(AudioController.SOUND_ID.PARRY_WIN);

            enemyCoins -= 1;
            enemy.coinUI.AnimateCoinBreakUI();
        }
        else if (characterPower < enemyPower)
        {
            enemy.entity.SpawnVFX(CombatEntity.VFX_ID.PARRY);
            AudioController.Instance.PlayUI(AudioController.SOUND_ID.PARRY_WIN);

            characterCoins -= 1;
            character.coinUI.AnimateCoinBreakUI();
        }
        else
        {
            character.entity.SpawnVFX(CombatEntity.VFX_ID.PARRY);
            enemy.entity.SpawnVFX(CombatEntity.VFX_ID.PARRY);
            AudioController.Instance.PlayUI(AudioController.SOUND_ID.PARRY_DRAW);
        }
        yield return character.coinUI.GetCoinBreakDuration();


        //5. Clash again, if both sides still have coins.
        if (characterCoins != 0 && enemyCoins != 0)
        {
            StartCoroutine(ClashAnimation(character, enemy, characterAction, enemyAction));
            yield break;
        }


        //6. Winner of the clash gets to attack their opponent.
        character.coinUI.ResetSkillUI(characterAction.skillData.baseCoinPower);
        enemy.coinUI.ResetSkillUI(enemyAction.skillData.baseCoinPower);

        if (characterCoins > 0)
        {
            enemy.coinUI.ToggleUI();
            enemy.coinUI.ResetCoinUI();
            StartCoroutine(AttackAnimation(character.entity, enemy.entity, characterAction, characterCoins));
        }
        else
        {
            character.coinUI.ToggleUI();
            character.coinUI.ResetCoinUI();
            StartCoroutine(AttackAnimation(enemy.entity, character.entity, enemyAction, enemyCoins));
        }
        yield break;
    }
    private IEnumerator AttackAnimation(CombatEntity host, CombatEntity target, ActionSlot hostAction, int coins)
    {
        float attackInterval = host.coinUI.CalculateCoinDuration(coins);
        WaitForSeconds delay = new(attackInterval / 3f);

        int coinPower = hostAction.skillData.baseCoinPower;
        int incrementPower = hostAction.skillData.incrementCoinPower;
        RESISTANCE_TYPE resistance_type = hostAction.skillData.resistance;

        for (int i = 0; i < coins; i++)
        {
            // target died during one of coin attacks.
            if (target.HP <= 0) break;

            // Animate Coin Attack Toss.
            int randAccuracy = Random.Range(0, 100);
            bool IsHeads = randAccuracy >= 79;
            host.coinUI.AnimateCoinAttackTossUI(coinPower, incrementPower, i, IsHeads);
            yield return delay;

            host.AnimateCharacter(CombatEntity.Animation_ID.ATTACK);

            // Calculate attack result.
            if (IsHeads) coinPower += incrementPower;

            float fDamage = coinPower;
            float damageMultiplier = 1f;
            float resist = 0f;

            if (IsHeads) damageMultiplier *= 1.15f; // Multiply damage by 15% if heads.

            switch (resistance_type) // Multiply damage by target resistance
            {
                case RESISTANCE_TYPE.SLASH:
                    damageMultiplier *= target.slashResist;
                    resist = target.slashResist;
                    break;

                case RESISTANCE_TYPE.PIERCE:
                    damageMultiplier *= target.pierceResist;
                    resist = target.pierceResist;
                    break;

                case RESISTANCE_TYPE.BLUNT:
                    damageMultiplier *= target.bluntResist;
                    resist = target.bluntResist;
                    break;

                case RESISTANCE_TYPE.MAGIC:
                    damageMultiplier *= target.magicResist;
                    resist = target.magicResist;
                    break;
            }

            bool IsCrit = false;
            int randCrit = Random.Range(0, 100);
            float fCrit = host.criticalChance;

            // Increase crit chance by 50% if exploiting weakness. 
            if (resist > 1f) fCrit *= 1.5f;
            int crit = Mathf.RoundToInt(fCrit * 100f);

            // Random crit falls within eg. (1 - 49) crit zone, is critical hit.
            // Multiply damage by 200% on crit.
            if (randCrit <= crit)
            {
                IsCrit = true;
                damageMultiplier *= 2f;
            }

            // Add in damage multipliers and calculate new target hp.
            // Set damage to zero if its negative to prevent logic errors.
            fDamage *= damageMultiplier;
            int damage = Mathf.RoundToInt(fDamage);
            if (damage < 0) damage = 0;

            int targetNewHP = Mathf.RoundToInt(target.HP - damage);

            // Display damage indicator & character hp slider animation.
            target.hpUI.AnimateHPSlider(target.HP, targetNewHP);
            target.AnimateDamageUI(hostAction.skillData, damageMultiplier, resist, IsCrit, damage);
            target.AnimateCharacter(CombatEntity.Animation_ID.HIT);
            target.HP = targetNewHP;

            switch (resistance_type)
            {
                case RESISTANCE_TYPE.SLASH:
                    AudioController.Instance.PlayUI(AudioController.SOUND_ID.SLASH);
                    break;

                case RESISTANCE_TYPE.BLUNT:
                    AudioController.Instance.PlayUI(AudioController.SOUND_ID.BLUNT);
                    break;

                case RESISTANCE_TYPE.PIERCE:
                    AudioController.Instance.PlayUI(AudioController.SOUND_ID.PIERCE);
                    break;

                case RESISTANCE_TYPE.MAGIC:
                    AudioController.Instance.PlayUI(AudioController.SOUND_ID.MAGIC);
                    break;
            }
            if (IsCrit)
            {
                AudioController.Instance.PlayUI(AudioController.SOUND_ID.CRITICAL);
            }

            yield return delay;
        }

        // Host attack ends 
        host.coinUI.ToggleUI();
        host.coinUI.ResetCoinUI();

        // Show target dying animation & sound
        if (target.HP <= 0)
        {
            target.HP = 0;
            target.hpUI.ToggleUI();
            target.SpawnVFX(CombatEntity.VFX_ID.BLOOD);
            AudioController.Instance.PlayUI(AudioController.SOUND_ID.DEAD);
        }

        yield return new WaitForSeconds(0.5f);

        if (target.HP <= 0)
        {
            target.gameObject.SetActive(false);
        }

        StartCoroutine(CombatRoutine());
        yield break;
    }
    #endregion


    private int FindActionIndex(string id, ref List<ActionSlot> actions)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            ActionSlot playerAction = actions[i];
            string actionCurrent = playerAction.GetID();

            if (string.Compare(id, actionCurrent, System.StringComparison.Ordinal) == 0)
                return i;
        }
        return -1;
    }
    private void AssignCharacterEnemyReference(ActionSlot action,
        ref Character character, ref Enemy enemy,
        ref ActionSlot characterAction, ref ActionSlot enemyAction)
    {
        // If this action does not belong to a character, then targeted slot is character.
        // Else, the action its targeting is enemy.
        if (action.character != null)
        {
            characterAction = action;
            enemyAction = action.targetSlot;
        }
        else
        {
            characterAction = action.targetSlot;
            enemyAction = action;
        }

        character = characterAction.character;
        enemy = enemyAction.enemy;
    }
    private void HideActionSlotUI()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            Character character = characters[i];
            List<ActionSlot> actions = character.actions;

            for (int j = 0; j < actions.Count; j++)
                actions[j].canvas.enabled = false;
        }
        for (int i = 0; i < onFieldEnemies.Count; i++)
        {
            Enemy enemy = onFieldEnemies[i];
            List<ActionSlot> actions = enemy.actions;

            for (int j = 0; j < actions.Count; j++)
                actions[j].canvas.enabled = false;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CombatSystem))]
public class CombatSystemEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        CombatSystem combatSystem = (CombatSystem)target;

        // Executes whenever values in inspector changes.
        if (DrawDefaultInspector())
        {
        }

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Runtime Controls", EditorStyles.boldLabel, GUILayout.MaxWidth(200f));

        if (GUILayout.Button("Turn End", GUILayout.Width(200f), GUILayout.Height(30f)))
            combatSystem.TurnEnd();

        if (GUILayout.Button("Test Result", GUILayout.Width(200f), GUILayout.Height(30f)))
            combatSystem.TestAnimation();

        GUILayout.EndVertical();
    }
}
#endif