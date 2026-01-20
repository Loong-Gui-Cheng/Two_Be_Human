using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DBind_Teleport : MonoBehaviour, IDialogueBind, IEffector
{
    [Header("IDialogue Bind Settings")]
    [SerializeField] private Transform select;
    [SerializeField] private bool UnParent;
    [SerializeField] private bool IsCoordinates;
    [SerializeField] private bool hasNavMesh;

    [Header("Behaviour")]
    [ConditionalHide("IsCoordinates", true)]
    [SerializeField] private Vector3 destinationCoords;

    [ConditionalHide("IsCoordinates", true, true)]
    [SerializeField] private Transform destination;

    [ConditionalHide("UnParent", true, true)]
    [SerializeField] private Transform parent;

    public void IEffectorExecute()
    {
        IDialogueExecute();
    }
    public void IEffectorExit()
    {
    }
    public void IDialogueExecute()
    {
        if (select == null) return;

        if (UnParent) select.SetParent(null);
        else if (parent != null) select.SetParent(parent);

        if (hasNavMesh)
        {
            StartCoroutine(NavMeshRoutine());
        }
        else if (destination != null)
        {
            destinationCoords = destination.position;
            select.rotation = destination.rotation;
            select.position = new Vector3(destinationCoords.x, destinationCoords.y, destinationCoords.z);
        }
    }
    private void OnValidate()
    {
        if (destination != null)
            destinationCoords = destination.position;
    }

    private IEnumerator NavMeshRoutine()
    {
        if (!select.TryGetComponent(out NavMeshAgent agent))
            yield break;

        agent.SetDestination(destinationCoords);
        agent.isStopped = false;

        while (!agent.isStopped)
        {
            yield return null;
        }

        select.DOLocalRotate(destination.rotation.eulerAngles, 0.5f, RotateMode.Fast);
        yield break;
    }
}