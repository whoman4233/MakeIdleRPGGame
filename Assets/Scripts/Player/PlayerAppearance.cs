using UnityEngine;

/// <summary>
/// 측면 본 구조 플레이어 외형 교체.
/// GraftSlotType → 본 부위 매핑:
///   Head → Head / Core → Torso / ArmL → ArmBack / ArmR → ArmFront / Legs → LegFront+LegBack
/// </summary>
[RequireComponent(typeof(PlayerGraft))]
public class PlayerAppearance : MonoBehaviour
{
    [Header("Bone Renderers (비우면 자동 탐색)")]
    public SpriteRenderer headRenderer;
    public SpriteRenderer coreRenderer;
    public SpriteRenderer armBackRenderer;
    public SpriteRenderer armFrontRenderer;
    public SpriteRenderer legBackRenderer;
    public SpriteRenderer legFrontRenderer;

    [Header("Default Sprites")]
    public Sprite defaultHead, defaultCore, defaultArmBack, defaultArmFront, defaultLegBack, defaultLegFront;

    private PlayerGraft _graft;

    private void Awake(){ _graft=GetComponent<PlayerGraft>(); AutoFind(); }
    private void OnEnable(){ if(_graft!=null) _graft.OnGraftChanged+=Refresh; }
    private void OnDisable(){ if(_graft!=null) _graft.OnGraftChanged-=Refresh; }
    private void Start()=>Refresh();

    private void AutoFind()
    {
        var hips=transform.Find("Model/Rig/Hips"); if(hips==null) return;
        var torso=hips.Find("Torso");
        if(torso!=null){
            if(headRenderer==null)     headRenderer=Get(torso,"Head");
            if(coreRenderer==null)     coreRenderer=torso.GetComponent<SpriteRenderer>();
            if(armBackRenderer==null)  armBackRenderer=Get(torso,"ArmBack");
            if(armFrontRenderer==null) armFrontRenderer=Get(torso,"ArmFront");
        }
        if(legBackRenderer==null)  legBackRenderer=Get(hips,"LegBack");
        if(legFrontRenderer==null) legFrontRenderer=Get(hips,"LegFront");
    }
    private SpriteRenderer Get(Transform p,string n){ var t=p.Find(n); return t!=null?t.GetComponent<SpriteRenderer>():null; }

    public void Refresh()
    {
        if(_graft==null) return;
        Apply(headRenderer,     _graft.GetEquipped(GraftSlotType.Head), defaultHead);
        Apply(coreRenderer,     _graft.GetEquipped(GraftSlotType.Core), defaultCore);
        Apply(armBackRenderer,  _graft.GetEquipped(GraftSlotType.ArmL), defaultArmBack);
        Apply(armFrontRenderer, _graft.GetEquipped(GraftSlotType.ArmR), defaultArmFront);
        // 다리는 Legs 슬롯 하나로 양쪽 동시 (앞/뒤 같은 스프라이트면 자연스러움)
        var legG=_graft.GetEquipped(GraftSlotType.Legs);
        Apply(legBackRenderer,  legG, defaultLegBack);
        Apply(legFrontRenderer, legG, defaultLegFront);
    }

    private void Apply(SpriteRenderer sr, GraftData g, Sprite fb)
    {
        if(sr==null) return;
        bool has = g!=null && g.EffectiveAppearanceSprite!=null;
        Sprite tgt = has? g.EffectiveAppearanceSprite : fb;
        if(tgt!=null){ sr.sprite=tgt; sr.color=(has&&g.appearanceSprite!=null)?g.appearanceColor:Color.white; }
        else { sr.color=Color.clear; }
    }

    [ContextMenu("슬롯 재연결")]
    private void ReFind(){ headRenderer=coreRenderer=armBackRenderer=armFrontRenderer=legBackRenderer=legFrontRenderer=null; AutoFind(); }
}
