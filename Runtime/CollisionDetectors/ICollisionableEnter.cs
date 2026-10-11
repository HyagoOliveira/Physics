using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Interface used on objects able to have enter collisions.
    /// </summary>
    public interface ICollisionableEnter
    {
        /// <summary>
        /// Enters the collision using the given interactor.
        /// </summary>
        /// <param name="interactor">
        /// The interactor checking the collision. 
        /// It can be a Physics GameObject inside a Player or 
        /// other system checking its collision.
        /// </param>
        public void EnterCollision(Transform interactor);
    }
}