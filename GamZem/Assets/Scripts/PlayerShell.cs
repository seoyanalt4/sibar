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

    private int playerLayer;
    private int obstacleLayer;

    private GameObject droppedShell;

    void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);
        obstacleLayer = LayerMask.NameToLayer(obstacleLayerName);

        ApplyShellCollision();
    }

    public void SetShell(bool wearing)
    {
        hasShell = wearing;
        ApplyShellCollision();
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
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(hasShell)
                DropShell();
            else
                TryPickupShell();
        }
    }

    void DropShell()
    {
        if(shellPrefab == null) return;

        hasShell = false;
        ApplyShellCollision();

        if(shellPrefab == null) return;

        droppedShell = Instantiate(shellPrefab, transform.position, Quaternion.identity);
    }

    void TryPickupShell()
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, pickup, shellPickupMask);
        if(col == null) return;

        hasShell = true;
        ApplyShellCollision();

        if(droppedShell != null)
            Destroy(droppedShell);
        else
            Destroy(col.gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pickup);
    }

}
