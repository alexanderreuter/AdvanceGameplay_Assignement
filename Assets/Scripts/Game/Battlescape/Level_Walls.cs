using Game.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;

namespace Game.Battlescape
{
    public partial class Level
    {
        public class Wall
        {
            public Vector3Int   m_vA;
            public Vector3Int   m_vB;
            public Color        m_color;

            #region Properties

            public Vector3Int Direction => m_vB - m_vA;

            #endregion

            public Wall(Vector3Int vA, Vector3Int vB, Color color)
            {
                m_vA = vA;
                m_vB = vB;
                m_color = color;
            }

            public bool Contains(Vector3Int v)
            {
                return m_vA == v || m_vB == v;
            }
        }

        private List<Wall>                          m_walls = null;
        private Dictionary<Vector3Int, List<Wall>>  m_wallLookup = null;

        #region Properties

        #endregion

        protected RectInt ExpandRect(RectInt r, int iAmount)
        {
            return new RectInt(r.x - iAmount, r.y - iAmount, r.width + iAmount * 2, r.height + iAmount * 2);
        }

        protected void CreateHouses()
        {
            const int HOUSE_COUNT = 10;
            const int HOUSE_MIN_SIZE = 5;
            const int HOUSE_MAX_SIZE = 12;
            const int HOUSE_BORDER_MARGIN = 3;
            const int HOUSE_MARGIN = 2;

            List<RectInt> houses = new List<RectInt>();
            m_walls = new List<Wall>();
            m_wallLookup = new Dictionary<Vector3Int, List<Wall>>();
            for (int i = 0; i < HOUSE_COUNT; ++i)
            {
                // create random rectangle
                int iWidth = Random.Range(HOUSE_MIN_SIZE, HOUSE_MAX_SIZE);
                int iHeight = Random.Range(HOUSE_MIN_SIZE, HOUSE_MAX_SIZE);
                RectInt house = new RectInt(Random.Range(HOUSE_BORDER_MARGIN, m_vSize.x - (iWidth + HOUSE_BORDER_MARGIN)),
                                            Random.Range(HOUSE_BORDER_MARGIN, m_vSize.x - (iWidth + HOUSE_BORDER_MARGIN)),
                                            iWidth, iHeight);

                // get the expanded house rect
                RectInt expandedHouse = ExpandRect(house, HOUSE_MARGIN);

                // make sure it doesn't overlap an existing house
                if (houses.FindIndex(h => h.Overlaps(expandedHouse)) < 0)
                {
                    houses.Add(house);
                    CreateHouse(house);
                }
            }            
        }

        protected int GetHeightAt(Vector2Int v)
        {
            for (int y = 0; y < m_vSize.y; y++)
            {
                if (!this[v.x, y, v.y])
                {
                    return y - 1;
                }
            }

            return m_vSize.y;
        }

        protected Wall CreateWallBetween(Vector3Int vA, Vector3Int vB, Color color)
        {
            // TODO: check if one already exists?

            Wall newWall = new Wall(vA, vB, color);
            m_walls.Add(newWall);

            if (!m_wallLookup.ContainsKey(vA)) m_wallLookup[vA] = new List<Wall>();
            if (!m_wallLookup.ContainsKey(vB)) m_wallLookup[vB] = new List<Wall>();

            m_wallLookup[vA].Add(newWall);
            m_wallLookup[vB].Add(newWall);

            return newWall;
        }

        protected void CreateHouse(RectInt house)
        {
            // calculate average ground height below house
            int iTotalHeight = 0;
            int iNumNodes = 0;
            foreach (Vector2Int v in house.allPositionsWithin)
            {
                iTotalHeight += GetHeightAt(v);
                iNumNodes++;
            }
            int iAverageHeight = Mathf.RoundToInt(iTotalHeight / (float)iNumNodes);

            // flatten ground under (and around) house
            RectInt expandedHouse = ExpandRect(house, 2);
            foreach (Vector2Int v in expandedHouse.allPositionsWithin)
            {
                for (int y = 0; y < m_vSize.y; y++)
                {
                    m_voxels[v.x, y, v.y] = y <= iAverageHeight;
                }
            }

            // random wall color
            Color houseColor = new Color(Random.value, Random.value * 0.2f, Random.value);
            Color baseColor = Random.value > 0.5f ? new Color(Random.value, Random.value * 0.2f, Random.value) : houseColor;

            // random house height
            int iHeight = Random.Range(3, 5);

            // create random door coordinates
            List<Vector2Int> doorCoordinates = new List<Vector2Int>();
            {
                for (int x = house.x + 1; x < house.xMax - 1; ++x)
                {
                    doorCoordinates.Add(new Vector2Int(x, house.yMin));
                    doorCoordinates.Add(new Vector2Int(x, house.yMax));
                }
                for (int z = house.y + 1; z < house.yMax - 1; ++z)
                {
                    doorCoordinates.Add(new Vector2Int(house.xMin, z));
                    doorCoordinates.Add(new Vector2Int(house.xMax, z));
                }
                int iDoorCount = Random.Range(1, 3);
                while (doorCoordinates.Count > iDoorCount)
                {
                    doorCoordinates.RemoveAt(Random.Range(0, doorCoordinates.Count));
                }
            }

            // create walls along perimiter of house
            for (int h = 1; h <= iHeight; h++)
            {
                Color wallColor = h == 1 ? baseColor : houseColor;

                // X Walls
                for (int x = house.x; x < house.xMax; ++x)
                {
                    Vector3Int vCoord = new Vector3Int(x, iAverageHeight + h, house.yMin);
                    if (h >= 3 || !doorCoordinates.Contains(new Vector2Int(vCoord.x, vCoord.z)))
                    {
                        CreateWallBetween(vCoord, vCoord + Vector3Int.back, wallColor);
                    }

                    vCoord = new Vector3Int(x, iAverageHeight + h, house.yMax);
                    if (h >= 3 || !doorCoordinates.Contains(new Vector2Int(vCoord.x, vCoord.z)))
                    {
                        CreateWallBetween(vCoord, vCoord + Vector3Int.back, wallColor);
                    }
                }

                // Z Walls
                for (int z = house.y; z < house.yMax; ++z)
                {
                    Vector3Int vCoord = new Vector3Int(house.xMin, iAverageHeight + h, z);
                    if (h >= 3 || !doorCoordinates.Contains(new Vector2Int(vCoord.x, vCoord.z)))
                    {
                        CreateWallBetween(vCoord, vCoord + Vector3Int.left, wallColor);
                    }

                    vCoord = new Vector3Int(house.xMax, iAverageHeight + h, z);
                    if (h >= 3 || !doorCoordinates.Contains(new Vector2Int(vCoord.x, vCoord.z)))
                    {
                        CreateWallBetween(vCoord, vCoord + Vector3Int.left, wallColor);
                    }
                }
            }

            // create roof
            Color roofColor = new Color(Random.Range(0.7f, 1.0f), Random.Range(0.7f, 1.0f), Random.Range(0.7f, 1.0f));
            foreach (Vector2Int v in house.allPositionsWithin)
            {
                Vector3Int vCoord = new Vector3Int(v.x, iAverageHeight + iHeight, v.y);
                CreateWallBetween(vCoord, vCoord + Vector3Int.up, roofColor);
            }
        }

        public bool HasWall(Vector3Int vA, Vector3Int vB)
        {
            if (vA == vB)
            {
                return false;
            }

            // tough and heavy check
            for (int k = 0; k < 2; ++k)
            {
                Vector3Int vStart = k == 0 ? vA : vB;
                Vector3Int vGoal = k == 0 ? vB : vA;
                Vector3Int vDir = vGoal - vStart;

                List<Wall> walls;
                if (m_wallLookup.TryGetValue(vStart, out walls))
                {
                    // simple check?
                    if (Mathf.Abs(vDir.sqrMagnitude - 1.0f) < 0.0001f)
                    {
                        return walls.FindIndex(w => w.Contains(vGoal)) >= 0;
                    }

                    for (int i = 0; i < 3; ++i)
                    {
                        if (vDir[i] != 0)
                        {
                            Vector3Int vEnd = vStart;
                            vEnd[i] += vDir[i];

                            if (walls.FindIndex(w => w.Contains(vEnd)) >= 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false; 
        }
    }
}