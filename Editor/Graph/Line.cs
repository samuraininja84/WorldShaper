using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;


namespace WorldShaper
{
    [System.Serializable]
    public class Line : IMGUIContainer
    {
        public Rect start;
        public Rect end;
        public Vector2 startPos;
        public Vector2 endPos;
        public Vector2 startTan;
        public Vector2 endTan;
        public Color color;

        public Line()
        {
            startPos = Vector3.zero;
            endPos = Vector3.zero;
            startTan = Vector3.zero;
            endTan = Vector3.zero;
            color = Color.white;
        }

        public Line(Rect start, Rect end, Color color)
        {
            // Set the start and end positions of the line
            this.start = start;
            this.end = end;

            // Add a new connection to the area handle
            Vector2 startPos = new Vector2(start.x + start.width, start.y + start.height / 2);
            Vector2 endPos = new Vector2(end.x, end.y + end.height / 2);
            Vector2 startTan = startPos + Vector2.right * 50;
            Vector2 endTan = endPos + Vector2.left * 50;

            this.startPos = startPos;
            this.endPos = endPos;
            this.startTan = startTan;
            this.endTan = endTan;
            this.color = color;

            Add(new IMGUIContainer(() =>
            {
                Handles.BeginGUI();
                Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 5);
                Handles.EndGUI();
            }));
        }

        public Line(Vector2 startPos, Vector2 endPos, Vector2 startTan, Vector2 endTan, Color color)
        {
            this.startPos = startPos;
            this.endPos = endPos;
            this.startTan = startTan;
            this.endTan = endTan;
            this.color = color;

            Add(new IMGUIContainer(() =>
            {
                Handles.BeginGUI();
                Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 5);
                Handles.EndGUI();
            }));
        }
    }
}
