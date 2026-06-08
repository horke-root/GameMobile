using UnityEngine;

public class BackgroundAnimator : MonoBehaviour
{
    public GameObject backgroundObject;
    public float scrollSpeed = 0.5f;
    private float startPosX;
    public float resetPosX = -12.2f;



    void Start()
    {
        startPosX = backgroundObject.transform.position.x;
    }


    void FixedUpdate()
    {
        if (backgroundObject.transform.position.x <= resetPosX)
        {
            Vector3 resetPosition = backgroundObject.transform.position;
            resetPosition.x = startPosX;
            backgroundObject.transform.position = resetPosition;
        } else backgroundObject.transform.Translate(Vector3.left * scrollSpeed);
    }
}
