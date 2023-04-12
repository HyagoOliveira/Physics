using UnityEngine;

namespace ActionCode.Physics
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxBody))]
    public class BoxBodyGUI : MonoBehaviour
    {
        [SerializeField] protected BoxBody body;
        [SerializeField] protected string title = "BoxBody";
        [SerializeField] protected Rect area = new Rect(60f, 30f, 220f, 120f);

        private int lines;
        private GUIStyle style;

        protected virtual void Reset()
        {
            title = gameObject.name;
            body = GetComponent<BoxBody>();
        }

        private void OnGUI()
        {
            if (!HasRequiredComponents()) return;

            if (style == null) SetupStyle();

            lines = 1;

            GUI.BeginGroup(area, title, new GUIStyle("Box"));
            DrawValuesGroup();
            GUI.EndGroup();
        }

        protected virtual bool HasRequiredComponents() => body != null;

        protected virtual void DrawValuesGroup()
        {
            DrawValue("Gravity", body.Vertical.Gravity);
            DrawValue("IsGrounded", body.IsGrounded);
            DrawValue("Speed", body.GetSpeeds());
            DrawValue("Velocity", body.Velocity);
            DrawValue("Position", body.CurrentPosition);
            DrawValue("Last Position", body.LastPosition);
        }

        protected void DrawValue(string label, object value)
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