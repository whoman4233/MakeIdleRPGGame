using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(HealthSystem))]
public class PlayerRef : MonoBehaviour
{
    public static PlayerRef Instance { get; private set; }

    public PlayerController Controller { get; private set; }
    public PlayerStats Stats { get; private set; }
    public HealthSystem Health { get; private set; } // HealthSystem 프로퍼티 추가

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Controller = GetComponent<PlayerController>();
        Stats = GetComponent<PlayerStats>();
        Health = GetComponent<HealthSystem>(); // 컴포넌트 가져오기
    }
}