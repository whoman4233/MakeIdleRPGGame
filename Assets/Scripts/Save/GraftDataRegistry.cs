using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 GraftData SO를 등록해두는 레지스트리.
/// SaveManager가 SO 이름(string)으로 실제 GraftData 오브젝트를 찾을 때 사용한다.
///
/// [사용법]
///   1. Project 창에서 우클릭 → Create → Anomaly → Graft Data Registry 로 에셋 생성
///   2. 생성한 에셋의 All Grafts 리스트에 모든 GraftData SO를 등록
///   3. SaveManager 인스펙터의 Graft Registry 슬롯에 연결
/// </summary>
[CreateAssetMenu(fileName = "GraftDataRegistry", menuName = "Anomaly/Graft Data Registry")]
public class GraftDataRegistry : ScriptableObject
{
    [Tooltip("프로젝트 내 모든 GraftData SO를 여기에 등록하세요.")]
    [SerializeField] private List<GraftData> allGrafts = new List<GraftData>();

    public IReadOnlyList<GraftData> AllGrafts => allGrafts;

    /// <summary>SO 에셋 이름(파일명)으로 GraftData를 찾아 반환. 없으면 null.</summary>
    public GraftData Find(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return allGrafts.Find(g => g != null && g.name == id);
    }
}
