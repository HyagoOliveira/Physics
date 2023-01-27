using System;
using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Vertical Axis used by <see cref="BoxBody"/> component.
    /// </summary>
    [Serializable]
    public sealed class VerticalAxis : AbstractAxis
    {
        private readonly Quaternion upRotation = Quaternion.identity;
        private readonly Quaternion downRotation = Quaternion.Euler(Vector3.right * -180F);

        /// <summary>
        /// Action fired when the Box stops after colliding using the top side.
        /// </summary>
        public event Action OnHitTop;

        /// <summary>
        /// Action fired when the Box stops after colliding using the bottom side.
        /// </summary>
        public event Action OnHitBottom;

        /// <summary>
        /// Raycast information from the last top hit.
        /// </summary>
        public IRaycastHit TopHit => positiveHit;

        /// <summary>
        /// Raycast information from the last bottom hit.
        /// </summary>
        public IRaycastHit BottomHit => negativeHit;

        public override bool CanMove(Vector3 direction)
        {
            return
                direction.y < 0F && !IsCollisionDown() ||
                direction.y > 0F && !IsCollisionUp();
        }

        /// <summary>
        /// Checks if colliding on top. Triggers don't count as a solid collision.
        /// </summary>
        /// <returns>True if colliding on top. False otherwise.</returns>
        public bool IsCollisionUp() => IsCollisionOnPositiveSide();

        /// <summary>
        /// Checks if colliding on bottom. Triggers don't count as a solid collision.
        /// </summary>
        /// <returns>True if colliding on bottom. False otherwise.</returns>
        public bool IsCollisionDown() => IsCollisionOnNegativeSide();

        /// <summary>
        /// Check if moving upwards.
        /// </summary>
        /// <returns>True if moving upwards. False otherwise.</returns>
        public bool IsMovingUp() => IsMovingToPositiveSide();

        /// <summary>
        /// Check if moving downwards.
        /// </summary>
        /// <returns>True if moving downwards. False otherwise.</returns>
        public bool IsMovingDown() => IsMovingToNegativeSide();

        /// <summary>
        /// Checks if pushing against a solid top collider.
        /// </summary>
        /// <returns>True if pushing against a solid colliding at up.</returns>
        public bool IsPushingUpCollider() => IsPushingColliderOnPositiveSide();

        /// <summary>
        /// Checks if pushing against a solid bottom collider.
        /// </summary>
        /// <returns>True if pushing against a solid colliding at down.</returns>
        public bool IsPushingDownCollider() => IsPushingColliderOnNegativeSide();

        /// <summary>
        /// Check if gravity is pointing to up.
        /// </summary>
        /// <returns>True if gravity is pointing to right. False otherwise.</returns>
        public bool IsGravityUp() => Gravity > 0F;

        /// <summary>
        /// Check if gravity is pointing to down.
        /// </summary>
        /// <returns>True if gravity is pointing to left. False otherwise.</returns>
        public bool IsGravityDown() => Gravity < 0F;

        /// <summary>
        /// Rotates to up.
        /// </summary>
        public void RotateToUp() => Body.transform.rotation = upRotation;

        /// <summary>
        /// Rotates to down.
        /// </summary>
        public void RotateToDown() => Body.transform.rotation = downRotation;

        internal override void Reset(BoxBody body)
        {
            base.Reset(body);
            UseGravity = true;
            Gravity = Physics2D.gravity.y;
        }

        protected override float GetHalfScale() => Body.Collider.HalfSize.y;
        protected override float GetOutOfCollisionPointOnNegativeSide() => BottomHit.Point.y + GetHalfScale() - Body.Collider.Offset.y;
        protected override float GetOutOfCollisionPointOnPositiveSide() => TopHit.Point.y - GetHalfScale() - Body.Collider.Offset.y;

        protected override void InvokeOnHitNegativeSide() => OnHitBottom?.Invoke();
        protected override void InvokeOnHitPositiveSide() => OnHitTop?.Invoke();

        protected override Vector3 GetPositiveDirection() => Vector3.up;

        protected override (Vector3 one, Vector3 two) GetCollisionPoints()
        {
            var bounds = Body.Collider.Bounds;
            var middleCenter = bounds.center;
            var leftCenter = new Vector3(bounds.min.x, middleCenter.y, middleCenter.z);
            var rightCenter = new Vector3(bounds.max.x, middleCenter.y, middleCenter.z);
            var leftOffset = Vector3.left * Offset;
            var rightOffset = Vector3.right * Offset;

            leftCenter += rightOffset;
            rightCenter += leftOffset;

            return (leftCenter, rightCenter);
        }

        protected override void RotateToPositiveSide() => RotateToUp();
        protected override void RotateToNegativeSide() => RotateToDown();

        protected override void SetCollisionPoint(float point) => Body.currentPosition.y = point;

        protected override bool IsCollisionWithMovingPlatform() => IsNegativeCollisionWithMovingPlatform();
    }
}