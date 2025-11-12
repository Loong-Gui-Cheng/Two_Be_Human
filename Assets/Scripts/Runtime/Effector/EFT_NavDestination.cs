using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EFT_NavDestination : MonoBehaviour, IEffector
{
    public static event System.Action OnDestinationReached;

    public void IEffectorExecute()
    {
        OnDestinationReached?.Invoke();
        gameObject.SetActive(false);
    }

    public void IEffectorExit()
    {
    }
}