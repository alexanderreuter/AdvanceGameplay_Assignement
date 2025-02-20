using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Events;

namespace Game.Battlescape.UnitActions
{
    public partial class PlayerInput
    {
        public class OverwatchInput : Unit.UnitAction
        {
            private GameObject              m_overwatchArea;
            private bool                    m_bDone;
            private PlayerInput             m_parentEvent;
            private Mesh                    m_mesh;
            private HashSet<Level.Node>     m_overwatchNodes;
            private Level.Node              m_targetNode;

            public OverwatchInput(Unit unit, PlayerInput pi) : base(unit)
            {
                m_parentEvent = pi;
            }

            public override void OnBegin(bool bFirstTime)
            {
                base.OnBegin(bFirstTime);

                if (!bFirstTime)
                {
                    return;
                }

                // create mesh
                m_mesh = new Mesh();
                m_mesh.hideFlags = HideFlags.DontSave;
                m_mesh.name = "OverwatchMesh";
                m_mesh.MarkDynamic();

                // hide parent move area
                m_parentEvent.m_moveArea.gameObject.SetActive(false);

                // create gameobject
                m_overwatchArea = new GameObject("OverwatchArea");
                m_overwatchArea.AddComponent<MeshFilter>().mesh = m_mesh;
                m_overwatchArea.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/MoveArea");
            }

            protected void UpdateAreaPreview()
            {
                if (m_overwatchNodes == null)
                {
                    return;
                }

                // generate a mesh
                List<Vector3> vertices = new List<Vector3>();
                List<Color> colors = new List<Color>();
                List<int> triangles = new List<int>();

                foreach (Level.Node node in m_overwatchNodes)
                {
                    AddQuad(node.WorldPosition, Color.red, vertices, colors, triangles);
                }

                // create mesh      (TODO: don't update preview every frame, update only when cursor is moved a certain amount)
                m_mesh.Clear();
                m_mesh.vertices = vertices.ToArray();
                m_mesh.colors = colors.ToArray();
                m_mesh.triangles = triangles.ToArray();
                m_mesh.RecalculateBounds();
                m_mesh.RecalculateNormals();
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                Plane plane = new Plane(Vector3.up, m_unit.transform.position);
                Ray mouseRay = BattleCamera.Instance.Camera.ScreenPointToRay(Input.mousePosition);
                float fDistanceToPlane;

                // calculate overwatch nodes
                if (plane.Raycast(mouseRay, out fDistanceToPlane))
                {
                    Vector3 vHit = mouseRay.origin + mouseRay.direction * fDistanceToPlane;
                    Level.Node newTargetNode = GraphAlgorithms.GetClosestNode<Level.Node>(Level.Instance, vHit);

                    if (newTargetNode != m_targetNode)
                    {
                        m_targetNode = newTargetNode;
                        Vector3 vDirectionToHit = (vHit - m_unit.transform.position).normalized;

                        HashSet<Vector3Int> visibleVoxels = Level.Instance.GetVisibleVoxels(m_unit.HeadVoxel, Unit.VISION_RANGE);
                        m_overwatchNodes = new HashSet<Level.Node>();
                        foreach (Vector3Int vv in visibleVoxels)
                        {
                            Level.Node node = Level.Instance.GetNodeAt(vv + Vector3Int.down);
                            if (node != null &&
                                Vector3.Angle(vDirectionToHit, Vector3.Normalize(node.WorldPosition - m_unit.transform.position)) <= 45.0f)
                            {
                                m_overwatchNodes.Add(node);
                            }
                        }

                        // Why the heck is this so slow?
                        //m_overwatchNodes = GraphAlgorithms.GetNodesInRange<Level.Node>(m_unit.Node, Unit.VISION_RANGE);

                        UpdateAreaPreview();
                    }
                }

                // do overwatch?
                if (Input.GetMouseButtonDown(0))
                {
                    EventHandler.Main.RemoveEvent(m_parentEvent);
                    m_unit.OverwatchNodes = m_overwatchNodes;
                    m_bDone = true;
                }

                // abort overwatch?
                if (Input.GetMouseButtonDown(1))
                {
                    m_bDone = true;
                }
            }

            public override void OnEnd()
            {
                base.OnEnd();

                Object.Destroy(m_overwatchArea);

                // show parent move area
                m_parentEvent.m_moveArea?.gameObject.SetActive(true);
            }

            public override bool IsDone()
            {
                return m_bDone;
            }
        }
    }
}