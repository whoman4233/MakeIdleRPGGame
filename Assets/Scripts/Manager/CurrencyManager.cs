using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private const long MAX_DATA = 9_999_999_999L;
    private const int  MAX_CORE = 99_999;

    public static CurrencyManager Instance { get; private set; }

    // 관측 데이터 (long: 최대 99억까지 정확히 표현)
    public long Data { get; private set; }

    // 이상 코어
    public int Core { get; private set; }

    public event Action OnCurrencyChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddData(long amount)
    {
        Data = System.Math.Clamp(Data + amount, 0L, MAX_DATA);
        OnCurrencyChanged?.Invoke();
    }

    // 하위 호환: int 파라미터도 수용
    public void AddData(int amount) => AddData((long)amount);

    public void AddCore(int amount)
    {
        Core = Mathf.Clamp(Core + amount, 0, MAX_CORE);
        OnCurrencyChanged?.Invoke();
    }

    public void LoadValues(long data, int core)
    {
        Data = System.Math.Clamp(data, 0L, MAX_DATA);
        Core = Mathf.Clamp(core, 0, MAX_CORE);
        OnCurrencyChanged?.Invoke();
    }

    // 하위 호환: int 버전
    public void LoadValues(int data, int core) => LoadValues((long)data, core);
}