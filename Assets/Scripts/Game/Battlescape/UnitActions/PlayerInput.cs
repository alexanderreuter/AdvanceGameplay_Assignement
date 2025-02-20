using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Events;

namespace Game.Battlescape.UnitActions
{
    public partial class PlayerInput : Unit.UnitAction
    {
        private MeshCollider    m_moveArea;
        private Level.Node      m_targetNode;

        public PlayerInput(Unit unit) : base(unit)
        {
        }

        public override void OnBegin(bool bFirstTime)
        {
            base.OnBegin(bFirstTime);

            if (!bFirstTime)
            {
                return;
            }

            // generate a mesh
            List<Vector3> vertices = new List<Vector3>();
            List<Color> colors = new List<Color>();
            List<int> triangles = new List<int>();

            // get nodes in range
            HashSet<Level.Node> reachableNodes = GraphAlgorithms.GetNodesInRange(m_unit.Node, Unit.MOVEMENT_RANGE);
            foreach (Level.Node node in reachableNodes)
            {
                if (node.Unit == null)
                {
                    Vector3 vPosition = node.WorldPosition;
                    AddQuad(vPosition, new Color(0.5f, 0.5f, 1.0f, 0.5f), vertices, colors, triangles);
                }
            }

            // add quad for enemies in range
            foreach (Unit enemy in m_unit.EnemiesInRange)
            {
                Vector3 vPosition = (enemy.Node as IPositionNode).WorldPosition;
                AddQuad(vPosition, new Color(1.0f, 0.5f, 0.5f, 0.5f), vertices, colors, triangles);
            }

            // create mesh
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.colors = colors.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            // create gameobject
            GameObject go = new GameObject("MoveArea");
            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/MoveArea");
            m_moveArea = go.AddComponent<MeshCollider>();
            m_moveArea.sharedMesh = mesh;
        }

        public static void AddQuad(Vector3 vPosition, Color c, List<Vector3> vertices, List<Color> colors, List<int> triangles)
        {
            int iStart = vertices.Count;
            vertices.AddRange(new Vector3[]{
                new Vector3(vPosition.x - 0.5f, vPosition.y + 0.05f, vPosition.z - 0.5f),
                new Vector3(vPosition.x - 0.5f, vPosition.y + 0.05f, vPosition.z + 0.5f),
                new Vector3(vPosition.x + 0.5f, vPosition.y + 0.05f, vPosition.z + 0.5f),
                new Vector3(vPosition.x + 0.5f, vPosition.y + 0.05f, vPosition.z - 0.5f)
            });

            colors.AddRange(new Color[] { c, c, c, c });

            triangles.AddRange(new int[]{
                iStart + 0, iStart + 1, iStart + 2,
                iStart + 0, iStart + 2, iStart + 3
            });
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // do overwatch?
            if (Input.GetKeyDown(KeyCode.O))
            {
                EventHandler.Main.PushEvent(new OverwatchInput(m_unit, this));
            }

            // get move target node
            Ray mouseRay = BattleCamera.Instance.Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Input.GetMouseButtonDown(0) && 
                m_moveArea.Raycast(mouseRay, out hit, 100.0f))
            {
                m_targetNode = GraphAlgorithms.GetClosestNode<Level.Node>(Level.Instance, hit.point, 1.0f);
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();
            Object.Destroy(m_moveArea.gameObject);
            m_moveArea = null;

            if (m_targetNode != null)
            {
                if (m_targetNode.Unit != null)
                {
                    // shoot!
                    EventHandler.Main.PushEvent(new Shoot(m_unit, m_targetNode.Unit));
                }
                else
                {
                    // move!
                    EventHandler.Main.PushEvent(new Move(m_unit, m_targetNode));
                }
            }
        }

        public override bool IsDone()
        {
            return m_targetNode != null;
        }
    }
}