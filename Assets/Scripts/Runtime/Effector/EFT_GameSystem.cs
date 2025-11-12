using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EFT_GameSystem : MonoBehaviour, IEffector
{
    private enum ID
    {
    }

    [Header("IEffector Settings")]
    [SerializeField] private ID functionID;

    public void IEffectorExecute()
    {
    }

    public void IEffectorExit()
    {
    }

    public void IDialogueExecute()
    {
        IEffectorExecute();
    }
}
