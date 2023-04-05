#if MODULE_ANIMATION
using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Updates a local <see cref="Animator"/> using a local <see cref="BoxBody"/> properties.
    /// <para><b>It's important that your Animator Controller has the same Parameters expected here.</b></para>
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxBody))]
    [RequireComponent(typeof(Animator))]
    public sealed class BoxBodyAnimator : MonoBehaviour
    {
        [SerializeField, Tooltip("The local BoxBody component.")]
        private BoxBody body;
        [SerializeField, Tooltip("The local Animator component.")]
        private Animator animator;

        private readonly int isGrounded = Animator.StringToHash("IsGrounded");
        private readonly int isAirborne = Animator.StringToHash("IsAirborne");

        private void Reset()
        {
            body = GetComponent<BoxBody>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            animator.SetBool(isGrounded, body.IsGrounded);
            animator.SetBool(isAirborne, body.IsAirborne);
        }
    }
}
#endif