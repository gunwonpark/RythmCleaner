using UnityEngine;
using UnityEngine.EventSystems;

public class HoverImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject exitImage; // 이미지 오브젝트

    public void OnPointerEnter(PointerEventData eventData)
    {
        exitImage.SetActive(true); // 마우스가 이미지 위에 올라가면 이미지 활성화
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        exitImage.SetActive(false); // 마우스가 이미지에서 벗어나면 이미지 비활성화
    }
}
