using UnityEngine;

public class Monster2DController : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Returning }

    [Header("--- 상태 및 속도 ---")]
    public State currentState = State.Patrol;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4.5f;
    public float detectionRange = 5f;

    [Header("--- 공격 설정 ---")]
    public float attackRange = 1.2f;    // 공격 사거리
    public int attackDamage = 10;      // 공격 데미지
    public float attackCooldown = 1.5f; // 공격 간격 (초)
    private float lastAttackTime;

    [Header("--- 순찰 및 대기 ---")]
    public Transform[] patrolPoints; // 순찰 지점들
    public float waitTime = 1.5f; // 순찰 지점에서 머무르는 시간
    private int pointIndex = 0; // 몇번째 지점으로 가야하는지 알려주는 것
    private float waitTimer; // 한 지점에서 기다리는 시간 계산 -> 그 후 다시 다음 지역 이동

    [Header("--- 참조 ---")]
    public Transform player; // 플레이어의 위치 정보
    private SpriteRenderer spriteRenderer; // 몬스터의 이미지 관리
    private Vector2 originPos; // 몬스터가 처음 배치된 위치 기억

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        originPos = transform.position;
        waitTimer = waitTime;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform; // 실수로 Player 안집어 넣었으면 자동으로 찾기
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- 상태 결정 로직 ---
        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attack;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            currentState = State.Chase;
        }
        else if (currentState == State.Chase || currentState == State.Attack)
        {
            if (distanceToPlayer > detectionRange)
                currentState = State.Returning;
        }

        // --- 상태별 행동 실행 ---
        switch (currentState)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
            case State.Returning: HandleReturn(); break;
        }
    }

    void HandleAttack()
    {
        // 공격 상태에 진입하면 이동을 멈춤
        // 처음 공격 상태가 되었을 때 한 번만 출력하고 싶다면 아래와 같이 작성
        Debug.Log("<color=red>[공격]</color> 플레이어 감지! 몬스터가 이동을 멈추고 공격을 시작합니다.");

        // 몬스터가 플레이어를 바라보게 설정 (이미지 반전)
        if (player.position.x < transform.position.x) spriteRenderer.flipX = true;
        else if (player.position.x > transform.position.x) spriteRenderer.flipX = false;

        // 공격 쿨타임 체크 후 공격 실행
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    void AttackPlayer()
    {
        Debug.Log("몬스터가 플레이어를 공격합니다!");

        // 플레이어의 Health 스크립트를 가져와 데미지 입힘
        // PlayerHealth health = player.GetComponent<PlayerHealth>();
        //if (health != null)
        {
           // health.TakeDamage(attackDamage); // health에 있는 TakeDamage실행
        }

        // 여기에 공격 애니메이션 실행 코드 추가:
        // GetComponent<Animator>().SetTrigger("Attack");
    }

    void HandlePatrol()
    {
        if (patrolPoints.Length == 0) return; // 순찰 루트 없으면 그냥 가만히 있어
        Vector2 target = patrolPoints[pointIndex].position;
        MoveTowards(target, patrolSpeed);
        if (Vector2.Distance(transform.position, target) < 0.1f) // 지점이랑 몬스터거리가 0.1 안이야?
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
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
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
    }
}