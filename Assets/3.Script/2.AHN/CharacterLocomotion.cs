using UnityEngine;

public class CharacterLocomotion : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        TryGetComponent(out animator);
    }


}
