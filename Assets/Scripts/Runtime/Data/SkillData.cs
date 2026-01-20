using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "SkillData", menuName = "Custom/SkillData")]
public class SkillData : ScriptableObject
{
    public enum TYPE
    {
        CLASH = 0,
        DEFENCE = 1,
        ITEM = 2
    }
    // Attack type
    public enum RESISTANCE_TYPE
    {
        SLASH = 0,
        PIERCE = 1,
        BLUNT = 2,
        MAGIC = 3
    }


    [System.Serializable]
    public class CoinStatus
    {
        public enum TRIGGER
        {
            HIT = 0,
            HEADS = 1,
            TAILS = 2,
            CRIT = 3
        }
        public TRIGGER trigger;
        public StatusData status;
        public int potency;
        public int count;
    }
    [System.Serializable]
    public class Coin
    {
        public List<CoinStatus> infliction;
        public List<CoinStatus> gain;
        public string description;
    }


    [Header("Identifier")]
    public TYPE type;
    [ConditionalHide("type", true, true)] public RESISTANCE_TYPE resistance;

    [Header("Details")]
    public Sprite icon;
    public string Name;
    [TextArea(3, 5)] public string description;

    [Header("Coin Amount")]
    public int baseCoinPower;
    public int incrementCoinPower;
    public GameObject onUseEffect;
    public List<Coin> coins;

    public void FormatDescription(CoinStatus coin, ref string text, string verb)
    {
        string tag = string.Empty;
        switch (coin.trigger)
        {
            case CoinStatus.TRIGGER.HIT:
                tag = "<color=green>[On Hit]</color> ";
                break;
            case CoinStatus.TRIGGER.HEADS:
                tag = "<color=green>[On Heads]</color> ";
                break;
            case CoinStatus.TRIGGER.TAILS:
                tag = "<color=green>[On Tails]</color> ";
                break;
            case CoinStatus.TRIGGER.CRIT:
                tag = "<color=blue>[On Crit]</color> ";
                break;
        }

        string status = string.Empty;

        if (coin.potency > 0)
        {
            string potency = string.Format(" {0} <color=red>{1}</color> ", coin.potency, coin.status.Name);
            status = string.Concat(status, potency);           
        }
        if (coin.count > 0)
        {
            if (coin.potency > 0)
            {
                string connector = "and";
                status = string.Concat(status, connector);

            }
            string count = string.Format(" +{0} <color=red>{1}</color> Count", coin.count, coin.status.Name);
            status = string.Concat(status, count);
        }

        text = string.Concat(tag, verb, status);
        text = string.Concat(text, "\n");
    }
    public void OnValidate()
    {
        for (int i = 0; i < coins.Count; i++)
        {
            List<CoinStatus> infliction = coins[i].infliction;
            List<CoinStatus > gain = coins[i].gain;

            string debuffs = string.Empty;
            string buffs = string.Empty;

            for (int d = 0; d < infliction.Count; d++)
            {
                FormatDescription(infliction[d], ref debuffs, "Inflict");
            }
            for (int b = 0; b < gain.Count; b++)
            {
                FormatDescription(gain[b], ref buffs, "Gain");
            }

            coins[i].description = string.Concat(debuffs, buffs);
        }
    }
}