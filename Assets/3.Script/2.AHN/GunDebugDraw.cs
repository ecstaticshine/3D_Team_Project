using UnityEngine;

public class GunDebugDraw : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + transform.forward * 50f);
    }
}
