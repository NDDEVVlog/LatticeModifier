using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourTrees
{
    public class RandomRateSelector : Selector
    {
        // List to store weighted child nodes for random selection
        private List<Node> weightedChildren = new List<Node>();

        public RandomRateSelector(string name, int priority = 0) : base(name, priority)
        {
            // Initialize weighted children based on priority
            InitializeChildren();
        }

        private void InitializeChildren()
        {
            // Clear existing weighted children
            weightedChildren.Clear();

            // Calculate weighted children based on priority
            foreach (var child in children)
            {
                child.OnPriorityChanged += OnChildPriorityChanged;

                AddWeightedChildren(child);
            }
        }

        private void AddWeightedChildren(Node child)
        {
            float totalPriority = children.Sum(c => c.priority);
            float weight = totalPriority > 0 ? child.priority / totalPriority : 0;
            int weightCount = Mathf.RoundToInt(weight * 100); // Scale to a reasonable number

            for (int i = 0; i < weightCount; i++)
            {
                weightedChildren.Add(child);
            }
        }

        // Triggered when a child's priority changes
        private void OnChildPriorityChanged(Node node)
        {
            // Instead of recalculating the entire list, update only the node's entries
            weightedChildren.RemoveAll(n => n == node); // Remove old entries
            AddWeightedChildren(node);                  // Add new weighted entries
        }

        public override void AddChild(Node child)
        {
            base.AddChild(child);
            child.OnPriorityChanged += OnChildPriorityChanged;

            // Recalculate weighted children list
            AddWeightedChildren(child);
        }

        public override void Reset()
        {
            SetWeightedChildren();
        }

        private void SetWeightedChildren()
        {
            weightedChildren.Clear();
            foreach (var child in children)
            {
                AddWeightedChildren(child);
            }
        }

        public override Status Process()
        {
            if (weightedChildren.Count == 0)
            {
                return Status.Failure;
            }

            // Select a random child node from the weighted list
            var selectedChild = weightedChildren[Random.Range(0, weightedChildren.Count)];
            //Debug.Log($"Selected child: {selectedChild.name}");

            var status = selectedChild.Process();
            //Debug.Log($"Child {selectedChild.name} returned {status}");

            switch (status)
            {
                case Status.Running:
                    //Debug.Log(selectedChild.name + " : Running");
                    return Status.Running;

                case Status.Success:
                    Reset();
                    //Debug.Log(selectedChild.name + " : Success");
                    return Status.Success;

                default:
                    Reset();
                    return Status.Failure;
            }
        }
    }
}
