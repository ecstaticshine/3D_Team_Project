using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // 이게 없으면 UI 감지를 못 합니다!

public class Player : MonoBehaviour
{
    #region Variable

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
    [SerializeField] private float standHeight = 2.0f;
    [SerializeField] private float standBodyY = 0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchBodyY = 0.25f;

    [Header("점프 & 중력 설정")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("시간 조종 설정")]
    [SerializeField] private bool isTimeSlow = false;
    [SerializeField] private float slowFactor = 0.1f;
    private float abilityGauge = 100f;

    [Header("상태 확인")]
    [SerializeField] public bool isDie = false;
    [SerializeField] public float currentHP;
    private float maxHP = 200;
    [SerializeField] public bool isGround;
    [SerializeField] public bool isJumping = false;
    [SerializeField] public bool isDashing = false;
    [SerializeField] public bool isCrouching = false;
    private bool wasGround;

    [Header("애니메이션")]
    private Animator animator; 
    private int hashInputX; 
    private int hashInputY; 
    private int hashIsJump;
    private int hashIsCrouching; 

    private CharacterController characterController;
    private PlayerInput playerInput; //[추가] 플레이어인풋
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float recoverTimer = 0f;
    private float currentSpeed;
    private float lastDashTime = -10f;

    //점프
    private float landSoundCooldown = 0.2f;
    private float lastLandSoundTime;
    private float minFallVelocity = -3.0f; 

    #endregion

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        TryGetComponent(out characterController);
        TryGetComponent(out animator);
        TryGetComponent(out playerInput); //[추가]
        TryGetComponent(out animator);
        hashInputX = Animator.StringToHash("input_x"); 
        hashInputY = Animator.StringToHash("input_y"); 
        hashIsJump = Animator.StringToHash("IsJump"); 
        hashIsCrouching = Animator.StringToHash("isCrouching"); 

        characterController.height = standHeight;
        characterController.center = Vector3.up * (standHeight * 0.5f);
        currentSpeed = moveSpeed;

        currentHP = maxHP;
    }

    private void OnEnable()
    {
        if (GameManager.instance != null)
            GameManager.instance.OnPauseChanged += HandleInputOnPause;
    }

    private void OnDisable()
    {
        if (GameManager.instance != null)
            GameManager.instance.OnPauseChanged -= HandleInputOnPause;
    }

    private void HandleInputOnPause(bool isPaused)
    {

        if (isPaused)
        {
            // [중요 추가] 현재 남아있는 입력값을 강제로 0으로 만듭니다.
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            
        }
    }

    void Start()
    {
        AbilityUI();

        allGuns = GetComponentsInChildren<Gun>(true);

        foreach (var gun in allGuns)
        {
            if (gun.gameObject.activeSelf)
            {
                currentGun = gun;
                break;
            }
        }
        var settingManager = SettingsManager.instance;

        settingManager.OnMouseSensitivityChanged += ApplyMouseSensitivity;

        ApplyMouseSensitivity(settingManager.MouseSensitivity);
    }
    
    void Update()
    {
        // [핵심 추가 코드] 시간이 멈췄으면(일시정지), 아래 로직(이동, 회전)을 아예 실행하지 마라!
        if (Time.timeScale == 0f) return;

        isGround = characterController.isGrounded;

        if (isGround && !wasGround && !isDashing)
        {
            
            if (Time.unscaledTime - lastLandSoundTime > landSoundCooldown)
            {

                if (velocity.y < minFallVelocity)
                {
                    OnLand();
                    lastLandSoundTime = Time.unscaledTime;
                }
            }
        }

        wasGround = isGround;

        HandleAbilityGauge();
        ToggleAbility();
        DashUI();

        if (isDashing || isDie) return;

        if (isGround && velocity.y < 0)
        {
            isJumping = false;
            velocity.y = -2f;
        }

        UpdateAnimation();
        Crouch();
        Move();
        Look();
    }

    #region Movement

    private void Move()
    {
        currentSpeed = isCrouching ? crouchSpeed : moveSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 finalMove = move * currentSpeed;

        velocity.y += gravity * Time.unscaledDeltaTime;

        Vector3 finalVelocity = finalMove + velocity;
        characterController.Move(finalVelocity * Time.unscaledDeltaTime);
    }
    private void Crouch()
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
    private void OnDestroy()
    {
        if (SettingsManager.instance != null)
            SettingsManager.instance.OnMouseSensitivityChanged -= ApplyMouseSensitivity;
    }

    private void Look()
    {
        if (cameraTransform == null) return;
        xRotation -= lookInput.y * mouseSensitivity * Time.unscaledDeltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity * Time.unscaledDeltaTime);
    }
    private IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.unscaledTime;

        Vector3 dashDir = (moveInput.magnitude > 0) ?
            (transform.right * moveInput.x + transform.forward * moveInput.y) : transform.forward;

        dashDir.Normalize();

        ScreenEffectManager.instance.PlayDashEffect(dashDuration);

        float startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + dashDuration && !isDie)
        {
            characterController.Move(dashDir * dashSpeed * Time.unscaledDeltaTime);
            Look();
            yield return null;
        }

        isDashing = false;
        velocity = Vector3.zero;

        wasGround = true;
        lastLandSoundTime = Time.unscaledTime + 0.2f;

        yield return null;
    }

    #endregion

    #region Function

    private void ApplyMouseSensitivity(float value)
    {
        mouseSensitivity = value;
    }
    private void OnLand()
    {
        SoundSystem.EmitSound(gameObject.transform.position, 10f);
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Land");
        }
    }
    public void GunPickup(GunData newGunData)
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
        if (isDie) return;

        currentHP -= damage;

        UIManager.instance.UpdateHP(currentHP, maxHP);

        ScreenEffectManager.instance.PlayHitEffect();
        ScreenEffectManager.instance.CheckHealthStatus(currentHP, maxHP);
        ScreenEffectManager.instance.UpdateEffect(currentHP, maxHP);

        if (currentHP <= 0)
        {
            ScreenEffectManager.instance.SetDeathEffect();
            StartCoroutine(Die());
        }
    }
    private IEnumerator Die()
    {
        isDie = true;

        if (ScoreManager.instance != null) ScoreManager.instance.isTimerRunning = false;

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

        GameManager.instance.deathCount++;

        Resurrect();
    }
    private void Resurrect()
    {
        isDie = false;

        if (ScoreManager.instance != null) ScoreManager.instance.isTimerRunning = true;

        currentHP = maxHP;

        if (ScreenEffectManager.instance != null) ScreenEffectManager.instance.ResetEffect();

        if (UIManager.instance != null) UIManager.instance.UpdateHP(currentHP, maxHP);

        cameraTransform.localPosition = new Vector3(0, 0.6f, 0);
        cameraTransform.localRotation = Quaternion.identity;
        xRotation = 0f;

        if (characterController != null) characterController.enabled = true;
    }
    private void UpdateAnimation()
    {
        if (animator == null) return; 

        
        animator.SetFloat(hashInputX, moveInput.x); 
        animator.SetFloat(hashInputY, moveInput.y); 

        animator.SetBool(hashIsJump, !isGround || isJumping);

        animator.SetBool(hashIsCrouching, isCrouching); 
    }

    #endregion

    #region Ability

    private void Ability()
    {
        isTimeSlow = !isTimeSlow;
        Time.timeScale = isTimeSlow ? slowFactor : 1.0f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
    private void ToggleAbility()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame && abilityGauge >= 0 && !isDie)
        {
            Ability();
            ScreenEffectManager.instance.ToggleTimeEffect(isTimeSlow);
        }
        else if (isTimeSlow && abilityGauge <= 0)
        {
            Ability();
            ScreenEffectManager.instance.ToggleTimeEffect(false);
        }
    }
    private void HandleAbilityGauge()
    {
        if (isTimeSlow)
        {
            abilityGauge -= 10f * Time.unscaledDeltaTime;
            if (ScoreManager.instance != null) ScoreManager.instance.AddAbilityUsage(Time.unscaledDeltaTime);
            if (AudioManager.instance != null) AudioManager.instance.PlaySlow(slowFactor);
            recoverTimer = 0f;

            if (ScreenEffectManager.instance != null)
            {
                ScreenEffectManager.instance.UpdateAbilityIntensity(abilityGauge, 100f);
            }
        }
        else if (abilityGauge < 100f)
        {
            recoverTimer += Time.unscaledDeltaTime;
            if (AudioManager.instance != null) AudioManager.instance.PlayOriginal();
            if (recoverTimer >= 3.0f) abilityGauge += 100f * Time.unscaledDeltaTime;
        }

        abilityGauge = Mathf.Clamp(abilityGauge, 0f, 100f);
        AbilityUI();
    }
    public void Tranquilizer() { abilityGauge = 100f; AbilityUI(); }

    #endregion

    #region NewInputSystem

    public void OnFire(InputValue value)
    {
        if (Time.timeScale == 0f) return;
        // 1. [핵심 방어]
        // "지금 버튼을 눌렀고(isPressed)" + "마우스가 UI 위에 있다면" -> 무시해라!
        if (value.isPressed && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // 2. [기존 로직]
        // 버튼을 뗄 때(value.isPressed == false)는 위에서 걸러지지 않고 여기까지 와서
        // 정상적으로 총을 멈추게(SetTriggerPressed(false)) 됩니다.
        if (currentGun != null && !isDie)
        {
            currentGun.SetTriggerPressed(value.isPressed);
        }
    }
    //기존 온파이어
    //public void OnFire(InputValue value) { if (currentGun != null && !isDie) currentGun.SetTriggerPressed(value.isPressed); }
    public void OnReload(InputValue value)
    {
        if (Time.timeScale == 0f) return; // 추가
        if (currentGun != null && !isDie) currentGun.Reload();
    }
    public void OnMove(InputValue value) {if(!isDie) moveInput = value.Get<Vector2>(); }
    public void OnLook(InputValue value) {if(!isDie) lookInput = value.Get<Vector2>(); }
    public void OnJump(InputValue value)
    {
        if (Time.timeScale == 0f) return; // 추가
        if (isGround && !isJumping && !isCrouching && !isDie)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
        }
    }
    public void OnCrouch(InputValue value)
    {
        if (Time.timeScale == 0f) return; // 추가
        if (!isJumping && !isDashing) isCrouching = value.isPressed;
    }
    public void OnDash(InputValue value)
    {
        if (Time.timeScale == 0f) return;//추가
        if (value.isPressed && !isDashing && !isCrouching && isGround && Time.unscaledTime >= lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash());
        }
    }

    #endregion

    #region UI
    private void DashUI()
    {
        if (UIManager.instance != null)
        {
            float timeSinceLastDash = Time.unscaledTime - lastDashTime;

            float cooldownPercent = Mathf.Clamp01(timeSinceLastDash / dashCooldown);

            UIManager.instance.UpdateDashSlider(cooldownPercent);
        }
    }
    private void AbilityUI() { if (UIManager.instance != null) UIManager.instance.UpdateAbilitySlider(abilityGauge); }
    #endregion

    //private void OnApplicationFocus(bool hasFocus)
    //{
    //    if (hasFocus)
    //    {
    //        Cursor.lockState = CursorLockMode.Locked;
    //        Cursor.visible = false;
    //    }
    //}

    
}