using UnityEngine;

namespace WorldShaper
{
	public class ProgressBarAttribute : PropertyAttribute
	{
		public string Name = "Percentage";
        public string EmptyColor = "red";
        public string FullColor = "green";
        public string BackgroundColor = "white";

        public Color Empty => NameToColor(EmptyColor);
        public Color Full => NameToColor(FullColor);
        public Color Background => NameToColor(BackgroundColor);

        public float min => 0f;
        public float max => 1f;

        public ProgressBarAttribute(string name)
		{
			this.Name = name;
		}

        public ProgressBarAttribute(string name, string emptyColor, string fullColor)
        {
            this.Name = name;
            this.EmptyColor = emptyColor;
            this.FullColor = fullColor;
        }

        public ProgressBarAttribute(string name, string emptyColor, string fullColor, string backgroundColor)
        {
            this.Name = name;
            this.EmptyColor = emptyColor;
            this.FullColor = fullColor;
            this.BackgroundColor = backgroundColor;
        }

        public Color GetProgressColor(float value, float maxValue) => Color.Lerp(Empty, Full, Mathf.Pow(value / (maxValue / 2), 2));

        private Color NameToColor(string colorName)
        {
            switch (colorName.ToLower())
            {
                case "red":
                    return Color.red;
                case "green":
                    return Color.green;
                case "blue":
                    return Color.blue;
                case "yellow":
                    return Color.yellow;
                case "cyan":
                    return Color.cyan;
                case "magenta":
                    return Color.magenta;
                case "white":
                    return Color.white;
                case "black":
                    return Color.black;
                default:
                    Debug.LogWarning($"Unknown color name: {colorName}. Defaulting to white.");
                    return Color.white;
            }
        }

        private Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }
            else
            {
                Debug.LogWarning($"Invalid hex color string: {hex}. Defaulting to white.");
                return Color.white;
            }
        }

        private Color StringToColor(string color)
        {
            ColorUtility.TryParseHtmlString(color, out Color result);
            return result;
        }
    }
}