using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace BehaviourTrees
{
    [CustomEditor(typeof(BehaviourTree))]
    public class BehaviourTreeEditor : Editor
    {
        private BehaviourTree behaviourTree;

        private void OnEnable()
        {
            behaviourTree = (BehaviourTree)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Open Behaviour Tree Visualizer"))
            {
                BehaviourTreeVisualizerWindow.ShowWindow(behaviourTree);
            }
        }
    }

    
}


