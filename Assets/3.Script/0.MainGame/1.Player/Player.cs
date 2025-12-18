using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] public Gun currentGun;
    [SerializeField] private Transform cameraTransform;
    private Gun[] allGuns;

    [Header("점프 & 중력 설정")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("설정 값")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float mouseSensitivity = 25.0f;

    [Header("대시 설정")]
    [SerializeField] private float dashSpeed = 15.0f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.0f;

    [Header("앉기 설정")]
    [SerializeField] private float standHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("상태")]
    [SerializeField] public bool isGround;
    [SerializeField] public bool isJumping = false;
    [SerializeField] public bool isDashing = false;
    [SerializeField] public bool isCrouching = false;
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
    private float currentSpeed;
    private float lastDashTime = -10f;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        TryGetComponent(out characterController);

        allGuns = GetComponentsInChildren<Gun>(true);
        foreach (var gun in allGuns)
        {
            if (gun.gameObject.activeSelf)
            {
                currentGun = gun;
                break;
            }
        }

        characterController.height = standHeight;
        currentSpeed = moveSpeed;

        AbilityGaugeSlider();
    }

    private void Update()
    {
        if (isDashing) return;

        isGround = characterController.isGrounded;
        currentVelocity = velocity;

        HandleAbilityGauge();
        HandleTimeInput();
        HandleCrouch();

        if (isGround && velocity.y < 0)
        {
            isJumping = false;
            velocity.y = -2f;
        }

        MovePlayer();
        Look();
    }

    #region Move
    private void MovePlayer()
    {
        currentSpeed = isCrouching ? crouchSpeed : moveSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 finalMove = move * currentSpeed;

        velocity.y += gravity * Time.unscaledDeltaTime;

        Vector3 finalVelocity = finalMove + velocity;
        characterController.Move(finalVelocity * Time.unscaledDeltaTime);
    }
    #endregion

    #region Look
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
    #endregion

    #region Dash
    IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.unscaledTime;

        Vector3 dashDir;
        if (moveInput.magnitude > 0)
        {
            dashDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        }
        else
        {
            dashDir = transform.forward;
        }
        dashDir.Normalize();

        float startTime = Time.unscaledTime;

        while (Time.unscaledTime < startTime + dashDuration)
        {
            characterController.Move(dashDir * dashSpeed * Time.unscaledDeltaTime);

            Look();

            yield return null;
        }

        isDashing = false;

        velocity = Vector3.zero;
    }
    #endregion

    #region Crouch
    private void HandleCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standHeight;

        if (isCrouching)
        {
            if (Mathf.Abs(characterController.height - targetHeight) > 0.01f)
            {
                characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.unscaledDeltaTime * crouchTransitionSpeed);
                characterController.center = Vector3.up * (characterController.height / 2f);
            }
        }
        else
        {
            characterController.height = targetHeight;
            characterController.center = Vector3.zero;
            gameObject.transform.position = Vector3.up * (characterController.height / 2f);
        }
    }
    #endregion

    #region Ability
    // 능력 제어
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

    // 능력 UI
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
            AudioManager.instance.PlayOriginal();
            if (recoverTimer >= 3.0f)
            {
                abilityGauge += 100f * Time.unscaledDeltaTime;
            }
        }
        abilityGauge = Mathf.Clamp(abilityGauge, 0f, 100f);
        AbilityGaugeSlider();
    }

    private void AbilityGaugeSlider()
    {
        UIManager.instance.UpdateAbilitySlider(abilityGauge);
    }

    // 안정제용
    public void RestoreAbilityGauge()
    {
        abilityGauge = 100f;
        AbilityGaugeSlider();
    }

    #endregion

    #region Gun
    public void HandleGunPickup(GunData newGunData)
    {
        Gun targetGun = null;

        foreach (var gun in allGuns)
        {
            if (gun.currentGunData == newGunData)
            {
                targetGun = gun;
                break;
            }
        }

        if (targetGun == null)
        {
            return;
        }

        if (currentGun == targetGun)
        {
            currentGun.AddAmmo(newGunData.maxAmmo);
        }
        else
        {
            if (currentGun != null)
                currentGun.gameObject.SetActive(false);

            targetGun.InitializeGun();

            targetGun.gameObject.SetActive(true);
            currentGun = targetGun;
        }
    }
    #endregion

    #region NewInputSystem
    public void OnFire(InputValue value) { if (currentGun != null) currentGun.SetTriggerPressed(value.isPressed); }
    public void OnReload(InputValue value) { if (currentGun != null) currentGun.Reload(); }
    public void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }
    public void OnLook(InputValue value) { lookInput = value.Get<Vector2>(); }
    public void OnJump(InputValue value)
    {
        if (isGround && !isJumping && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
        }
    }
    public void OnDash(InputValue value)
    {
        if (value.isPressed && !isDashing && !isCrouching && Time.unscaledTime >= lastDashTime + dashCooldown)
        {
            StartCoroutine(DashRoutine());
        }
    }
    public void OnCrouch(InputValue value)
    {
        isCrouching = value.isPressed;
    }
    #endregion
}
