using UnityEngine;

public class UIFollowPlayer: MonoBehaviour
{
    public Transform target;       // 플레이어
    public RectTransform uiElement; // 게이지 바 UI
    public Vector3 offset;        

    void LateUpdate()
    {
        if (target == null) return;

        uiElement.position = target.position + offset;
    }
        
}
