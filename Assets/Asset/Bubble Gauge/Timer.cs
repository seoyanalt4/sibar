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
        bool isSafe = false;
        if (playerShell != null) isSafe = playerShell.hasShell;

        // 시간 계산 로직
        if (isSafe)
        {
            // 껍데기 안 = 회복 -> 게이지 감소 (위로 사라짐)
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime * recoverySpeed;
            }
            if (currentTime < 0) currentTime = 0;
        }
        else
        {
            // 껍데기 밖 = 위험 -> 게이지 증가 (아래로 차오름)
            if (currentTime < maxTime)
            {
                currentTime += Time.deltaTime;
            }
            else
            {
                TriggerGameOver();
            }
        }

        //  화면 반영
        if (gaugeSlider != null)
        {
            gaugeSlider.value = currentTime;
        }
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