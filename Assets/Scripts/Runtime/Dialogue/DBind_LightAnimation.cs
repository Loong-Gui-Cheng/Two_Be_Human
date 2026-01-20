using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LightAnimation))]
public class DBind_LightAnimation : MonoBehaviour, IDialogueBind
{
    [Header("IDialogue Bind Settings")]
    [SerializeField] private LightAnimation lightAnim;

    public void IDialogueExecute()
    {
        if (lightAnim == null) return;
        lightAnim.StartAnimating();
    }

    private void OnValidate()
    {
        if (lightAnim == null)
        {
            if (TryGetComponent(out LightAnimation lightAnimator))
                lightAnim = lightAnimator;
        }
    }
}
