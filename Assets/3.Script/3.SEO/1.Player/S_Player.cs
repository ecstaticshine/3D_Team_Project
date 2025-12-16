using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class S_Player : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] public S_Gun gun;
    [SerializeField] private Transform cameraTransform;

    [Header("설정 값")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float mouseSensitivity = 25.0f;

    [Header("점프 & 중력 설정")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("상태")]
    [SerializeField] public bool isGround;
    [SerializeField] public bool isJumping = false;
    private Vector3 currentVelocity;

    [Header("시간 조종 설정")]
    [SerializeField] private bool isTimeSlow = false;
    [SerializeField] private float slowFactor = 0.1f;
    private float abilityGauge = 100f;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float recoverTimer = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        TryGetComponent(out characterController);
        AbilityGaugeSlider();
    }

    void Update()
    {
        isGround = characterController.isGrounded;
        currentVelocity = velocity;

        HandleAbilityGauge();
        HandleTimeInput();

        if (isGround && velocity.y < 0)
        {
            isJumping = false;
            velocity.y = -2f;
        }

        MovePlayer();
        Look();
    }

    private void HandleTimeInput()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame && abilityGauge >= 0)
        {
            ToggleTime();
        }
        else if (isTimeSlow && abilityGauge <= 0)
        {
            ToggleTime();
        }
    }

    private void MovePlayer()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 finalMove = move * moveSpeed;

        velocity.y += gravity * Time.unscaledDeltaTime;

        Vector3 finalVelocity = finalMove + velocity;
        characterController.Move(finalVelocity * Time.unscaledDeltaTime);
    }

    private void Look()
    {
        if (cameraTransform == null) return;

        float mouseX = lookInput.x * mouseSensitivity * Time.unscaledDeltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.unscaledDeltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnFire(InputValue value) { if (value.isPressed && gun != null) gun.Fire(); }
    public void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }
    public void OnLook(InputValue value) { lookInput = value.Get<Vector2>(); }
    public void OnReload(InputValue value) { gun.Reload(); }
    public void OnJump(InputValue value)
    {
        if (isGround && !isJumping)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
        }
    }

    private void AbilityGaugeSlider() 
    {
        S_UIManager.instance.UpdateAbilitySlider(abilityGauge);
    }

    private void ToggleTime()
    {
        isTimeSlow = !isTimeSlow;
        if (isTimeSlow)
        {
            Time.timeScale = slowFactor;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else
        {
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    private void HandleAbilityGauge()
    {
        if (isTimeSlow)
        {
            abilityGauge -= 10f * Time.unscaledDeltaTime;
            AudioManager.instance.PlaySlow(slowFactor);
            recoverTimer = 0f;
        }
        else if (abilityGauge < 100f)
        {
            recoverTimer += Time.unscaledDeltaTime;
            if (recoverTimer >= 3.0f)
            {
                abilityGauge += 100f * Time.unscaledDeltaTime;
                AudioManager.instance.PlayOriginal();
            }
        }
        abilityGauge = Mathf.Clamp(abilityGauge, 0f, 100f);
        AbilityGaugeSlider();
    }

    public void RestoreAbilityGauge()
    {
        abilityGauge = 100f;
        AbilityGaugeSlider();
    }
}