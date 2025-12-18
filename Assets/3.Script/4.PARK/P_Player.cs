using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class P_Player : MonoBehaviour
{
//    [Header("연결")]
//    [SerializeField] public S_Gun gun;
//    [SerializeField] private Transform cameraTransform;
//    [SerializeField] private Transform playerBody;
//
//    [Header("이동 설정")]
//    [SerializeField] private float walkSpeed = 5.0f;
//    [SerializeField] private float crouchSpeed = 2.5f;
//    [SerializeField] private float mouseSensitivity = 25.0f;
//
//    [Header("대시(순간이동) 설정")]
//    [SerializeField] private float dashSpeed = 50.0f;
//    [SerializeField] private float dashDuration = 0.2f;
//    [SerializeField] private float dashCooldown = 1.0f;
//
//    [Header("앉기 설정 (물리 & 비주얼)")]
//    [SerializeField] private float crouchTransitionSpeed = 10f;
//
//    // [유니] 서 있을 때 설정
//    private float standHeight = 2.0f;
//    private float standBodyY = 1.0f;
//
//    // [유니] 앉았을 때 설정
//    private float crouchHeight = 1.0f;
//    private float crouchBodyY = 0.5f;
//
//    [Header("점프 & 중력 설정")]
//    [SerializeField] private float jumpHeight = 1.5f;
//    [SerializeField] private float gravity = -9.81f;
//
//    [Header("상태 확인용")]
//    [SerializeField] public bool isGround;
//    [SerializeField] public bool isJumping = false;
//    [SerializeField] public bool isDashing = false;
//    [SerializeField] public bool isCrouching = false;
//
//    [Header("시간 조종 설정")]
//    [SerializeField] private bool isTimeSlow = false;
//    [SerializeField] private float slowFactor = 0.1f;
//    private float abilityGauge = 100f;
//
//    private CharacterController characterController;
//    private Vector2 moveInput;
//    private Vector2 lookInput;
//    private Vector3 velocity;
//    private float xRotation = 0f;
//    private float recoverTimer = 0f;
//    private float currentSpeed;
//    private float lastDashTime = -10f;
//
//    void Start()
//    {
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//
//        TryGetComponent(out characterController);
//
//        characterController.height = standHeight;
//        characterController.center = Vector3.up * (standHeight * 0.5f);
//        currentSpeed = walkSpeed;
//
//        AbilityGaugeSlider();
//    }
//
//    void Update()
//    {
//        if (isDashing) return;
//
//        isGround = characterController.isGrounded;
//
//        HandleAbilityGauge();
//        HandleTimeInput();
//        HandleCrouch();
//
//        if (isGround && velocity.y < 0)
//        {
//            isJumping = false;
//            velocity.y = -2f;
//        }
//
//        MovePlayer();
//        Look();
//    }
//
//    private void HandleCrouch()
//    {
//        float targetHeight = isCrouching ? crouchHeight : standHeight;
//        float targetBodyY = isCrouching ? crouchBodyY : standBodyY;
//        Vector3 targetBodyScale = isCrouching ? new Vector3(1, 0.5f, 1) : Vector3.one;
//
//        float speed = Time.unscaledDeltaTime * crouchTransitionSpeed;
//
//        characterController.height = Mathf.Lerp(characterController.height, targetHeight, speed);
//
//        characterController.center = Vector3.up * (characterController.height * 0.5f);
//
//        if (playerBody != null)
//        {
//            playerBody.localScale = Vector3.Lerp(playerBody.localScale, targetBodyScale, speed);
//
//            Vector3 bodyPos = playerBody.localPosition;
//            bodyPos.y = Mathf.Lerp(bodyPos.y, targetBodyY, speed);
//            playerBody.localPosition = bodyPos;
//        }
//    }
//
//    public void OnDash(InputValue value)
//    {
//        if (value.isPressed && !isDashing && !isCrouching && Time.unscaledTime >= lastDashTime + dashCooldown)
//        {
//            StartCoroutine(DashRoutine());
//        }
//    }
//
//    IEnumerator DashRoutine()
//    {
//        isDashing = true;
//        lastDashTime = Time.unscaledTime;
//
//        Vector3 dashDir;
//        if (moveInput.magnitude > 0)
//            dashDir = transform.right * moveInput.x + transform.forward * moveInput.y;
//        else
//            dashDir = transform.forward;
//
//        dashDir.Normalize();
//
//        float startTime = Time.unscaledTime;
//
//        while (Time.unscaledTime < startTime + dashDuration)
//        {
//            characterController.Move(dashDir * dashSpeed * Time.unscaledDeltaTime);
//            Look();
//            yield return null;
//        }
//
//        isDashing = false;
//        velocity = Vector3.zero;
//    }
//
//    private void MovePlayer()
//    {
//        currentSpeed = isCrouching ? crouchSpeed : walkSpeed;
//
//        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
//        Vector3 finalMove = move * currentSpeed;
//
//        velocity.y += gravity * Time.unscaledDeltaTime;
//
//        Vector3 finalVelocity = finalMove + velocity;
//        characterController.Move(finalVelocity * Time.unscaledDeltaTime);
//    }
//
//    private void Look()
//    {
//        if (cameraTransform == null) return;
//        float mouseX = lookInput.x * mouseSensitivity * Time.unscaledDeltaTime;
//        float mouseY = lookInput.y * mouseSensitivity * Time.unscaledDeltaTime;
//        xRotation -= mouseY;
//        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
//        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
//        transform.Rotate(Vector3.up * mouseX);
//    }
//
//    private void HandleTimeInput()
//    {
//        if (Keyboard.current.tKey.wasPressedThisFrame && abilityGauge >= 0) ToggleTime();
//        else if (isTimeSlow && abilityGauge <= 0) ToggleTime();
//    }
//
//    private void ToggleTime()
//    {
//        isTimeSlow = !isTimeSlow;
//        if (isTimeSlow) { Time.timeScale = slowFactor; Time.fixedDeltaTime = 0.02f * Time.timeScale; }
//        else { Time.timeScale = 1.0f; Time.fixedDeltaTime = 0.02f; }
//    }
//
//    private void HandleAbilityGauge()
//    {
//        if (isTimeSlow) { abilityGauge -= 10f * Time.unscaledDeltaTime; recoverTimer = 0f; }
//        else if (abilityGauge < 100f)
//        {
//            recoverTimer += Time.unscaledDeltaTime;
//            if (recoverTimer >= 3.0f) abilityGauge += 100f * Time.unscaledDeltaTime;
//        }
//        abilityGauge = Mathf.Clamp(abilityGauge, 0f, 100f);
//        AbilityGaugeSlider();
//    }
//
//    private void AbilityGaugeSlider() { if (P_UIManager.instance != null) P_UIManager.instance.UpdateAbilitySlider(abilityGauge); }
//
//    public void OnFire(InputValue value) { if (value.isPressed && gun != null) gun.TryFire(); }
//    public void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }
//    public void OnLook(InputValue value) { lookInput = value.Get<Vector2>(); }
//    public void OnReload(InputValue value) { gun.Reload(); }
//    public void OnJump(InputValue value)
//    {
//        if (isGround && !isJumping && !isCrouching)
//        {
//            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); isJumping = true;
//        }
//    }
//
//    public void OnCrouch(InputValue value)
//    {
//        isCrouching = value.isPressed;
//    }
}