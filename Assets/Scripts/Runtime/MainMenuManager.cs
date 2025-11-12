using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private enum CanvasID
    {
        MAIN = 0,
        START = 1,
        OPTION = 2
    }

    [Header("UI/UX Menu Canvas")]
    [SerializeField, CE_ReadOnly] private CanvasID activeCanvasID;
    [SerializeField] private List<Canvas> menuCanvas;

    private static readonly Dictionary<CanvasID, Canvas> menuCanvasMap = new();


    private void Start()
    {
        for (int i = 0; i < menuCanvas.Count; i++)
        {
            if (menuCanvasMap.TryAdd((CanvasID)i, menuCanvas[i]))
                Debug.Log("Canvas added success!");
        }
    }


    public void SwapMenu(int id)
    {
        if (id < 0) return;
        if (id >= menuCanvas.Count) return;

        CanvasID cID = (CanvasID)id; 
        menuCanvasMap.TryGetValue(cID, out Canvas selectedCanvas);
        if (selectedCanvas == null) return;

        menuCanvasMap.TryGetValue(activeCanvasID, out Canvas oldCanvas);
        oldCanvas.enabled = false;
        selectedCanvas.enabled = true;

        activeCanvasID = cID;
    }
    public void QuitGame()
    {
        if (!Application.isPlaying) return;

        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif

        Application.Quit();
    }
}
