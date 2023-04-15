using System;
using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Horizontal Axis used by <see cref="BoxBody"/> component.
    /// </summary>
    [Serializable]
    public sealed class HorizontalAxis : AbstractAxis
    {
        [SerializeField, Tooltip("The rotation facing the left side.")]
        private Vector3 leftRotation;
        [SerializeField, Tooltip("The rotation facing the right side.")]
        private Vector3 rightRotation;

        /// <summary>
        /// Action fired when the <see cref="BoxBody"/> stops after colliding using the left side.
        /// </summary>
        public event Action OnHitLeft;

        /// <summary>
        /// Action fired when the <see cref="BoxBody"/> stops after colliding using the right side.
        /// </summary>
        public event Action OnHitRight;

        /// <summary>
        /// Action fired when moving right.
        /// </summary>
        public event Action OnMovingRight;

        /// <summary>
        /// Action fired when moving left.
        /// </summary>
        public event Action OnMovingLeft;

        /// <summary>
        /// Raycast information from the last left hit.
        /// </summary>
        public IRaycastHit LeftHit => negativeHit;

        /// <summary>
        /// Raycast information from the last right hit.
        /// </summary>
        public IRaycastHit RightHit => positiveHit;

        internal override void Reset(BoxBody body)
        {
            base.Reset(body);

            rightRotation = body.transform.eulerAngles;
            leftRotation = -rightRotation;

            if (Mathf.Approximately(leftRotation.y, 0f)) leftRotation = Vector3.up * -180f;
        }

        public override bool CanMove(Vector3 direction)
        {
            return
                direction.x < 0F && !IsCollisionLeft() ||
                direction.x > 0F && !IsCollisionRight();
        }

        /// <summary>
        /// Checks if colliding on left side. Triggers don't count as a solid collision.
        /// </summary>
        /// <returns>True if colliding. False otherwise.</returns>
        public bool IsCollisionLeft() => IsCollisionOnNegativeSide();

        /// <summary>
        /// Checks if colliding on right side. Triggers don't count as a solid collision.
        /// </summary>
        /// <returns><inheritdoc cref="IsCollisionLeft"/></returns>
        public bool IsCollisionRight() => IsCollisionOnPositiveSide();

        /// <summary>
        /// Check if moving leftwards.
        /// </summary>
        /// <returns>True if moving to left. False otherwise.</returns>
        public bool IsMovingLeft() => IsMovingToNegativeSide();

        /// <summary>
        /// Check if moving rightwards.
        /// </summary>
        /// <returns>True if moving to right. False otherwise.</returns>
        public bool IsMovingRight() => IsMovingToPositiveSide();

        /// <summary>
        /// Checks if pushing against a solid left collider.
        /// </summary>
        /// <returns>True if pushing against a solid colliding at left.</returns>
        public bool IsPushingLeftCollider() => IsPushingColliderOnNegativeSide();

        /// <summary>
        /// Checks if pushing against a solid right collider.
        /// </summary>
        /// <returns>True if pushing against a solid colliding at right.</returns>
        public bool IsPushingRightCollider() => IsPushingColliderOnPositiveSide();

        /// <summary>
        /// Check if gravity is pointing to left.
        /// </summary>
        /// <returns>True if gravity is pointing to left. False otherwise.</returns>
        public bool IsGravityLeft() => IsGravityNegative();

        /// <summary>
        /// Check if gravity is pointing to right.
        /// </summary>
        /// <returns>True if gravity is pointing to right. False otherwise.</returns>
        public bool IsGravityRight() => IsGravityPositive();

        /// <summary>
        /// Rotates to the left.
        /// </summary>
        public void RotateToLeft() => Body.transform.rotation = Quaternion.Euler(leftRotation);

        /// <summary>
        /// Rotates to the right.
        /// </summary>
        public void RotateToRight() => Body.transform.rotation = Quaternion.Euler(rightRotation);

        internal override void UpdatePositionUsingMovingPlatform(ref Vector3 position)
        {
            if (IsUsingMovingPlatform()) position.x += platform.Velocity.x;
        }

        protected override bool IsValidNegativeCollision() => IsAllowedAngle(negativeHit.Normal);
        protected override bool IsValidPositiveCollision() => IsAllowedAngle(positiveHit.Normal);

        protected override Vector3 GetPositiveDirection() => Vector3.right;

        protected override (Vector3 one, Vector3 two) GetCollisionPoints()
        {
            var bounds = Body.Collider.Bounds;
            var middleCenter = bounds.center;
            var topCenter = new Vector3(middleCenter.x, bounds.max.y, middleCenter.z);
            var bottomCenter = new Vector3(middleCenter.x, bounds.min.y, middleCenter.z);
            var upOffset = Vector3.up * COLLISION_OFFSET;
            var downOffset = Vector3.down * COLLISION_OFFSET;

            topCenter += downOffset;
            bottomCenter += upOffset;

            return (topCenter, bottomCenter);
        }

        protected override void InvokeOnHitNegativeSide() => OnHitLeft?.Invoke();
        protected override void InvokeOnHitPositiveSide() => OnHitRight?.Invoke();

        protected override void InvokeOnMovingNegativeSide() => OnMovingLeft?.Invoke();
        protected override void InvokeOnMovingPositiveSide() => OnMovingRight?.Invoke();

        protected override void RotateToNegativeSide() => RotateToLeft();
        protected override void RotateToPositiveSide() => RotateToRight();

        protected override void SetCollisionPoint(float point) => Body.currentPosition.x = point;

        protected override float GetHalfScale() => Body.Collider.HalfSize.x;

        protected override float GetOutOfCollisionPointOnNegativeSide() => LeftHit.Point.x + GetHalfScale() - Body.Collider.Offset.x;
        protected override float GetOutOfCollisionPointOnPositiveSide() => RightHit.Point.x - GetHalfScale() - Body.Collider.Offset.x;

        private bool IsAllowedAngle(Vector3 normal)
        {
            var hasVerticalNormal = Mathf.Abs(normal.y) > 0f;
            if (!hasVerticalNormal) return true;

            var angle = Vector3.Angle(normal, Vector3.up);
            return angle > Body.SlopeLimit || Mathf.Approximately(angle, Body.SlopeLimit);
        }
    }
}