using System;
using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Abstract caster component implementing <see cref="ICasterable"/> interface.
    /// <para>
    /// The cast process is done using <b>Update</b> function. Disable this component to turn it off.
    /// </para>
    /// </summary>
    public abstract class AbstractCaster : MonoBehaviour, ICasterable
    {
        [SerializeField, Tooltip("The layers used on the cast.")]
        private LayerMask layers;
        [SerializeField, Min(0f), Tooltip("The maximum distance used on the cast.")]
        private float distance = 10F;

        public event Action<RaycastHit> OnHitChanged;

        public bool HasHit { get; private set; }

        public LayerMask Layers
        {
            get => layers;
            set => layers = value;
        }

        public float Distance
        {
            get => distance;
            set => distance = Mathf.Min(0F, value);
        }

        private RaycastHit lastHit;
        private RaycastHit currentHit;

        private void Update() => UpdateCast();
        private void OnDrawGizmosSelected() => DrawCast();

        public bool TryGetEnabledComponent<T>(out T component) where T : IEnable
        {
            if (HasHit && currentHit.transform.TryGetComponent(out component))
                return component.IsEnabled;

            component = default;
            return false;
        }

        public bool TryGetHittingComponent<T>(out T component)
        {
            if (HasHit) return currentHit.transform.TryGetComponent(out component);

            component = default;
            return false;
        }

        protected abstract void DrawCast();
        protected abstract bool Cast(out RaycastHit hit);

        private void UpdateCast()
        {
            lastHit = currentHit;

            HasHit = Cast(out currentHit);

            var hasHitChanged = currentHit.transform != lastHit.transform;
            if (hasHitChanged) OnHitChanged?.Invoke(currentHit);
        }
    }
}