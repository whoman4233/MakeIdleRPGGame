using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    // Gold -> 관측 데이터
    public int Data { get; private set; }
    
    // Gem -> 이상 코어
    public int Core { get; private set; }

    public event Action OnCurrencyChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddData(int amount)
    {
        Data += amount;
        OnCurrencyChanged?.Invoke();
    }

    public void AddCore(int amount)
    {
        Core += amount;
        OnCurrencyChanged?.Invoke();
    }
}