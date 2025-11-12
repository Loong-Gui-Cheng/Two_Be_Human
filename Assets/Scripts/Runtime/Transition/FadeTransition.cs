using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SceneTransitionManager;

/*********************************************************************************
Written by: Loong Gui Cheng
Description: This class handles the fade transition effect.

Class relation:
Inherits from ITransition interface to handle custom transition effect.

NOTE: My apologies if some of the code here are confusing. 
*********************************************************************************/
[RequireComponent(typeof(Image))]
public class FadeTransition : MonoBehaviour, ITransition
{
    private TransitionData data;
    [SerializeField] private List<Image> backgrounds;

    private void OnEnable()
    {
        OnSceneTransitionIn += TransitionInToScene;
        EFT_SceneTransition.OnSceneTransitionOut += TransitionOutToScene;

        EFT_SceneTransition.OnTransitionOut += TransitionOutSameScene;
    }
    private void OnDisable()
    {
        OnSceneTransitionIn -= TransitionInToScene;
        EFT_SceneTransition.OnSceneTransitionOut -= TransitionOutToScene;

        EFT_SceneTransition.OnTransitionOut -= TransitionOutSameScene;
    }
 

    public void TransitionInToScene(TransitionData data)
    {
        // From SceneTransitionManager, OnSceneTransitionIn.
        if (data.type != TRANSITION_TYPE.FADE) return;

        this.data = data;
        In();
    }
    public void TransitionOutToScene(SCENE_NAME scene, TransitionData data)
    {
        // From TriggerSceneTransition, OnSceneTransitionOut.
        // Check if data received matches this transition type.
        if (data.type != TRANSITION_TYPE.FADE) return;

        this.data = data;
        SceneTransitionManager.Instance.TransitionOutToScene(scene, this, data);
    }

    public void TransitionOutSameScene(Vector3 sceneLocation, Vector3 sceneOrientation, TransitionData data)
    {
        // From SceneTransitionManager, OnTransitionOut.
        if (data.type != TRANSITION_TYPE.FADE) return;

        this.data = data;
        SceneTransitionManager.Instance.TransitionOutToLocation(sceneLocation, sceneOrientation, this, data);
    }

    public void In() => Fade(data.InColorStart, data.InColorEnd, true);
    public void Out() => Fade(data.OutColorStart, data.OutColorEnd, false);
    public void SetData(ref TransitionData data) => this.data = data;
    private void Fade(Color colorStart, Color colorEnd, bool bufferOnStart) => StartCoroutine(FadeRoutine(colorStart, colorEnd, bufferOnStart));
    private IEnumerator FadeRoutine(Color colorStart, Color colorEnd, bool bufferOnStart)
    {
        float bufferDuration = data.bufferTime;
        float transitionDuration = data.duration;

        for (int i = 0; i < backgrounds.Count; i++)
            backgrounds[i].color = colorStart;

        // (On Transition In)
        if (bufferOnStart)
        {
            yield return new WaitForSeconds(bufferDuration);

            for (int i = 0; i < backgrounds.Count; i++)
                backgrounds[i].DOColor(colorEnd, transitionDuration);

            yield return new WaitForSeconds(transitionDuration);
        }
        // (On Transition Out)
        else
        {
            for (int i = 0; i < backgrounds.Count; i++)
                backgrounds[i].DOColor(colorEnd, transitionDuration);

            yield return new WaitForSeconds(transitionDuration);

            yield return new WaitForSeconds(bufferDuration);
        }

        for (int i = 0; i < backgrounds.Count; i++)
            backgrounds[i].DOKill(true);

        for (int i = 0; i < backgrounds.Count; i++)
            backgrounds[i].color = new(colorEnd.r, colorEnd.g, colorEnd.b, colorEnd.a);

        yield break;
    }
}
