using UnityEngine;
using ActionCode.Shapes;

namespace ActionCode.Physics
{
    /// <summary>
    /// Casts a Box, filtering by the Collisions layers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BoxCaster : AbstractCaster
    {
        [SerializeField, Tooltip("The box size used on the cast.")]
        private Vector3 size = Vector3.one * 0.5F;

        /// <summary>
        /// The box size used on the cast.
        /// </summary>
        public Vector3 Size
        {
            get => size;
            set => size = value;
        }

        protected override bool Cast(out RaycastHit hit)
        {
            return UnityEngine.Physics.BoxCast(
                GetCastOrigin(),
                halfExtents: size * 0.5F,
                GetCastDirection(),
                out hit,
                GetCastOrientation(),
                Distance,
                Collisions
            );
        }

        protected override void DrawCast()
        {
            var origin = GetCastOrigin();
            var end = origin + GetCastDirection() * Distance;
            var color = GetDrawColor();

            Debug.DrawLine(origin, end, color);
            ShapeDebug.DrawCuboid(end, size, GetCastOrientation(), color);
        }

        private Vector3 GetCastOrigin() => transform.position;
        private Vector3 GetCastDirection() => transform.forward;
        private Quaternion GetCastOrientation() => transform.rotation;
    }
}