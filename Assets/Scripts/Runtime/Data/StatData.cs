using UnityEngine;

[CreateAssetMenu(fileName = "STAT_DATA", menuName = "Custom/StatData")]
public class StatData : ScriptableObject
{
    public enum TYPE
    {
        NONE = 0,
        MAX_HP_BOOST_1 = 1,
        MAX_HP_BOOST_2 = 2,
        ATK_BOOST_1 = 3,
        ATK_BOOST_2 = 4,
        CRIT_CHANCE_BOOST_1 = 5,
        CRIT_CHANCE_BOOST_2 = 6,
        BASE_POWER_BOOST = 7,
        INCREMENT_POWER_BOOST = 8
    }

    [Header("User Interface (UI)")]
    public TYPE type;
    public Sprite icon;
    public Color backgroundColor;
    public Color backgroundColorUnlockable;
}
