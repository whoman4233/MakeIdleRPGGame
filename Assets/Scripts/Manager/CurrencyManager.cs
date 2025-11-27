using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Runtime")]
    [SerializeField] private long gold;
    [SerializeField] private long gem; // ÇÊ¿ä ¾øÀ¸¸é »©µµ µÊ

    public long Gold => gold;
    public long Gem => gem;

    public event Action OnCurrencyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddGold(long amount)
    {
        gold = Math.Max(0, gold + amount);
        OnCurrencyChanged?.Invoke();
    }

    public void AddGem(long amount)
    {
        gem = Math.Max(0, gem + amount);
        OnCurrencyChanged?.Invoke();
    }
}
