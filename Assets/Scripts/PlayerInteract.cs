using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    void Awake()
    {
        // 컴포넌트 참조
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Rigidbody2D 설정 자동화
        if (rb != null)
        {
            rb.gravityScale = 0f; // 2D 횡스크롤이 아니면 중력 0
            rb.freezeRotation = true; // 물리 충돌로 캐릭터가 회전하는 것 방지
        }
    }

    void Update()
    {
        // 1. 키보드 입력 받기 (W,A,S,D 또는 화살표)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 입력값 정규화 (대각선 이동 시 빨라지는 것 방지)
        moveInput = moveInput.normalized;

        // 2. 이동 방향에 따라 캐릭터 이미지 반전
        if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true; // 왼쪽
        }
        else if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false; // 오른쪽
        }
    }

    void FixedUpdate()
    {
        // 3. 물리 엔진을 이용한 부드러운 이동
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}