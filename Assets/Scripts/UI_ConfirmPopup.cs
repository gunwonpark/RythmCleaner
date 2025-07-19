using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_ConfirmPopup : MonoBehaviour
{
    private Image Image;

    private void Awake()
    {
        Image = GetComponent<Image>();
    }
    private void OnEnable()
    {
        Image.DOFade(0, 1f).SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                this.gameObject.SetActive(false);   
            });
    }
}
