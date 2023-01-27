using UnityEngine;

namespace ActionCode.Physics
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxBody))]
    public sealed class BoxBodyGUI : MonoBehaviour
    {
        public BoxBody body;
        public string title = "BoxBody";
        public Rect area = new Rect(60f, 30f, 220f, 120f);

        private int lines;
        private GUIStyle style;

        private void Reset() => body = GetComponent<BoxBody>();

        private void OnGUI()
        {
            if (body == null) return;
            if (style == null) SetupStyle();

            GUI.BeginGroup(area, title, new GUIStyle("Box"));
            lines = 1;

            DrawValue("Facing", body.Horizontal.Facing);
            DrawValue("Gravity", body.Vertical.Gravity);
            DrawValue("Speed", body.GetSpeeds());
            DrawValue("Velocity", body.Velocity);
            DrawValue("Position", body.CurrentPosition);
            DrawValue("Last Position", body.LastPosition);

            GUI.EndGroup();
        }

        private void DrawValue(string label, object value)
        {
            var position = new Vector2(8F, 8F + lines * GetLineHeight());
            var size = new Vector2(area.width, GetLineHeight());

            GUI.Label(new Rect(position, size), $"{label}: {value}", style);
            lines++;
        }

        private float GetLineHeight() => style.lineHeight;

        private void SetupStyle()
        {
            style = GUIStyle.none;
            style.normal.textColor = Color.white;
        }
    }
}