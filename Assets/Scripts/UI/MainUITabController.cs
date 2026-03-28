using UnityEngine;
using UnityEngine.UI;

// 하단 메인 UI 탭 전환을 관리하는 클래스
public class MainUITabController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject[] tabPanels; // 장비, 추출, 합성, 뽑기 패널들을 순서대로 할당

    [Header("Tab Buttons")]
    public Button[] tabButtons;    // 위 패널과 동일한 순서로 버튼들을 할당

    private void Start()
    {
        // 각 버튼에 클릭 이벤트 등록
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i; // 람다식 내부에서 현재 인덱스를 사용하기 위해 지역 변수로 캡처
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }

        // 게임 시작 시 기본으로 보여줄 탭 설정 (0번: 장비 탭)
        SwitchTab(0);
    }

    // 선택된 탭만 활성화하고 나머지는 비활성화하는 함수
    public void SwitchTab(int tabIndex)
    {
        for (int i = 0; i < tabPanels.Length; i++)
        {
            if (tabPanels[i] != null)
            {
                tabPanels[i].SetActive(i == tabIndex);
            }
        }

        // 선택된 탭 버튼의 시각적 강조 효과(색상 변경 등)가 필요하다면 이 위치에 추가
    }
}