using System;
using UnityEngine;
using ActionCode.ColliderAdapter;

namespace ActionCode.BoxBodies
{
    /// <summary>
    /// BoxBody used to control the GameObject position and rotation through simple physics simulation.
    /// <para>
    /// It uses a <see cref="HorizontalAxis"/>, <see cref="VerticalAxis"/> and <see cref="DistalAxis"/> 
    /// axes to move and detect collisions.
    /// </para>
    /// </summary>
    public sealed class BoxBody : MonoBehaviour
    {
        [SerializeField, Tooltip("The local Collider Adapter component used to detect 2D or 3D collisions.")]
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        private AbstractColliderAdapter collider;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        [Header("Axes")]
        [SerializeField, Tooltip("The horizontal (left/right) axis.")]
        [ContextMenuItem("Reset", "ResetHorizontalFields")]
        private HorizontalAxis horizontal;

        [SerializeField, Tooltip("The vertical (up/down) axis.")]
        [ContextMenuItem("Reset", "ResetVerticalFields")]
        private VerticalAxis vertical;

        [SerializeField, Tooltip("The distal (forward/backward) axis.")]
        [ContextMenuItem("Reset", "ResetDistalFields")]
        private DistalAxis distal;

        #region Events
        /// <summary>
        /// Action fired when the Box stops after colliding using any side.
        /// </summary>
        public event Action OnHitAnySide;

        /// <summary>
        /// Action fired when the Box starts to move in any direction.
        /// </summary>
        public event Action OnMoving;
        #endregion

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
        #endregion

        /// <summary>
        /// The local Collider Adapter component used to detect 2D or 3D collisions.
        /// </summary>
        public AbstractColliderAdapter Collider => collider;

        /// <summary>
        /// The current velocity.
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// The difference between the last frame position and the current one.
        /// </summary>
        public Vector3 DeltaPosition { get; private set; }

        /// <summary>
        /// The <see cref="DeltaPosition"/> from the last frame.
        /// </summary>
        public Vector3 LastDeltaPosition { get; private set; }

        /// <summary>
        /// The position from the last frame.
        /// </summary>
        public Vector3 LastPosition { get; private set; }

        /// <summary>
        /// The current position.
        /// </summary>
        public Vector3 CurrentPosition => currentPosition;

        /// <summary>
        /// Whether is moving in any side.
        /// </summary>
        public bool IsMovingAnySide { get; private set; }

        /// <summary>
        /// Whether was moving in any side in the last frame.
        /// </summary>
        public bool WasMovingAnySide { get; private set; }

        private Vector3 currentPosition;
        private bool areAxesInitialized;

        private void Reset() => FindCollider();
        private void Awake()
        {
            InitializeAxes();
            currentPosition = transform.position;
        }
        private void FixedUpdate() => UpdatePhysics();
        private void OnEnable() => AddAxesListeners();
        private void OnDisable() => RemoveAxesListeners();
        private void OnValidate() => ValidateAxes();

        /// <summary>
        /// Initializes all the axes.
        /// </summary>
        public void InitializeAxes()
        {
            if (areAxesInitialized) return;
            areAxesInitialized = true;

            Horizontal.Initialize(this);
            Vertical.Initialize(this);
            Distal.Initialize(this);
        }

        /// <summary>
        /// Gets the current axes speeds.
        /// <para>This vector is not multiplied by DeltaTime.</para>
        /// </summary>
        /// <returns>A Vector3 representing speeds.</returns>
        public Vector3 GetSpeeds() => new Vector3(Horizontal.Speed, Vertical.Speed, Distal.Speed);

        private void FindCollider() => collider = AbstractColliderAdapter.ResolveCollider(gameObject);

        private void AddAxesListeners()
        {
            Horizontal.OnHitAnySide += RaiseOnHitAnySide;
            Vertical.OnHitAnySide += RaiseOnHitAnySide;
            Distal.OnHitAnySide += RaiseOnHitAnySide;
        }

        private void RemoveAxesListeners()
        {
            Horizontal.OnHitAnySide -= RaiseOnHitAnySide;
            Vertical.OnHitAnySide -= RaiseOnHitAnySide;
            Distal.OnHitAnySide -= RaiseOnHitAnySide;
        }

        private void UpdatePhysics()
        {
            LastPosition = currentPosition;
            WasMovingAnySide = IsMovingAnySide;
            LastDeltaPosition = DeltaPosition;
            currentPosition = transform.position;

            Horizontal.Update();
            Vertical.Update();
            Distal.Update();

            UpdateCollisions();
            UpdatePosition();
        }

        public void UpdateCollisions()
        {
            if (Horizontal.ShouldRestrictPosition()) currentPosition.x = Horizontal.CollisionPoint;
            if (Vertical.ShouldRestrictPosition()) currentPosition.y = Vertical.CollisionPoint;
            if (Distal.ShouldRestrictPosition()) currentPosition.z = Distal.CollisionPoint;
        }

        private void UpdatePosition()
        {
            Velocity = new Vector3(Horizontal.Speed, Vertical.Speed, Distal.Speed) * Time.deltaTime;
            currentPosition += Velocity;
            transform.position = currentPosition;
            DeltaPosition = RemoveSmallValues(currentPosition - LastPosition);

            IsMovingAnySide = DeltaPosition.sqrMagnitude > 0f;
            if (IsMovingAnySide)
            {
                OnMoving?.Invoke();

                Horizontal.UpdateMovement();
                Vertical.UpdateMovement();
                Distal.UpdateMovement();
            }
        }

        private void ValidateAxes()
        {
            distal?.ValidateFields();
            vertical?.ValidateFields();
            horizontal?.ValidateFields();
        }

        private void RaiseOnHitAnySide() => OnHitAnySide?.Invoke();

        private void ResetDistalFields() => distal = new DistalAxis();
        private void ResetVerticalFields() => vertical = new VerticalAxis();
        private void ResetHorizontalFields() => horizontal = new HorizontalAxis();

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