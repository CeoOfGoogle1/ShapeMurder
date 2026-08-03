using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed;
    public float sprintSpeed;
    public float zoomSpeed;
    public float sensitivity;
    float currentMoveSpeed;
    Transform cameraTransform;
    Vector3 movement;

    void Start()
    {
        cameraTransform = transform.GetChild(0);
    }

    void Update()
    {
        if (Keyboard.current.leftShiftKey.isPressed) currentMoveSpeed = sprintSpeed;
        else currentMoveSpeed = moveSpeed;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        movement = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) movement += forward;
        else if (Keyboard.current.sKey.isPressed) movement -= forward;
        if (Keyboard.current.aKey.isPressed) movement -= right;
        else if (Keyboard.current.dKey.isPressed) movement += right;

        transform.position += movement * currentMoveSpeed * Time.deltaTime;

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            float mouseX = mouseDelta.x * sensitivity;
            float mouseY = mouseDelta.y * sensitivity;

            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.right, -mouseY, Space.Self);
        }

        float scroll = Mouse.current.scroll.ReadValue().y * zoomSpeed;
        cameraTransform.localPosition += Vector3.forward * scroll;
    }
}
