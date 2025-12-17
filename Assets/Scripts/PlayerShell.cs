using UnityEngine;

public class PlayerShell : MonoBehaviour
{
    [Header("껍데기 착용 여부")]
    public bool hasShell = true;

    [Header("껍데기 드랍 프리팹")]
    public GameObject shellPrefab;

    [Header("줍기 거리")]
    public float pickup = 1.0f;

    [Header("레이어 이름")]
    public string playerLayerName = "Player";
    public string obstacleLayerName = "Obstacle";
    public LayerMask shellPickupMask;

    [Header("태그 설정")]
    public string playerTagName = "Player";
    public string originTagName = "Crab";

    [Header("껍질 착용 금지")]
    public LayerMask obstacleMask;
    public float obstacleCheckRadius = 0.15f;

    [Header("바꿔치기 대상(현재 플레이어 외형)")]
    public SpriteRenderer targetRenderer;
    public Animator targetAnimator;

    [Header("껍데기 ON(착용) 세트")]
    public Sprite shellOnSprite;
    public RuntimeAnimatorController shellOnController;

    [Header("껍데기 OFF(미착용) 세트")]
    public Sprite shellOffSprite;
    public RuntimeAnimatorController shellOffController;

    public bool startCrabTag = true;

    private int playerLayer;
    private int obstacleLayer;

    private GameObject droppedShell;

    void Awake()
    {
        // 자동 연결 (인스펙터에서 안 넣었을 때 대비)
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<SpriteRenderer>();
        if (targetAnimator == null) targetAnimator = GetComponentInChildren<Animator>();

        ApplyShellGraphics();

        playerLayer = LayerMask.NameToLayer(playerLayerName);
        obstacleLayer = LayerMask.NameToLayer(obstacleLayerName);

        ApplyShellCollision();
        gameObject.tag = startCrabTag ? originTagName : playerTagName;
    }

    public void SetShell(bool wearing)
    {
        hasShell = wearing;
        ApplyShellCollision();
        ApplyPlayerTag();
        ApplyShellGraphics();
    }

    void ApplyPlayerTag()
    {
        gameObject.tag = hasShell ? originTagName : playerTagName;
    }

    void ApplyShellCollision()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, obstacleLayer, !hasShell);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (hasShell) DropShell();
            else TryPickupShell();
        }
    }

    void ApplyShellGraphics()
    {
        if (targetRenderer == null) return;

        // 교체 전 애니 상태 저장(가능하면)
        int prevHash = 0;
        float prevTime = 0f;
        bool canCarryAnim = (targetAnimator != null &&
                            targetAnimator.isActiveAndEnabled &&
                            targetAnimator.runtimeAnimatorController != null);

        if (canCarryAnim)
        {
            var st = targetAnimator.GetCurrentAnimatorStateInfo(0);
            prevHash = st.fullPathHash;
            prevTime = st.normalizedTime;
        }

        // ✅ Sprite 교체
        targetRenderer.sprite = hasShell ? shellOnSprite : shellOffSprite;

        // ✅ AnimatorController 교체
        if (targetAnimator != null)
        {
            var nextController = hasShell ? shellOnController : shellOffController;
            targetAnimator.runtimeAnimatorController = nextController;

            // 같은 state가 있으면 이어서 재생
            if (nextController != null && canCarryAnim && targetAnimator.HasState(0, prevHash))
            {
                targetAnimator.Play(prevHash, 0, prevTime);
                targetAnimator.Update(0f);
            }
            else if (nextController != null)
            {
                // 없으면 기본 상태로 리바인드
                targetAnimator.Rebind();
                targetAnimator.Update(0f);
            }
        }
    }

    void DropShell()
    {
        if (shellPrefab == null) return;

        SetShell(false);
        droppedShell = Instantiate(shellPrefab, transform.position, Quaternion.identity);
    }

    void TryPickupShell()
    {
        if (Physics2D.OverlapCircle(transform.position, obstacleCheckRadius, obstacleMask) != null)
            return;

        Collider2D col = Physics2D.OverlapCircle(transform.position, pickup, shellPickupMask);
        if (col == null) return;

        SetShell(true);

        if (droppedShell != null) Destroy(droppedShell);
        else Destroy(col.gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pickup);
        Gizmos.DrawWireSphere(transform.position, obstacleCheckRadius);
    }
}
