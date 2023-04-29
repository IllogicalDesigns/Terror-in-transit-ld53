using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    public float crouchSpeed = 3.0f;
    public float walkSpeed = 6.0f;
    public float runSpeed = 10.0f;
    public float currentSpeed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    [SerializeField] private Transform cameraTransform;

    private const float maxY = 90f;

    public enum MovementState {
        Crouching,
        Sprinting,
        Walking
    }

    public MovementState movementState = MovementState.Walking;

    private void Start() {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor to center of screen
    }

    private void Update() {
        // Handle movement
        if (controller.isGrounded) {
            HandleCrouching();
            HandleSprinting();

            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= currentSpeed;

            HandleJumpPress();
        }

        HandleRotation();

        HandleGravity();
    }

    private void HandleJumpPress() {
        if (Input.GetButton("Jump")) {
            moveDirection.y = jumpSpeed;
        }
    }

    private void HandleRotation() {
        // Handle rotation
        rotationX += Input.GetAxis("Mouse X") * lookSpeed;
        rotationY += Input.GetAxis("Mouse Y") * lookSpeed;
        rotationY = Mathf.Clamp(rotationY, -maxY, maxY); // Clamp vertical rotation to prevent flipping

        transform.localRotation = Quaternion.Euler(0, rotationX, 0); // Apply rotation to player object
        cameraTransform.localRotation = Quaternion.Euler(-rotationY, 0, 0); // Apply rotation to player object
    }

    private void HandleGravity() {
        // Handle gravity
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void HandleSprinting() {
        if (movementState == MovementState.Crouching) return;

        if (Input.GetKey(KeyCode.LeftShift)) {
            currentSpeed = runSpeed;
            movementState = MovementState.Sprinting;
        }
        else {
            currentSpeed = walkSpeed;
            movementState = MovementState.Walking;
        }
    }

    private void HandleCrouching() {
        if (movementState == MovementState.Sprinting) return;

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C)) {
            currentSpeed = crouchSpeed;
            movementState = MovementState.Crouching;
        }
        else {
            currentSpeed = walkSpeed;
            movementState = MovementState.Walking;
        }
    }
}
