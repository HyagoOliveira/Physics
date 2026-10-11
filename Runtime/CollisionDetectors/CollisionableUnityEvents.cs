using UnityEngine;
using UnityEngine.Events;

namespace ActionCode.Physics
{
    /// <summary>
    /// Simple implementation of Collisionables interfaces using UnityEvents.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CollisionableUnityEvents : MonoBehaviour, ICollisionableEnter, ICollisionableExit
    {
        public UnityEvent<Transform> OnCollisionEntered;
        public UnityEvent<Transform> OnCollisionExited;

        public void EnterCollision(Transform interactor) => OnCollisionEntered?.Invoke(interactor);
        public void ExitCollision(Transform interactor) => OnCollisionExited?.Invoke(interactor);
    }
}