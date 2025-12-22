using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("무기 및 연결")]
    [SerializeField] public Gun currentGun;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform playerBody;
    private Gun[] allGuns;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float mouseSensitivity = 25.0f;

    [Header("대시 설정")]
    [SerializeField] private float dashSpeed = 50.0f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.0f;

    [Header("앉기 설정")]
    [SerializeField] private float crouchTransitionSpeed = 10f;
    private float standHeight = 2.0f;
    [SerializeField] private float standBodyY = 0f;
    private float crouchHeight = 1.0f;
    [SerializeField] private float crouchBodyY = 0.25f;

    [Header("점프 & 중력 설정")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("시간 조종 설정")]
    [SerializeField] private bool isTimeSlow = false;
    [SerializeField] private float slowFactor = 0.1f;
    private float abilityGauge = 100f;

    [Header("상태 확인")]
    [SerializeField] public bool isDead = false;
    [SerializeField] public float currentHP;
    private float maxHP = 200;
    [SerializeField] public bool isGround;
    [SerializeField] public bool isJumping = false;
    [SerializeField] public bool isDashing = false;
    [SerializeField] public bool isCrouching = false;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float recoverTimer = 0f;
    private float currentSpeed;
    private float lastDashTime = -10f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        TryGetComponent(out characterController);

        characterController.height = standHeight;
        characterController.center = Vector3.up * (standHeight * 0.5f);
        currentSpeed = moveSpeed;

        currentHP = maxHP;
    }

    void Start()
    {
        AbilityGaugeSlider();

        allGuns = GetComponentsInChildren<Gun>(true);
        foreach (var gun in allGuns)
        {
            if (gun.gameObject.activeSelf)
            {
                currentGun = gun;
                break;
            }
        }
    }

    void Update()
    {
        if (isDashing || isDead) return;

        isGround = characterController.isGrounded;

        HandleAbilityGauge();
        HandleTimeInput();
        HandleCrouch();
        UpdateDashUI();

        if (isGround && velocity.y < 0)
        {
            isJumping = false;
            velocity.y = -2f;
        }

        MovePlayer();
        Look();
    }

    private void MovePlayer()
    {
        currentSpeed = isCrouching ? crouchSpeed : moveSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 finalMove = move * currentSpeed;

        velocity.y += gravity * Time.unscaledDeltaTime;

        Vector3 finalVelocity = finalMove + velocity;
        characterController.Move(finalVelocity * Time.unscaledDeltaTime);
    }

    private void HandleCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        float targetBodyY = isCrouching ? crouchBodyY : standBodyY;
        Vector3 targetBodyScale = isCrouching ? new Vector3(1, 0.5f, 1) : Vector3.one;

        float speed = Time.unscaledDeltaTime * crouchTransitionSpeed;

        characterController.height = Mathf.Lerp(characterController.height, targetHeight, speed);
        characterController.center = Vector3.up * (characterController.height * 0.5f);

        if (playerBody != null)
        {
            playerBody.localScale = Vector3.Lerp(playerBody.localScale, targetBodyScale, speed);

            Vector3 bodyPos = playerBody.localPosition;
            bodyPos.y = Mathf.Lerp(bodyPos.y, targetBodyY, speed);
            playerBody.localPosition = bodyPos;
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && !isDashing && !isCrouching && isGround && Time.unscaledTime >= lastDashTime + dashCooldown)
        {
            StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.unscaledTime;

        Vector3 dashDir = (moveInput.magnitude > 0) ?
            (transform.right * moveInput.x + transform.forward * moveInput.y) : transform.forward;

        dashDir.Normalize();

        float startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + dashDuration && !isDead)
        {
            characterController.Move(dashDir * dashSpeed * Time.unscaledDeltaTime);
            Look();
            yield return null;
        }

        isDashing = false;
        velocity = Vector3.zero;
    }

    public void OnCrouch(InputValue value)
    {
        if (!isJumping && !isDashing) isCrouching = value.isPressed;
    }

    public void OnFire(InputValue value) { if (currentGun != null && !isDead) currentGun.SetTriggerPressed(value.isPressed); }
    public void OnReload(InputValue value) { if (currentGun != null && !isDead) currentGun.Reload(); }
    public void OnMove(InputValue value) {if(!isDead) moveInput = value.Get<Vector2>(); }
    public void OnLook(InputValue value) {if(!isDead) lookInput = value.Get<Vector2>(); }
    public void OnJump(InputValue value)
    {
        if (isGround && !isJumping && !isCrouching && !isDead)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
        }
    }

    private void HandleTimeInput()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame && abilityGauge >= 0 && !isDead) ToggleTime();
        else if (isTimeSlow && abilityGauge <= 0) ToggleTime();
    }

    private void ToggleTime()
    {
        isTimeSlow = !isTimeSlow;
        Time.timeScale = isTimeSlow ? slowFactor : 1.0f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void HandleAbilityGauge()
    {
        if (isTimeSlow)
        {
            abilityGauge -= 10f * Time.unscaledDeltaTime;
            if (ScoreManager.instance != null) ScoreManager.instance.AddAbilityUsage(Time.unscaledDeltaTime);
            if (AudioManager.instance != null) AudioManager.instance.PlaySlow(slowFactor);
            recoverTimer = 0f;
        }
        else if (abilityGauge < 100f)
        {
            recoverTimer += Time.unscaledDeltaTime;
            if (AudioManager.instance != null) AudioManager.instance.PlayOriginal();
            if (recoverTimer >= 3.0f) abilityGauge += 100f * Time.unscaledDeltaTime;
        }

        abilityGauge = Mathf.Clamp(abilityGauge, 0f, 100f);

        AbilityGaugeSlider();

        if (ScreenEffectManager.instance != null)
        {
            ScreenEffectManager.instance.UpdateEffect(abilityGauge, 100f);
        }
    }

    private void UpdateDashUI()
    {
        if (UIManager.instance != null)
        {
            float timeSinceLastDash = Time.unscaledTime - lastDashTime;

            float cooldownPercent = Mathf.Clamp01(timeSinceLastDash / dashCooldown);

            UIManager.instance.UpdateDashSlider(cooldownPercent);
        }
    }

    public void RestoreAbilityGauge() { abilityGauge = 100f; AbilityGaugeSlider(); }

    private void AbilityGaugeSlider() { if (UIManager.instance != null) UIManager.instance.UpdateAbilitySlider(abilityGauge); }

    public void HandleGunPickup(GunData newGunData)
    {
        Gun targetGun = null;
        foreach (var gun in allGuns)
        {
            if (gun.currentGunData == newGunData) { targetGun = gun; break; }
        }

        if (targetGun == null) return;

        if (currentGun == targetGun) currentGun.AddAmmo(newGunData.maxAmmo);
        else
        {
            if (currentGun != null) currentGun.gameObject.SetActive(false);
            targetGun.InitializeGun();
            targetGun.gameObject.SetActive(true);
            currentGun = targetGun;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;

        UIManager.instance.UpdateHP(currentHP, maxHP);

        ScreenEffectManager.instance.PlayHitEffect();
        ScreenEffectManager.instance.CheckHealthStatus(currentHP, maxHP);
        ScreenEffectManager.instance.UpdateEffect(currentHP, maxHP);

        if (currentHP <= 0)
        {
            ScreenEffectManager.instance.SetDeathEffect();
            StartCoroutine(DeathRoutine());
        }
    }

    private IEnumerator DeathRoutine()
    {
        isDead = true;
        isTimeSlow = false;
        Time.timeScale = 1.0f;

        if (characterController != null) characterController.enabled = false;

        if (ScreenEffectManager.instance != null)
        {
            ScreenEffectManager.instance.SetDeathEffect();
        }

        Vector3 startPos = cameraTransform.localPosition;
        Quaternion startRot = cameraTransform.localRotation;
        Vector3 targetPos = new Vector3(startPos.x, -1f, startPos.z);
        Quaternion targetRot = Quaternion.Euler(0, 0, -60f);

        float elapsed = 0f;
        float duration = 1.0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            cameraTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            cameraTransform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        if (TimeRewindManager.Instance != null)
        {
            TimeRewindManager.Instance.StartFullRewind();

            yield return new WaitForSecondsRealtime(5.5f);
        }

        Resurrect();
    }

    private void Resurrect()
    {
        isDead = false;
        currentHP = maxHP;

        if (ScreenEffectManager.instance != null) ScreenEffectManager.instance.ResetEffect();

        if (UIManager.instance != null) UIManager.instance.UpdateHP(currentHP, maxHP);

        cameraTransform.localPosition = new Vector3(0, 0.6f, 0);
        cameraTransform.localRotation = Quaternion.identity;
        xRotation = 0f;

        if (characterController != null) characterController.enabled = true;
    }

    private void Look()
    {
        if (cameraTransform == null) return;
        xRotation -= lookInput.y * mouseSensitivity * Time.unscaledDeltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity * Time.unscaledDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            ScoreManager.instance.CalculateFinalScore();

            SceneManager.LoadScene("ScoreScene");
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}