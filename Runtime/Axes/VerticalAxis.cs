using System;
using UnityEngine;

namespace ActionCode.BoxBodies
{
    /// <summary>
    /// Vertical Axis used by <see cref="BoxBody"/> component.
    /// </summary>
    [Serializable]
    public sealed class VerticalAxis : AbstractAxis
    {
        /// <summary>
        /// Action fired when the Box stops after colliding using the top side.
        /// </summary>
        public event Action OnHitTop;

        /// <summary>
        /// Action fired when the Box stops after colliding using the bottom side.
        /// </summary>
        public event Action OnHitBottom;

        /// <summary>
        /// Action fired when the Box starts to move up.
        /// </summary>
        public event Action OnStartMoveUp;

        /// <summary>
        /// Action fired when the Box starts to move down.
        /// </summary>
        public event Action OnStartMoveDown;

        /// <summary>
        /// Action fired when the Box is moving up.
        /// </summary>
        public event Action OnMovingUp;

        /// <summary>
        /// Action fired when the Box is moving down.
        /// </summary>
        public event Action OnMovingDown;

        /// <summary>
        /// Raycast information from the last top hit.
        /// </summary>
        public IRaycastHit TopHit => positiveHit;

        /// <summary>
        /// Raycast information from the last bottom hit.
        /// </summary>
        public IRaycastHit BottomHit => negativeHit;

        public VerticalAxis() => Gravity = Physics.gravity.y;

        public bool UpdateFarBottomCollisions(out IRaycastHit hit)
        {
            var points = GetCollisionPoints();
            var distance = GetHalfSize() + 0.25F;

            return Body.Collider.Raycasts(points.one, points.two, Vector3.down,
                out hit, distance, Collisions, SlopeLimit, RaysCount, DrawCollisions);
        }

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
        public bool IsGravityUp() => IsGravityPositive();

        /// <summary>
        /// Check if gravity is pointing to down.
        /// </summary>
        /// <returns>True if gravity is pointing to left. False otherwise.</returns>
        public bool IsGravityDown() => IsGravityNegative();

        protected override float GetHalfSize() => Body.Collider.HalfSize.y;
        protected override float GetDeltaMovement() => Body.DeltaPosition.y;
        protected override float GetCollisionPointOnNegativeSide() => BottomHit.Point.y + GetHalfSize() - Body.Collider.Offset.y;
        protected override float GetCollisionPointOnPositiveSide() => TopHit.Point.y - GetHalfSize() - Body.Collider.Offset.y;

        protected override void RaiseOnHitNegativeSide() => OnHitBottom?.Invoke();
        protected override void RaiseOnHitPositiveSide() => OnHitTop?.Invoke();

        protected override void CheckMovementActions()
        {
            var wasMovingUp = Body.LastDeltaPosition.y > 0f;
            var wasMovingDown = Body.LastDeltaPosition.y < 0f;

            var isMovingUp = Body.DeltaPosition.y > 0F;
            var isMovingDown = Body.DeltaPosition.y < 0F;

            var startMoveUp = !wasMovingUp && isMovingUp;
            var startMoveDown = !wasMovingDown && isMovingDown;

            if (startMoveUp) OnStartMoveUp?.Invoke();
            else if (startMoveDown) OnStartMoveDown?.Invoke();

            if (isMovingUp) OnMovingUp?.Invoke();
            else if (isMovingDown) OnMovingDown?.Invoke();
        }

        protected override Vector3 GetPositiveDirection() => Vector3.up;
        protected override (Vector3 one, Vector3 two) GetCollisionPoints()
        {
            var bounds = Body.Collider.Bounds;
            var middleCenter = bounds.center;
            var leftCenter = new Vector3(bounds.min.x, middleCenter.y, middleCenter.z);
            var rightCenter = new Vector3(bounds.max.x, middleCenter.y, middleCenter.z);

            var leftOffset = Vector3.left * Offset + Body.Velocity;
            var rightOffset = Vector3.right * Offset + Body.Velocity;

            leftCenter += rightOffset;
            rightCenter += leftOffset;

            return (leftCenter, rightCenter);
        }
    }
}