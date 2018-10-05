using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Opsive.DeathmatchAIKit.AI;
using Opsive.DeathmatchAIKit.AI.Actions;

namespace KopliSoft.Behaviour
{
    public class WaypointSwitch : MonoBehaviour
    {
        public List<GameObject> planA;
        public List<GameObject> planB;
        public List<GameObject> planC;
        public List<GameObject> planD;
        public List<GameObject> planE;
        public List<GameObject> planF;
        public List<GameObject> planG;
        public List<GameObject> planH;

        [SerializeField]
        protected string flowchartName;
        [SerializeField]
        private Fungus.Flowchart flowchart;
        [SerializeField]
        protected GameObject trackedTarget;

        private BehaviorTree behaviorTree;
        private DeathmatchAgent deathmatchAgent;
        private bool trackedTargetFound;

        void Start()
        {
            behaviorTree = GetComponent<BehaviorTree>();
            deathmatchAgent = GetComponent<DeathmatchAgent>();

            if (flowchart == null && flowchartName != null && flowchartName.Trim().Length != 0)
            {
                flowchart = GameObject.Find("/Fungus/Flowcharts/" + flowchartName).GetComponent<Fungus.Flowchart>();
            }
        }

        void Update()
        {
            if (flowchart != null && !trackedTargetFound && behaviorTree.GetVariable("Target").GetValue() == trackedTarget)
            {
                trackedTargetFound = true;
                flowchart.ExecuteBlock("Main");
            }
        }

        public void FollowPlanA()
        {
            FollowPlan(planA);
        }

        public void FollowPlanB()
        {
            FollowPlan(planB);
        }

        public void FollowPlanC()
        {
            FollowPlan(planC);
        }

        public void FollowPlanD()
        {
            FollowPlan(planD);
        }

        public void FollowPlanE()
        {
            FollowPlan(planE);
        }

        public void FollowPlanF()
        {
            FollowPlan(planF);
        }

        public void FollowPlanG()
        {
            FollowPlan(planG);
        }

        public void FollowPlanH()
        {
            FollowPlan(planH);
        }

        private void FollowPlan(List<GameObject> plan)
        {
            behaviorTree.DisableBehavior();
            behaviorTree.SetVariableValue("Waypoints", plan);
            behaviorTree.EnableBehavior();
        }

        public void UntrackPlayer()
        {
            deathmatchAgent.TargetLayerMask = new LayerMask();
        }

        public void TrackPlayer()
        {
            deathmatchAgent.TargetLayerMask = LayerMask.GetMask("Player");
        }
    }
}
