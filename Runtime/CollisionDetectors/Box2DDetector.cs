using ActionCode.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace ActionCode.Physics
{
    /// <summary>
    /// Component used to detect collisions with <see cref="ICollisionable"/> objects.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class Box2DDetector : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D boxCollider;
        [SerializeField] private ContactFilter2D collisionFilter;

        [Header("RUNTIME")]
        [SerializeField, Readonly]
        private List<Collider2D> collidingInstances = new();

        private readonly Collider2D[] buffer = new Collider2D[32];

        private void Reset()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
        }

        private void Update() => TryUpdateCollisions();

        public static bool IsGameRunning() => Time.timeScale > 0f;

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

        private int GetHits()
        {
            var bounds = boxCollider.bounds;
            return Physics2D.OverlapBox(
                bounds.center,
                bounds.size,
                angle: 0F,
                collisionFilter,
                buffer
            );
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

        private void EnterCollision(Collider2D collider)
        {
            var instance = collider.gameObject;
            var hasCollisionable = instance.TryGetComponent(out ICollisionable collisionable);
            if (!hasCollisionable) return;

            collidingInstances.Add(collider);
            collisionable.EnterCollision(transform);
        }

        private void ExitCollision(int index)
        {
            var collider = collidingInstances[index];
            collidingInstances.RemoveAt(index);

            // Collider may be deleted by Gameplay
            if (collider == null) return;

            var hasCollisionable = collider.gameObject.TryGetComponent(out ICollisionable collisionable);
            if (hasCollisionable) collisionable.ExitCollision(transform);
        }

        private bool IsColliding(Collider2D collider, int hits) => Contains(buffer, collider, hits);
        private static bool Contains(List<Collider2D> colliders, Collider2D collider) => Contains(colliders, collider, colliders.Count);

        private static bool Contains(List<Collider2D> colliders, Collider2D collider, int size)
        {
            for (var i = 0; i < size; i++)
            {
                if (colliders[i] == collider) return true;
            }
            return false;
        }

        private static bool Contains(Collider2D[] array, Collider2D collider, int size)
        {
            for (var i = 0; i < size; i++)
            {
                if (array[i] == collider) return true;
            }
            return false;
        }
    }
}