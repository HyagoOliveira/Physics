using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Interface used on objects able to have exit collisions.
    /// </summary>
    public interface ICollisionableExit
    {
        /// <summary>
        /// Exits the collision using the given interactor.
        /// </summary>
        /// <param name="interactor">
        /// <inheritdoc cref="ICollisionableEnter.EnterCollision(Transform)" path="/param[@name='interactor']"/>
        /// </param>
        public void ExitCollision(Transform interactor);
    }
}