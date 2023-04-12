using System;
using UnityEngine;
using ActionCode.ColliderAdapter;

namespace ActionCode.Physics
{
    /// <summary>
    /// Controls the GameObject position and rotation through Axis Aligned Bounding Box (AABB) physics simulation.
    /// <para>
    /// It uses a <see cref="HorizontalAxis"/>, <see cref="VerticalAxis"/> and <see cref="DistalAxis"/> 
    /// axes to move and detect collisions using Raycasts.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BoxBody : MonoBehaviour
    {
        [SerializeField, Tooltip("The local Collider Adapter component used to detect 2D or 3D collisions.")]
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        private AbstractColliderAdapter collider;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        [SerializeField, Tooltip("The layer mask collisions. Only layers on this mask will be used on this axis.")]
        private LayerMask collisions;
        [SerializeField, Range(minSlopeLimit, maxSlopeLimit), Tooltip("The maximum angle limit (in degrees) of a valid slope.")]
        private float slopeLimit = 45F;

        [Header("Axes")]
        [SerializeField, Tooltip("The horizontal (left/right) axis.")]
        [ContextMenuItem("Reset", nameof(ResetHorizontal))]
        private HorizontalAxis horizontal = new HorizontalAxis();

        [SerializeField, Tooltip("The vertical (up/down) axis.")]
        [ContextMenuItem("Reset", nameof(ResetVertical))]
        private VerticalAxis vertical = new VerticalAxis();

        [SerializeField, Tooltip("The distal (forward/backward) axis.")]
        [ContextMenuItem("Reset", nameof(ResetDistal))]
        private DistalAxis distal = new DistalAxis();

        #region Events
        /// <summary>
        /// Action fired when the RigidBody stops after colliding using any side.
        /// </summary>
        public event Action OnHitAnySide;

        /// <summary>
        /// Action fired when the RigidBody starts to move in any direction.
        /// </summary>
        //public event Action OnMoving;
        #endregion // Events

        #region Properties

        #region Axes
        /// <summary>
        /// The horizontal (left/right) axis.
        /// </summary>
        public HorizontalAxis Horizontal => horizontal;

        /// <summary>
        /// The vertical (up/down) axis.
        /// </summary>
        public VerticalAxis Vertical => vertical;

        /// <summary>
        /// The distal (forward/backward) axis.
        /// </summary>
        public DistalAxis Distal => distal;
        #endregion // Axes

        /// <summary>
        /// The local Collider Adapter component used to detect 2D or 3D collisions.
        /// </summary>
        public ICollider Collider => collider;

        /// <summary>
        /// The current velocity.
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// The current position.
        /// </summary>
        public Vector3 CurrentPosition => currentPosition;

        /// <summary>
        /// The position from the last frame.
        /// </summary>
        public Vector3 LastPosition { get; private set; }

        /// <summary>
        /// The difference between <see cref="CurrentPosition"/> and <see cref="LastPosition"/>.
        /// </summary>
        public Vector3 DeltaPosition { get; private set; }

        /// <summary>
        /// Whether was grounded in the last frame.
        /// </summary>
        public bool WasGrounded { get; private set; }

        /// <summary>
        /// Whether is grounded.
        /// </summary>
        public bool IsGrounded => Vertical.IsCollisionDown();

        /// <summary>
        /// Whether is airborne.
        /// </summary>
        public bool IsAirborne => !Vertical.IsCollisionDown();

        /// <summary>
        /// The maximum angle limit (in degrees) of a valid slope.
        /// </summary>
        public float SlopeLimit
        {
            get => slopeLimit;
            set => slopeLimit = Mathf.Clamp(value, minSlopeLimit, maxSlopeLimit);
        }

        /// <summary>
        /// The layer mask collisions. Only layers on this mask will be used on this axis.
        /// </summary>
        public LayerMask Collisions
        {
            get => collisions;
            set => collisions = value;
        }
        #endregion // Properties

        #region Constants
        private const float minSlopeLimit = 0F;
        private const float maxSlopeLimit = 90F;
        #endregion //Constants

        internal Vector3 currentPosition;

        private void Reset()
        {
            collider = AbstractColliderAdapter.ResolveCollider(gameObject);

            ResetDistal();
            ResetVertical();
            ResetHorizontal();
        }

        private void Awake() => currentPosition = transform.position;
        private void FixedUpdate() => UpdatePhysics();
        private void LateUpdate() => UpdateMovingPlatformPosition();
        private void OnEnable() => AddAxesListeners();
        private void OnDisable() => RemoveAxesListeners();
        private void OnValidate() => ValidateAxes();

        public Vector3 GetSpeeds()
        {
            return new Vector3(
                Horizontal.Speed,
                Vertical.Speed,
                Distal.Speed
            );
        }

        public bool IsUsingMovingPlatform() =>
            Vertical.IsEnabledAndUsingMovingPlatform() ||
            Horizontal.IsEnabledAndUsingMovingPlatform() ||
            Distal.IsEnabledAndUsingMovingPlatform();

        /// <summary>
        /// Enables collisions in all axes.
        /// </summary>
        public void EnableCollisions()
        {
            Horizontal.EnableCollisions();
            Vertical.EnableCollisions();
            Distal.EnableCollisions();
        }

        /// <summary>
        /// Disables collisions in all axes.
        /// </summary>
        public void DisableCollisions()
        {
            Horizontal.DisableCollisions();
            Vertical.DisableCollisions();
            Distal.DisableCollisions();
        }

        private void UpdatePhysics()
        {
            WasGrounded = IsGrounded;
            LastPosition = currentPosition;
            currentPosition = transform.position;

            UpdateAxesPhysics();
            UpdateVelocity();
            UpdatePosition();

            DeltaPosition = RemoveSmallValues(currentPosition - LastPosition);
            UpdateAxesMovingEvents();
        }

        private void AddAxesListeners()
        {
            Horizontal.OnHitAnySide += InvokeOnHitAnySide;
            Vertical.OnHitAnySide += InvokeOnHitAnySide;
            Distal.OnHitAnySide += InvokeOnHitAnySide;
        }

        private void RemoveAxesListeners()
        {
            Horizontal.OnHitAnySide -= InvokeOnHitAnySide;
            Vertical.OnHitAnySide -= InvokeOnHitAnySide;
            Distal.OnHitAnySide -= InvokeOnHitAnySide;
        }

        private void ValidateAxes()
        {
            Horizontal.Validate();
            Vertical.Validate();
            Distal.Validate();
        }

        private void UpdateVelocity() => Velocity = GetSpeeds() * Time.deltaTime;

        private void UpdateAxesPhysics()
        {
            Vertical.UpdatePhysics();
            Horizontal.UpdatePhysics();
            Distal.UpdatePhysics();
        }

        private void UpdateAxesMovingEvents()
        {
            var isMovingAnySide = DeltaPosition.sqrMagnitude > 0f;
            if (!isMovingAnySide) return;

            Horizontal.InvokeMovingEvent();
            Vertical.InvokeMovingEvent();
            Distal.InvokeMovingEvent();
        }

        private void UpdatePosition()
        {
            currentPosition += Velocity;
            transform.position = currentPosition;
        }

        private void UpdateMovingPlatformPosition()
        {
            if (!IsUsingMovingPlatform()) return;

            var latePosition = transform.position;

            Vertical.UpdatePositionUsingMovingPlatform(ref latePosition);
            Horizontal.UpdatePositionUsingMovingPlatform(ref latePosition);
            Distal.UpdatePositionUsingMovingPlatform(ref latePosition);

            transform.position = latePosition;

            Collider.SyncTransforms();
        }

        private void InvokeOnHitAnySide() => OnHitAnySide?.Invoke();

        private void ResetDistal() => Distal.Reset(this);
        private void ResetVertical() => Vertical.Reset(this);
        private void ResetHorizontal() => Horizontal.Reset(this);

        private static Vector3 RemoveSmallValues(Vector3 v)
        {
            const float threshold = 0.0001F;
            var abs = new Vector3(
                Mathf.Abs(v.x),
                Mathf.Abs(v.y),
                Mathf.Abs(v.z)
            );

            if (abs.x < threshold) v.x = 0F;
            if (abs.y < threshold) v.y = 0F;
            if (abs.z < threshold) v.z = 0F;

            return v;
        }
    }
}