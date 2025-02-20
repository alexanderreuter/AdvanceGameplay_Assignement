using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Graphs
{
    public static class GraphAlgorithms 
    {
        private class NodeWrapper<T> where T : INode
        {
            public T                m_node;
            public NodeWrapper<T>   m_parent;
            public float            m_fDistance;
            public float            m_fRemainingDistance;

            public NodeWrapper(T node)
            {
                m_node = node;
                m_parent = null;
                m_fDistance = float.MaxValue;
                m_fRemainingDistance = float.MaxValue;
            }
        }

        public delegate bool LinkEvaluator(ILink link);

        public class Path : List<ILink>
        {
        }

        public static T GetClosestNode<T>(IGraph graph, Vector3 vWorldPos, float fMaxDistance = float.MaxValue) where T : class, IPositionNode 
        {
            T bestNode = null;
            if (graph != null)
            {
                foreach (INode node in graph.Nodes)
                {
                    if (node is T pn)
                    {
                        float fDistance = Vector3.Distance(vWorldPos, pn.WorldPosition);
                        if (fDistance < fMaxDistance)
                        {
                            fMaxDistance = fDistance;
                            bestNode = pn;
                        }
                    }
                }
            }

            return bestNode;
        }

        public static HashSet<T> FloodFill<T>(T start) where T : class, INode
        {
            // setup
            Queue<T> open = new Queue<T>();
            HashSet<T> closed = new HashSet<T>();
            open.Enqueue(start);

            // search / iteration
            while (open.Count > 0) 
            {
                T node = open.Dequeue();
                closed.Add(node);

                // search the neighbors
                foreach (ILink link in node.Links)
                {
                    if (link.Target is T neighbor &&
                        !open.Contains(neighbor) &&
                        !closed.Contains(neighbor))
                    {
                        open.Enqueue(neighbor);
                    }
                }
            }

            // goodies in here
            return closed;
        }

        public static HashSet<T> GetNodesInRange<T>(T start, int iRange) where T : class, INode
        {
            // setup
            Queue<T> open = new Queue<T>();
            HashSet<T> closed = new HashSet<T>();
            open.Enqueue(start);

            for (int i = 0; i <= iRange; ++i)
            {
                Queue<T> nextRipple = new Queue<T>();
                while (open.Count > 0)
                {
                    T node = open.Dequeue();
                    closed.Add(node);

                    // search the neighbors
                    foreach (ILink link in node.Links)
                    {
                        if (link.Target is T neighbor &&
                            !open.Contains(neighbor) &&
                            !closed.Contains(neighbor))
                        {
                            nextRipple.Enqueue(neighbor);
                        }
                    }
                }
                open = nextRipple;
            }

            // goodies in here
            return closed;
        }

        public static List<T> FindPath_BreadthFirstSearch<T>(T start, T goal) where T : class, INode
        {
            // setup
            Queue<T> open = new Queue<T>();
            HashSet<T> closed = new HashSet<T>();
            open.Enqueue(start);
            Dictionary<T, T> parentLookup = new Dictionary<T, T>();

            // search / iteration
            while (open.Count > 0)
            {
                T current = open.Dequeue();
                closed.Add(current);

                // did we find the goal?
                if (current == goal)
                {
                    // construct path
                    List<T> path = new List<T>();
                    while (current != null)
                    {
                        path.Add(current);
                        current = parentLookup.ContainsKey(current) ? parentLookup[current] : null;
                    }
                    return path;
                }

                // search the neighbors
                foreach (ILink link in current.Links)
                {
                    if (link.Target is T neighbor &&
                        !open.Contains(neighbor) &&
                        !closed.Contains(neighbor))
                    {
                        open.Enqueue(neighbor);
                        parentLookup[neighbor] = current;
                    }
                }
            }

            // no path :(
            return null;
        }

        private static NodeWrapper<T> GetNode<T>(T node, Dictionary<T, NodeWrapper<T>> wrapperLookup) where T : INode
        {
            NodeWrapper<T> wrapper;
            if (!wrapperLookup.TryGetValue(node, out wrapper))
            {
                wrapper = new NodeWrapper<T>(node);
                wrapperLookup[node] = wrapper;
            }

            return wrapper;
        }

        static float GetNodeDistance<T>(T A, T B) where T : class, INode
        {
            if (A is IPositionNode nA && B is IPositionNode nB)
            {
                return Vector3.Distance(nA.WorldPosition, nB.WorldPosition);
            }

            return A != B ? 1.0f : 0.0f;
        }

        static float GetNodeDistance<T>(IGraph graph, T A, T B) where T : class, INode
        {
            if (graph is ISearchableGraph searchableGraph)
            {
                return searchableGraph.Heuristic(A, B);
            }

            return GetNodeDistance<T>(A, B);
        }

        public static List<T> FindShortestPath_Dijkstra<T>(T start, T goal) where T : class, INode
        {
            // setup
            Dictionary<T, NodeWrapper<T>> wrapperLookup = new Dictionary<T, NodeWrapper<T>>();
            List<NodeWrapper<T>> open = new List<NodeWrapper<T>>();
            HashSet<NodeWrapper<T>> closed = new HashSet<NodeWrapper<T>>();
            NodeWrapper<T> startNode = GetNode(start, wrapperLookup);
            startNode.m_fDistance = 0.0f;
            open.Add(startNode);

            // search / iteration
            while (open.Count > 0)
            {
                // find node with smallest distance from start
                NodeWrapper<T> current = open[0];
                for (int i = 1; i < open.Count; ++i)
                {
                    if (open[i].m_fDistance < current.m_fDistance)
                    {
                        current = open[i];
                    }
                }
                open.Remove(current);
                closed.Add(current);

                // did we find the goal?
                if (current.m_node == goal)
                {
                    // construct path
                    List<T> path = new List<T>();
                    while (current != null)
                    {
                        path.Add(current.m_node);
                        current = current.m_parent;
                    }

                    return path;
                }

                // search the neighbors
                foreach (ILink link in current.m_node.Links)
                {
                    if (link.Target is T target)
                    {
                        NodeWrapper<T> neighbor = GetNode(target, wrapperLookup);
                        float fNewDistance = current.m_fDistance + GetNodeDistance(current.m_node, neighbor.m_node);

                        // investigate neighbor?
                        if (!open.Contains(neighbor) &&
                            !closed.Contains(neighbor))
                        {
                            open.Add(neighbor);
                            neighbor.m_parent = current;
                        }

                        // update parent?
                        if (fNewDistance < neighbor.m_fDistance)
                        {
                            neighbor.m_fDistance = fNewDistance;
                            neighbor.m_parent = current;
                        }
                    }
                }
            }

            // no path :(
            return null;
        }

        public static Dictionary<T, T> CalculateShortestPathTree<T>(T start) where T : class, INode
        {
            // setup
            Dictionary<T, NodeWrapper<T>> wrapperLookup = new Dictionary<T, NodeWrapper<T>>();
            List<NodeWrapper<T>> open = new List<NodeWrapper<T>>();
            HashSet<NodeWrapper<T>> closed = new HashSet<NodeWrapper<T>>();
            NodeWrapper<T> startNode = GetNode(start, wrapperLookup);
            startNode.m_fDistance = 0.0f;
            open.Add(startNode);

            // search / iteration
            while (open.Count > 0)
            {
                // find node with smallest distance from start
                NodeWrapper<T> current = open[0];
                for (int i = 1; i < open.Count; ++i)
                {
                    if (open[i].m_fDistance < current.m_fDistance)
                    {
                        current = open[i];
                    }
                }
                open.Remove(current);
                closed.Add(current);

                // search the neighbors
                foreach (ILink link in current.m_node.Links)
                {
                    if (link.Target is T target)
                    {
                        NodeWrapper<T> neighbor = GetNode(target, wrapperLookup);
                        float fNewDistance = current.m_fDistance + GetNodeDistance(current.m_node, neighbor.m_node);

                        // investigate neighbor?
                        if (!open.Contains(neighbor) &&
                            !closed.Contains(neighbor))
                        {
                            open.Add(neighbor);
                            neighbor.m_parent = current;
                        }

                        // update parent?
                        if (fNewDistance < neighbor.m_fDistance)
                        {
                            neighbor.m_fDistance = fNewDistance;
                            neighbor.m_parent = current;
                        }
                    }
                }
            }

            // create result
            Dictionary<T, T> parentLookup = new Dictionary<T, T>();
            foreach (NodeWrapper<T> node in closed)
            {
                if (node.m_parent != null)
                {
                    parentLookup[node.m_node] = node.m_parent.m_node;
                }
            }

            return parentLookup;
        }

        public static List<T> FindShortestPath_AStar<T>(IGraph graph, T start, T goal, LinkEvaluator linkEvaluator = null) where T : class, INode
        {
            // setup
            Dictionary<T, NodeWrapper<T>> wrapperLookup = new Dictionary<T, NodeWrapper<T>>();
            List<NodeWrapper<T>> open = new List<NodeWrapper<T>>();
            HashSet<NodeWrapper<T>> closed = new HashSet<NodeWrapper<T>>();
            NodeWrapper<T> startNode = GetNode(start, wrapperLookup);
            startNode.m_fDistance = 0.0f;
            startNode.m_fRemainingDistance = GetNodeDistance(graph, start, goal);
            open.Add(startNode);

            // search / iteration
            while (open.Count > 0 && open.Count < 5000)
            {
                // find node with smallest remainging distance from start
                NodeWrapper<T> current = open[0];
                for (int i = 1; i < open.Count; ++i)
                {
                    if (open[i].m_fRemainingDistance < current.m_fRemainingDistance)
                    {
                        current = open[i];
                    }
                }
                open.Remove(current);
                closed.Add(current);

                // did we find the goal?
                if (current.m_node.Equals(goal))
                {
                    // construct path
                    List<T> path = new List<T>();
                    while (current != null)
                    {
                        path.Add(current.m_node);
                        current = current.m_parent;
                    }

                    path.Reverse();
                    return path;
                }

                // search the neighbors
                foreach (ILink link in current.m_node.Links)
                {
                    // got link evaluator?
                    if (linkEvaluator != null &&
                        !linkEvaluator(link))
                    {
                        continue;
                    }

                    if (link.Target is T target)
                    {
                        NodeWrapper<T> neighbor = GetNode(target, wrapperLookup);
                        float fNewDistance = current.m_fDistance + GetNodeDistance(current.m_node, neighbor.m_node);
                        float fNewRemainingDistance = fNewDistance + GetNodeDistance<T>(graph, target, goal);

                        // investigate neighbor?
                        if (!open.Contains(neighbor) &&
                            !closed.Contains(neighbor))
                        {
                            open.Add(neighbor);
                            neighbor.m_parent = current;
                        }

                        // update parent?
                        if (fNewRemainingDistance < neighbor.m_fRemainingDistance)
                        {
                            neighbor.m_fDistance = fNewDistance;
                            neighbor.m_fRemainingDistance = fNewRemainingDistance;
                            neighbor.m_parent = current;
                        }
                    }
                }
            }

            // no path :(
            return null;
        }
    }
}