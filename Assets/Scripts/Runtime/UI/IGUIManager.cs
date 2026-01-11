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
        EQUIP_INSPECTOR = 2,
        EQUIP_GEAR = 3,
        ITEM = 4,
        STAT_TREE = 5,
        SYSTEM = 6,
        CONFIG = 7,
        LOAD_DATA = 8,
        SAVE_DATA = 9
    }

    [Header("UI/UX In-Game UI")]
    [SerializeField, CE_ReadOnly] private CanvasID activeCanvasID;
    [SerializeField] private Canvas GUICanvas;
    [SerializeField] private Canvas MenuCanvas;

    [Header("Canvas Menu")]
    [SerializeField] private List<Canvas> menuCanvas;

    private readonly Dictionary<CanvasID, Canvas> menuCanvasMap = new();
    public static event System.Action<bool> OnReceiveMenuState;

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


    private void ToggleMenu()
    {
        bool MenuState = MenuCanvas.enabled;

        if (MenuState)
        {
            switch (activeCanvasID)
            {
                // Toggles off menu screen.
                case CanvasID.MAIN:
                    MenuState = !MenuState;

                    GUICanvas.enabled = !MenuState;
                    MenuCanvas.enabled = MenuState;
                    break;

                case CanvasID.TEAM:
                    MainMenuUIManager.OnUpdateTeam?.Invoke();
                    SwapMenu((int)CanvasID.MAIN);
                    break;

                case CanvasID.EQUIP_GEAR:
                    EquipmentInspector.OnUpdateEquipment?.Invoke();
                    MainMenuUIManager.OnUpdateTeam?.Invoke();
                    SwapMenu((int)CanvasID.EQUIP_INSPECTOR);
                    break;

                // Switch to system screen.
                case CanvasID.CONFIG:
                case CanvasID.LOAD_DATA:
                case CanvasID.SAVE_DATA:
                    SwapMenu((int)CanvasID.SYSTEM);
                    break;

                // Switch to main screen.
                default:
                    SwapMenu((int)CanvasID.MAIN);
                    break;
            }
        }
        else
        {
            // Toggles on 
            MenuState = !MenuState;
            GUICanvas.enabled = !MenuState;
            MenuCanvas.enabled = MenuState;
        }

        OnReceiveMenuState?.Invoke(MenuState);
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
