using System.Collections;
using UnityEngine;

/*********************************************************************************
Written by: Loong Gui Cheng
Description: This generic class triggers IEffector scripts (EFT) attached to this object based on collision tags met.

Class relation:
Triggers IEffector scripts (ETF) to activate a custom script effect without needing to repeat trigger boilerplate code.

NOTE: My apologies if some of the code here are confusing. 
*********************************************************************************/

public class Trigger : MonoBehaviour
{
    [Header("Collision Triggers")]
    [Tooltip("Tags that activate this dialogue start option.")]
    [SerializeField] private bool triggerByPlayer = true;
    [SerializeField] private string[] triggerTags;

    [Header("Behaviour")]
    [Tooltip("Delay before trigger fires")]
    [SerializeField] private float delay = 0f;
    [SerializeField] private bool TriggerEnter = true;
    [SerializeField] private bool TriggerStay = true;

    [Tooltip("Make this GameObject disappear on trigger.")]
    [SerializeField] private bool onDisappear;
    [Tooltip("Disable this script when triggered.")]
    [SerializeField] private bool onDisable;
    [SerializeField] private bool onGizmos;

    private bool IsActive;

    // Shorthand: EFT
    private IEffector[] effectors;

    private void Start() => effectors = GetComponents<IEffector>();
    private void OnEnable()
    {
        if (delay <= 0f) IsActive = true;
        else StartCoroutine(DelayRoutine());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        IsActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        if (!TriggerEnter) return;
        if (!IsActive) return;

        if (SceneTransitionManager.Instance != null)
        {
            if (SceneTransitionManager.Instance.IsCurrentlyLoading())
                return;
        }

        // Dialogue Trigger By Tag
        bool IsPlayerCondition = triggerByPlayer && other.CompareTag("Player");

        if (IsPlayerCondition)
        {
            ExecuteEffectors();
            return;
        }

        if (TagsAreMatching(other))
            ExecuteEffectors();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!enabled) return;
        if (!TriggerStay) return;
        if (!IsActive) return;

        if (SceneTransitionManager.Instance != null)
        {
            if (SceneTransitionManager.Instance.IsCurrentlyLoading())
                return;
        }

        // Dialogue Trigger By Tag
        bool IsPlayerCondition = triggerByPlayer && other.CompareTag("Player");

        if (IsPlayerCondition)
        {
            ExecuteEffectors();
            return;
        }

        if (TagsAreMatching(other))
            ExecuteEffectors();
    }

    private void ExecuteEffectors()
    {
        if (effectors == null) return;

        for (int i = 0; i < effectors.Length; i++)
        {
            if (effectors[i] == null) continue;
            effectors[i].IEffectorExecute();
        }

        if (onDisappear)
            gameObject.SetActive(false);

        if (onDisable)
            enabled = false;
    }
    private bool TagsAreMatching(Collider other)
    {
        for (int i = 0; i < triggerTags.Length; i++)
        {
            string tag = triggerTags[i];
            if (string.IsNullOrEmpty(tag)) continue;
            if (other.CompareTag(tag)) return true;
        }
        return false;
    }
    private IEnumerator DelayRoutine()
    {
        yield return new WaitForSeconds(delay);
        IsActive = true;
        yield break;
    }
    private void OnDrawGizmos()
    {
        if (!onGizmos) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}

/// <summary>
/// A script to execute customizable events on run-time, ran with Trigger script as entry point.
/// </summary>
public interface IEffector
{
    void IEffectorExecute();
    void IEffectorExit();
}
