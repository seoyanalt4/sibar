using UnityEngine;
using UnityEngine.UI; 

public class Timer : MonoBehaviour
{
    public Slider gaugeSlider;  
    public PlayerShell playerShell; // 껍데기 상태
    public float maxTime = 30.0f; // 게이지가 다 닳는 데 걸리는 시간
    public float recoverySpeed = 1.0f; // 안전할 때 게이지가 줄어드는 속도
    private float currentTime;  // 현재 남은 시간

    [Header("게임 오버 설정")]
    public GameObject gameOverUI; // 게임 오버 시 띄울 UI
    private bool isGameOver = false; // 중복 실행 방지
    void Start()
    {
        // 슬라이더 연결 안되어 있으면 본인 몸에서 찾기
        if (gaugeSlider == null)
            gaugeSlider = GetComponent<Slider>();

        if (playerShell == null)
            playerShell = FindAnyObjectByType<PlayerShell>();
        // 시간 초기화
        currentTime = 0;
        
        // 슬라이더 설정 초기화
        gaugeSlider.maxValue = maxTime; // 슬라이더 최대값을 시간과 맞춤
        gaugeSlider.value = currentTime; // 꽉 채운 상태로 시작

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    void Update()
    {
       if (isGameOver) return;

       bool isSafe = false;

        // PlayerShell 스크립트가 연결되어 있다면 상태 확인
        if (playerShell != null)
        {
            isSafe = playerShell.hasShell;
        }

        if (isSafe)
        {
            // 안전 상태면 게이지가 서서히 줄어듦 (회복)
            if (currentTime > 0)
            {
                // recoverySpeed만큼 빠르게 줄어듦 (숫자를 키우면 더 빨리 회복됨)
                currentTime -= Time.deltaTime * recoverySpeed;
            }
            
            // 0보다 작아지지 않게 고정
            if (currentTime < 0) currentTime = 0;
        }
        else
        {
            // 위험 상태에선 게이지가 차오름
            if (currentTime < maxTime)
            {
                currentTime += Time.deltaTime; 
            }
            else
            {
                // 꽉 찼을 때 처리
                currentTime = maxTime;
                OnGameOver();
            }
        // 슬라이더에 반영
        gaugeSlider.value = currentTime;
    }
    void OnGameOver()
    {
        isGameOver = true; 
        Debug.Log("Game Over!");

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        
        Time.timeScale = 0; 
    }
}
}