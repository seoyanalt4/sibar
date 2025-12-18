using UnityEngine;
using UnityEngine.UI; 

public class Timer : MonoBehaviour
{
    [Header("필수 연결")]
    public Slider gaugeSlider;
    public PlayerShell playerShell; // 껍데기 상태 확인용

    [Header("시간 설정")]
    public float maxTime = 30.0f; // 최대 시간 (게이지 꽉 차는 시간)
    public float recoverySpeed = 5.0f; // 껍데기 안 회복 속도

    // '산소'이자 '시간'
    private float currentTime = 0f; 

    [Header("게임 오버")]
    public GameObject gameOverUI;
    private bool isGameOver = false;

    bool isInBubble = false;

    void Start()
    {
        if (gaugeSlider == null) gaugeSlider = GetComponent<Slider>();
        
        if (playerShell == null)
        {
            playerShell = FindFirstObjectByType<PlayerShell>();
        }

        currentTime = 0;
        
        if (gaugeSlider != null)
        {
            gaugeSlider.minValue = 0;
            gaugeSlider.maxValue = maxTime;
            gaugeSlider.value = currentTime;
        }

        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    void Update()
    {
        if (isGameOver) return;

        // 껍데기 상태 확인
        bool isSafe = (playerShell != null && playerShell.hasShell);

        if (!isInBubble)
        {
            if (isSafe)
            {
                // 껍데기 안: 게이지 감소(회복)
                currentTime -= Time.deltaTime * recoverySpeed;
                if (currentTime < 0) currentTime = 0;
            }
            else
            {
                // 껍데기 밖: 게이지 증가(질식)
                currentTime += Time.deltaTime;

                if (currentTime >= maxTime)
                {
                    currentTime = maxTime;
                    TriggerGameOver();
                }
            }
        }
    

        // 화면 반영 (버블이어도 UI는 갱신되게)
        if (gaugeSlider != null)
        gaugeSlider.value = currentTime;
    }


    public void SetInBubble(bool value)
    {
        isInBubble = value;
    }

    public void AddAir(float amount)
    {
        currentTime -= amount;
        currentTime = Mathf.Clamp(currentTime, 0, maxTime);
    }

    // 플레이어가 몬스터에게 맞았을 때 호출
    public void ApplyDamage(float damageAmount)
    {
        if (isGameOver) return;

        // 데미지를 입으면 시간이 추가되어 게임오버에 가까워짐
        currentTime += damageAmount;
        Debug.Log($"아야! 데미지 입음. 현재 게이지: {currentTime}/{maxTime}");

        // 데미지로 인해 최대 시간을 넘기면 게임오버
        if (currentTime >= maxTime)
        {
            currentTime = maxTime;
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        if (isGameOver) return; // 중복 실행 방지
        
        isGameOver = true; 
        Debug.Log("Game Over!");

        if (gameOverUI != null) gameOverUI.SetActive(true);
        Time.timeScale = 0; 
    }
}