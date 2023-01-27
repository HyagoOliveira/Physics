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
        [SerializeField, Range(0f, MAX_SLOPE_LIMIT), Tooltip("The maximum angle limit (in degrees) of a slope.")]
        private float slopeLimit = 0F;
        [SerializeField, Min(MIN_RAYS_COUNT), Tooltip("The number of raycasts for this axis.")]
        private int raysCount = 3;
        [SerializeField, Tooltip("The layer mask collisions. Only layers on this mask will be used on this axis.")]
        private LayerMask collisions;

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
        public const int MIN_RAYS_COUNT = 1;

        /// <summary>
        /// Maximum allowed rays.
        /// </summary>
        public const int MAX_RAYS_COUNT = 64;

        /// <summary>
        /// Maximum allowed slope limit.
        /// </summary>
        public const float MAX_SLOPE_LIMIT = 90F;
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
                    platform = null;
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
        /// Whether collisions are locked.
        /// <para>If enabled, no collisions will be calculated.</para>
        /// </summary>
        public bool IsCollisionsLocked { get; set; }

        /// <summary>
        /// The number of raycasts for this axis.
        /// </summary>
        public int RaysCount
        {
            get => raysCount;
            set => raysCount = Mathf.Clamp(value, MIN_RAYS_COUNT, MAX_RAYS_COUNT);
        }

        /// <summary>
        /// The layer mask collisions. Only layers on this mask will be used on this axis.
        /// </summary>
        public LayerMask Collisions
        {
            get => collisions;
            set => collisions = value;
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
        /// The maximum angle limit (in degrees) of a slope.
        /// </summary>
        public float SlopeLimit
        {
            get => slopeLimit;
            set => slopeLimit = Mathf.Clamp(value, 0f, MAX_SLOPE_LIMIT);
        }

        /// <summary>
        /// The gravity speed for this axis.
        /// </summary>
        public float Gravity
        {
            get => gravity;
            set => gravity = value;
        }

        /// <summary>
        /// The current facing direction.
        /// </summary>
        public float Facing
        {
            get => facing;
            set
            {
                facing = value;
                UpdateRotation();
            }
        }
        #endregion

        protected IRaycastHit negativeHit;
        protected IRaycastHit positiveHit;

        public MovingPlatform platform;

        private bool isNegativeCollision;
        private bool isPositiveCollision;

        private float speed;
        private float facing;

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

        public bool IsEnabledAndUsingMovingPlatform() => Enabled && IsUsingMovingPlatform();

        /// <summary>
        /// Whether can move in any side.
        /// </summary>
        /// <returns>True if can move. False otherwise.</returns>
        public bool CanMove() => !IsCollisionOnNegativeSide() || !IsCollisionOnPositiveSide();

        /// <summary>
        /// Whether if facing a collision. Triggers don't count as a solid collision.
        /// </summary>
        /// <returns>True if facing a collision. False otherwise.</returns>
        public bool IsFacingCollision() =>
            IsFacingNegativeSide() && IsCollisionOnNegativeSide() ||
            IsFacingPositiveSide() && IsCollisionOnPositiveSide();

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
        /// Update all axis collisions.
        /// </summary>
        public void UpdateCollisions()
        {
            var points = GetCollisionPoints();
            var distance = GetHalfScale() + COLLISION_SKIN;
            var speedPerFrame = Mathf.Abs(Speed * Time.deltaTime);
            var distanceUsingSpeed = distance + speedPerFrame;
            var negativeDistance = IsMovingToNegativeSide() ? distanceUsingSpeed : distance;
            var positiveDistance = IsMovingToPositiveSide() ? distanceUsingSpeed : distance;

            if (DrawCollisions) Debug.DrawLine(points.one, points.two, Color.green);

            isNegativeCollision = Body.Collider.Raycasts(points.one, points.two, -GetPositiveDirection(),
                out negativeHit, negativeDistance, Collisions, SlopeLimit, RaysCount, DrawCollisions);
            isPositiveCollision = Body.Collider.Raycasts(points.one, points.two, GetPositiveDirection(),
                out positiveHit, positiveDistance, Collisions, SlopeLimit, RaysCount, DrawCollisions);
        }

        internal void UpdatePhysics()
        {
            if (!Enabled) return;

            UpdateCollisions();
            UpdateGravity();
            RestrictCollisions();
            UpdateMovingPlatformCollisions();
        }

        internal virtual void UpdatePositionUsingMovingPlatform(ref Vector3 position)
        {
            if (IsUsingMovingPlatform()) position += platform.Velocity;
        }

        protected bool IsMovingToNegativeSide() => Speed < 0F;
        protected bool IsMovingToPositiveSide() => Speed > 0F;

        protected bool IsGravityPositive() => Gravity > 0F;
        protected bool IsGravityNegative() => Gravity < 0F;

        protected bool IsFacingNegativeSide() => Facing < 0F;
        protected bool IsFacingPositiveSide() => Facing > 0F;

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

        protected float GetCollisionDistance() => GetHalfScale() + COLLISION_SKIN;

        protected abstract void InvokeOnHitNegativeSide();
        protected abstract void InvokeOnHitPositiveSide();

        protected abstract void RotateToNegativeSide();
        protected abstract void RotateToPositiveSide();

        protected abstract void SetCollisionPoint(float point);

        protected abstract float GetHalfScale();
        protected abstract float GetOutOfCollisionPointOnNegativeSide();
        protected abstract float GetOutOfCollisionPointOnPositiveSide();

        protected abstract Vector3 GetPositiveDirection();
        protected abstract (Vector3 one, Vector3 two) GetCollisionPoints();

        private void UpdateGravity()
        {
            var updateSpeedWithGravity = UseGravity && ShouldApplyGravity();
            if (updateSpeedWithGravity) Speed += Gravity * Time.deltaTime;
        }

        private void RestrictCollisions()
        {
            if (IsCollisionsLocked) return;

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

        private void UpdateRotation()
        {
            if (IsFacingNegativeSide()) RotateToNegativeSide();
            else if (IsFacingPositiveSide()) RotateToPositiveSide();
        }

        private void InvokeOnHitAnySide() => OnHitAnySide?.Invoke();
        private void InvokeOnCollisionWithMovingPlatform() => OnCollision?.Invoke(platform);

        private bool ShouldApplyGravity() =>
            IsGravityNegative() && !IsCollisionOnNegativeSide() ||
            IsGravityPositive() && !IsCollisionOnPositiveSide();
    }
}