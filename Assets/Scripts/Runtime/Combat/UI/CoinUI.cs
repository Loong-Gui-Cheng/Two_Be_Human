using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    [Header("User Interface (UI)")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image type_icon;
    [SerializeField] private Image action_icon;
    [SerializeField] private TextMeshProUGUI name_TMP;
    [SerializeField] private TextMeshProUGUI incrementPower_TMP;
    public TextMeshProUGUI coinPower_TMP;
    public Light2D coinLight;

    [Header("Damage Types (UI)")]
    [SerializeField] private Sprite slash_icon;
    [SerializeField] private Sprite pierce_icon;
    [SerializeField] private Sprite blunt_icon;
    [SerializeField] private Sprite magic_icon;

    [Header("Coins")]
    [SerializeField] private RectTransform tossGroup;
    [SerializeField] private GameObject coinPrefab;
    public List<Image> coins;

    private const float DURATION_COIN_TOSS = 1f;
    private const float DURATION_COIN_BREAK = 0.3f;
    private const float DURATION_ATTACK = 0.6f;

    private readonly WaitForSeconds YIELD_COIN_TOSS = new(DURATION_COIN_TOSS);
    private readonly WaitForSeconds YIELD_COIN_BREAK = new(DURATION_COIN_BREAK);


    public void ToggleUI() => canvas.enabled = !canvas.enabled;
    public void SetupUI(SkillData input)
    {
        switch (input.resistance)
        {
            case SkillData.RESISTANCE_TYPE.SLASH:
                type_icon.sprite = slash_icon;
                break;

            case SkillData.RESISTANCE_TYPE.PIERCE:
                type_icon.sprite = pierce_icon;
                break;

            case SkillData.RESISTANCE_TYPE.BLUNT:
                type_icon.sprite = blunt_icon;
                break;
        }

        action_icon.sprite = input.icon;
        name_TMP.text = string.Format("{0}", input.Name);
        incrementPower_TMP.text = string.Format("+{0}", input.incrementCoinPower);
        coinPower_TMP.text = string.Format("{0}", input.baseCoinPower);

        for (int i = 0; i < input.coins; i++)
        {
            GameObject coinGO = Instantiate(coinPrefab, tossGroup);
            if (coinGO.TryGetComponent(out Image coinSprite))
                coins.Add(coinSprite);
        }

        ToggleUI();
    }
    public void ResetSkillUI(int basePower)
    {
        coinLight.enabled = false;
        coinPower_TMP.text = string.Format("{0}", basePower);

        for (int i = 0; i < coins.Count; i++)
            coins[i].color = Color.white;
    }
    public void ResetCoinUI()
    {
        for (int i = coins.Count - 1; i >= 0; i--)
            Destroy(coins[i].gameObject);

        coinLight.enabled = false;
        coins.Clear();
    }


    public void AnimateCoinTossUI(int heads, int tails, int basePower, int incrementPower) => StartCoroutine(CoinTossAnimation(heads, tails, basePower, incrementPower));
    public void AnimateCoinBreakUI() => StartCoroutine(CoinBreakAnimation());
    public void AnimateCoinAttackTossUI(int coinPower, int increment, int index, bool isHeads) => StartCoroutine(CoinAttackAnimation(coinPower, increment, index, isHeads));


    #region Animation Functions
    private IEnumerator CoinTossAnimation(int heads, int tails, int basePower, int incrementPower)
    {
        int headCoins = heads;
        int tailCoins = tails;

        int coinPower = basePower;
        coinPower_TMP.text = string.Format("{0}", coinPower);

        for (int i = 0; i < coins.Count; i++)
        {
            coinLight.enabled = false;

            if (headCoins != 0 && tailCoins != 0)
            {
                int randResult = Random.Range(0, 100);
                if (randResult > 49)
                {
                    headCoins--;
                    coinPower += incrementPower;
                    coins[i].color = Color.yellow;
                    coinPower_TMP.text = string.Format("{0}", coinPower);
                    AudioController.Instance.PlayUI(AudioController.SOUND_ID.COIN_WIN);
                    coinLight.enabled = true;
                }
                else
                {
                    tailCoins--;
                    coins[i].color = Color.black;
                    AudioController.Instance.PlayUI(AudioController.SOUND_ID.COIN_FAIL);
                }
            }
            else if (headCoins != 0)
            {
                headCoins--;
                coinPower += incrementPower;
                coins[i].color = Color.yellow;
                coinPower_TMP.text = string.Format("{0}", coinPower);
                AudioController.Instance.PlayUI(AudioController.SOUND_ID.COIN_WIN);
                coinLight.enabled = true;
            }
            else
            {
                tailCoins--;
                coins[i].color = Color.black;
                AudioController.Instance.PlayUI(AudioController.SOUND_ID.COIN_FAIL);
            }

            yield return new WaitForSeconds(DURATION_COIN_TOSS / coins.Count);
        }

        coinLight.enabled = false;
        yield break;
    }
    private IEnumerator CoinBreakAnimation()
    {
        int lastIndex = coins.Count - 1;
        coins[lastIndex].DOFade(0f, DURATION_COIN_BREAK);
        yield return YIELD_COIN_BREAK;

        Destroy(coins[lastIndex].gameObject);
        coins.RemoveAt(lastIndex);
        yield break;
    }
    private IEnumerator CoinAttackAnimation(int coinPower, int increment, int index, bool isHeads)
    {
        int finalPower = coinPower;
        coinLight.enabled = false;

        if (isHeads)
        {
            finalPower += increment;
            coinLight.enabled = true;
            coins[index].color = Color.yellow;
            AudioController.Instance.PlayUI(AudioController.SOUND_ID.COIN_WIN);
        }
        else
        {
            coins[index].color = Color.black;
            AudioController.Instance.PlayUI(AudioController.SOUND_ID.COIN_FAIL);
        }

        coinPower_TMP.text = string.Format("{0}", finalPower);
        yield return new WaitForSeconds(DURATION_ATTACK / 2f);


        yield break;
    }
    #endregion

    #region Animation Duration
    public float CalculateCoinDuration(int coins)
    {
        float coinAttackDuration = coins * DURATION_ATTACK;
        return coinAttackDuration;
    }
    public WaitForSeconds GetCoinTossDuration()
    {
        return YIELD_COIN_TOSS;
    }
    public WaitForSeconds GetCoinBreakDuration()
    {
        return YIELD_COIN_BREAK;
    }
    #endregion
}
