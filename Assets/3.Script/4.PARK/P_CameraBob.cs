using UnityEngine;

public class P_CameraBob : MonoBehaviour
{
    [Header("플레이어 연결")]
    [SerializeField] private P_Player player;

    [Header("카메라 흔들림 설정")]
    [SerializeField] private float bobSpeed = 14f;
    [SerializeField] private float bobAmount = 0.05f;

    [Header("높이 설정 (여기서 제어할게!)")]
    [SerializeField] private float standHeight = 1.6f;
    [SerializeField] private float crouchHeight = 0.8f;
    [SerializeField] private float changeSpeed = 10f;

    private CharacterController playerController;
    private float currentBaseHeight;
    private float timer = 0;

    void Start()
    {
        player.TryGetComponent(out playerController);

        currentBaseHeight = standHeight;
    }

    void Update()
    {
        if (player == null || playerController == null) return;

        float targetHeight = player.isCrouching ? crouchHeight : standHeight;

        currentBaseHeight = Mathf.Lerp(currentBaseHeight, targetHeight, Time.unscaledDeltaTime * changeSpeed);

        float speed = playerController.velocity.magnitude;
        float finalY = currentBaseHeight;

        if (player.isGround && !player.isJumping && speed > 0.1f)
        {
            timer += Time.unscaledDeltaTime * bobSpeed;
            finalY = currentBaseHeight + Mathf.Sin(timer) * bobAmount;
        }
        else
        {
            timer = 0;
        }

        transform.localPosition = new Vector3(transform.localPosition.x, finalY, transform.localPosition.z);
    }
}