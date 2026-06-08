using UnityEngine;






public class Parallax : MonoBehaviour

{
    public float parrallax;
    public float timeCoof = 15f;
    public Camera cam;
    public bool enableVerticalParallax = false;
    private float startPos;
    private float startPosY;

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
        startPos = transform.position.x;
        startPosY = transform.position.y;
    }


    void Update()
    {
        float distX = (cam.transform.position.x * (1 - parrallax));
        float newY = transform.position.y;

        if (enableVerticalParallax)
        {
            float distY = (cam.transform.position.y * (1 - parrallax));
            newY = startPosY + distY;
        }
        Vector3 newPosition = new Vector3(startPos + distX, newY, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * timeCoof);
    }
}
