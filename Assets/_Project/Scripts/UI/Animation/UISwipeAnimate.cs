using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEditor;






public class UISwipeAnimate : MonoBehaviour

{
    [SerializeField] private RectTransform menuPanel;
    [SerializeField] private float openPositionXY = 0f;
    [SerializeField] private float closedPositionXY = -800f;
    [SerializeField] private float duration = 0.5f;
    public bool isVertical = false;
    private void Start()
    {

        if(!isVertical) menuPanel.anchoredPosition = new Vector2(closedPositionXY, menuPanel.anchoredPosition.y);
        else menuPanel.anchoredPosition = new Vector2(menuPanel.anchoredPosition.x, closedPositionXY);
    }

    public void OpenMenu()
    {
        if(!isVertical) menuPanel.DOAnchorPosX(openPositionXY, duration).SetEase(Ease.OutCubic);
        else menuPanel.DOAnchorPosY(openPositionXY, duration).SetEase(Ease.OutCubic);
    }

    public void CloseMenu()
    {
        if(!isVertical) menuPanel.DOAnchorPosX(closedPositionXY, duration).SetEase(Ease.InSine);
        else menuPanel.DOAnchorPosY(closedPositionXY, duration).SetEase(Ease.InSine);
    }
}
