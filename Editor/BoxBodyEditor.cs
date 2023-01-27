using UnityEditor;
using UnityEngine;
using ActionCode.Shapes;

namespace ActionCode.Physics.Editor
{
    /// <summary>
    /// Custom Editor for <see cref="BoxBody"/>.
    /// <para>
    /// It draws all current axis collisions if the axis is expanded on the Inspector window.
    /// </para>
    /// </summary>
    [CustomEditor(typeof(BoxBody))]
    public sealed class BoxBodyEditor : UnityEditor.Editor
    {
        private readonly Color RECT_OUTLINE = Color.red;
        private readonly Color RECT_FACE = Color.red * 0.25F;

        private BoxBody body;
        private SerializedProperty distalAxis;
        private SerializedProperty verticalAxis;
        private SerializedProperty horizontalAxis;

        private void OnEnable()
        {
            body = (BoxBody)target;

            distalAxis = serializedObject.FindProperty("distal");
            verticalAxis = serializedObject.FindProperty("vertical");
            horizontalAxis = serializedObject.FindProperty("horizontal");
        }

        private void OnSceneGUI()
        {
            if (body.Collider == null) return;

            DrawCurrentCollisions();
            DrawRaycastCollisions();
        }

        private void DrawCurrentCollisions()
        {
            if (body.Distal.IsForwardCollision()) DrawDistalCollision(Vector3.forward);
            if (body.Distal.IsBackwardCollision()) DrawDistalCollision(Vector3.back);

            if (body.Horizontal.IsCollisionRight()) DrawHorizontalCollision(Vector3.right);
            if (body.Horizontal.IsCollisionLeft()) DrawHorizontalCollision(Vector3.left);

            if (body.Vertical.IsCollisionUp()) DrawVerticalCollision(Vector3.up);
            if (body.Vertical.IsCollisionDown()) DrawVerticalCollision(Vector3.down);
        }

        private void DrawRaycastCollisions()
        {
            body.Distal.DrawCollisions = distalAxis.isExpanded;
            body.Vertical.DrawCollisions = verticalAxis.isExpanded;
            body.Horizontal.DrawCollisions = horizontalAxis.isExpanded;

            if (!Application.isPlaying)
            {
                body.Distal.UpdateCollisions();
                body.Vertical.UpdateCollisions();
                body.Horizontal.UpdateCollisions();
            }
        }

        private void DrawDistalCollision(Vector3 direction)
        {
            var size = body.Collider.Size;
            var rotation = Quaternion.LookRotation(direction);
            var position = body.Collider.Center + direction * size.z * 0.5f;
            var verts = ShapePoints.GetQuad(position, size, rotation);
            Handles.DrawSolidRectangleWithOutline(verts, RECT_FACE, RECT_OUTLINE);
        }

        private void DrawHorizontalCollision(Vector3 direction)
        {
            var size = body.Collider.Size;
            var rotation = Quaternion.LookRotation(direction);
            var position = body.Collider.Center + direction * size.x * 0.5f;
            var sideSize = new Vector2(size.z, size.y);
            var verts = ShapePoints.GetQuad(position, sideSize, rotation);
            Handles.DrawSolidRectangleWithOutline(verts, RECT_FACE, RECT_OUTLINE);
        }

        private void DrawVerticalCollision(Vector3 direction)
        {
            var size = body.Collider.Size;
            var rotation = Quaternion.LookRotation(direction);
            var position = body.Collider.Center + direction * size.y * 0.5f;
            var sideSize = new Vector2(size.x, size.z);
            var verts = ShapePoints.GetQuad(position, sideSize, rotation);
            Handles.DrawSolidRectangleWithOutline(verts, RECT_FACE, RECT_OUTLINE);
        }
    }
}