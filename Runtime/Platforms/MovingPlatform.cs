using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Component for a Moving Platform.
    /// <para>
    /// Attach it on a GameObject with a <see cref="Collider"/> or <see cref="Collider2D"/>.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1)]
    public sealed class MovingPlatform : MonoBehaviour
    {
        /// <summary>
        /// The current platform velocity.
        /// </summary>
        public Vector3 Velocity { get; private set; }

        private Vector3 lastPosition;

        private void Start() => lastPosition = transform.position;

        private void LateUpdate()
        {
            Velocity = transform.position - lastPosition;
            lastPosition = transform.position;
        }
    }
}