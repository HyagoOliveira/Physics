using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Interface used on objects able to have enter/exit collisions.
    /// </summary>
    public interface ICollisionable
    {
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