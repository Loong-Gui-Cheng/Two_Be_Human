using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBind_Toggle : MonoBehaviour, IDialogueBind, IEffector
{
    [Header("Toggle GameObject")]
    [SerializeField] private List<GameObject> GOList;
    [SerializeField] private List<MonoBehaviour> GOScriptList;

    public void IDialogueExecute()
    {
        ToggleObject();
    }
    public void IEffectorExecute()
    {
        ToggleObject();
    }
    public void IEffectorExit()
    {
    }

    private void ToggleObject()
    {
        for (int i = 0; i < GOList.Count; i++)
        {
            GameObject go = GOList[i];
            if (go == null) continue;

            go.SetActive(!go.activeSelf);
        }
        for (int i = 0; i < GOScriptList.Count; i++)
        {
            MonoBehaviour mb = GOScriptList[i];
            if (mb == null) continue;
            mb.enabled = !mb.enabled;
        }
    }
}