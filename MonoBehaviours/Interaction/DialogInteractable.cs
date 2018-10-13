using System.Collections;
using UnityEngine;
using Opsive.ThirdPersonController;
using BehaviorDesigner.Runtime;
using UnityEngine.AI;
using KopliSoft.Behaviour;

namespace KopliSoft.Interaction
{
    public class DialogInteractable : BaseInteractable
    {
        [Tooltip("Should this person turn to the interviewer?")]
        [SerializeField]
        protected bool m_ShouldTurnToInerviewer = true;
        [SerializeField]
        protected string flowchartName;
        [SerializeField]
        private Fungus.Flowchart flowchart;

        private bool inProgress;
        private BehaviorTreeController behaviorTreeController;
        private BehaviorTree behaviorTree;
        private NavMeshAgent navMeshAgent;
        private Vector3 destination;

        void Start()
        {
            if (flowchart == null && flowchartName != null && flowchartName.Trim().Length != 0)
            {
                flowchart = GameObject.Find("/Fungus/Flowcharts/" + flowchartName).GetComponent<Fungus.Flowchart>();
            }

            behaviorTreeController = GetComponentInParent<BehaviorTreeController>();
            behaviorTree = GetComponentInParent<BehaviorTree>();
            navMeshAgent = GetComponentInParent<NavMeshAgent>();
        }

        public override bool CanInteract()
        {
            return !inProgress
                && m_Interactor != null
                && flowchart != null
                && !IsInCriminalMode();
        }

        public bool IsInProgress()
        {
            return inProgress;
        }

        public override void Interact()
        {
            if (m_Interactor != null && flowchart != null)
            {
                inProgress = true;
                flowchart.ExecuteBlock("Start");
                if (m_ShouldTurnToInerviewer && navMeshAgent != null)
                {
                    destination = navMeshAgent.destination;
                    DisableBehavior();
                    SetDestination(m_InteractorGameObject.transform.position);
                    StartCoroutine(CheckFacingInterviewer());
                }
                Fungus.BlockSignals.OnBlockEnd += OnBlockEnd;
            }
        }

        IEnumerator CheckFacingInterviewer()
        {
            Vector3 dir = Vector3.ProjectOnPlane(m_InteractorGameObject.transform.position - transform.parent.position, Vector3.up).normalized;
            while (Vector3.Dot(transform.forward, dir) < 0.9f)
            {
                yield return new WaitForSeconds(.1f);
            }
            navMeshAgent.isStopped = true;
        }

        void OnBlockEnd(Fungus.Block block)
        {
            if (block.BlockName.Equals("End"))
            {
                Fungus.BlockSignals.OnBlockEnd -= OnBlockEnd;
                inProgress = false;
                EventHandler.ExecuteEvent(m_InteractorGameObject, "OnAnimatorInteractionComplete");
                if (m_ShouldTurnToInerviewer && navMeshAgent != null)
                {
                    EnableBehavior();
                    SetDestination(destination);
                }
            }
        }

        private void EnableBehavior()
        {
            if (behaviorTreeController != null)
            {
                behaviorTreeController.EnableBehavior();
            }
            else
            {
                behaviorTree.EnableBehavior();
            }
        }

        private void DisableBehavior()
        {
            if (behaviorTreeController != null)
            {
                behaviorTreeController.DisableBehavior();
            }
            else
            {
                behaviorTree.DisableBehavior();
            }
        }

        protected void SetDestination(Vector3 destination)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(destination);
        }
    }
}
