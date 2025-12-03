using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CombatEntity : MonoBehaviour
{
    public enum AnimationID
    {
        MOVE = 0,
        ATTACK = 1,
        HIT = 2
    }

    [Header("Runtime (Stat)")]
    public float MaxHP;
    public float HP;
    public float ATK;
    public float DEF;
    public float criticalChance = 0.1f;

    [Header("Runtime (Resistances)")]
    public float slashResist;
    public float bluntResist;
    public float pierceResist;
    public float magicResist;

    [Header("User Interface (UI)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private RectTransform damageGroup;
    [SerializeField] private GameObject damageUIPrefab;
    public HPUI hpUI;
    public CoinUI coinUI;

    [Header("Animations")]
    [SerializeField] private Animator animator;
    private int MoveHashID;
    private int AttackHashID;
    private int HitHashID;


    private void Start()
    {
        MoveHashID = Animator.StringToHash("Move");
        AttackHashID = Animator.StringToHash("Attack");
        HitHashID = Animator.StringToHash("Hit");
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    public void Initialise(EnemyData input)
    {
        if (input == null) return;

        MaxHP = input.MaxHP;
        HP = input.HP;
        MaxHP = input.MaxHP;
        ATK = input.ATK;
        DEF = input.DEF;

        slashResist = input.slashResist;
        bluntResist = input.bluntResist;
        pierceResist = input.pierceResist;
        magicResist = input.magicResist;
        criticalChance = 0.1f;
    }
    public void Initialise(CharacterData input)
    {
        if (input == null) return;

        MaxHP = input.MaxHP;
        HP = input.HP;
        MaxHP = input.MaxHP;
        ATK = input.ATK;
        DEF = input.DEF;

        slashResist = input.slashResist;
        bluntResist = input.bluntResist;
        pierceResist = input.pierceResist;
        magicResist = input.magicResist;
        criticalChance = 0.1f;
    }

    public void AnimateCharacter(AnimationID id)
    {
        switch (id)
        {
            case AnimationID.MOVE:
                animator.Play(MoveHashID);
                break;

            case AnimationID.ATTACK:
                animator.Play(AttackHashID);
                break;

            case AnimationID.HIT:
                animator.Play(HitHashID);
                break;
        }
    }

    public void AnimateDamageUI(SkillData input, float damage_multiplier, float weakness_multiplier, bool IsCrit, int damage) 
        => StartCoroutine(DamagedAnimation(input, damage_multiplier, weakness_multiplier, IsCrit, damage));
    private IEnumerator DamagedAnimation(SkillData input, float total_multiplier, float weakness_multiplier, bool IsCrit, int damage)
    {
        GameObject damageUIGO = Instantiate(damageUIPrefab, damageGroup);

        if (damageUIGO.TryGetComponent(out DamageUI damageUI))
            damageUI.SetupUI(input, total_multiplier, weakness_multiplier, IsCrit, damage);

        float duration = damageUI.GetIndicatorDuration();
        float half = duration / 2f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(Color.white, Color.red, t / half);
            yield return null;
        }
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(Color.red, Color.white, t / half);
            yield return null;
        }

        // Ensure final color is set
        spriteRenderer.color = Color.white;
        yield break;
    }
}