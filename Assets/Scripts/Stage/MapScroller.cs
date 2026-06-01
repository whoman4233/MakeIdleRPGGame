using UnityEngine;

/// <summary>
/// 배경 세그먼트 무한 스크롤 + 스테이지 전환 시 배경색/스프라이트 교체
/// </summary>
public class MapScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 2f;
    public float bgWidth     = 20f;

    [Header("Background Segments")]
    [Tooltip("씬에 배치된 배경 세그먼트 오브젝트들 (BgSegment_0~N)")]
    public Transform[] backgrounds;

    [Header("Segment Renderers — Background Quad")]
    [Tooltip("각 세그먼트의 Background Quad MeshRenderer (스프라이트/색 교체 대상)")]
    public MeshRenderer[] bgRenderers;

    [Header("Stage Color & Sprite Transition")]
    [Tooltip("스테이지 전환 시 카메라 배경색 전환 속도")]
    public float colorLerpSpeed = 1.5f;

    private Camera   _cam;
    private Color    _targetColor;
    private Color    _currentColor;
    private Sprite   _currentBgSprite;

    private void Awake()
    {
        _cam = Camera.main;
        if (_cam != null) { _currentColor = _cam.backgroundColor; _targetColor = _currentColor; }
    }

    private void OnEnable()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged += OnStageChanged;
            OnStageChanged();
        }
    }

    private void OnDisable()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnStageChanged -= OnStageChanged;
    }

    private void OnStageChanged()
    {
        var stage = StageManager.Instance?.CurrentStage;
        if (stage == null) return;

        _targetColor = stage.bgColor;

        // 배경 스프라이트 교체 (있을 때만)
        if (stage.bgSprite != null && stage.bgSprite != _currentBgSprite)
        {
            _currentBgSprite = stage.bgSprite;
            ApplyBgSprite(stage.bgSprite);
        }
    }

    private void ApplyBgSprite(Sprite sprite)
    {
        if (bgRenderers == null) return;
        foreach (var mr in bgRenderers)
        {
            if (mr == null) continue;
            // MaterialPropertyBlock으로 인스턴스 머티리얼 오염 없이 텍스처 교체
            var block = new MaterialPropertyBlock();
            mr.GetPropertyBlock(block);
            block.SetTexture("_MainTex", sprite.texture);
            mr.SetPropertyBlock(block);
        }
    }

    private void Update()
    {
        var ctrl = PlayerRef.Instance?.Controller;
        if (ctrl != null && ctrl.IsDead) return;

        for (int i = 0; i < backgrounds.Length; i++)
        {
            Transform bg = backgrounds[i];
            bg.Translate(Vector3.left * (scrollSpeed * Time.deltaTime));
            if (bg.position.x <= -bgWidth)
            {
                Vector3 p = bg.position;
                p.x += bgWidth * backgrounds.Length;
                bg.position = p;
            }
        }

        if (_cam != null && _currentColor != _targetColor)
        {
            _currentColor         = Color.Lerp(_currentColor, _targetColor, Time.deltaTime * colorLerpSpeed);
            _cam.backgroundColor  = _currentColor;
        }
    }
}
