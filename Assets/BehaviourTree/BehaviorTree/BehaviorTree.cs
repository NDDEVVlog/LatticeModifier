using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BehaviourTrees
{
    [CreateAssetMenu]
    public class BehaviourTree : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public Node rootNode;

        [SerializeReference, HideInInspector]
        public List<Node> nodes = new List<Node>();

        internal bool running = false;

        protected virtual void OnEnable()
        {
            #if UNITY_EDITOR
            InitializeNodes();
            #endif  

            if (rootNode == null)
            {
                CreateRootNode();
            }
        }

        private void CreateRootNode()
        {
            rootNode = ScriptableObject.CreateInstance<Selector>();
            rootNode.name = "Root Node";
            nodes.Add(rootNode);

            #if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(rootNode, this);
            AssetDatabase.SaveAssets();
            #endif
        }

        protected virtual void Reset()
        {
            #if UNITY_EDITOR
            InitializeNodes();
            #endif
        }

        internal void InitializeNodes()
        {
            if (rootNode == null) return;

            // Initialize each node in the nodes list
            foreach (Node node in nodes)
            {
                node?.Initialize(this, -1);
            }

            int order = -1;
            rootNode.Traverse(n => n.Initialize(this, order++));
        }
    }
}
