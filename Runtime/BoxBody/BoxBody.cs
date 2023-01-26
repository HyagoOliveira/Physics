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

        [Header("Axes")]
        [SerializeField, Tooltip("The horizontal (left/right) axis.")]
        [ContextMenuItem("Reset", nameof(ResetHorizontal))]
        private HorizontalAxis horizontal;

        [SerializeField, Tooltip("The vertical (up/down) axis.")]
        [ContextMenuItem("Reset", nameof(ResetVertical))]
        private VerticalAxis vertical;

        /*[SerializeField, Tooltip("The distal (forward/backward) axis.")]
        [ContextMenuItem("Reset", "nameof(ResetDistal)")]
        private DistalAxis distal;*/

        #region Events
        /// <summary>
        /// Action fired when the RigidBody stops after colliding using any side.
        /// </summary>
        public event Action OnHitAnySide;

        /// <summary>
        /// Action fired when the RigidBody starts to move in any direction.
        /// </summary>
        //public event Action OnMoving;
        #endregion

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
        //public DistalAxis Distal => distal;
        #endregion

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
        #endregion

        internal Vector3 currentPosition;

        private void Reset()
        {
            collider = AbstractColliderAdapter.ResolveCollider(gameObject);
            //ResetDistal();
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
                /*Distal.Speed*/ 0F
            );
        }

        public bool IsUsingMovingPlatform() =>
            Horizontal.IsUsingMovingPlatform();
        //Vertical.IsUsingMovingPlatform();

        private void UpdatePhysics()
        {
            //WasGrounded = IsGrounded;
            LastPosition = currentPosition;
            //WasMovingAnySide = IsMovingAnySide;
            //LastDeltaPosition = DeltaPosition;
            currentPosition = transform.position;

            UpdateAxesPhysics();
            UpdateVelocity();
            UpdatePosition();
        }

        private void AddAxesListeners()
        {
            Horizontal.OnHitAnySide += InvokeOnHitAnySide;
            Vertical.OnHitAnySide += InvokeOnHitAnySide;
            //Distal.OnHitAnySide += InvokeOnHitAnySide;
        }

        private void RemoveAxesListeners()
        {
            Horizontal.OnHitAnySide -= InvokeOnHitAnySide;
            Vertical.OnHitAnySide -= InvokeOnHitAnySide;
            //Distal.OnHitAnySide -= InvokeOnHitAnySide;
        }

        private void ValidateAxes()
        {
            Horizontal.Validate();
            Vertical.Validate();
            //Distal.Validate();
        }

        private void UpdateVelocity() => Velocity = GetSpeeds() * Time.deltaTime;

        private void UpdateAxesPhysics()
        {
            Vertical.UpdatePhysics();
            Horizontal.UpdatePhysics();
            //Distal.UpdatePhysics();
        }

        private void UpdatePosition()
        {
            currentPosition += Velocity;
            transform.position = currentPosition;

            //DeltaPosition = RemoveSmallValues(currentPosition - LastPosition);
            //IsMovingAnySide = DeltaPosition.sqrMagnitude > 0f;
        }

        private void UpdateMovingPlatformPosition()
        {
            if (!IsUsingMovingPlatform()) return;

            var latePosition = transform.position;

            Horizontal.UpdateMovingPlatformPoint(ref latePosition.x);

            transform.position = latePosition;

            //TODO put this code inside AbstractColliderAdapter
            Physics2D.SyncTransforms();
            //UnityEngine.Physics.SyncTransforms();
        }

        private void InvokeOnHitAnySide() => OnHitAnySide?.Invoke();

        //private void ResetDistal() => Vertical.Reset(this);
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