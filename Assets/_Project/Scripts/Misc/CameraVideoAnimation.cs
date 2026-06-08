using UnityEngine;

public class CameraVideoAnimation : MonoBehaviour
{
    Transform cam;
    public float speed = 0.1f;
    private void Start()
    {
        cam = Camera.main.transform;
    }
    private void Onnable()
    {
        cam = Camera.main.transform;
    }

    void FixedUpdate()
    {
        cam.position = new Vector3(cam.position.x+speed, cam.position.y, cam.position.z);
    }
}
