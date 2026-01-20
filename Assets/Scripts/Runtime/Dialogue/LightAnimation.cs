using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(LightAnimation))]
public class LightAnimationEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        LightAnimation lightAnim = (LightAnimation)target;

        // Executes whenever values in inspector changes.
        if (DrawDefaultInspector())
        {
        }

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label("Runtime Controls", EditorStyles.boldLabel, GUILayout.MaxWidth(250f));
        if (GUILayout.Button("Start Animation", GUILayout.Width(250f), GUILayout.Height(30f)))
            lightAnim.StartAnimating();
        if (GUILayout.Button("Stop Animation", GUILayout.Width(250f), GUILayout.Height(30f)))
            lightAnim.StopAnimating();
        if (GUILayout.Button("Reset to Defaults", GUILayout.Width(250f), GUILayout.Height(30f)))
            lightAnim.ResetPower();
        GUILayout.EndVertical();
    }
}
#endif

public class LightAnimation : MonoBehaviour
{
    [SerializeField] private Light lightSrc;

    [Header("Light Intensity")]
    [SerializeField] private float minPower;
    [SerializeField] private float maxPower;

    [Header("Animation")]
    [Tooltip("Start animation when this GO is enabled.")]
    [SerializeField] private bool onStart;
    [ConditionalHide("onStart", true), Tooltip("Delay start animation from playing before x seconds.")]
    [SerializeField] private float delayStart = 0f;

    [SerializeField] private float duration;
    [SerializeField] private AnimationCurve curve;

    [CE_ReadOnly, SerializeField] private float power;
    [CE_ReadOnly, SerializeField] private float initialPower;

    private void OnEnable()
    {
        if (lightSrc == null) return;
        initialPower = lightSrc.intensity;

        if (onStart)
            StartCoroutine(DelayRoutine());
    }
    public void ResetPower()
    {
        if (lightSrc == null) return;
        lightSrc.intensity = initialPower;
    }
    public void StartAnimating()
    {
        StartCoroutine(AnimateRoutine());
    }
    public void StopAnimating()
    {
        StopAllCoroutines();
    }
    public IEnumerator AnimateRoutine()
    {
        if (duration <= 0) yield break;
        lightSrc.intensity = minPower;

        float timer = 0f;
        float p;
        float range = maxPower - minPower;

        while (timer <= duration)
        {
            timer += Time.deltaTime;
            float percent = timer / duration;
            p = curve.Evaluate(percent);
            power = (p * range) + minPower;

            if (lightSrc != null)
                lightSrc.intensity = power;

            yield return null;
        }
        yield break;
    }

    private IEnumerator DelayRoutine()
    {
        yield return new WaitForSeconds(delayStart);
        StartCoroutine(AnimateRoutine());
        yield break;
    }
    private void OnValidate()
    {
        if (lightSrc == null)
        {
            if (TryGetComponent(out Light light))
                lightSrc = light;
        }

        if (duration <= 0f)
            duration = 1f;
    }
}
