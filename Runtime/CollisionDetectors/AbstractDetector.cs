using ActionCode.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Abstract class used to detect collisions with <see cref="ICollisionable"/> objects.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class AbstractDetector : MonoBehaviour
    {
        [SerializeField, Tooltip("The maximum collisions allowed."), Min(1), DisableInPlayMode]
        private uint maxCollisions = 1;
        [Tooltip("Specifies whether should hit Triggers.")]
        public QueryTriggerInteraction TriggerInteraction = QueryTriggerInteraction.Collide;
        [Tooltip("The layers used to cast collisions.")]
        public LayerMask Collisions;

        [Header("RUNTIME")]
        [SerializeField, Readonly]
        private List<Collider> collidingInstances = new();

        protected Collider[] buffer = new Collider[0];

        private void Reset()
        {
            FindCollider();
            SetupCollider();
        }

        private void Awake() => SetMaxCollisions(maxCollisions);
        private void Update() => TryUpdateCollisions();

        public static bool IsGameRunning() => Time.timeScale > 0f;

        public static bool TryGetCollidingComponent<T>(GameObject instance, out T component)
        {
            component = GetCollidingComponent<T>(instance);
            return component != null;
        }

        public static T GetCollidingComponent<T>(Collider collider) =>
            collider ? GetCollidingComponent<T>(collider.gameObject) : default;

        public static T GetCollidingComponent<T>(GameObject instance)
        {
            if (instance == null) return default;

            var component = instance.GetComponentInChildren<T>();
            if (component != null) return component;
            return instance.GetComponentInParent<T>();
        }

        public bool TryGetCollidingComponent<T>(out T component)
        {
            component = GetCollidingComponent<T>();
            return component != null;
        }

        public T GetCollidingComponent<T>()
        {
            foreach (var colision in collidingInstances)
            {
                var component = GetCollidingComponent<T>(colision);
                if (component != null) return component;
            }
            return default;
        }

        public void SetMaxCollisions(uint maxCollisions)
        {
            this.maxCollisions = maxCollisions;
            buffer = new Collider[maxCollisions];
        }

        protected abstract int GetHits();
        protected abstract void FindCollider();
        protected abstract Collider GetCollider();

        private void SetupCollider() => GetCollider().isTrigger = true;

        private void TryUpdateCollisions()
        {
            // Don't check collisions if game is paused
            if (IsGameRunning()) UpdateCollisions();
        }

        private void UpdateCollisions()
        {
            var hits = GetHits();
            ExitCollisionsOutsideArea(hits);
            EnterNewCollisions(hits);
        }

        private void ExitCollisionsOutsideArea(int hits)
        {
            for (var i = collidingInstances.Count - 1; i >= 0; i--)
            {
                var collider = collidingInstances[i];
                var isOutside = collider == null || !IsColliding(collider, hits);
                if (isOutside) ExitCollision(i);
            }
        }

        private void EnterNewCollisions(int hits)
        {
            for (var i = 0; i < hits; i++)
            {
                var collision = buffer[i];
                var isNew = !Contains(collidingInstances, collision);
                if (isNew) EnterCollision(collision);
            }
        }

        private void EnterCollision(Collider collider)
        {
            var instance = collider.gameObject;
            var collisionable = instance.GetComponent<ICollisionable>();

            collidingInstances.Add(collider);
            collisionable?.EnterCollision(transform);
        }

        private void ExitCollision(int index)
        {
            collidingInstances.RemoveAt(index);

            // Collider may be deleted by Gameplay
            if (collidingInstances[index] == null) return;

            var instance = collidingInstances[index].gameObject;
            var collisionable = instance.GetComponent<ICollisionable>();
            collisionable?.ExitCollision(transform);
        }

        private bool IsColliding(Collider collider, int hits) => Contains(collidingInstances, collider, hits);

        private static bool Contains(List<Collider> colliders, Collider collider) => Contains(colliders, collider, colliders.Count);

        private static bool Contains(List<Collider> colliders, Collider collider, int size)
        {
            for (var i = 0; i < size; i++)
            {
                if (colliders[i] == collider) return true;
            }
            return false;
        }
    }
}