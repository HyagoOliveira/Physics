using UnityEngine;

namespace ActionCode.BoxBodies
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
        [SerializeField, Tooltip("The platform vertical offset. Change it to fit into the platform surface.")]
        private float verticalOffset;

        private void Reset() => UpdateVerticalOffsetToColliderTopPosition();

        /// <summary>
        /// Gets the top center position in the Platform surface.
        /// </summary>
        /// <returns>Always a <see cref="Vector3"/> instance.</returns>
        public Vector3 GetSurfacePosition() => transform.position + Vector3.up * verticalOffset;

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

        private void OnDrawGizmosSelected() => GetSurfacePosition().Draw(Color.red, size: 0.4F);

        private void UpdateVerticalOffsetToColliderTopPosition()
        {
            var height = 0F;
            var collider3D = GetComponent<Collider>();

            if (collider3D) height = collider3D.bounds.size.y;
            else
            {
                var collider2D = GetComponent<Collider2D>();
                if (collider2D) height = collider2D.bounds.size.y;
            }

            verticalOffset = Mathf.Abs(height) * 0.5F;
        }
    }
}