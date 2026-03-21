using System;
using System.Collections.Generic;
using UnityEngine;

// 인스펙터에서 가중치와 육체 데이터를 짝지어 입력하기 위한 클래스
[Serializable]
public class DropEntry
{
    public GraftData graftData;
    public int weight;
}

[CreateAssetMenu(fileName = "NewDropTable", menuName = "Anomaly/Drop Table")]
public class DropTable : ScriptableObject
{
    public List<DropEntry> entries = new List<DropEntry>();

    // 가중치 기반 랜덤 픽 로직
    public GraftData GetRandomDrop()
    {
        if (entries == null || entries.Count == 0) return null;

        int totalWeight = 0;
        foreach (var entry in entries)
        {
            totalWeight += entry.weight;
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var entry in entries)
        {
            currentWeight += entry.weight;
            if (randomValue < currentWeight)
            {
                return entry.graftData;
            }
        }

        return null;
    }
}