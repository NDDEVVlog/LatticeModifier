using UnityEngine;

namespace BehaviourTrees
{
    public class NodeDrawer
    {
        private const float NodeWidth = 150f;
        private const float NodeHeight = 50f;
        private Node node;
        private Rect rect;
        private Vector2 position;

        public NodeDrawer(Node node, Vector2 position)
        {
            this.node = node;
            this.position = position;
            rect = new Rect(position.x, position.y, NodeWidth, NodeHeight);
        }

        public Rect GetRect() => rect;

        public void Draw()
        {
            GUI.Box(rect, node.name);

            // Additional node styles or content (like icons or labels) can be added here
        }

        public bool Contains(Vector2 point) => rect.Contains(point);

        public void SetPosition(Vector2 newPosition)
        {
            position = newPosition;
            rect.position = newPosition;
        }

        public Vector2 GetCenter() => rect.center;
    }
}
