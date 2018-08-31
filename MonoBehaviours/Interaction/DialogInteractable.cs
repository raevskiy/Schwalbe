using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opsive.ThirdPersonController;
using BehaviorDesigner.Runtime;
using UnityEngine.AI;

namespace KopliSoft.Interaction
{
    public class DialogInteractable : MonoBehaviour, IInteractable
    {
        [Tooltip("The ID of the Interactable. Used for ability filtering by the character. -1 indicates no ID")]
        [SerializeField]
        protected int m_ID = -1;
        [Tooltip("The layer of objects that can perform the interaction")]
        [SerializeField]
        protected LayerMask m_InteractorLayer;
        [Tooltip("The offset that the interactor should move to when interacting. A value of -1 means no movement on that axis")]
        [SerializeField]
        protected Vector3 m_TargetInteractorOffset = new Vector3(-1, -1, -1);
        [Tooltip("Should this person turn to the interviewer?")]
        [SerializeField]
        protected bool m_ShouldTurnToInerviewer = true;
        [SerializeField]
        protected string flowchartName;
        [SerializeField]
        private Fungus.Flowchart flowchart;

        private Transform m_Interactor;
        private Transform m_Transform;
        private bool inProgress;
        private GameObject m_InteractorGameObject;
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

        private void Awake()
        {
            m_Transform = transform;
            // Activate when an object with the specified interactor layer is within the trigger.
            enabled = false;
        }

        /// <summary>
        /// An object has entered the trigger. Determine if it is an interactor.
        /// </summary>
        /// <param name="other">The potential interactor.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (Utility.InLayerMask(other.gameObject.layer, m_InteractorLayer.value))
            {
                m_Interactor = other.transform;
                m_InteractorGameObject = other.gameObject;
                EventHandler.ExecuteEvent<IInteractable>(m_Interactor.gameObject, "OnInteractableHasInteractable", this);
                enabled = true;
            }
        }

        /// <summary>
        /// The interactor can no longer interact with the object if it leaves the trigger.
        /// </summary>
        /// <param name="other">The potential interactor.</param>
        private void OnTriggerExit(Collider other)
        {
            if (other.transform.Equals(m_Interactor))
            {
                EventHandler.ExecuteEvent<IInteractable>(m_Interactor.gameObject, "OnInteractableHasInteractable", null);
                m_Interactor = null;
                enabled = false;
            }
        }

        /// <summary>
        /// Returns the ID of the interactable object.
        /// </summary>
        /// <returns>The ID of the interactable object.</returns>
        public int GetInteractableID()
        {
            return m_ID;
        }

        /// <summary>
        /// Determines if the Interactor can interact with the InteractableTarget. Cases where the Interactor cannot interact include not facing the Interactable object or the camera
        /// not looking at the Interactable object.
        /// </summary>
        /// <returns>True if the Interactor can interact with the InteractableTarget</returns>
        public bool CanInteract()
        {
            return !inProgress
                && m_Interactor != null
                && flowchart != null
                && !IsInCriminalMode();
        }

        private bool IsInCriminalMode()
        {
            Criminal criminal = m_Interactor.GetComponent<Criminal>();
            return criminal != null && criminal.IsInCriminalMode();
        }

        /// <summary>
        /// Does the interactable object require the interactor to be in a target position?
        /// </summary>
        /// <returns>True if the interactor is required to be in a target position.</returns>
        public bool RequiresTargetInteractorPosition()
        {
            return m_TargetInteractorOffset.x != -1 || m_TargetInteractorOffset.y != -1 || m_TargetInteractorOffset.z != -1;
        }

        /// <summary>
        /// Returns the rotation that the interactor should face before interacting with the object.
        /// </summary>
        /// <returns>The target interactor rotation.</returns>
        public Quaternion GetTargetInteractorRotation()
        {
            // The character should be facing the opposite direction the interactable object is facing.
            return Quaternion.LookRotation(-m_Transform.forward);
        }

        /// <summary>
        /// Returns the position that the interactor should move to before interacting with the object.
        /// </summary>
        /// <param name="interactorTransform">The transform of the interactor.</param>
        /// <returns>The target interactor position.</returns>
        public Vector3 GetTargetInteractorPosition(Transform interactorTransform)
        {
            // Ignore the axis if the offset has a -1 value.
            var position = m_Transform.InverseTransformPoint(interactorTransform.position);
            if (m_TargetInteractorOffset.x != -1)
            {
                position.x = m_TargetInteractorOffset.x;
            }
            if (m_TargetInteractorOffset.y != -1)
            {
                position.y = m_TargetInteractorOffset.y;
            }
            if (m_TargetInteractorOffset.z != -1)
            {
                position.z = m_TargetInteractorOffset.z;
            }
            return m_Transform.TransformPoint(position);
        }

        public void Interact()
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
