using System.Collections;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("연결 필수")]
    public Timer timerScript; // Timer 스크립트를 여기에 넣어주세요!

    [Header("플레이어 이동속도")]
    public float moveSpeed = 5f;

    [Header("플레이어 데미지 설정")]
    public float hitDamage = 5f; // 맞으면 게이지가 5초만큼 확 참

    [Header("무적 판정")]
    public float hitInvincibleTime = 0.5f;
    bool isInvincible = false;

    [Header("버블 공기 회복")]
    public float bubbleHeal = 5f;
    public float bubbleHealCoolTime = 2f;

    [Header("아이템 설정")]
    public int totalItems = 4;      // 총 아이템 개수
    private int currentItems = 0;   // 현재 먹은 개수
    private bool allItemsCollected = false;

    Coroutine bubbleHealCo;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Vector3 startPosition; // 시작 지점 저장

    public GameObject gameClearUI;

    bool isInBubble = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.gravityScale = 0f; 
            rb.freezeRotation = true; 
        }
    }

    void Start()
    {
        // 만약 Timer를 연결 안 했으면 자동으로 찾기
        if (timerScript == null)
            timerScript = FindFirstObjectByType<Timer>();

        startPosition = transform.position;
    }

    void Update()
    {
        // HandleAir();  <-- 삭제됨 (Timer가 알아서 함)

        // 이동 입력
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        // 방향 전환
        if (moveInput.x < 0) spriteRenderer.flipX = true;
        else if (moveInput.x > 0) spriteRenderer.flipX = false;

        if (allItemsCollected)
        {
            float distance = Vector3.Distance(transform.position, startPosition);
            if (distance < 1.0f) // 시작 지점 반경 1미터 이내로 들어오면
            {
                WinGame();
            }
        }
    
    }

    void FixedUpdate()
    {
        // 이동
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }


    void WinGame()
    {
        Debug.Log("게임 클리어!");
        if (gameClearUI != null) gameClearUI.SetActive(true);
        Time.timeScale = 0; // 게임 정지
    }

    // 데미지 입는 함수
    public void TakeDamage(float amount)
    {
        if(isInvincible) return;

        // Timer에게 데미지를 적용하라고 명령
        if (timerScript != null)
        {
            timerScript.ApplyDamage(amount);
        }

        StartCoroutine(hitRoutine());
    }

    System.Collections.IEnumerator hitRoutine()
    {
        isInvincible = true;

        // 무적 시간 동안 깜빡거리는 효과 등을 넣을 수 있음
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = new Color(1, 0, 0, 0.5f); // 빨간색 반투명

        yield return new WaitForSeconds(hitInvincibleTime);

        spriteRenderer.color = originalColor; // 원상복구
        isInvincible = false;
    }

    /*public void AddAir(float amount)
    {
        currentTime += amount;
        currentTime = Mathf.Clamp(currentTime, 0, maxTime);
    }*/

    private void OnTriggerEnter2D(Collider2D other)
    {
        //아이템 먹기
        if (other.CompareTag("Item"))
        {
            currentItems++;
            Destroy(other.gameObject); // 아이템 삭제
            Debug.Log($"아이템 획득! ({currentItems}/{totalItems})");

            if (currentItems >= totalItems)
            {
                allItemsCollected = true;
                Debug.Log("모든 재료를 모았다! 시작 지점으로 돌아가자!");
            }

            return;
        }

        if (other.CompareTag("Bubble"))
        {
            if (timerScript != null) timerScript.SetInBubble(true);

            if (bubbleHealCo == null)
                bubbleHealCo = StartCoroutine(BubbleHealRoutine());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bubble"))
        {
            if (timerScript != null) timerScript.SetInBubble(false);

            if (bubbleHealCo != null)
            {
                StopCoroutine(bubbleHealCo);
                bubbleHealCo = null;
            }
        }
    }
    
    IEnumerator BubbleHealRoutine()
    {
        while (true)
        {
            if (timerScript != null)
            {
                timerScript.AddAir(bubbleHeal);
            }
            yield return new WaitForSeconds(bubbleHealCoolTime);
        }
    }

}