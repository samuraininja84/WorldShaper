using UnityEngine;

namespace WorldShaper
{
    public static class DrawArrow
    {
        public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float arrowPosition = 1f)
        {
            ForGizmo(pos, direction, Gizmos.color, arrowHeadLength, arrowHeadAngle, arrowPosition);
        }

        public static void ForGizmoTwoPoints(Vector3 from, Vector3 to, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float arrowPosition = 1f)
        {
            ForGizmoTwoPoints(from, to, Gizmos.color, arrowHeadLength, arrowHeadAngle, arrowPosition);
        }

        public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float arrowPosition = 1f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);
            DrawArrowEnd(true, pos, direction, color, arrowHeadLength, arrowHeadAngle, arrowPosition);
        }

        public static void ForGizmoTwoPoints(Vector3 from, Vector3 to, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float arrowPosition = 1f)
        {
            Gizmos.DrawLine(from, to);
            Vector3 direction = to - from;
            DrawArrowEnd(true, from, direction, color, arrowHeadLength, arrowHeadAngle, arrowPosition);
        }

        public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float arrowPosition = 1f)
        {
            ForDebug(pos, direction, Color.white, arrowHeadLength, arrowHeadAngle, arrowPosition);
        }

        public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float arrowPosition = 1f)
        {
            Debug.DrawRay(pos, direction, color);
            DrawArrowEnd(false, pos, direction, color, arrowHeadLength, arrowHeadAngle, arrowPosition);
        }

        private static void DrawArrowEnd(bool gizmos, Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float arrowPosition = 1f)
        {
            Vector3 right = (Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back) * arrowHeadLength;
            Vector3 left = (Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back) * arrowHeadLength;
            Vector3 up = (Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back) * arrowHeadLength;
            Vector3 down = (Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back) * arrowHeadLength;

            Vector3 arrowTip = pos + (direction * arrowPosition);

            if (gizmos)
            {
                Gizmos.color = color;
                Gizmos.DrawRay(arrowTip, right);
                Gizmos.DrawRay(arrowTip, left);
                Gizmos.DrawRay(arrowTip, up);
                Gizmos.DrawRay(arrowTip, down);
            }
            else
            {
                Debug.DrawRay(arrowTip, right, color);
                Debug.DrawRay(arrowTip, left, color);
                Debug.DrawRay(arrowTip, up, color);
                Debug.DrawRay(arrowTip, down, color);
            }
        }
    }
}
