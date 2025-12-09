using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float dragSensitivity = 1f;      // Drag speed
    [SerializeField] private float inertiaDamping = 4f;       // Higher = slow down faster
    private Vector3 velocity;               // Inertia velocity

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float smoothZoomSpeed = 10f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 12f;
    private float targetZoom;

    [Header("Bounds (World Space)")]
    [SerializeField] private Vector2 minBounds = new(-10f, -10f);
    [SerializeField] private Vector2 maxBounds = new(10f, 10f);
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private Camera cam;
    private Vector3 dragOrigin;
    private bool dragging;

    private void Awake()
    {
        cam = Camera.main;
    }
    private void Update()
    {
        //HandleZoom();
        HandleDrag();
        ApplyInertia();
        ClampToBounds();
    }

    private void HandleZoom()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom -= scroll * zoomSpeed * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            cinemachineCamera.Lens.OrthographicSize = Mathf.Clamp(cinemachineCamera.Lens.OrthographicSize, minZoom, maxZoom);
        }

        cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(cinemachineCamera.Lens.OrthographicSize, targetZoom, Time.deltaTime * smoothZoomSpeed);
    }
    private void HandleDrag()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        // On mouse press
        if (mouse.leftButton.wasPressedThisFrame)
        {
            dragging = true;
            dragOrigin = cam.ScreenToWorldPoint(mouse.position.ReadValue());
            velocity = Vector3.zero; // Stop inertia when dragging starts
        }

        // While held
        if (mouse.leftButton.isPressed && dragging)
        {
            Vector3 current = cam.ScreenToWorldPoint(mouse.position.ReadValue());
            Vector3 delta = dragOrigin - current;

            transform.position += delta * dragSensitivity;
            velocity = delta / Time.deltaTime; // Update velocity for inertia
        }

        // Let go of mouse
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
        }
    }
    private void ApplyInertia()
    {
        if (dragging) return;

        if (velocity.magnitude > 0.01f)
        {
            transform.position += velocity * Time.deltaTime;
            velocity = Vector3.Lerp(velocity, Vector3.zero, inertiaDamping * Time.deltaTime);
        }
    }
    private void ClampToBounds()
    {
        Vector3 pos = transform.position;

        // Bound camera center
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);

        transform.position = pos;
    }
}
