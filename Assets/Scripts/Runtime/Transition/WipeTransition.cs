using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SceneTransitionManager;
using static Unity.VisualScripting.StickyNote;

/*********************************************************************************
Written by: Loong Gui Cheng
Description: This class handles the wipe transition effect.

Class relation:
Inherits from ITransition interface to handle custom transition effect.

NOTE: My apologies if some of the code here are confusing. 
*********************************************************************************/
[RequireComponent(typeof(Image))]
public class WipeTransition : MonoBehaviour, ITransition
{
    private TransitionData data;
    [SerializeField] private List<Image> backgrounds;

    [Header("Wipe In")]
    [SerializeField] private Vector3 wipeInStart;
    [SerializeField] private Vector3 wipeInEnd;

    [Header("Wipe Out")]
    [SerializeField] private Vector3 wipeOutStart;
    [SerializeField] private Vector3 wipeOutEnd;

    private void OnEnable()
    {
        for (int i = 0; i < backgrounds.Count; i++)
            backgrounds[i].color = Color.clear;

        OnSceneTransitionIn += TransitionInToScene;
        EFT_SceneTransition.OnSceneTransitionOut += TransitionOutToScene;

        EFT_SceneTransition.OnTransitionOut += TransitionOutSameScene;
    }
    private void OnDisable()
    {
        for (int i = 0; i < backgrounds.Count; i++)
            backgrounds[i].color = Color.clear;

        OnSceneTransitionIn -= TransitionInToScene;
        EFT_SceneTransition.OnSceneTransitionOut -= TransitionOutToScene;

        EFT_SceneTransition.OnTransitionOut -= TransitionOutSameScene;
    }

    public void TransitionInToScene(TransitionData data)
    {
        // From SceneTransitionManager, OnTransitionIn.
        if (data.type != TRANSITION_TYPE.WIPE) return;

        this.data = data;
        In();
    }

    public void TransitionOutToScene(SCENE_NAME scene, TransitionData data)
    {
        // From TriggerSceneTransition, OnTransitionOut.
        // Check if data received matches this transition type.
        if (data.type != TRANSITION_TYPE.WIPE) return;

        this.data = data;
        SceneTransitionManager.Instance.TransitionOutToScene(scene, this, data);
    }
    public void TransitionOutSameScene(Vector3 sceneLocation, Vector3 sceneOrientation, TransitionData data)
    {
        // From TriggerSceneTransition, OnTransitionOut.
        // Check if data received matches this transition type.
        if (data.type != TRANSITION_TYPE.WIPE) return;

        this.data = data;
        SceneTransitionManager.Instance.TransitionOutToLocation(sceneLocation, sceneOrientation, this, data);
    }

    public void In() => Wipe(wipeInStart, wipeInEnd, data.InColorStart, data.InColorEnd, true);
    public void Out() => Wipe(wipeOutStart, wipeOutEnd, data.OutColorStart, data.OutColorEnd, false);
    public void SetData(ref TransitionData data) => this.data = data;
    private void Wipe(Vector3 start, Vector3 end, Color colorStart, Color colorEnd, bool bufferOnStart) => StartCoroutine(WipeRoutine(start, end, colorStart, colorEnd, bufferOnStart));
    private IEnumerator WipeRoutine(Vector3 start, Vector3 end, Color colorStart, Color colorEnd, bool bufferOnStart)
    {
        float bufferDuration = data.bufferTime;
        float transitionDuration = data.duration;

        for (int i = 0; i < backgrounds.Count; i++)
        {
            backgrounds[i].color = colorStart;
            backgrounds[i].material.color = backgrounds[i].color;

            backgrounds[i].transform.localPosition = start;
        }

        // (On Transition In)
        if (bufferOnStart)
        {
            yield return new WaitForSeconds(bufferDuration);

            transform.DOLocalMove(end, transitionDuration, false);

            for (int i = 0; i < backgrounds.Count; i++)
                backgrounds[i].material.DOColor(colorEnd, transitionDuration);

            yield return new WaitForSeconds(transitionDuration);
        }
        // (On Transition Out)
        else
        {
            transform.DOLocalMove(end, transitionDuration, false);

            for (int i = 0; i < backgrounds.Count; i++)
            {
                backgrounds[i].DOColor(colorEnd, transitionDuration);
                backgrounds[i].material.DOColor(colorEnd, transitionDuration);
            }

            yield return new WaitForSeconds(transitionDuration);
            yield return new WaitForSeconds(bufferDuration);
        }

        transform.DOKill(true);

        for (int i = 0; i < backgrounds.Count; i++)
        {
            backgrounds[i].DOKill(true);
            backgrounds[i].color = new(colorEnd.r, colorEnd.g, colorEnd.b, 0);
        }
        yield break;
    }
}