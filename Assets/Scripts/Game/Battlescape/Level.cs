using Game.General;
using Graphs;
using Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Battlescape
{
    public partial class Level : ProceduralMesh, ISearchableGraph
    {
        public class Node : IPositionNode
        {
            public Vector3Int       m_vCoord;
            public Vector3          m_vPosition;
            public List<Link>       m_links = new List<Link>();

            #region Properties

            public IEnumerable<ILink> Links => m_links;

            public Unit Unit { get; set; }

            public Vector3 WorldPosition => m_vPosition;

            #endregion
        }

        [SerializeField]
        public Vector3Int                           m_vSize = new Vector3Int(64, 12, 64);

        private bool[,,]                            m_voxels = null;
        private Dictionary<Vector3Int, Node>        m_nodes = null;
        private Texture3D                           m_sight;

        private static Level                        sm_instance;

        public static readonly Vector3Int[]         Directions = new Vector3Int[]{
            Vector3Int.forward, Vector3Int.right, Vector3Int.back,
            Vector3Int.left, Vector3Int.up, Vector3Int.down
        };

        public static readonly Vector3Int[]         Ups = new Vector3Int[]{
            Vector3Int.up, Vector3Int.up, Vector3Int.up,
            Vector3Int.up, Vector3Int.forward, Vector3Int.forward
        };

        #region Properties

        public bool this[Vector3Int v] => this[v.x, v.y, v.z];

        public bool this[int x, int y, int z] => x >= 0 && x < m_vSize.x &&
                                                 y >= 0 && y < m_vSize.y &&
                                                 z >= 0 && z < m_vSize.z ? m_voxels[x, y, z] : false;

        public Texture3D Sight => m_sight;

        public IEnumerable<INode> Nodes => m_nodes != null ? m_nodes.Values : null;

        public static Level Instance => sm_instance;

        #endregion

        private void OnEnable()
        {
            sm_instance = this;
        }

        private void OnDisable()
        {
            sm_instance = null;
        }

        protected void CreateVoxels()
        {
            // generate random voxel terrain
            m_voxels = new bool[m_vSize.x, m_vSize.y, m_vSize.z];
            for (int z = 0; z < m_vSize.z; z++)
            {
                for (int x = 0; x < m_vSize.x; x++)
                {
                    float fNoise = Mathf.PerlinNoise(x * 0.025f, z * 0.025f) * 0.5f +
                                   Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 0.25f +
                                   Mathf.PerlinNoise(x * 0.1f, z * 0.1f) * 0.125f;

                    int iHeight = Mathf.Clamp(Mathf.RoundToInt(fNoise * m_vSize.y), 1, m_vSize.y);

                    for (int y = 0; y < iHeight; ++y)
                    {
                        m_voxels[x, y, z] = true;
                    }
                }
            }
        }

        public Node GetNodeAt(Vector3Int v)
        {
            Node result = null;
            m_nodes.TryGetValue(v, out result);
            return result;
        }

        protected override Mesh CreateMesh()
        {
            // generate voxels
            CreateVoxels();
            CreateHouses();
            CreateNodes();
            CreateSightTexture();

            // create mesh
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Color> colors = new List<Color>();
            List<int> triangles = new List<int>();

            // create mesh from voxels
            Color grass = new Color(0.2f, 1.0f, 0.3f);
            Color dirt = new Color(0.5f, 0.25f, 0.0f);

            for (int z = 0; z < m_vSize.z; z++)
            {
                for (int y = 0; y < m_vSize.y; y++)
                {
                    for (int x = 0; x < m_vSize.x; x++)
                    {
                        // do we have a voxel?
                        if (m_voxels[x, y, z])
                        {
                            Vector3Int vPosition = new Vector3Int(x, y, z);
                            for (int i = 0; i < 5; ++i)
                            {
                                Vector3Int vNeighborVoxel = vPosition + Directions[i];
                                if (!this[vNeighborVoxel])
                                {
                                    AddQuad(vPosition, Directions[i], Ups[i], i == 4 ? grass : dirt, vertices, uv, colors, triangles);
                                }
                            }
                        }
                    }
                }
            }

            // create mesh from walls
            foreach (Wall wall in m_walls)
            {
                int i = System.Array.IndexOf(Directions, wall.Direction);
                AddQuad(wall.m_vA, Directions[i], Ups[i], wall.m_color, vertices, uv, colors, triangles);
                
                i = System.Array.IndexOf(Directions, -wall.Direction);
                AddQuad(wall.m_vB, Directions[i], Ups[i], wall.m_color, vertices, uv, colors, triangles);
            }

            // create mesh
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = vertices.ToArray();
            mesh.colors = colors.ToArray();
            mesh.uv = uv.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

        protected void CreateNodes()
        {
            // create nodes
            m_nodes = new Dictionary<Vector3Int, Node>();
            for (int z = 0; z < m_vSize.z; z++)
            {
                for (int y = 0; y < m_vSize.y; y++)
                {
                    for (int x = 0; x < m_vSize.x; x++)
                    {
                        // do we have a voxel?
                        if (this[x, y, z] && !this[x, y + 1, z])
                        {
                            Vector3Int vCoord = new Vector3Int(x, y, z);
                            m_nodes[vCoord] = new Node { 
                                m_vCoord = vCoord,
                                m_vPosition = vCoord + Vector3.up * 0.5f 
                            };
                        }
                    }
                }
            }

            // create links
            foreach (Node node in Nodes)
            {
                for (int z = -1; z <= 1; ++z)
                {
                    for (int x = -1; x <= 1; ++x)
                    {
                        if (x == 0 && z == 0)
                        {
                            continue;
                        }

                        for (int y = -1; y <= 1; ++y)
                        {
                            Vector3Int vNeighbor = node.m_vCoord + new Vector3Int(x, y, z);
                            Node neighbor;
                            if (m_nodes.TryGetValue(vNeighbor, out neighbor))
                            {
                                if (!HasWall(node.m_vCoord + Vector3Int.up, neighbor.m_vCoord + Vector3Int.up))
                                {
                                    node.m_links.Add(new Link(node, neighbor));
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void AddQuad(Vector3 vPosition, Vector3 vDirection, Vector3 vUp, Color c, List<Vector3> vertices, List<Vector2> uv, List<Color> colors, List<int> triangles)
        {
            Vector3 vRight = Vector3.Cross(vDirection, vUp).normalized;

            // calculate verts
            int iStart = vertices.Count;
            vertices.AddRange(new Vector3[]{
                vPosition + vDirection * 0.5f - vRight * 0.5f - vUp * 0.5f,
                vPosition + vDirection * 0.5f - vRight * 0.5f + vUp * 0.5f,
                vPosition + vDirection * 0.5f + vRight * 0.5f + vUp * 0.5f,
                vPosition + vDirection * 0.5f + vRight * 0.5f - vUp * 0.5f
            });

            // calculate uvs (planar mapping)
            for (int i = 0; i < 4; ++i)
            {
                Vector3 v = vertices[iStart + i];
                uv.Add(new Vector2(Vector3.Dot(vRight, v), Vector3.Dot(vUp, v)));
            }

            // add colors
            colors.AddRange(new Color[] { c, c, c, c });

            // add triangles
            triangles.AddRange(new int[]{
                iStart + 0, iStart + 1, iStart + 2,
                iStart + 0, iStart + 2, iStart + 3
            });
        }

        public float Heuristic(INode start, INode goal)
        {
            if (start is Node A &&
                goal is Node B)
            {
                return Vector3.Distance(A.m_vPosition, B.m_vPosition);
            }

            return 1.0f;
        }

        public HashSet<Vector3Int> GetVisibleVoxels(Vector3Int vCenter, int iRange)
        {
            // calculate sphere
            HashSet<Vector3Int> sphere = new HashSet<Vector3Int>();
            for (int z = -iRange; z <= iRange; z++)
            {
                for (int y = -iRange; y <= iRange; y++)
                {
                    for (int x = -iRange; x <= iRange; x++)
                    {
                        Vector3Int v = new Vector3Int(x, y, z);
                        if (v.magnitude <= iRange && !this[vCenter + v])
                        {
                            sphere.Add(vCenter + v);
                        }
                    }
                }
            }

            // find surface of sphere
            HashSet<Vector3Int> sphereSurface = new HashSet<Vector3Int>();
            foreach (Vector3Int c in sphere)
            {
                if (System.Array.FindIndex(Directions, vDir => !sphere.Contains(c + vDir)) >= 0)
                {
                    sphereSurface.Add(c);
                }
            }

            // do line of sight within sphere
            HashSet<Vector3Int> visible = new HashSet<Vector3Int>();
            foreach (Vector3Int vEnd in sphereSurface)
            {
                Vector3Int vPrev = vCenter;
                foreach (Vector3Int c in MathUtil.Bresenham3D(vCenter, vEnd))
                {
                    if (this[c] || HasWall(vPrev, c))
                    {
                        break;
                    }

                    visible.Add(c);
                    vPrev = c;
                }
            }

            return visible;
        }

        protected void CreateSightTexture()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            // create sight texture
            m_sight = new Texture3D(m_vSize.x, m_vSize.y, m_vSize.z, TextureFormat.ARGB32, false);
            m_sight.name = "LevelVisionTexture";
            m_sight.filterMode = FilterMode.Trilinear;
            m_sight.wrapMode = TextureWrapMode.Clamp;
            m_sight.hideFlags = HideFlags.DontSave;

            for (int z = 0; z < m_vSize.z; z++)
            {
                for (int y = 0; y < m_vSize.y; y++)
                {
                    for (int x = 0; x < m_vSize.x; x++)
                    {
                        m_sight.SetPixel(x, y, z, Color.black);
                    }
                }
            }
            m_sight.Apply();
        }

        public void UpdateSightTexture()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            // reset sight texture
            for (int z = 0; z < m_vSize.z; z++)
            {
                for (int y = 0; y < m_vSize.y; y++)
                {
                    for (int x = 0; x < m_vSize.x; x++)
                    {
                        Color c = m_sight.GetPixel(x, y, z);
                        m_sight.SetPixel(x, y, z, new Color(c.r, 0.0f, 0.0f));
                    }
                }
            }

            // get combined vision of all player teams
            Battlescape battlescape = GetComponentInParent<Battlescape>();
            foreach (Team team in battlescape.GetComponentsInChildren<Team>())
            {
                if (!team.m_bIsPlayerTeam)
                {
                    continue;
                }

                foreach (Unit unit in team.GetComponentsInChildren<Unit>())
                {
                    HashSet<Vector3Int> vision = GetVisibleVoxels(unit.HeadVoxel, Unit.VISION_RANGE);
                    foreach (Vector3Int vv in vision)
                    {
                        m_sight.SetPixel(vv.x, vv.y, vv.z, Color.white);
                    }
                }
            }

            m_sight.Apply();
        }
    }
}