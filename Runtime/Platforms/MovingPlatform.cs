using UnityEngine;
using ActionCode.ColliderAdapter;

namespace ActionCode.Physics
{
    /// <summary>
    /// Component for a Moving Platform.
    /// <para>
    /// Attach it on a GameObject with a <see cref="Collider"/> or <see cref="Collider2D"/>
    /// and set the <see cref="verticalOffset"/> as you like.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MovingPlatform : MonoBehaviour
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public AbstractColliderAdapter collider;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        public Vector3 Position => transform.position;
        public Vector3 Direction { get; private set; }

        private Vector3 lastPosition;

        private void Reset() => collider = AbstractColliderAdapter.ResolveCollider(gameObject);

        private void Update()
        {
            Direction = (transform.position - lastPosition).normalized;
            lastPosition = transform.position;
        }

        /// <summary>
        /// Gets the top center position in the Platform surface.
        /// </summary>
        /// <returns>Always a <see cref="Vector3"/> instance.</returns>
        public Vector3 GetSurfacePosition()
        {
            var position = collider.Center;
            position.y = collider.Bounds.max.y;
            return position;
        }

        /// <summary>
        /// Gets the top center position in the Platform surface relative from th given position
        /// </summary>
        /// <param name="position">The position from the platform passenger.</param>
        /// <returns><inheritdoc cref="GetSurfacePosition"/></returns>
        public Vector3 GetSurfacePositionRelativeFrom(Vector3 position)
        {
            var surface = GetSurfacePosition();
            var deltaHorPos = Mathf.Abs(surface.x - position.x);
            var isLeftwards = position.x < surface.x;

            if (isLeftwards) deltaHorPos *= -1f;

            position.y = surface.y;
            position.x = surface.x + deltaHorPos;

            return position;
        }

        public float GetSidePointRelativeFrom(float side) => Position.x + side * collider.HalfSize.x;
    }
}