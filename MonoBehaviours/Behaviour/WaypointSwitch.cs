using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Opsive.DeathmatchAIKit.AI;

public class WaypointSwitch : MonoBehaviour {
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
        behaviorTree.SetVariableValue("Waypoints", planA);
    }

    public void FollowPlanB()
    {
        behaviorTree.SetVariableValue("Waypoints", planB);
    }

    public void FollowPlanC()
    {
        behaviorTree.SetVariableValue("Waypoints", planC);
    }

    public void FollowPlanD()
    {
        behaviorTree.SetVariableValue("Waypoints", planD);
    }

    public void FollowPlanE()
    {
        behaviorTree.SetVariableValue("Waypoints", planE);
    }

    public void FollowPlanF()
    {
        behaviorTree.SetVariableValue("Waypoints", planF);
    }

    public void FollowPlanG()
    {
        behaviorTree.SetVariableValue("Waypoints", planG);
    }

    public void FollowPlanH()
    {
        behaviorTree.SetVariableValue("Waypoints", planH);
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
