# 3D Idle RPG Prototype

> Nonstop Knight / 오늘도 우라라 스타일의 **3D 방치형 RPG 프로토타입**  
> 플레이어는 직접 조작하지 않고, 캐릭터가 자동으로 전투/성장을 반복하는 구조

---

## 🎮 프로젝트 개요

이 프로젝트는 자동 전투와 성장 루프에 집중한 **3D 방치형 RPG**입니다.

플레이어 캐릭터는 앞으로 전진하면서 적을 자동으로 탐색·공격하고,  
유저는 화면 하단 UI를 통해 **스탯 업그레이드 / 장비 장착 / 뽑기(가챠)**에만 개입합니다.

- 레퍼런스
  - Nonstop Knight
  - 오늘도 우라라

---

## ✨ 주요 특징

### 1. 자동 전투 & FSM 기반 플레이어 AI

- **Finite State Machine(FSM)** 으로 플레이어 상태 관리
  - `Idle` → `MoveForward` → `Chase` → `Attack` → `Dead`
- 일정 거리 안의 적을 자동으로 탐색(`detectRange`)하고 가장 가까운 적을 타겟으로 설정
- `AttackInterval` / `AttackSpeed`는 **스탯 & 업그레이드/장비**에 따라 실시간 변동
- 플레이어는 항상 **앞으로 전진**하면서 전투를 반복

### 2. ScriptableObject 기반 데이터 관리

모든 핵심 데이터는 `ScriptableObject`로 관리:

- `PlayerStatsData`
  - 기본 레벨, HP/MP/공격력/방어력, 이동 속도, 공격 간격
  - 레벨당 성장량, 경험치 커브(`AnimationCurve`)
- (필요 시) `EnemyStatsData`
  - 적 체력, 공격력, 공격 간격, 보상 골드/경험치 등
- (선택) `StageData`
  - 스테이지별 적 구성, 필요 처치 수, 보상 등
- `EquipmentData`
  - 장비 ID / 이름 / 아이콘 / 슬롯 타입(Weapon, Armor, Accessory)
  - `StatModifier[]`로 어떤 스탯을 얼마나 올려주는지 정의

**장점**

- 데이터/밸런싱 수정 시 코드 수정 없이 SO만 조정하면 됨
- 플레이어/적/스테이지/장비 모두 동일한 파이프라인에서 관리 가능

### 3. 스탯 시스템 & Modifier 파이프라인

`PlayerStats`는 다음 3가지 소스에서 스탯을 합산:

- 업그레이드(Upgrade)
- 장비(Equipment)
- 버프(Buff)

```csharp
Attack       = (BaseAttack + FlatUpgrades + FlatEquip + FlatBuff) * (1 + PercentTotal)
MaxHP        = ...
AttackSpeed  = ...
ExpGainBonus = ...
SkillDamageBonus
GoldGainBonus
StatType 으로 스탯 종류를 분리 (Attack, MaxHP, AttackSpeed, ExpGain, SkillDamage, GoldGain 등)

StatModifier를 어디서 붙이든(업그레이드, 장비, 버프) 같은 계산 로직으로 처리

4. 스테이지 & 보스 루프
일반 몬스터 처치 → 보스 등장/이동 → 보스 처치 시 다음 스테이지 진행

KillCount UI를 통해 현재 스테이지에서 처치한 적 수 표시

스테이지 진행 실패 시(사망 등) → 현재 스테이지에서 재도전(파밍 루프)

5. 통화 시스템 (Gold)
CurrencyManager 싱글톤

Gold 보유량 관리

OnCurrencyChanged 이벤트로 UI와 자동 연동

Enemy 처치 시 Gold 획득

업그레이드/뽑기에서 Gold 소모

csharp
코드 복사
CurrencyManager.Instance.AddGold(+100);
CurrencyManager.Instance.AddGold(-cost);
6. 업그레이드 시스템 (Upgrade)
화면 하단 Upgrade 버튼 → 업그레이드 패널 열기

Upgrade 슬롯 리스트:

이름, 레벨, 비용, 효과 설명 표시

버튼 클릭 → Gold 차감 → StatModifier 추가 → Stats 재계산

예시

공격력 증가

최대 HP 증가

경험치 획득량 증가

스킬 데미지 증가

Gold 획득량 증가

7. 장비 시스템 (Equipment)
장비 타입

무기(Weapon)

방어구(Armor)

장신구(Accessory)

PlayerEquipment가 현재 장착 장비를 관리

장비 교체 시 기존 Modifier 제거 → 새 장비 Modifier 적용

OnEquipmentChanged 이벤트로 UI 갱신

상단/장비창 UI에서 현재 장착중인:

무기 이름

방어구 이름

장신구 이름
확인 가능

8. 뽑기(Gacha) & 인벤토리
뽑기(Gacha)

버튼 하나로 간단한 뽑기

정해진 비용만큼 Gold 소모

EquipmentData[] pool 중 하나 랜덤 지급

지급된 장비는 인벤토리에도 추가되고, 자동 장착

인벤토리(Inventory)

InventoryManager에서 보유 장비 목록 관리

인벤토리 패널에서

아이템 아이콘/이름/타입(Weapon/Armor/Accessory) 리스트 표시

각 아이템에 “장착” 버튼

클릭 시 해당 장비를 즉시 장비

9. UI / UX
PlayerHUD

HP / EXP 바 (Filled Image + Tween)

Gold 표시

레벨 / 현재 경험치

하단 버튼 바

Upgrade

Inventory(장비창+인벤토리)

Gacha(뽑기)

패널들은 CanvasGroup + TogglePanel로 On/Off

🧱 기술 스택
Engine: Unity 2022.x (LTS 기준)

Language: C#

UI: UGUI, TextMeshPro

아키텍처/패턴

FSM (플레이어 AI 상태 관리)

ScriptableObject 기반 데이터 드리븐 설계

Modifier 패턴으로 스탯 합성

싱글톤 매니저 (PlayerRef, CurrencyManager, InventoryManager 등)
