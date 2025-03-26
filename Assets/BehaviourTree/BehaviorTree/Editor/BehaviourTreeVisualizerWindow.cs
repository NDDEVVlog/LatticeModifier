using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BehaviourTrees
{
    public class BehaviourTreeVisualizerWindow : EditorWindow
    {
        private BehaviourTree behaviourTree;
        private Vector2 offset;
        private Vector2 drag;
        private Node selectedNode = null;
        private Vector2 dragOffset;
        private Vector2 rightClickPosition;

        private Dictionary<Node, NodeDrawer> nodeDrawers = new Dictionary<Node, NodeDrawer>();

        public static void ShowWindow(BehaviourTree tree)
        {
            var window = GetWindow<BehaviourTreeVisualizerWindow>("Behaviour Tree Visualizer");
            window.behaviourTree = tree;
            window.InitializeNodeDrawers();
        }

        private void InitializeNodeDrawers()
        {
            nodeDrawers.Clear();
            if (behaviourTree != null && behaviourTree.rootNode != null)
            {
                Vector2 startPosition = new Vector2(100, 100);
                CreateNodeDrawersRecursively(behaviourTree.rootNode, startPosition);
            }
        }

        private void CreateNodeDrawersRecursively(Node node, Vector2 position)
        {
            if (node == null) return;

            NodeDrawer nodeDrawer = new NodeDrawer(node, position);
            nodeDrawers[node] = nodeDrawer;

            Vector2 childPosition = position + new Vector2(0, 75);
            foreach (Node child in node.children)
            {
                CreateNodeDrawersRecursively(child, childPosition);
                childPosition.x += 200;
            }
        }

        private void OnGUI()
        {
            if (behaviourTree == null) return;

            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.red);

            DrawNodes();
            DrawConnections();

            ProcessEvents(Event.current);

            if (GUI.changed) Repaint();
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, 0, 0), new Vector3(gridSpacing * i, position.height, 0f));
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(0, gridSpacing * j, 0), new Vector3(position.width, gridSpacing * j, 0f));
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawNodes()
        {
            foreach (var nodeDrawer in nodeDrawers.Values)
            {
                nodeDrawer.Draw();
            }
        }

        private void DrawConnections()
        {
            foreach (var kvp in nodeDrawers)
            {
                Node node = kvp.Key;
                NodeDrawer drawer = kvp.Value;

                Vector2 startPos = drawer.GetCenter();
                Vector2 childPosition = drawer.GetRect().position + new Vector2(0, 75);
                
                foreach (Node child in node.children)
                {
                    if (child != null && nodeDrawers.TryGetValue(child, out NodeDrawer childDrawer))
                    {
                        Vector2 endPos = childDrawer.GetCenter();
                        DrawConnectionLine(startPos, endPos);
                    }
                }
            }
        }

        private void DrawConnectionLine(Vector2 start, Vector2 end)
        {
            Handles.DrawLine(start, end);
        }

        private void ProcessEvents(Event e)
        {
            drag = Vector2.zero;

            switch (e.type)
            {
                case EventType.MouseDrag:
                    if (e.button == 0 && selectedNode != null)
                    {
                        OnDrag(e.delta);
                        Vector2 newPosition = e.mousePosition - dragOffset;
                        nodeDrawers[selectedNode].SetPosition(newPosition);
                    }
                    break;

                case EventType.MouseUp:
                    selectedNode = null;
                    break;

                case EventType.ContextClick:
                    rightClickPosition = e.mousePosition;
                    ShowContextMenu();
                    break;
            }
        }

        private void OnDrag(Vector2 delta)
        {
            foreach (var nodeDrawer in nodeDrawers.Values)
            {
                Vector2 newPosition = nodeDrawer.GetRect().position + delta;
                nodeDrawer.SetPosition(newPosition);
            }

            GUI.changed = true;
        }

        private void ShowContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create Node"), false, CreateNode);
            menu.ShowAsContext();
        }

        private void CreateNode()
        {
            if (behaviourTree == null) return;

            Node newNode = ScriptableObject.CreateInstance<Selector>();
            newNode.name = "New Node";
            newNode.SetPriority(0);
            behaviourTree.nodes.Add(newNode);

            if (behaviourTree.rootNode == null)
            {
                behaviourTree.rootNode = newNode;
            }

            newNode.Initialize(behaviourTree, -1);
            AssetDatabase.AddObjectToAsset(newNode, behaviourTree);
            AssetDatabase.SaveAssets();

            InitializeNodeDrawers();
        }
    }
}
