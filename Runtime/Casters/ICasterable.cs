using System;
using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Interface used on objects able to cast other objects on the Scene.
    /// </summary>
    public interface ICasterable
    {
        /// <summary>
        /// Whether has a current hit.
        /// </summary>
        bool HasHit { get; }

        /// <summary>
        /// The maximum distance used on the cast.
        /// </summary>
        float Distance { get; set; }

        /// <summary>
        /// The layers used on the cast collision.
        /// </summary>
        LayerMask Collisions { get; set; }

        /// <summary>
        /// Event fired every time a hit changes.
        /// <para>Use <see cref="HasHit"/> to check whether it was a valid hit.</para>
        /// </summary>
        event Action<RaycastHit> OnHitChanged;

        /// <summary>
        /// Tries to get a hitting component implementing <see cref="IEnable"/> interface.
        /// </summary>
        /// <typeparam name="T">The generic component type.</typeparam>
        /// <param name="component">
        /// The component instance implementing <see cref="IEnable"/> interface.
        /// </param>
        /// <returns>Whether a hitting component was found.</returns>
        bool TryGetEnabledComponent<T>(out T component) where T : IEnable;

        /// <summary>
        /// Tries to get the hitting component.
        /// </summary>
        /// <typeparam name="T">The generic component type.</typeparam>
        /// <param name="component">The component instance.</param>
        /// <returns>Whether a hitting component was found.</returns>
        bool TryGetHittingComponent<T>(out T component);
    }
}