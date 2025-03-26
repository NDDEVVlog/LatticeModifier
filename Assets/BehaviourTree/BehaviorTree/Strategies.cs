using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourTrees {
    public interface IStrategy {
        Node.Status Process();

        void Reset() {
            // Noop
        }
    }

    public class ActionStrategy : IStrategy {
        readonly Action doSomething;
        
        public ActionStrategy(Action doSomething) {
            this.doSomething = doSomething;
        }
        
        public Node.Status Process() {
            doSomething();
            return Node.Status.Success;
        }
    }

    public class Condition : IStrategy {
        readonly Func<bool> predicate;
        
        public Condition(Func<bool> predicate) {
            this.predicate = predicate;
        }
        
        public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
    }

    public class ReturnFail : IStrategy
    {
        public Node.Status Process()
        {
            Debug.Log("Return Fail");
            return Node.Status.Failure;
        }
    }

    public class ReturnSuccess : IStrategy
    {
        public Node.Status Process()
        {
            Debug.Log("Return Success");
            return Node.Status.Success;
        }
    }


    public class PatrolStrategy : IStrategy
    {
        readonly Transform entity;
        readonly NavMeshAgent agent;
        readonly List<Transform> patrolPoints;
        readonly float patrolSpeed;
        int currentIndex;
        NavMeshPath path;

        public PatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> patrolPoints, float patrolSpeed = 2f)
        {
            this.entity = entity;
            this.agent = agent;
            this.patrolPoints = patrolPoints;
            this.patrolSpeed = patrolSpeed;
            this.currentIndex = 0;
            this.path = new NavMeshPath();
            agent.speed = patrolSpeed;
        }

        public Node.Status Process()
        {
            // If no patrol points are set, return failure
            if (patrolPoints == null || patrolPoints.Count == 0)
            {
                return Node.Status.Failure;
            }

            // Set the destination to the current patrol point if agent is not already moving
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                agent.SetDestination(patrolPoints[currentIndex].position);
            }

            // Check if agent has reached the current patrol point
            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                // Move to the next patrol point
                currentIndex = (currentIndex + 1) % patrolPoints.Count;
                return Node.Status.Success; 
            }

            return Node.Status.Running;
        }
    }




    public class MoveToTarget : IStrategy {
        readonly Transform entity;
        readonly NavMeshAgent agent;
        readonly Transform target;
        bool isPathCalculated;

        public MoveToTarget(Transform entity, NavMeshAgent agent, Transform target) {
            this.entity = entity;
            this.agent = agent;
            this.target = target;
        }

        public Node.Status Process() {
            Debug.Log("Move to Target");
            if (Vector3.Distance(entity.position, target.position) < 1f) {
                return Node.Status.Success;
            }
            
            agent.SetDestination(target.position);
            entity.LookAt(target.position.With(y:entity.position.y));

            if (agent.pathPending) {
                isPathCalculated = true;
            }
            return Node.Status.Running;
        }

        public void Reset() => isPathCalculated = false;
    }    
}
