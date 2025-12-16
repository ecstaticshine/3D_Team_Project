using UnityEngine;

public class A_CameraBob : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private A_Player player;

    [Header("카메라 설정")]
    [SerializeField] private float bobSpeed = 14f;
    [SerializeField] private float bobAmount = 0.05f;
    private CharacterController playerController;
    private float defaultPosY = 0;
    private float timer = 0;

    void Start()
    {
        defaultPosY = transform.localPosition.y;

        player.TryGetComponent(out playerController);
    }

    void Update()
    {
        if (player == null || playerController == null) return;

        float speed = playerController.velocity.magnitude;

        if (!player.isJumping)
        {
            if (speed > 0.1f)
            {
                timer += Time.unscaledDeltaTime * bobSpeed;

                float newY = defaultPosY + Mathf.Sin(timer) * bobAmount;

                transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
            }
            else
            {
                timer = 0;

                float newY = Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.unscaledDeltaTime * bobSpeed);

                transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
            }
        }
    }
}