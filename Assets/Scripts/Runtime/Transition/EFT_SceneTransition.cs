using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*********************************************************************************
Written by: Loong Gui Cheng
Description: This class triggers/activates the transition system based on conditions met (if there are any).
This can be done via collision (Tag checking) / button press.

Class relation:
Sends over Transition data to Scene Transition Manager to handle transition logic. 

NOTE: My apologies if some of the code here are confusing. 
*********************************************************************************/
public class EFT_SceneTransition : MonoBehaviour, IEffector
{
    [Header("Transition Settings")]
    [SerializeField] private TransitionData transitionData;
    [Tooltip("Toggle transition to new scene.")]
    public bool isTransitToScene = false;

    [Header("Behaviour")]
    [ConditionalHide("isTransitToScene", true)]
    [Tooltip("Transition to a different scene.")]
    [SerializeField] private SceneTransitionManager.SCENE_NAME sceneToLoad;

    [ConditionalHide("isTransitToScene", true, true)]
    [Tooltip("Player starting coordinates within same scene.")]
    [SerializeField] private Vector3 sceneCoordinates;
    [ConditionalHide("isTransitToScene", true, true)]
    [Tooltip("Player starting orientation within same scene.")]
    [SerializeField] private Vector3 sceneOrientation;


    public static event System.Action<SceneTransitionManager.SCENE_NAME, TransitionData> OnSceneTransitionOut;
    public static event System.Action<Vector3, Vector3, TransitionData> OnTransitionOut;

    public void EnterScene()
    {
        if (SceneTransitionManager.Instance == null) return;
        if (IsSceneLoading()) return;
        if (IsSceneSame()) return;

        // Dispatch transition data to all transitions currently in scene.
        OnSceneTransitionOut?.Invoke(sceneToLoad, transitionData);
    }
    public void EnterLocation()
    {
        if (SceneTransitionManager.Instance == null) return;
        if (IsSceneLoading()) return;

        // Dispatch transition data to all transitions currently in scene.
        OnTransitionOut?.Invoke(sceneCoordinates, sceneOrientation, transitionData);
    }

    public void IEffectorExecute()
    {
        if (!enabled) return;

        if (isTransitToScene) EnterScene();
        else EnterLocation();
    }

    public void IEffectorExit()
    {
        throw new System.NotImplementedException();
    }


    public TransitionData GetData()
    {
        return transitionData;
    }
    public void SetData(TransitionData data) => transitionData = data;
    public void SetSceneDestination(SceneTransitionManager.SCENE_NAME sceneName) => sceneToLoad = sceneName;
    public void SetSceneLocation(Vector3 coordinates) => sceneCoordinates = coordinates;
    public void SetSceneOrientation(Vector3 orientation) => sceneOrientation = orientation;

    private bool IsSceneLoading()
    {
        if (SceneTransitionManager.Instance.IsCurrentlyLoading())
        {
            return true;
        }
        return false;
    }
    private bool IsSceneSame()
    {
        if (SceneTransitionManager.Instance.IsSameScene(sceneToLoad))
        {
            return true;
        }
        return false;
    }

    public void OnValidate()
    {
        if (transitionData.duration < 0)
            transitionData.duration = 0;

        if (transitionData.bufferTime < 0)
            transitionData.bufferTime = 0;
    }
}
