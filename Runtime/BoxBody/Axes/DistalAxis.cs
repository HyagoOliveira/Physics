using System;
using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Distal Axis (forward/backward direction) used by <see cref="BoxBody"/> component.
    /// </summary>
    [Serializable]
    public sealed class DistalAxis //: AbstractAxis
    {/*
        /// <summary>
        /// Action fired when the Box stops after colliding using the forward side.
        /// </summary>
        public event Action OnHitForward;

        /// <summary>
        /// Action fired when the Box stops after colliding using the backward side.
        /// </summary>
        public event Action OnHitBackward;

        /// <summary>
        /// Action fired when the Box starts to move forward.
        /// </summary>
        public event Action OnStartMoveForward;

        /// <summary>
        /// Action fired when the Box starts to move backwards.
        /// </summary>
        public event Action OnStartMoveBackward;

        /// <summary>
        /// Action fired when the Box is moving forward.
        /// </summary>
        public event Action OnMovingForward;

        /// <summary>
        /// Action fired when the Box is moving backward.
        /// </summary>
        public event Action OnMovingBackward;

        /// <summary>
        /// Raycast information from the last forward hit.
        /// </summary>
        public IRaycastHit ForwardHit => positiveHit;

        /// <summary>
        /// Raycast information from the last backward hit.
        /// </summary>
        public IRaycastHit BackwardHit => negativeHit;

        public override bool CanMove(Vector3 direction)
        {
            return
                direction.z < 0F && !IsBackwardCollision() ||
                direction.z > 0F && !IsForwardCollision();
        }

        /// <summary>
        /// Checks if colliding backward. Triggers don't count as a solid collision.
        /// </summary>
        /// <returns>True if colliding backward. False otherwise.</returns>
        public bool IsBackwardCollision() => IsCollisionOnNegativeSide();

        /// <summary>
        /// Checks if colliding forward. Triggers don't count as a solid collision.
        /// <returns>True if colliding forward. False otherwise.</returns>
        /// </summary>
        public bool IsForwardCollision() => IsCollisionOnPositiveSide();

        /// <summary>
        /// Check if moving backwards.
        /// </summary>
        /// <returns>True if moving backwards. False otherwise.</returns>
        public bool IsMovingBackward() => IsMovingToNegativeSide();

        /// <summary>
        /// Check if moving forward.
        /// </summary>
        /// <returns>True if moving forward. False otherwise.</returns>
        public bool IsMovingForward() => IsMovingToPositiveSide();

        /// <summary>
        /// Check if gravity is pointing to backward.
        /// </summary>
        /// <returns>True if gravity is pointing to backward. False otherwise.</returns>
        public bool IsGravityBackward() => IsGravityNegative();

        /// <summary>
        /// Check if gravity is pointing to forward.
        /// </summary>
        /// <returns>True if gravity is pointing to forward. False otherwise.</returns>
        public bool IsGravityToForward() => IsGravityPositive();

        /// <summary>
        /// Checks if pushing against a solid backward collider.
        /// </summary>
        /// <returns>True if pushing against a solid colliding at back.</returns>
        public bool IsPushingBackwardCollider() => IsPushingColliderOnNegativeSide();

        /// <summary>
        /// Checks if pushing against a solid forward collider.
        /// </summary>
        /// <returns>True if pushing against a solid colliding at forward.</returns>
        public bool IsPushingForwardCollider() => IsPushingColliderOnPositiveSide();

        protected override float GetHalfSize() => Body.Collider.HalfSize.z;
        protected override float GetDeltaMovement() => Body.DeltaPosition.z;
        protected override float GetCollisionPointOnNegativeSide() => BackwardHit.Point.z + GetHalfSize() - Body.Collider.Offset.z;
        protected override float GetCollisionPointOnPositiveSide() => ForwardHit.Point.z - GetHalfSize() - Body.Collider.Offset.z;

        protected override void InvokeOnHitNegativeSide() => OnHitBackward?.Invoke();
        protected override void InvokeOnHitPositiveSide() => OnHitForward?.Invoke();

        protected override void CheckMovementActions()
        {
            var wasMovingForward = Body.LastDeltaPosition.z > 0f;
            var wasMovingBackward = Body.LastDeltaPosition.z < 0f;

            var isMovingForward = Body.DeltaPosition.x > 0F;
            var isMovingBackward = Body.DeltaPosition.x < 0F;

            var startMoveForward = !isMovingForward && isMovingForward;
            var startMoveBackward = !isMovingBackward && isMovingBackward;

            if (startMoveForward) OnStartMoveForward?.Invoke();
            else if (startMoveBackward) OnStartMoveBackward?.Invoke();

            if (isMovingForward) OnMovingForward?.Invoke();
            else if (isMovingBackward) OnMovingBackward?.Invoke();
        }

        protected override Vector3 GetPositiveDirection() => Vector3.forward;
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
        }*/
    }
}