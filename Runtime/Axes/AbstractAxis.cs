using System;
using UnityEngine;

namespace ActionCode.BoxBodies
{
    /// <summary>
    /// Abstract axis class.
    /// <para>
    /// An axis consists of two sides: a negative and a positive. 
    /// Physics calculations are done on both sides.
    /// </para>
    /// <para>
    /// Negative and Positive side are concepts relative only to this abstract class.
    /// Each implementation should replace them by the axis real side. 
    /// </para>
    /// </summary>
    public abstract class AbstractAxis
    {
        [SerializeField, Tooltip("Whether this axis is enabled.")]
        private bool enabled = true;
        [SerializeField, Min(MIN_RAYS_COUNT), Tooltip("The number of raycasts for this axis.")]
        private int raysCount = 3;
        [SerializeField, Tooltip("The gravity speed for this axis.")]
        private float gravity;
        [SerializeField, Min(0f), Tooltip("The maximum speed allowed. Should always be positive.")]
        private float maxSpeed = 25f;
        [SerializeField, Tooltip("Raycast offset. It changes the raycast origin positions.")]
        private float offset = 0.001f;
        [SerializeField, Range(0f, MAX_SLOPE_LIMIT), Tooltip("The maximum angle limit (in degrees) of a slope.")]
        private float slopeLimit = 0F;
        [SerializeField, Tooltip("The layer mask collisions. Only layers on this mask will be used on this axis.")]
        private LayerMask collisions;

        /// <summary>
        /// Action fired when stops after colliding using any side of this axis.
        /// </summary>
        public event Action OnHitAnySide;

        #region Constants
        /// <summary>
        /// The default collision skin.
        /// </summary>
        public const float COLLISION_SKIN = 0.0001F;

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
        /// The BoxBody object.
        /// </summary>
        public BoxBody Body { get; private set; }

        /// <summary>
        /// Whether this axis is enabled.
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public bool GravityEnabled { get; set; } = true;

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
        /// Raycast offset. It changes the raycast origin positions
        /// </summary>
        public float Offset
        {
            get => offset;
            set => offset = Mathf.Clamp(value, -GetHalfSize(), GetHalfSize());
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
        /// The collision point after the last collisions calculations.
        /// </summary>
        public float CollisionPoint { get; private set; }

        /// <summary>
        /// The current movement input on this axis.
        /// </summary>
        public virtual float MoveInput { get; set; }
        #endregion

        protected IRaycastHit negativeHit;
        protected IRaycastHit positiveHit;

        private bool isNegativeCollision;
        private bool isPositiveCollision;

        private float speed;

        /// <summary>
        /// Initializes this axis using the given body.
        /// </summary>
        /// <param name="body">A BoxBody instance.</param>
        public virtual void Initialize(BoxBody body) => Body = body;

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
        /// Whether is moving to any side.
        /// </summary>
        /// <returns>True if moving. False otherwise.</returns>
        public bool IsMoving() => IsMovingToNegativeSide() || IsMovingToPositiveSide();

        /// <summary>
        /// Whether can move in any side.
        /// </summary>
        /// <returns>True if can move. False otherwise.</returns>
        public bool CanMove() => !IsCollisionOnNegativeSide() || !IsCollisionOnPositiveSide();

        /// <summary>
        /// Whether it has any move input.
        /// </summary>
        /// <returns>True if it has any move input. False otherwise.</returns>
        public bool HasMoveInput() => Math.Abs(MoveInput) > 0F;

        /// <summary>
        /// Whether can move in the given direction.
        /// </summary>
        /// <param name="direction">The direction to check.</param>
        /// <returns>True if can move. False otherwise.</returns>
        public abstract bool CanMove(Vector3 direction);

        /// <summary>
        /// Whether is pushing against a collider in any side.
        /// </summary>
        /// <returns>True if pushing against a collider. False otherwise.</returns>
        public bool IsPushingCollider() => IsPushingColliderOnNegativeSide() || IsPushingColliderOnPositiveSide();

        /// <summary>
        /// Stops the current speed.
        /// </summary>
        public void StopSpeed() => Speed = 0F;

        /// <summary>
        /// Update the axis collisions.
        /// </summary>
        public void UpdateCollisions()
        {
            var points = GetCollisionPoints();
            var distance = GetHalfSize() + COLLISION_SKIN;

            isNegativeCollision = Body.Collider.Raycasts(points.one, points.two, -GetPositiveDirection(),
                out negativeHit, distance, Collisions, SlopeLimit, RaysCount, DrawCollisions);
            isPositiveCollision = Body.Collider.Raycasts(points.one, points.two, GetPositiveDirection(),
                out positiveHit, distance, Collisions, SlopeLimit, RaysCount, DrawCollisions);
        }

        /// <summary>
        /// Updates this axis.
        /// </summary>
        internal void Update()
        {
            if (!Enabled) return;

            UpdateCollisions();
            UpdateGravity();
            RestrictMovement();
        }

        /// <summary>
        /// Checks this axis movement.
        /// </summary>
        internal void CheckMovement()
        {
            if (Enabled) CheckMovementActions();
        }

        /// <summary>
        /// Stops the current Speed if moving into collision.
        /// </summary>
        internal void StopSpeedIfMovingIntoCollision()
        {
            var isMovingToCollision =
                IsCollisionOnPositiveSide() && IsMovingToPositiveSide() ||
                IsCollisionOnNegativeSide() && IsMovingToNegativeSide();
            if (isMovingToCollision) StopSpeed();
        }

        /// <summary>
        /// Checks if is necessary to restrict the axis position using the last collisions.
        /// </summary>
        /// <returns>True if necessary to restrict the axis position. False otherwise.</returns>
        internal bool ShouldRestrictPosition()
        {
            var isMovingOutOfCollision =
                IsCollisionOnPositiveSide() && IsMovingToNegativeSide() ||
                IsCollisionOnNegativeSide() && IsMovingToPositiveSide();
            return !isMovingOutOfCollision && IsCollision();
        }

        /// <summary>
        /// Validates all axis properties.
        /// </summary>
        internal void ValidateFields()
        {
            RaysCount = raysCount;
            if (Body != null) Offset = offset;
        }

        /// <summary>
        /// Returns half of the size
        /// </summary>
        /// <returns>A float value.</returns>
        protected abstract float GetHalfSize();

        /// <summary>
        /// Returns the difference between the last frame position and the current one
        /// </summary>
        /// <returns>A float value.</returns>
        protected abstract float GetDeltaMovement();

        /// <summary>
        /// Returns the positive axis direction
        /// </summary>
        /// <returns>A normalized direction.</returns>
        protected abstract Vector3 GetPositiveDirection();
        protected abstract (Vector3 one, Vector3 two) GetCollisionPoints();

        protected abstract float GetCollisionPointOnNegativeSide();
        protected abstract float GetCollisionPointOnPositiveSide();

        protected abstract void RaiseOnHitNegativeSide();
        protected abstract void RaiseOnHitPositiveSide();

        protected abstract void CheckMovementActions();

        protected bool IsCollisionOnNegativeSide() => isNegativeCollision;
        protected bool IsCollisionOnPositiveSide() => isPositiveCollision;

        protected bool IsMovingToNegativeSide() => Speed < 0F;
        protected bool IsMovingToPositiveSide() => Speed > 0F;

        protected bool IsGravityPositive() => Gravity > 0F;
        protected bool IsGravityNegative() => Gravity < 0F;

        protected bool IsPushingColliderOnNegativeSide() => IsMovingToNegativeSide() && IsCollisionOnNegativeSide();
        protected bool IsPushingColliderOnPositiveSide() => IsMovingToPositiveSide() && IsCollisionOnPositiveSide();

        protected void RaiseOnHitAnySide() => OnHitAnySide?.Invoke();

        private void UpdateGravity()
        {
            var updateNegativeGravity = GravityEnabled &&
                IsGravityNegative() && !IsCollisionOnNegativeSide() ||
                IsGravityPositive() && !IsCollisionOnPositiveSide();
            if (updateNegativeGravity) Speed += Gravity * Time.deltaTime;
        }

        private void RestrictMovement()
        {
            if (IsCollisionsLocked) return;

            if (IsCollisionOnNegativeSide())
            {
                CollisionPoint = GetCollisionPointOnNegativeSide();
                if (IsMovingToNegativeSide())
                {
                    StopSpeed();
                    RaiseOnHitAnySide();
                    RaiseOnHitNegativeSide();
                }
            }

            if (IsCollisionOnPositiveSide())
            {
                CollisionPoint = GetCollisionPointOnPositiveSide();
                if (IsMovingToPositiveSide())
                {
                    StopSpeed();
                    RaiseOnHitAnySide();
                    RaiseOnHitPositiveSide();
                }
            }
        }
    }
}