using UnityEngine;

namespace WorldShaper.Editor
{
    public static class RectExtensions
    {
        public static Rect AddY(this Rect rect, float y)
        {
            rect.y += y;
            return rect;
        }
    }
}