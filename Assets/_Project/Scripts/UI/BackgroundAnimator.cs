using UnityEngine;

public class BackgroundAnimator : MonoBehaviour
{
    public GameObject backgroundObject;
    public float scrollSpeed = 0.5f;
    private float startPosX;
    public float resetPosX = -12.2f; // Example reset position


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosX = backgroundObject.transform.position.x;
    }

    // Update is called once per frame
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
