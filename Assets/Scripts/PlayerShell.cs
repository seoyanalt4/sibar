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

    
    public bool startCrabTag = true;

    private int playerLayer;
    private int obstacleLayer;

    private GameObject droppedShell;

    void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);
        obstacleLayer = LayerMask.NameToLayer(obstacleLayerName);

        ApplyShellCollision();
        gameObject.tag = startCrabTag ? originTagName : playerTagName;
    }

    public void SetPlayerTag(bool asplayer)
    {
        gameObject.tag = asplayer ? playerTagName : originTagName;
    }

    public void SetShell(bool wearing)
    {
        hasShell = wearing;
        ApplyShellCollision();
        ApplyPlayerTag();
    }

    void ApplyPlayerTag()
    {
        gameObject.tag = hasShell ? playerTagName : originTagName;
    }

    void ApplyShellCollision()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, obstacleLayer, !hasShell);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            

            if (hasShell)
                DropShell();
            else
                TryPickupShell();
        }
    }

    void DropShell()
    {
        if (shellPrefab == null) return;

        hasShell = false;
        ApplyShellCollision();
        SetPlayerTag(true);

        droppedShell = Instantiate(shellPrefab, transform.position, Quaternion.identity);
    }

    void TryPickupShell()
    {
        if(Physics2D.OverlapCircle(transform.position, obstacleCheckRadius, obstacleMask) != null)
        {
            return;
        }

        Collider2D col = Physics2D.OverlapCircle(transform.position, pickup, shellPickupMask);
        if (col == null) return;

        hasShell = true;
        ApplyShellCollision();
        SetPlayerTag(false);

        if (droppedShell != null)
            Destroy(droppedShell);
        else
            Destroy(col.gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pickup);
        Gizmos.DrawWireSphere(transform.position, obstacleCheckRadius);
    }

}
