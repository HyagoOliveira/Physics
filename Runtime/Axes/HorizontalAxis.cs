using System;
using UnityEngine;

namespace ActionCode.BoxBodies
{
    /// <summary>
    /// Horizontal Axis used by <see cref="BoxBody"/> component.
    /// </summary>
    [Serializable]
    public sealed class HorizontalAxis : AbstractAxis
    {
        /// <summary>
        /// Action fired when the Box stops after colliding using the left side.
        /// </summary>
        public event Action OnHitLeft;

        /// <summary>
        /// Action fired when the Box stops after colliding using the right side.
        /// </summary>
        public event Action OnHitRight;

        /// <summary>
        /// Action fired when the Box starts to move left.
        /// </summary>
        public event Action OnStartMoveLeft;

        /// <summary>
        /// Action fired when the Box starts to move right.
        /// </summary>
        public event Action OnStartMoveRight;

        /// <summary>
        /// Action fired when the Box is moving left.
        /// </summary>
        public event Action OnMovingLeft;

        /// <summary>
        /// Action fired when the Box is moving right.
        /// </summary>
        public event Action OnMovingRight;

        /// <summary>
        /// The current movement input on this axis.
        /// </summary>
        public override float MoveInput
        {
            get => moveInput;
            set
            {
                moveInput = value;
                UpdateRotation();
            }
        }

        /// <summary>
        /// The current direction side.
        /// </summary>
        public float FacingSide { get; private set; }

        /// <summary>
        /// Raycast information from the last left hit.
        /// </summary>
        public IRaycastHit LeftHit => negativeHit;

        /// <summary>
        /// Raycast information from the last right hit.
        /// </summary>
        public IRaycastHit RightHit => positiveHit;

        private float moveInput = 0f;

        public HorizontalAxis() => SlopeLimit = 45f;

        public override void Initialize(BoxBody body)
        {
            base.Initialize(body);
            FacingSide = 1F;
        }

        public override bool CanMove(Vector3 direction)
        {
            return
                direction.x < 0F && !IsCollisionLeft() ||
                direction.x > 0F && !IsCollisionRight();
        }

        /// <summary>
        /// Checks if colliding on left. Triggers don't count as a solid collision.
        /// </summary>
        /// <returns>True if colliding on left. False otherwise.</returns>
        public bool IsCollisionLeft() => IsCollisionOnNegativeSide();

        /// <summary>
        /// Checks if colliding on right. Triggers don't count as a solid collision.
        /// </summary>
        /// <returns>True if colliding on right. False otherwise.</returns>
        public bool IsCollisionRight() => IsCollisionOnPositiveSide();

        /// <summary>
        /// Checks if facing colliding. Triggers don't count as a solid collision.
        /// </summary>
        /// <returns>True if facing colliding on right. False otherwise.</returns>
        public bool IsFaceCollision() =>
            FacingSide < 0F && IsCollisionLeft() ||
            FacingSide > 0F && IsCollisionRight();

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
        public void RotateToLeft()
        {
            FacingSide = -1;
            Body.transform.rotation = Quaternion.Euler(Vector3.up * -180F);
        }

        /// <summary>
        /// Rotates to the right.
        /// </summary>
        public void RotateToRight()
        {
            FacingSide = 1;
            Body.transform.rotation = Quaternion.identity;
        }

        private void UpdateRotation()
        {
            if (MoveInput < 0) RotateToLeft();
            else if (MoveInput > 0) RotateToRight();
        }

        protected override float GetHalfSize() => Body.Collider.HalfSize.x;
        protected override float GetDeltaMovement() => Body.DeltaPosition.x;
        protected override float GetCollisionPointOnNegativeSide() => LeftHit.Point.x + GetHalfSize() - Body.Collider.Offset.x;
        protected override float GetCollisionPointOnPositiveSide() => RightHit.Point.x - GetHalfSize() - Body.Collider.Offset.x;

        protected override void InvokeOnHitNegativeSide() => OnHitLeft?.Invoke();
        protected override void InvokeOnHitPositiveSide() => OnHitRight?.Invoke();

        protected override void CheckMovementActions()
        {
            var wasMovingLeft = Body.LastDeltaPosition.x < 0f;
            var wasMovingRight = Body.LastDeltaPosition.x > 0f;

            var isMovingLeft = Body.DeltaPosition.x < 0F;
            var isMovingRight = Body.DeltaPosition.x > 0F;

            var startMoveLeft = !wasMovingLeft && isMovingLeft;
            var startMoveRight = !wasMovingRight && isMovingRight;

            if (startMoveLeft) OnStartMoveLeft?.Invoke();
            else if (startMoveRight) OnStartMoveRight?.Invoke();

            if (isMovingLeft) OnMovingLeft?.Invoke();
            else if (isMovingRight) OnMovingRight?.Invoke();
        }

        protected override Vector3 GetPositiveDirection() => Vector3.right;
        protected override (Vector3 one, Vector3 two) GetCollisionPoints()
        {
            var bounds = Body.Collider.Bounds;
            var middleCenter = bounds.center;
            var topCenter = new Vector3(middleCenter.x, bounds.max.y, middleCenter.z);
            var bottomCenter = new Vector3(middleCenter.x, bounds.min.y, middleCenter.z);

            var upOffset = Vector3.up * Offset + Body.Velocity;
            var downOffset = Vector3.down * Offset + Body.Velocity;

            topCenter += downOffset;
            bottomCenter += upOffset;

            return (topCenter, bottomCenter);
        }
    }
}