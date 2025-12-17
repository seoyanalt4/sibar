using UnityEngine;
using UnityEngine.UI;

public class AirNoiseManager : MonoBehaviour
{
    [Header("상태")]
    public bool isShellSafe; // 현재 껍데기 상태 (True면 안전, False면 위험)
    public float currentAlpha; // 현재 투명도 (0이면 안보임, 1이면 보임)

    [Header("플레이어 주변 설정")]
    [Range(0, 1)] public float holeRadius = 0.2f;

    [Header("연결")]
    public PlayerShell playerShell; 
    public RawImage noiseImage;

    private Texture2D noiseTexture;
    private Material noiseMaterial;

    void Start()
    {
        // 자동 연결
        if (playerShell == null) playerShell = FindAnyObjectByType<PlayerShell>();
        if (noiseImage == null) noiseImage = GetComponent<RawImage>();

        // 텍스처 생성
        noiseTexture = GenerateNoiseTexture(256, 256);
        noiseImage.texture = noiseTexture;
        noiseMaterial = noiseImage.material;
    }

    void Update()
    {
        UpdateNoiseEffect();
    }

    void UpdateNoiseEffect()
    {
        // 껍데기 상태 확인
        if (playerShell != null)
        {
            isShellSafe = playerShell.hasShell;
        }

        // 투명도 결정 로직
        // 껍데기가 없으면(false) -> 보인다(1)
        if (!isShellSafe)
        {
            currentAlpha = 1f;
        }
        else
        {
            currentAlpha = 0f;
        }

        // 색상 및 투명도 적용
        Color finalColor = Color.black;
        finalColor.a = currentAlpha; 
        noiseImage.color = finalColor;

        // 노이즈 흔들기 & 쉐이더 값 전달
        if (currentAlpha > 0)
        {
            float shakeX = Random.Range(0f, 1f);
            float shakeY = Random.Range(0f, 1f);
            noiseImage.uvRect = new Rect(shakeX, shakeY, 1f, 1f); //노이즈 이미지 좌표 랜덤하게

            if (playerShell != null && noiseMaterial != null)
            {
                // 플레이어 위치 전달
                Vector3 viewportPos = Camera.main.WorldToViewportPoint(playerShell.transform.position);
                noiseMaterial.SetVector("_PlayerPos", viewportPos);
                noiseMaterial.SetFloat("_HoleRadius", holeRadius);
                noiseMaterial.SetColor("_NoiseColor", finalColor);
            }
        }
        else
        {
            noiseImage.uvRect = new Rect(0, 0, 1f, 1f);
        }
    }

    Texture2D GenerateNoiseTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point; 
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = (Random.value > 0.5f) ? Color.white : Color.clear;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}