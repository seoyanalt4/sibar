using UnityEngine;

public class Monster2DController : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Returning }

    private Rigidbody2D rb;

    [Header("--- 상태 및 속도 ---")]
    public State currentState = State.Patrol;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4.5f;

    [Header("--- 범위 및 시야 설정 ---")]
    public float detectionRange = 5f;   // 감지 범위
    public float attackRange = 1.2f;      // 공격 범위
    public LayerMask obstacleLayer;     // 장애물 레이어 (벽 등)

    [Header("--- 공격 설정 ---")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    [Header("--- 순찰 설정 ---")]
    public Transform[] patrolPoints;
    public float waitTime = 1.5f;
    private int pointIndex = 0;
    private float waitTimer;

    [Header("--- 참조 ---")]
    public Transform player;
    private SpriteRenderer spriteRenderer;
    private Vector2 originPos;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 가져오기
    }

    void Start()
    {
        originPos = transform.position;
        waitTimer = waitTime;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 상태 설정 로직
        bool canSeePlayer = false;

        // 거리가 범위 안일 때만 시야 체크(Raycast)를 실행하여 최적화
        if (distanceToPlayer <= detectionRange)
        {
            canSeePlayer = HasLineOfSight();
        }

        if (canSeePlayer)
        {
            if (distanceToPlayer <= attackRange)
            {
                if (currentState != State.Attack) Debug.Log("<color=red>공격 상태 진입!</color>");
                currentState = State.Attack;
            }
            else
            {
                currentState = State.Chase;
            }
        }
        else
        {
            // 플레이어가 범위를 벗어났거나 장애물 뒤에 숨었을 때
            if (currentState == State.Chase || currentState == State.Attack)
            {
                Debug.Log("<color=yellow>타겟 상실:</color> 복귀합니다.");
                currentState = State.Returning;
            }
        }

        // 상태별 행동
        switch (currentState)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
            case State.Returning: HandleReturn(); break;
        }
    }

    // 시야 체크 (Raycast)
    bool HasLineOfSight()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        // 몬스터 위치에서 플레이어 방향으로 레이저를 쏩니다.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);

        // 레이저가 장애물 레이어에 걸리는 게 없다면 플레이어가 보이는 것
        if (hit.collider == null)
        {
            Debug.Log("플레이어가 보입니다.");
        }
        return hit.collider == null;
    }

    void HandleAttack()
    {
        // 공격 중 이동 중지 및 방향 전환
        FlipSprite(player.position);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    void AttackPlayer()
    {
        //PlayerHealth health = player.GetComponent<PlayerHealth>();
        //if (health != null)
        {
        //    health.TakeDamage(attackDamage);
        //    Debug.Log("플레이어에게 데미지를 입혔습니다!");
        }
    }

    void HandlePatrol()
    {
        if (patrolPoints.Length == 0) return;
        Vector2 target = patrolPoints[pointIndex].position;
        MoveTowards(target, patrolSpeed);

        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                pointIndex = (pointIndex + 1) % patrolPoints.Length;
                waitTimer = waitTime;
            }
        }
    }

    void HandleChase()
    {
        MoveTowards(player.position, chaseSpeed);
    }

    void HandleReturn()
    {
        MoveTowards(originPos, patrolSpeed);
        if (Vector2.Distance(transform.position, originPos) < 0.1f)
        {
            currentState = State.Patrol;
        }
    }

    void MoveTowards(Vector2 target, float speed)
    {
        // transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime); 움직이는 방식 변경
        Vector2 currentPos = rb.position;
        Vector2 newPos = Vector2.MoveTowards(currentPos, target, speed * Time.deltaTime);
        rb.MovePosition(newPos); // 벽에 부딪히면 멈추게 함

        FlipSprite(target);
    }

    void FlipSprite(Vector2 target)
    {
        if (target.x < transform.position.x) spriteRenderer.flipX = true;
        else if (target.x > transform.position.x) spriteRenderer.flipX = false;
    }

    private void OnDrawGizmosSelected()
    {
        // 감지 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        // 공격 범위 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 시야 레이저 시각화 (플레이어가 있을 때만)
        if (player != null)
        {
            Gizmos.color = HasLineOfSight() ? Color.green : Color.grey;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}