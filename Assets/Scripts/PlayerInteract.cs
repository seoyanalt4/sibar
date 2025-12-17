using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("플레이어 이동속도")]
    public float moveSpeed = 5f;

    [Header("플레이어 산소 수치")]
    public float maxAir = 100f;
    public float currentAir;
    public float airDecreasePerSecond = 1f;
    public float hitDamage = 10f;

    [Header("무적 판정")]
    public float hitInvincibleTime = 0.5f;
    bool isInvincible = false;

    bool isTouchingMonster = false;

    int mobTouchCount = 0;


    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    void Start()
    {
        currentAir = maxAir;
    }

    public void TakeDamage(float amount)
    {
        if(isInvincible) return;

        currentAir -= amount;
        currentAir = Mathf.Clamp(currentAir, 0, maxAir);

        if(currentAir <= 0)
        {
            Die();
        }

        StartCoroutine(hitRoutine());
    }

    System.Collections.IEnumerator hitRoutine()
    {
        Debug.Log($"데미지 닳음. 현재 체력 = {currentAir}");
        isInvincible = true;
        yield return new WaitForSeconds(hitInvincibleTime);
        isInvincible = false;
    }

    void Awake()
    {
        // ������Ʈ ����
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Rigidbody2D ���� �ڵ�ȭ
        if (rb != null)
        {
            rb.gravityScale = 0f; // 2D Ⱦ��ũ���� �ƴϸ� �߷� 0
            rb.freezeRotation = true; // ���� �浹�� ĳ���Ͱ� ȸ���ϴ� �� ����
        }
    }

    void Update()
    {
        HandleAir();

        // 1. Ű���� �Է� �ޱ� (W,A,S,D �Ǵ� ȭ��ǥ)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // �Է°� ����ȭ (�밢�� �̵� �� �������� �� ����)
        moveInput = moveInput.normalized;

        // 2. �̵� ���⿡ ���� ĳ���� �̹��� ����
        if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true; // ����
        }
        else if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false; // ������
        }
    }

    void HandleAir()
    {
        if(CompareTag("Player"))
        {
            currentAir -= airDecreasePerSecond * Time.deltaTime;
            currentAir = Mathf.Clamp(currentAir, 0, maxAir);
            Debug.Log($"Player의 남은 산소 : {currentAir}");

            if(currentAir <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        Debug.Log("산소 부족 사망");
    }

    public void AddMobTouch()
    {
        mobTouchCount++;
    }

    public void ReduceMobTouch()
    {
        mobTouchCount--;
        if(mobTouchCount < 0)
        {
            mobTouchCount = 0;
        }
    }

    void FixedUpdate()
    {
        // 3. ���� ������ �̿��� �ε巯�� �̵�
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}