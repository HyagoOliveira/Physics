using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Casts a Ray from the local Camera central position, filtering by the Collisions layers.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class ScreenCenterCaster : AbstractCaster
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        [SerializeField] private Camera camera;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        private static readonly Vector3 halfScreen = new(0.5F, 0.5F, 0F);

        private void Reset() => camera = GetComponent<Camera>();

        protected override bool Cast(out RaycastHit hit) =>
            UnityEngine.Physics.Raycast(GetCameraRay(), out hit, Distance, Collisions);

        protected override void DrawCast()
        {
            var ray = GetCameraRay();
            var end = ray.GetPoint(Distance);
            Debug.DrawLine(ray.origin, end, GetDrawColor());
        }

        private Ray GetCameraRay() => camera.ViewportPointToRay(halfScreen);
    }
}