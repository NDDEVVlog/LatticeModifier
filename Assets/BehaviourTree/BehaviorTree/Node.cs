using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace BehaviourTrees
{
    // UntilSuccess
    // Repeat
    public class UntilFail : Node
    {
        public UntilFail(string name) : base(name) { }

        public override Status Process()
        {
            if (children[0].Process() == Status.Failure)
            {
                Reset();
                return Status.Failure;
            }

            return Status.Running;
        }
    }

    public class Inverter : Node
    {
        public Inverter(string name) : base(name) { }

        public override Status Process()
        {
            switch (children[0].Process())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    return Status.Success;
                default:
                    return Status.Failure;
            }
        }
    }

    public class RandomSelector : PrioritySelector
    {
        bool shuffled = false;

        public RandomSelector(string name, int priority = 0) : base(name, priority) { }

        protected override List<Node> SortChildren() => children.Shuffle().ToList();

        public override Status Process()
        {
            // Shuffle children only once at the beginning
            if (!shuffled)
            {
                sortedChildren = SortChildren();
                shuffled = true;
                currentChild = 0; // Start with the first child after shuffle
            }

            if (currentChild < sortedChildren.Count)
            {
                var child = sortedChildren[currentChild];

                // Process the current child
                switch (child.Process())
                {
                    case Status.Running:
                        // Continue processing the current child until it finishes
                        return Status.Running;

                    case Status.Success:
                        // If the child succeeds, reset and return success
                        Reset();
                        shuffled = false;
                        return Status.Success;

                    case Status.Failure:
                        // Move to the next child if this one fails
                        Debug.Log(" Name :" + name + " return Fail");
                        Reset();
                        return Status.Failure;
                }
            }

            // If all children fail, reset and return failure
            Reset();
            shuffled = false;
            Debug.Log(" Name :" + name + " return Fail");
            return Status.Failure;
        }

        public override void Reset()
        {
            base.Reset();
            shuffled = false;
            currentChild = 0; // Reset the index when resetting
        }
    }





    public class PrioritySelector : Selector
    {
        public List<Node> sortedChildren;
        List<Node> SortedChildren => sortedChildren ??= SortChildren();

        protected virtual List<Node> SortChildren() => children.OrderByDescending(child => child.priority).ToList();

        public PrioritySelector(string name, int priority = 0) : base(name, priority) { }

        public override void Reset()
        {
            base.Reset();
            sortedChildren = null;
        }

        public override Status Process()
        {
            foreach (var child in SortedChildren)
            {
                switch (child.Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        continue;
                }
            }

            Reset();
            return Status.Failure;
        }
    }

    public class Selector : Node
    {
        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if (currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        currentChild++;
                        return Status.Running;
                }
            }

            Reset();
            return Status.Failure;
        }
    }

    public class Sequence : Node
    {
        public Sequence(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if (currentChild < children.Count)
            {
                //Debug.LogWarning(children[currentChild].name + " return : " + children[currentChild].Process());
                switch (children[currentChild].Process())
                {
                    case Status.Running:
                        //Debug.LogWarning(children[currentChild].name + " return : " + children[currentChild].Process());
                        return Status.Running;
                    case Status.Failure:
                        //Debug.LogWarning(" Name :" + name + " return Fail");
                        currentChild = 0;
                        return Status.Failure;
                    default:
                        currentChild++;

                        return currentChild == children.Count ? Status.Success : Status.Running;
                }
            }

            Reset();
            return Status.Success;
        }
    }




    public class SequenceDependLeaf : Sequence
    {
        protected Node DependLeaf;
        public SequenceDependLeaf(string name, Node DependLeaf, int priority = 0) : base(name, priority)
        {
            this.DependLeaf = DependLeaf;
        }

        public override Status Process()
        {
            if (DependLeaf.Process() == Status.Failure)
            {
                // Debug.Log(name + " Reset");
                Reset();
                return Status.Failure;
            }
            return base.Process();
        }
    }


    public class Leaf : Node
    {
        readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
        {
            // Preconditions.CheckNotNull(strategy);
            this.strategy = strategy;
        }

        public override Status Process() => strategy.Process();

        public override void Reset() => strategy.Reset();
    }

    public abstract class Node : ScriptableObject
    {
        public enum Status { Success, Failure, Running }

        public string name = string.Empty;
        private int _priority;
        public int priority => _priority; // Expose priority but keep setter private

        private int order = -1;
        private BehaviourTree tree;

#if UNITY_EDITOR
        [SerializeField]
        [HideInInspector]
        private Vector2 nodePosition = Vector2.zero;

        [SerializeField]
        [HideInInspector]
        private bool breakpoint = false;

        [SerializeField]
        [HideInInspector]
        private bool arrangeable = true;

#endif
#if UNITY_EDITOR
        /// <summary>
        /// Editor only method to initialize node in behaviour tree.
        /// </summary>
        /// <param name="tree">Behaviour tree reference. (Owner of this node.)</param>
        /// <param name="order">Node order in behaviour tree.</param>
        public  void Initialize(BehaviourTree tree, int order)
        {
            this.tree = tree;
            this.order = order;

            SetupParentReference();
            OnInspectorChanged();
        }
#endif


        public virtual void Traverse(Action<Node> visiter)
        {
            visiter(this);
        }

        // Make sure this is not readonly if you need to reassign it
        public List<Node> children = new List<Node>();
        private Node parent;
        protected int currentChild;

        /// <summary>
        /// Sets a reference to the parent node.
        /// </summary>
        internal virtual void SetupParentReference() { }

        protected internal virtual void SetParent(Node parent)
        {
            this.parent = parent;
        }

        public Node(string name = "Node", int priority = 0)
        {
            this.name = name;
            _priority = priority;
        }


        public virtual void AddChild(Node child) => children.Add(child);

        public virtual Status Process() => children[currentChild].Process();

        public virtual void Reset()
        {
            currentChild = 0;
            foreach (var child in children)
            {
                child.Reset();
            }
        }

        // Update priority and trigger recalculation in RandomRateSelector
        public virtual void SetPriority(int priority)
        {
            _priority = priority;
            // Notify any listeners that the priority has changed (e.g., RandomRateSelector)
            OnPriorityChanged?.Invoke(this);
        }

        // Event to notify when priority changes
        public event Action<Node> OnPriorityChanged;

#if UNITY_EDITOR
        /// <summary>
        /// Editor only callback called when the value in the inspector changes.
        /// </summary>
        public event Action InspectorChanged;
#endif


#if UNITY_EDITOR
        /// <summary>
        /// Editor only method called when the value in the inspector or changes.
        /// </summary>
        //[OnObjectChanged(DelayCall = true)]
        protected virtual void OnInspectorChanged()
        {
            InspectorChanged?.Invoke();
        }

        private void UpdateNodePosition(Node node, Vector2 newPosition)
        {
            // Set new position to node's property if it has one, or store positions in a dictionary
            Debug.Log($"Node {node.name} moved to {newPosition}");
        }


#endif

    }


    public interface IPolicy
    {
        bool ShouldReturn(Node.Status status);
    }

    public static class Policies
    {
        public static readonly IPolicy RunForever = new RunForeverPolicy();
        public static readonly IPolicy RunUntilSuccess = new RunUntilSuccessPolicy();
        public static readonly IPolicy RunUntilFailure = new RunUntilFailurePolicy();

        class RunForeverPolicy : IPolicy
        {
            public bool ShouldReturn(Node.Status status) => false;
        }

        class RunUntilSuccessPolicy : IPolicy
        {
            public bool ShouldReturn(Node.Status status) => status == Node.Status.Success;
        }

        class RunUntilFailurePolicy : IPolicy
        {
            public bool ShouldReturn(Node.Status status) => status == Node.Status.Failure;
        }
    }

    // public class BehaviourTree : Node {
    //     readonly IPolicy policy;

    //     public BehaviourTree(string name, IPolicy policy = null) : base(name) {
    //         this.policy = policy ?? Policies.RunForever;
    //     }

    //     public override Status Process() {
    //         Status status = children[currentChild].Process();
    //         if (policy.ShouldReturn(status)) {

    //             return status;
    //         }

    //         currentChild = (currentChild + 1) % children.Count;
    //         return Status.Running;
    //     }

    //     public void PrintTree() {
    //         StringBuilder sb = new StringBuilder();
    //         PrintNode(this, 0, sb);
    //         Debug.Log(sb.ToString());
    //     }

    //     static void PrintNode(Node node, int indentLevel, StringBuilder sb) {
    //         sb.Append(' ', indentLevel * 2).AppendLine(node.name);
    //         foreach (Node child in node.children) {
    //             PrintNode(child, indentLevel + 1, sb);
    //         }
    //     }
    // }
}