using UnityEngine;
using UnityEngine.UI;

public class StatUnlockableUI : MonoBehaviour
{
    public enum STATUS
    {
        LOCKED = 0,
        UNLOCKABLE = 1,
        UNLOCKED = 2
    }

    [Header("Data Reference")]
    public StatData data;

    [Header("User Interface (UI)")]
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private Image buttonOverlay;
    [SerializeField] private Color lockedColor;
    [SerializeField] private Color lockedIconColor;
    [SerializeField] private Color unlockableIconColor;

    public Button button;

    private void Start()
    {
        InitialiseUI();
    }
    public void InitialiseUI()
    {
        if (data == null) return;
        icon.sprite = data.icon;
    }
    public void UpdateUI(STATUS status)
    {
        switch (status)
        {
            case STATUS.LOCKED:
                background.color = lockedColor; 
                icon.color = lockedIconColor; 
                buttonOverlay.color = Color.clear;
                break;

            case STATUS.UNLOCKABLE:
                background.color = data.backgroundColorUnlockable;
                icon.color = unlockableIconColor;
                buttonOverlay.color = Color.white;
                break;

            case STATUS.UNLOCKED:
                background.color = data.backgroundColor;
                icon.color = Color.white;
                buttonOverlay.color = Color.clear;
                break;
        }
    }
}
