using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float parrallax;
    public Camera cam;
    public bool enableVerticalParallax = false; // Toggle for Y-axis movement
    private float startPos;
    private float startPosY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
        startPos = transform.position.x;
        startPosY = transform.position.y;
    }

    // Update is called once per frame
    void FixedUpdate() //Android: LateUnpdate
    {
        float distX = (cam.transform.position.x * (1 - parrallax));
        float newY = transform.position.y;
        
        if (enableVerticalParallax)
        {
            float distY = (cam.transform.position.y * (1 - parrallax));
            newY = startPosY + distY;
        }

        transform.position = new Vector3(startPos + distX, newY, transform.position.z);
    }
}