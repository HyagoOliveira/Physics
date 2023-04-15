using System;
using UnityEngine;

namespace ActionCode.Physics
{
    public abstract class AbstractAxis
    {
        [SerializeField, Tooltip("The local BoxBody component.")]
        private BoxBody body;
        [SerializeField, Tooltip("Whether this axis is enabled.")]
        private bool enabled = true;
        [SerializeField, Tooltip("Whether to use Moving Platforms.")]
        private bool useMovingPlatform;
        [SerializeField, Tooltip("Whether to use gravity.")]
        private bool useGravity;
        [SerializeField, Tooltip("The gravity speed for this axis.")]
        private float gravity;
        [SerializeField, Min(0f), Tooltip("The maximum speed allowed. Should always be positive.")]
        private float maxSpeed = 50f;
        [SerializeField, Tooltip("The number of raycasts for this axis.")]
        private int raysCount = 3;

        /// <summary>
        /// Action fired when stops after colliding using any side of this axis.
        /// </summary>
        public event Action OnHitAnySide;

        /// <summary>
        /// Action fired when collision with a <see cref="MovingPlatform"/>.
        /// </summary>
        public event Action<MovingPlatform> OnCollision;

        #region Constants
        /// <summary>
        /// The default collision skin.
        /// </summary>
        public const float COLLISION_SKIN = 0.0001F;

        /// <summary>
        /// The default collision offset.
        /// </summary>
        public const float COLLISION_OFFSET = 0.0075f;

        /// <summary>
        /// Minimum allowed rays.
        /// </summary>
        public const int MIN_RAYS_COUNT = 2;

        /// <summary>
        /// Maximum allowed rays.
        /// </summary>
        public const int MAX_RAYS_COUNT = 64;
        #endregion

        #region Properties
        /// <summary>
        /// The local <see cref="BoxBody"/> component.
        /// </summary>
        public BoxBody Body => body;

        /// <summary>
        /// Whether this axis is enabled.
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        /// <summary>
        /// Whether to use Moving Platforms.
        /// </summary>
        public bool UseMovingPlatform
        {
            get => useMovingPlatform;
            set
            {
                useMovingPlatform = value;
                if (!UseMovingPlatform && IsUsingMovingPlatform())
                    ResetMovingPlatform();
            }
        }

        /// <summary>
        /// Whether to use gravity.
        /// </summary>
        public bool UseGravity
        {
            get => useGravity;
            set => useGravity = value;
        }

        /// <summary>
        /// Whether should draw the raycast collisions.
        /// </summary>
        public bool DrawCollisions { get; set; }

        /// <summary>
        /// Whether collisions are disabled.
        /// </summary>
        public bool IsCollisionsDisabled { get; private set; }

        /// <summary>
        /// The number of raycasts for this axis.
        /// </summary>
        public int RaysCount
        {
            get => raysCount;
            set => raysCount = Mathf.Clamp(value, MIN_RAYS_COUNT, MAX_RAYS_COUNT);
        }

        /// <summary>
        /// The current speed.
        /// </summary>
        public float Speed
        {
            get => speed;
            set => speed = Mathf.Clamp(value, -MaxSpeed, MaxSpeed);
        }

        /// <summary>
        /// The maximum speed allowed. Should always be positive.
        /// </summary>
        public float MaxSpeed
        {
            get => maxSpeed;
            set => maxSpeed = Mathf.Max(value, 0f);
        }

        /// <summary>
        /// The gravity speed for this axis.
        /// </summary>
        public float Gravity
        {
            get => gravity;
            set => gravity = value;
        }
        #endregion

        protected IRaycastHit negativeHit;
        protected IRaycastHit positiveHit;

        protected MovingPlatform platform;

        private bool isNegativeCollision;
        private bool isPositiveCollision;

        private float speed;

        internal virtual void Reset(BoxBody body) => this.body = body;

        internal void Validate() => RaysCount = raysCount;

        /// <summary>
        /// Whether has speed. 
        /// </summary>
        /// <returns>True if has speed. False otherwise.</returns>
        public bool HasSpeed() => Mathf.Abs(Speed) > 0f;

        /// <summary>
        /// Whether has gravity.
        /// </summary>
        /// <returns>True if has gravity. False otherwise.</returns>
        public bool HasGravity() => Mathf.Abs(Gravity) > 0f;

        /// <summary>
        /// Whether is colliding in any side.
        /// </summary>
        /// <returns>True if colliding. False otherwise.</returns>
        public bool IsCollision() => IsCollisionOnNegativeSide() || IsCollisionOnPositiveSide();

        /// <summary>
        /// Whether is moving into any side.
        /// </summary>
        /// <returns>True if moving. False otherwise.</returns>
        public bool IsMoving() => IsMovingToNegativeSide() || IsMovingToPositiveSide();

        /// <summary>
        /// Whether is moving using a <see cref="MovingPlatform"/>.
        /// </summary>
        /// <returns>True if using a <see cref="MovingPlatform"/>. False otherwise.</returns>
        public bool IsUsingMovingPlatform() => platform;

        /// <summary>
        /// Whether this axis is Enabled and using a moving platform.
        /// </summary>
        /// <returns></returns>
        public bool IsEnabledAndUsingMovingPlatform() => Enabled && IsUsingMovingPlatform();

        /// <summary>
        /// Whether can move in any side.
        /// </summary>
        /// <returns>True if can move. False otherwise.</returns>
        public bool CanMove() => !IsCollisionOnNegativeSide() || !IsCollisionOnPositiveSide();

        /// <summary>
        /// Whether can move in the given direction.
        /// </summary>
        /// <param name="direction">The direction to check.</param>
        /// <returns>True if can move. False otherwise.</returns>
        public abstract bool CanMove(Vector3 direction);

        /// <summary>
        /// Stops the current speed.
        /// </summary>
        public void StopSpeed() => Speed = 0F;

        /// <summary>
        /// Enables collisions in this axis.
        /// </summary>
        public void EnableCollisions() => IsCollisionsDisabled = false;

        /// <summary>
        /// Disables collisions in this axis.
        /// </summary>
        public void DisableCollisions() => IsCollisionsDisabled = true;

        /// <summary>
        /// Updates the rotation based on the given direction.
        /// </summary>
        /// <param name="direction">A small byte value of -1 or 1.</param>
        public void UpdateRotation(sbyte direction)
        {
            if (direction < 0) RotateToNegativeSide();
            else if (direction > 0) RotateToPositiveSide();
        }

        /// <summary>
        /// Update all axis collisions.
        /// </summary>
        public void UpdateCollisions()
        {
            if (IsCollisionsDisabled) return;

            var points = GetCollisionPoints();
            var distance = GetHalfScale() + COLLISION_SKIN;
            var speedPerFrame = Mathf.Abs(Speed * Time.deltaTime);
            var distanceUsingSpeed = distance + speedPerFrame;
            var negativeDistance = IsMovingToNegativeSide() ? distanceUsingSpeed : distance;
            var positiveDistance = IsMovingToPositiveSide() ? distanceUsingSpeed : distance;

            if (DrawCollisions) Debug.DrawLine(points.one, points.two, Color.green);

            isNegativeCollision = Body.Collider.Raycasts(
                points.one,
                points.two,
                -GetPositiveDirection(),
                out negativeHit,
                negativeDistance,
                Body.Collisions,
                RaysCount,
                DrawCollisions
            ) && IsValidNegativeCollision();

            isPositiveCollision = Body.Collider.Raycasts(
                points.one,
                points.two,
                GetPositiveDirection(),
                out positiveHit,
                positiveDistance,
                Body.Collisions,
                RaysCount,
                DrawCollisions
            ) && IsValidPositiveCollision();
        }

        internal void UpdatePhysics()
        {
            if (!Enabled) return;

            UpdateCollisions();
            UpdateGravity();
            RestrictCollisions();
            UpdateMovingPlatformCollisions();
        }

        internal void InvokeMovingEvent()
        {
            if (IsMovingToNegativeSide()) InvokeOnMovingNegativeSide();
            else if (IsMovingToPositiveSide()) InvokeOnMovingPositiveSide();
        }

        internal virtual void UpdatePositionUsingMovingPlatform(ref Vector3 position)
        {
            if (IsUsingMovingPlatform()) position += platform.Velocity;
        }

        protected bool IsMovingToNegativeSide() => Speed < 0F;
        protected bool IsMovingToPositiveSide() => Speed > 0F;

        protected bool IsGravityPositive() => Gravity > 0F;
        protected bool IsGravityNegative() => Gravity < 0F;

        protected bool IsCollisionOnNegativeSide() => isNegativeCollision;
        protected bool IsCollisionOnPositiveSide() => isPositiveCollision;

        protected bool IsPushingColliderOnNegativeSide() => IsMovingToNegativeSide() && IsCollisionOnNegativeSide();
        protected bool IsPushingColliderOnPositiveSide() => IsMovingToPositiveSide() && IsCollisionOnPositiveSide();

        protected bool IsNegativeCollisionWithMovingPlatform() =>
            isNegativeCollision && negativeHit.Transform.TryGetComponent(out platform);

        protected bool IsPositiveCollisionWithMovingPlatform() =>
            isPositiveCollision && positiveHit.Transform.TryGetComponent(out platform);

        protected virtual bool IsCollisionWithMovingPlatform() =>
            IsNegativeCollisionWithMovingPlatform() ||
            IsPositiveCollisionWithMovingPlatform();

        protected virtual bool IsValidNegativeCollision() => true;
        protected virtual bool IsValidPositiveCollision() => true;

        protected float GetCollisionDistance() => GetHalfScale() + COLLISION_SKIN;

        protected abstract void InvokeOnHitNegativeSide();
        protected abstract void InvokeOnHitPositiveSide();

        protected abstract void InvokeOnMovingNegativeSide();
        protected abstract void InvokeOnMovingPositiveSide();

        protected abstract void RotateToNegativeSide();
        protected abstract void RotateToPositiveSide();

        protected abstract void SetCollisionPoint(float point);

        protected abstract float GetHalfScale();
        protected abstract float GetOutOfCollisionPointOnNegativeSide();
        protected abstract float GetOutOfCollisionPointOnPositiveSide();

        protected abstract Vector3 GetPositiveDirection();
        protected abstract (Vector3 one, Vector3 two) GetCollisionPoints();

        internal void Disable()
        {
            ResetCollisions();
            ResetMovingPlatform();
        }

        private void ResetCollisions()
        {
            isNegativeCollision = false;
            isPositiveCollision = false;

            negativeHit = null;
            positiveHit = null;
        }

        private void ResetMovingPlatform() => platform = null;

        private void UpdateGravity()
        {
            var updateSpeedWithGravity = UseGravity && ShouldApplyGravity();
            if (updateSpeedWithGravity) Speed += Gravity * Time.deltaTime;
        }

        private void RestrictCollisions()
        {
            if (IsCollisionsDisabled) return;

            RestrictCollisionsUsingPushingColliders();
            RestrictCollisionsUsingMovement();
        }

        private void RestrictCollisionsUsingPushingColliders()
        {
            if (IsMoving()) return;

            var isBothSideCollision = IsCollisionOnNegativeSide() && IsCollisionOnPositiveSide();
            if (isBothSideCollision)
            {
                var negativePointCollision = GetOutOfCollisionPointOnNegativeSide();
                var positivePointCollision = GetOutOfCollisionPointOnPositiveSide();
                var centerPoint = Mathf.Lerp(negativePointCollision, positivePointCollision, 0.5F);

                SetCollisionPoint(centerPoint);
                //TODO invoke OnCrushed event.
            }
            else if (IsCollisionOnPositiveSide())
                SetCollisionPoint(GetOutOfCollisionPointOnPositiveSide());
            else if (IsCollisionOnNegativeSide())
                SetCollisionPoint(GetOutOfCollisionPointOnNegativeSide());
        }

        private void RestrictCollisionsUsingMovement()
        {
            var isMovingIntoNegativeSideCollision = IsMovingToNegativeSide() && IsCollisionOnNegativeSide();
            if (isMovingIntoNegativeSideCollision)
            {
                SetCollisionPoint(GetOutOfCollisionPointOnNegativeSide());

                StopSpeed();
                InvokeOnHitAnySide();
                InvokeOnHitNegativeSide();

                return;
            }

            var isMovingIntoPositiveSideCollision = IsMovingToPositiveSide() && IsCollisionOnPositiveSide();
            if (isMovingIntoPositiveSideCollision)
            {
                SetCollisionPoint(GetOutOfCollisionPointOnPositiveSide());

                StopSpeed();
                InvokeOnHitAnySide();
                InvokeOnHitPositiveSide();
            }
        }

        private void UpdateMovingPlatformCollisions()
        {
            if (!UseMovingPlatform) return;

            if (IsUsingMovingPlatform())
            {
                var shouldLeavePlatform =
                    !platform.gameObject.activeInHierarchy ||
                    !IsCollisionWithMovingPlatform();

                if (shouldLeavePlatform) platform = null;
            }
            else if (IsCollisionWithMovingPlatform())
                InvokeOnCollisionWithMovingPlatform();
        }

        private void InvokeOnHitAnySide() => OnHitAnySide?.Invoke();
        private void InvokeOnCollisionWithMovingPlatform() => OnCollision?.Invoke(platform);

        private bool ShouldApplyGravity() =>
            IsGravityNegative() && !IsCollisionOnNegativeSide() ||
            IsGravityPositive() && !IsCollisionOnPositiveSide();
    }
}