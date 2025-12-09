using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*********************************************************************************
Written by: Loong Gui Cheng
Description: This class manages the transition system logic between scenes / positions.
These include teleporting between scenes or to an area in a scene.

Class relation:
Manipulates Transition Data struct inside this file.

Uses Trigger Scene Transition script as an entry/activation point.
ITranistion interface class to activate effect logic in transition scripts. (via System.Action)

NOTE: My apologies if some of the code here are confusing. 
*********************************************************************************/

/// <summary>
/// Manages the transition logic between scenes or to an area in a scene.
/// </summary>
public class SceneTransitionManager : SingletonPersistent<SceneTransitionManager>
{
    public enum TRANSITION_TYPE
    {
        FADE = 0,
        WIPE,
        EASE,
        BOUNCE,
    }
    public enum SCENE_NAME
    {
        SCENE_MENU = 0,
        SCENE_DREAM = 1,
        SCENE_REALITY = 2,
        SCENE_COMBAT = 3,
    }

    [Header("Transition Settings")]
    [SerializeField] private TransitionData m_Initialize;
    [SerializeField] private LayerMask m_TransitionMask;
    [SerializeField] private LayerMask m_InitialMask;

    private TransitionData data;
    private ITransition transition;
    private bool IsLoading = false;

    private readonly Dictionary<SCENE_NAME, string> m_Scenes = new();
    public static event System.Action<TransitionData> OnSceneTransitionIn;

    private void Start()
    {
        // Load in transition every time scene loads. 
        SceneManager.sceneLoaded += OnLoadCallback;
        OnSceneTransitionIn?.Invoke(m_Initialize);

        // Add scenes to the available entries.
        m_Scenes.Add(SCENE_NAME.SCENE_MENU, "SceneMenu");
        m_Scenes.Add(SCENE_NAME.SCENE_DREAM, "SceneDream");
        m_Scenes.Add(SCENE_NAME.SCENE_REALITY, "SceneReality");
        m_Scenes.Add(SCENE_NAME.SCENE_COMBAT, "SceneCombat");
    }

    private void OnDisable() 
        => SceneManager.sceneLoaded -= OnLoadCallback;

    private void OnLoadCallback(Scene scene, LoadSceneMode sceneMode) 
        => OnSceneTransitionIn?.Invoke(data);

    public void TransitionOutToScene(SCENE_NAME scene, ITransition screen, TransitionData data)
    {
        // If a transition is already in progress, return immediately.
        if (IsLoading) return;
        if (!m_Scenes.TryGetValue(scene, out string sceneName)) return;

        transition = screen;
        this.data = data;

        IsLoading = true;
        StartCoroutine(GoToSceneAsyncRoutine(sceneName, transition));
    }
    public void TransitionOutToLocation(Vector3 sceneLocation, Vector3 sceneOrientation, ITransition screen, TransitionData data)
    {
        // If a transition is already in progress, return immediately.
        if (IsLoading) return;

        transition = screen;
        this.data = data;

        IsLoading = true;
        StartCoroutine(GoToLocationAsyncRoutine(sceneLocation, sceneOrientation, transition));
    }

    private IEnumerator GoToSceneAsyncRoutine(string sceneName, ITransition transition)
    {
        transition.Out();

        // Launch the new scene
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float totalDuration = data.duration + data.bufferTime;
        StartCoroutine(StopClipping());

        // Only enter scene when transition is finished and is fully loaded.
        yield return new WaitForSeconds(totalDuration);

        // Clear all resources and allow scene to be entered.
        DOTween.Clear(true);
        operation.allowSceneActivation = true;
        IsLoading = false;
        yield break;
    }
    private IEnumerator GoToLocationAsyncRoutine(Vector3 sceneLocation, Vector3 sceneOrientation, ITransition transition)
    {
        transition.Out();

        float totalDuration = data.duration + data.bufferTime;
        StartCoroutine(StopClipping());

        // Only teleport to location when transition is finished.
        yield return new WaitForSeconds(totalDuration);

        // Clear all resources and allow location to be teleported.
        DOTween.Clear(true);
        IsLoading = false;

        //Transform player = FindAnyObjectByType<OVRCameraRig>(FindObjectsInactive.Include).gameObject.transform;
        //player.localPosition = new Vector3(sceneLocation.x, sceneLocation.y, sceneLocation.z);
        //player.localRotation = Quaternion.identity;
        //player.localRotation = Quaternion.Euler(sceneOrientation.x, sceneOrientation.y, sceneOrientation.z);

        transition.In();
        StartCoroutine(StartClipping());
        yield break;
    }

    private IEnumerator StartClipping()
    {
        yield return new WaitForSeconds(data.duration);
        Camera.main.cullingMask = m_InitialMask;
        yield break;
    }
    private IEnumerator StopClipping()
    {
        yield return new WaitForSeconds(data.duration * 0.95f);
        Camera.main.cullingMask = m_TransitionMask;
        yield break;
    }

    public bool IsCurrentlyLoading()
    { return IsLoading; }

    public bool IsSameScene(SCENE_NAME sceneToLoad)
    {
        if (!m_Scenes.TryGetValue(sceneToLoad, out string sceneName)) return true;

        string activeScene = SceneManager.GetActiveScene().name;
        return string.Compare(sceneName, activeScene) == 0;
    }
}

[System.Serializable]
public struct TransitionData
{
    public SceneTransitionManager.TRANSITION_TYPE type;
    public float duration;
    public float bufferTime;
    public Color InColorStart;
    public Color InColorEnd;
    public Color OutColorStart;
    public Color OutColorEnd;
}

public interface ITransition
{
    void SetData(ref TransitionData data);
    void TransitionInToScene(TransitionData data);
    void TransitionOutToScene(SceneTransitionManager.SCENE_NAME scene, TransitionData data);
    void TransitionOutSameScene(Vector3 sceneLocation, Vector3 sceneOrientation, TransitionData data);
    void In();
    void Out();
}