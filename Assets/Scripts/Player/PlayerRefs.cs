using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerRef : MonoBehaviour
{
    public static PlayerRef Instance { get; private set; }

    public PlayerController Controller { get; private set; }
    public PlayerStats Stats { get; private set; }

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
    }
}
