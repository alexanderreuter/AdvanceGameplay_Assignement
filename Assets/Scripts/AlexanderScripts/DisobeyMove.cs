using System.Collections;
using System.Collections.Generic;
using Game.Battlescape;
using Game.Battlescape.UnitActions;
using Graphs;
using UnityEngine;

public class DisobeyMove : Move
{
    public DisobeyMove(Unit unit, Level.Node goal) : base(unit, goal) {}
    
    // Set the goal to a random valid node
    protected override void SetGoal()
    {
        HashSet<Level.Node> reachableNodes = GraphAlgorithms.GetNodesInRange(m_unit.Node, Unit.MOVEMENT_RANGE);
        reachableNodes.RemoveWhere(n => n.Unit != null); 
        reachableNodes.Remove(m_goal); 

        if (reachableNodes.Count > 0)
        {
            List<Level.Node> availableNodes = new List<Level.Node>(reachableNodes);
            m_goal = availableNodes[Random.Range(0, availableNodes.Count)];
        }
    }
}
