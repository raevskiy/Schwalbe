using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opsive.ThirdPersonController;
using BehaviorDesigner.Runtime;
using UnityEngine.AI;

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
        private BehaviorTree behaviorTree;
        private NavMeshAgent navMeshAgent;
        private Vector3 destination;

        void Start()
        {
            if (flowchart == null && flowchartName != null && flowchartName.Trim().Length != 0)
            {
                flowchart = GameObject.Find("/Fungus/Flowcharts/" + flowchartName).GetComponent<Fungus.Flowchart>();
            }

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

        public override void Interact()
        {
            if (m_Interactor != null && flowchart != null)
            {
                inProgress = true;
                flowchart.ExecuteBlock("Start");
                if (m_ShouldTurnToInerviewer && navMeshAgent != null)
                {
                    destination = navMeshAgent.destination;
                    behaviorTree.enabled = false;
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
                    behaviorTree.enabled = true;
                    SetDestination(destination);
                }
            }
        }

        protected void SetDestination(Vector3 destination)
        {
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
            navMeshAgent.Resume();
#else
            navMeshAgent.isStopped = false;
#endif
            navMeshAgent.SetDestination(destination);
        }
    }
}
