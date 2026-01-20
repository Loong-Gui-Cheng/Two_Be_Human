using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBind_TransitionScene : MonoBehaviour, IDialogueBind
{
    [Header("IDialogue Bind Settings")]
    [SerializeField] private EFT_SceneTransition transition;
    [SerializeField] private bool IsCoordinates;

    [Header("Behaviour")]
    [ConditionalHide("IsCoordinates", true)]
    [SerializeField] private Vector3 destinationCoords;
    [ConditionalHide("IsCoordinates", true)]
    [SerializeField] private Vector3 destinationRotation;

    [ConditionalHide("IsCoordinates", true, true)]
    [SerializeField] private Transform destination;


    public void IDialogueExecute()
    {
        if (transition == null) return;
        if (transition.isTransitToScene)
        {
            transition.EnterScene();
            return;
        }

        if (destination != null)
        {
            destinationCoords = destination.position;
            destinationRotation = destination.rotation.eulerAngles;
        }

        transition.SetSceneLocation(destinationCoords);
        transition.SetSceneOrientation(destinationRotation);
        transition.EnterLocation();
    }
    private void OnValidate()
    {
        TryGetComponent(out transition);

        if (destination != null)
        {
            destinationCoords = destination.position;
            destinationRotation = destination.rotation.eulerAngles;
        }
    }
}