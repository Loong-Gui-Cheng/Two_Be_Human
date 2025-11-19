using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class IGUIManager : MonoBehaviour
{
    private enum CanvasID
    {
        MAIN = 0,
        TEAM = 1,
        INVENTORY = 2,
        POINT = 3,
        CONFIG = 4
    }

    [Header("UI/UX In-Game UI")]
    [SerializeField, CE_ReadOnly] private CanvasID activeCanvasID;
    [SerializeField] private Canvas GUICanvas;
    [SerializeField] private Canvas MenuCanvas;

    [Header("Canvas Menu")]
    [SerializeField] private List<Canvas> menuCanvas;

    private readonly Dictionary<CanvasID, Canvas> menuCanvasMap = new();

    private void Start()
    {
        for (int i = 0; i < menuCanvas.Count; i++)
            menuCanvasMap.TryAdd((CanvasID)i, menuCanvas[i]);
    }
    private void OnEnable()
    {
        PlayerController2D.OnToggleMenu += ToggleMenu;
    }
    private void OnDisable()
    {
        PlayerController2D.OnToggleMenu -= ToggleMenu;
    }


    private void ToggleMenu(bool IsMenuOn)
    {
        MenuCanvas.enabled = IsMenuOn;
        GUICanvas.enabled = !IsMenuOn;
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
