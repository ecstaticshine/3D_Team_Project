using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections; // IEnumerator 사용을 위해 필수


public class P_Player : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] public S_Gun gun;
    [SerializeField] private Transform cameraTransform;

    [Header("이동 설정")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float mouseSensitivity = 25.0f;

    [Header("대시(순간이동) 설정")]
    [SerializeField] private float dashSpeed = 50.0f;      // 순간이동처럼 보일 만큼 빠른 속도
    [SerializeField] private float dashDuration = 0.2f;    // 이동 지속 시간 (짧게)
    [SerializeField] private float dashCooldown = 1.0f;    // 재사용 대기시간

    [Header("앉기 설정")]
    [SerializeField] private float standHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("점프 & 중력 설정")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("상태 확인용")]
    [SerializeField] public bool isGround;
    [SerializeField] public bool isJumping = false;
    [SerializeField] public bool isDashing = false; // 대시 중인지 확인
    [SerializeField] public bool isCrouching = false;

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
    private float lastDashTime = -10f; // 마지막 대시 시간

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        TryGetComponent(out characterController);
        characterController.height = standHeight;
        currentSpeed = walkSpeed;

        AbilityGaugeSlider();
    }

    void Update()
    {
        // 대시 중일 때는 일반 이동 로직을 수행하지 않음 (대시 코루틴이 이동 제어)
        if (isDashing) return;

        isGround = characterController.isGrounded;

        HandleAbilityGauge();
        HandleTimeInput();
        HandleCrouch();

        // 바닥 체크 및 중력 초기화
        if (isGround && velocity.y < 0)
        {
            isJumping = false;
            velocity.y = -2f;
        }

        // 일반 이동 수행
        MovePlayer();
        Look(); // 시야 회전은 대시 중에도 가능하게 할지, 막을지 선택 가능 (여기선 둠)
    }

    // --- 대시 핵심 로직 ---
    public void OnDash(InputValue value)
    {
        // 1. 키를 눌렀고(isPressed)
        // 2. 대시 중이 아니며(!isDashing)
        // 3. 쿨타임이 지났고
        // 4. 앉은 상태가 아닐 때 발동
        if (value.isPressed && !isDashing && !isCrouching && Time.unscaledTime >= lastDashTime + dashCooldown)
        {
            StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.unscaledTime; // 쿨타임 갱신

        // 1. 대시 방향 계산
        // 입력이 있으면 입력 방향으로, 없으면 카메라가 보는 방향으로
        Vector3 dashDir;
        if (moveInput.magnitude > 0)
        {
            dashDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        }
        else
        {
            dashDir = transform.forward;
        }
        dashDir.Normalize(); // 방향 벡터 정규화

        // 2. 대시 실행 (지정된 시간 동안 고속 이동)
        float startTime = Time.unscaledTime;

        while (Time.unscaledTime < startTime + dashDuration)
        {
            // 중력 영향 없이 수평으로만 빠르게 이동
            // Time.unscaledDeltaTime을 사용하여 시간이 느려져도 대시는 빠르게(정상 속도 느낌)
            characterController.Move(dashDir * dashSpeed * Time.unscaledDeltaTime);

            // 대시 중에도 시야 회전은 가능하게 (원치 않으면 Update에서 Look 호출 제어)
            Look();

            yield return null; // 다음 프레임까지 대기
        }

        // 3. 종료
        isDashing = false;

        // 대시가 끝난 후 관성을 없애려면 velocity 초기화 (선택 사항)
        velocity = Vector3.zero;
    }
    // -----------------------

    private void MovePlayer()
    {
        // 앉기 상태에 따른 속도 설정
        currentSpeed = isCrouching ? crouchSpeed : walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 finalMove = move * currentSpeed;

        // 중력 적용
        velocity.y += gravity * Time.unscaledDeltaTime;

        Vector3 finalVelocity = finalMove + velocity;
        characterController.Move(finalVelocity * Time.unscaledDeltaTime);
    }

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

    // ... (나머지 Look, HandleTimeInput, HandleAbilityGauge 등의 함수는 기존과 동일) ...

    //public void OnCrouch(InputValue value)
    //{
    //    isCrouching = value.isPressed;
    //    Debug.Log($"Crouch State: {isCrouching}");
    //}

    // ... (Input System의 나머지 함수들 동일) ...

    // 편의를 위해 중복된 나머지 함수들은 생략했습니다. 
    // 기존 코드의 Look, HandleTimeInput, HandleAbilityGauge, OnFire 등은 그대로 유지하십시오.

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

    private void HandleTimeInput()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame && abilityGauge >= 0) ToggleTime();
        else if (isTimeSlow && abilityGauge <= 0) ToggleTime();
    }

    private void ToggleTime()
    {
        isTimeSlow = !isTimeSlow;
        if (isTimeSlow) { Time.timeScale = slowFactor; Time.fixedDeltaTime = 0.02f * Time.timeScale; }
        else { Time.timeScale = 1.0f; Time.fixedDeltaTime = 0.02f; }
    }

    private void HandleAbilityGauge()
    {
        if (isTimeSlow) { abilityGauge -= 10f * Time.unscaledDeltaTime; recoverTimer = 0f; }
        else if (abilityGauge < 100f)
        {
            recoverTimer += Time.unscaledDeltaTime;
            if (recoverTimer >= 3.0f) abilityGauge += 100f * Time.unscaledDeltaTime;
        }
        abilityGauge = Mathf.Clamp(abilityGauge, 0f, 100f);
        AbilityGaugeSlider();
    }

    private void AbilityGaugeSlider() { if (P_UIManager.instance != null) P_UIManager.instance.UpdateAbilitySlider(abilityGauge); }

    public void OnFire(InputValue value) { if (value.isPressed && gun != null) gun.TryFire(); }
    public void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }
    public void OnLook(InputValue value) { lookInput = value.Get<Vector2>(); }
    public void OnReload(InputValue value) { gun.Reload(); }
    public void OnJump(InputValue value)
    {
        if (isGround && !isJumping && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); isJumping = true;
        }
    }

    public void OnCrouch(InputValue value)
    {
        isCrouching = value.isPressed;
    }
}