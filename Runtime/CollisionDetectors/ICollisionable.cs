using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Interface used on objects able to have enter/exit collisions with an <see cref="AbstractInteractor{T}"/>.
    /// </summary>
    public interface ICollisionable
    {
        /// <summary>
        /// The local Collider.
        /// </summary>
        public Collider Collider { get; }

        /// <summary>
        /// Whether can collider.
        /// </summary>
        /// <returns></returns>
        public bool CanCollide();

        /// <summary>
        /// Enters the collision using the given interactor.
        /// </summary>
        /// <param name="interactor">
        /// The interactor checking the collision.
        /// <para>
        /// It can be a Physics GameObject inside a Player or 
        /// other system checking its collision.
        /// </para>
        /// </param>
        public void EnterCollision(Transform interactor);

        /// <summary>
        /// Exits the collision using the given interactor.
        /// </summary>
        /// <param name="interactor">
        /// <inheritdoc cref="EnterCollision(Transform)" path="/param[@name='interactor']"/>
        /// </param>
        public void ExitCollision(Transform interactor);
    }
}