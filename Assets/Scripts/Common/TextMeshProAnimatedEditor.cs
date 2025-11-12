using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using TMPro.EditorUtilities;
[CustomEditor(typeof(TextMeshProAnimated), true)]
[CanEditMultipleObjects]
public class TextMeshProAnimatedEditor : TMP_BaseEditorPanel
{
    private SerializedProperty _speed;
    private SerializedProperty _isAnimating;

    protected override void OnEnable()
    {
        base.OnEnable();
        _speed = serializedObject.FindProperty("speed");
        _isAnimating = serializedObject.FindProperty("isAnimating");
    }
    protected override void DrawExtraSettings()
    {
        EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_speed, new GUIContent("Default Speed"));
        EditorGUILayout.PropertyField(_isAnimating);
    }

    protected override bool IsMixSelectionTypes()
    { return false; }
    protected override void OnUndoRedo()
    {}
}
#endif