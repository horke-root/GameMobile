using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(BoxCollider2D))]
public class BikeSticker : MonoBehaviour
{
    [Header("Настройки")]
    public float scaleSpeed = 0.005f;
    public float minScale = 0.2f;
    public float maxScale = 3.0f;
    public float scaleStep = 0.1f;
    public bool editable = false;

    public string guid;
    public string stickerId;

    public LineRenderer colliderView;

    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCam;
    private Vector3 scaleStepVector;

    private void Awake()
    {
        mainCam = Camera.main;


        if (scaleStep <= 0f) scaleStep = 0.1f;
        if (scaleSpeed <= 0f) scaleSpeed = 0.005f;
        if (minScale <= 0f) minScale = 0.2f;
        if (maxScale <= minScale) maxScale = 3.0f;

        scaleStepVector = new Vector3(scaleStep, scaleStep, scaleStep);
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    public void EnableCollider() => colliderView.enabled = true;
    public void DisableCollider() => colliderView.enabled = false;

    void Update()
    {
        if (!editable) return;

        HandleDragging();
        HandleScaling();
    }

    private void HandleDragging()
    {
        if (Pointer.current == null) return;

        Vector2 pointerPos = Pointer.current.position.ReadValue();

        if (Pointer.current.press.wasPressedThisFrame)
        {
            Vector3 worldPos = mainCam.ScreenToWorldPoint(pointerPos);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null && hit.gameObject == gameObject)
            {
                isDragging = true;
                offset = transform.position - worldPos;
            }
        }

        if (Pointer.current.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging && Pointer.current.press.isPressed)
        {
            Vector3 worldPos = mainCam.ScreenToWorldPoint(pointerPos);
            worldPos.z = 0f;
            transform.position = worldPos + offset;
        }
    }

    private void HandleScaling()
    {



        if (Touch.activeTouches.Count >= 2 && isDragging)
        {
            Touch touchZero = Touch.activeTouches[0];
            Touch touchOne = Touch.activeTouches[1];

            Vector2 touchZeroPrevPos = touchZero.screenPosition - touchZero.delta;
            Vector2 touchOnePrevPos = touchOne.screenPosition - touchOne.delta;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.screenPosition - touchOne.screenPosition).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            ApplyZoom(difference * scaleSpeed);
        }

        else if (Mouse.current != null)
        {
            if (Pointer.current == null) return;
            Vector2 pointerPos = Pointer.current.position.ReadValue();
            Vector3 worldPos = mainCam.ScreenToWorldPoint(pointerPos);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            bool isHovered = (hit != null && hit.gameObject == gameObject);

            if (isHovered)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (scroll != 0)
                {
                    ApplyZoom(Mathf.Sign(scroll) * scaleSpeed * 50f);
                }
            }
        }
    }

    private void ApplyZoom(float increment)
    {
        Vector3 newScale = transform.localScale + scaleStepVector * increment;

        newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
        newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
        newScale.z = 1f;

        transform.localScale = newScale;
    }
}
