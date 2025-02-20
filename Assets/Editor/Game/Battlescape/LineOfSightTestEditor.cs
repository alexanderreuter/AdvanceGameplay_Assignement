using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Math;

namespace Game.Battlescape
{
    [CustomEditor(typeof(LineOfSightTest))]
    public class LineOfSightTestEditor : Editor
    {
        private int             m_iRange = 5;
        private Vector3[]       m_points = new Vector3[2] { new Vector3(10, 5, 10), new Vector3(20, 10, 20) };
        private int             m_iNumTests = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Num Tests", m_iNumTests.ToString());
            m_iRange = EditorGUILayout.IntSlider("Sight Range", m_iRange, 1, 20);
        }

        private void OnSceneGUI()
        {
            Tools.current = Tool.None;

            // move our test points
            for (int i = 0; i < m_points.Length; i++)
            {
                m_points[i] = Handles.DoPositionHandle(m_points[i], Quaternion.identity);
            }

            // snap to coordinates
            Vector3Int[] coords = System.Array.ConvertAll(m_points, p => new Vector3Int(Mathf.RoundToInt(p.x),
                                                                                        Mathf.RoundToInt(p.y),
                                                                                        Mathf.RoundToInt(p.z)));

            // line of sight with physics
            //LineOfSight_Physics(coords[0], coords[1]);

            // line of sight with Bresenham
            //LineOfSight_Bresenham(coords[0], coords[1]);

            // line of sight sphere
            LineOfSight_Sphere(coords[0], m_iRange);

            // draw end points
            Handles.SphereHandleCap(0, coords[0], Quaternion.identity, 0.25f, EventType.Repaint);
            Handles.SphereHandleCap(0, coords[1], Quaternion.identity, 0.25f, EventType.Repaint);
        }

        public void LineOfSight_Physics(Vector3Int vA, Vector3Int vB)
        {
            Ray ray = new Ray(vA, vB - vA);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Vector3.Distance(vA, vB)))
            {
                Handles.color = Color.red;
                Handles.DrawLine(vA, hit.point, 3.0f);
                Handles.DrawDottedLine(hit.point, vB, 6.0f);
                Handles.SphereHandleCap(0, hit.point, Quaternion.identity, 0.15f, EventType.Repaint);
            }
            else
            {
                Handles.color = Color.green;
                Handles.DrawLine(vA, vB, 5.0f);
            }
        }

        public void LineOfSight_Bresenham(Vector3Int vA, Vector3Int vB)
        {
            if (!Application.isPlaying || Level.Instance == null)
            {
                return;
            }

            bool bHit = false;
            Vector3Int vPrev = vA;
            foreach (Vector3Int c in MathUtil.Bresenham3D(vA, vB))
            {
                if (Level.Instance[c] || Level.Instance.HasWall(vPrev, c))
                {
                    bHit = true;
                }

                Handles.color = new Color(bHit ? 1.0f : 0.0f, bHit ? 0.0f : 1.0f, 0.0f, 0.5f);
                Handles.CubeHandleCap(0, c, Quaternion.identity, 1.0f, EventType.Repaint);
                Handles.DrawWireCube(c, Vector3.one);
                vPrev = c;
            }
        }

        public void LineOfSight_Sphere(Vector3Int vCenter, int iRange)
        {
            if (!Application.isPlaying || Level.Instance == null)
            {
                return;
            }

            // debug out the visible coordinates            
            HashSet<Vector3Int> visible = Level.Instance.GetVisibleVoxels(vCenter, iRange);
            Handles.color = Color.green;
            foreach (Vector3Int v in visible)
            {
                Handles.CubeHandleCap(0, v, Quaternion.identity, 0.15f, EventType.Repaint);
            }
        }
    }
}