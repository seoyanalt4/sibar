using System.Collections;
using UnityEngine;

public class Monster2DController : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Returning }

    [Header("--- 상태 및 속도 ---")]
    public State currentState = State.Patrol;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4.5f;

    [Header("--- 범위 및 시야 설정 ---")]
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public LayerMask obstacleLayer; // Wall 레이어 등
    public bool useLineOfSight = true;

    [Header("--- 공격 설정 ---")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    [Header("--- 공격 후 딜레이(멈춤) ---")]
    public float AttackCoolTime = 1f; // 공격 후 멈추는 시간(=너가 원한 1초)
    bool isAttacked = false;
    Coroutine attackedCo;

    [Header("--- 순찰 설정 ---")]
    public Transform[] patrolPoints;
    public float waitTime = 1.5f;
    private int pointIndex = 0;
    private float waitTimer;

    [Header("--- 참조 ---")]
    public Transform player; // 인스펙터로 넣어도 되고 자동 탐색됨


    [Header("--- 변신(플레이어 hasShell 연동) ---")]
    public bool transformByPlayerShell = true;

    public SpriteRenderer targetRenderer;   // 몬스터 외형 바꿀 대상
    public Animator targetAnimator;

    [Header("평상시(얌전)")]
    public Sprite calmSprite;
    public RuntimeAnimatorController calmController;

    [Header("괴물(변신)")]
    public Sprite monsterSprite;
    public RuntimeAnimatorController monsterController;

    PlayerShell playerShell;     // 플레이어의 PlayerShell 캐싱
    bool lastPlayerHasShell;

    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    Vector2 originPos;

    Vector2 moveTarget;
    float moveSpeed;
    bool hasMoveTarget;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (targetRenderer == null) targetRenderer = spriteRenderer;
        if (targetAnimator == null) targetAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        originPos = transform.position;
        waitTimer = waitTime;

        // 최초 1회 플레이어 찾기
        TryFindPlayer();

        CachePlayerShell();
        ApplyFormByPlayerShell(force:true);
    }

    void Update()
    {
        ApplyFormByPlayerShell();

        /*if (player != null && !player.CompareTag("Player"))
        {
            player = null;
        }*/
        
        // 공격 후 멈춤 시간 동안은 AI 정지
        if (isAttacked) return;

        // 플레이어가 없으면(태그 변경/crab 등) 다시 찾기
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            TryFindPlayer();
        }

        float distanceToPlayer = 0f;
        bool canSeePlayer = false;

        // 플레이어가 있을 때만 거리/시야 체크
        if (player != null && player.gameObject.activeInHierarchy)
        {
            distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRange)
            {
                canSeePlayer = useLineOfSight ? HasLineOfSight() : true;
            }
        }

        // 상태 결정
        if (canSeePlayer)
        {
            if (distanceToPlayer <= attackRange) currentState = State.Attack;
            else currentState = State.Chase;
        }
        else
        {
            if (currentState == State.Chase || currentState == State.Attack)
            {
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

    void FixedUpdate()
    {
        if (!hasMoveTarget) return;
        if (isAttacked) return;

        Vector2 newPos = Vector2.MoveTowards(rb.position, moveTarget, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        hasMoveTarget = false; // 매 프레임 목표 다시 받는 구조
    }

        void CachePlayerShell()
        {
            // if (player == null) TryFindPlayer();
            if (player != null)
            playerShell = FindFirstObjectByType<PlayerShell>();
        }

    void ApplyFormByPlayerShell(bool force = false)
    {
        if (!transformByPlayerShell) return;

        if (playerShell == null) CachePlayerShell();
        if (playerShell == null) return;

        bool pHasShell = playerShell.hasShell;

        if (!force && pHasShell == lastPlayerHasShell) return;
        lastPlayerHasShell = pHasShell;

        
        Sprite nextSprite = pHasShell ? calmSprite : monsterSprite;
        RuntimeAnimatorController nextCtrl = pHasShell ? calmController : monsterController;

        
        int prevHash = 0;
        float prevTime = 0f;
        bool canCarry = (targetAnimator != null &&
                         targetAnimator.isActiveAndEnabled &&
                         targetAnimator.runtimeAnimatorController != null);

        if (canCarry)
        {
            var st = targetAnimator.GetCurrentAnimatorStateInfo(0);
            prevHash = st.fullPathHash;
            prevTime = st.normalizedTime;
        }

        if (targetRenderer != null && nextSprite != null)
            targetRenderer.sprite = nextSprite;

        if (targetAnimator != null)
        {
            targetAnimator.runtimeAnimatorController = nextCtrl;

            if (nextCtrl != null && canCarry && targetAnimator.HasState(0, prevHash))
            {
                targetAnimator.Play(prevHash, 0, prevTime);
                targetAnimator.Update(0f);
            }
            else if (nextCtrl != null)
            {
                targetAnimator.Rebind();
                targetAnimator.Update(0f);
            }
        }
    }


    void MoveTowards(Vector2 target, float speed)
    {
        moveTarget = target;
        moveSpeed = speed;
        hasMoveTarget = true;

        FlipSprite(target);
    }

  

    void TryFindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        player = (playerObj != null) ? playerObj.transform : null;
    }

    bool HasLineOfSight()
    {
        if (player == null) return false;

        Vector2 origin = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 direction = ((Vector2)player.position - origin).normalized;
        float distance = Vector2.Distance(origin, player.position);

        // 장애물 레이어에 막히면 못 봄
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, obstacleLayer);
        return hit.collider == null;
    }

    void HandleAttack()
    {
        if (player == null) return;

        FlipSprite(player.position);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    void AttackPlayer()
    {
        if (player == null) return;

        Debug.Log("몬스터가 플레이어를 공격합니다!");
        player.GetComponent<PlayerInteract>()?.TakeDamage(attackDamage);

        StartAttackCoolTime(); // ✅ 공격 후 1초 멈춤
    }

    void StartAttackCoolTime()
    {
        if (attackedCo != null) StopCoroutine(attackedCo);
        attackedCo = StartCoroutine(AttackedRoutine());
    }

    IEnumerator AttackedRoutine()
    {
        isAttacked = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        yield return new WaitForSeconds(AttackCoolTime);

        isAttacked = false;
        attackedCo = null;
    }

    void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

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
        if (player == null) return;
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



    void FlipSprite(Vector2 target)
    {
        float xDiff = target.x - transform.position.x;
        if (Mathf.Abs(xDiff) < 0.05f) return;
        spriteRenderer.flipX = xDiff < 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 시야 디버그 라인 (플레이 중엔 player가 null일 수 있음)
        if (useLineOfSight && player != null)
        {
            Gizmos.color = HasLineOfSight() ? Color.green : Color.gray;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
