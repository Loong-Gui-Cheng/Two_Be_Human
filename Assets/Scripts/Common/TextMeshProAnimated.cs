using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextMeshProAnimated : TextMeshProUGUI
{
    #region Events
    // We inherit from UnityEvents so that we could use RemoveAllListeners() for convenience.
    [System.Serializable] public class ActionEvent : UnityEvent<string> { }
    [System.Serializable] public class ScriptEvent : UnityEvent<string> { }
    [System.Serializable] public class AnimationEvent : UnityEvent<string> { }
    [System.Serializable] public class TextRevealEvent : UnityEvent<char> { }
    [System.Serializable] public class DialogueFinshEvent : UnityEvent { }
    #endregion

    // Animation Variables
    [SerializeField] private float speed = 100f;
    [CE_ReadOnly, SerializeField] private bool isAnimating = false;
    private float DEFAULT_SPEED = 0f;

    // Events (Tags & Voice)
    public ActionEvent onAction;
    public AnimationEvent onAnimation;
    public ScriptEvent onScript;

    public TextRevealEvent onTextReveal;
    public DialogueFinshEvent onDialogueFinish;

    protected override void Start()
    {
        base.Start();
        DEFAULT_SPEED = speed;
    }

    public void Animate()
    {
        // Reset to default speed
        if (DEFAULT_SPEED <= 0) DEFAULT_SPEED = speed;
        else speed = DEFAULT_SPEED;

        string input = string.Copy(text);
        text = string.Empty;

        // An array containing strings (words), split via <>.
        // Eg. <speed=100> <b> Hello World! </b>
        // This means even numbers = text, odd numbers = tags.

        string[] subTexts = input.Split('<', '>');
        string displayText = string.Empty;

        // Iterate through the word list.
        for (int i = 0; i < subTexts.Length; i++)
        {
            // Remove trailing spaces from tags.
            if (subTexts[i].CompareTo(" ") == 0)
                subTexts[i] = "";

            // Since it is even, string is a text.
            if (i % 2 == 0)
                displayText += subTexts[i];

            // Check if its TMP Rich Text Tags.
            else if (!IsCustomTag(subTexts[i].Replace(" ", "")))
                displayText += $"<{subTexts[i]}>";
        }

        // Send text without tags to UI, hide it and read the message.
        text = displayText;
        maxVisibleCharacters = 0;
        StartCoroutine(Read(subTexts));
    }
    private IEnumerator Read(string[] subTexts)
    {
        isAnimating = true;

        int subCounter = 0;
        int visibleCounter = 0;

        while (subCounter < subTexts.Length)
        {
            string text = subTexts[subCounter];

            // Odd Number = Tag
            if (subCounter % 2 == 1)
                yield return EvaluateTag(text.Replace(" ", ""));
            else
            {
                // Iterating through alphabets in a text.
                while (visibleCounter < text.Length)
                {
                    // Reveal alphabet
                    onTextReveal?.Invoke(subTexts[subCounter][visibleCounter]);

                    visibleCounter++;
                    maxVisibleCharacters++;
                    yield return new WaitForSeconds(1f / speed);
                }
                visibleCounter = 0;
            }
            subCounter++;
        }

        // No more words left, stop animating.
        isAnimating = false;
        onDialogueFinish?.Invoke();
        yield break;
    }


    private WaitForSeconds EvaluateTag(string tag)
    {
        if (tag.Length < 0) return null;

        // Use '=' delimiters to extract out the second half of string (which is a value). 
        if (tag.StartsWith("speed="))
            speed = float.Parse(tag.Split('=')[1]);

        else if (tag.StartsWith("pause="))
            return new WaitForSeconds(float.Parse(tag.Split("=")[1]));

        else if (tag.StartsWith("action="))
            onAction?.Invoke(tag.Split("=")[1]);

        // where script='IDialogueBindGO_Name'.
        else if (tag.StartsWith("script="))
            onScript?.Invoke(tag.Split("=")[1]);

        else if (tag.StartsWith("animation="))
            onAnimation?.Invoke(tag.Split("=")[1]);

        return null;
    }
    private bool IsCustomTag(string tag)
    {
        return tag.StartsWith("speed=") ||
               tag.StartsWith("pause=") ||
               tag.StartsWith("action=") ||
               tag.StartsWith("script=") ||
               tag.StartsWith("animation=");
    }


    public bool IsCurrentlyAnimating()
    { return isAnimating; }
    public void StopAnimating()
    {
        StopAllCoroutines();
        isAnimating = false;
    }
    public void RemoveAllListeners()
    {
        onAction.RemoveAllListeners();
        onScript.RemoveAllListeners();
        onAnimation?.RemoveAllListeners();

        onTextReveal.RemoveAllListeners();
        onDialogueFinish.RemoveAllListeners();
    }
}
