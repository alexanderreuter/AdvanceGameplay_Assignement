using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;

namespace Bezier
{
    [CustomEditor(typeof(BezierCurve), true)]
    public class BezierCurveEditor : Editor
    {
        private Tool                        m_oldTool;

        private void OnEnable()
        {
            m_oldTool = Tools.current;
        }

        private void OnDisable()
        {
            Tools.current = m_oldTool;
        }

        public override void OnInspectorGUI()
        {
            BezierCurve bc = target as BezierCurve;

            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                bc.UpdateDistances();   
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            BezierCurve bc = target as BezierCurve;
            Tools.current = Tool.None;

            // draw curves
            for (int i = 1; i < bc.m_points.Count; ++i)
            {
                DrawCurve_Bezier(bc.m_points[i - 1], bc.m_points[i]);
            }

            // draw control points
            foreach (BezierCurve.ControlPoint cp in bc.m_points)
            {
                // select point?
                Handles.color = new Color(0.3f, 1.0f, 0.3f);
                Handles.SphereHandleCap(0, cp.m_vPosition, Quaternion.identity, 0.5f, EventType.Repaint);

                // draw tangent line
                Handles.color = new Color(0.3f, 0.3f, 1.0f);
                Handles.DrawLine(cp.m_vPosition, cp.m_vPosition + cp.m_vTangent, 2.0f);
                Handles.DrawDottedLine(cp.m_vPosition, cp.m_vPosition - cp.m_vTangent, 5.0f);

                // draw point distance
                //Handles.Label(cp.m_vPosition + Vector3.up * 0.5f, cp.m_fDistance.ToString("0.00"));
            }
        }

        private void DrawCurve_Bezier(BezierCurve.ControlPoint A, BezierCurve.ControlPoint B)
        {
            const int SEGMENT_COUNT = 32;

            Handles.color = Color.cyan;
            Vector3 vLast = A.m_vPosition;
            for (int i = 0; i <= SEGMENT_COUNT; ++i)
            {
                float f = i / (float)SEGMENT_COUNT;
                Vector3 p = BezierCurve.GetPosition(A, B, f);
                Handles.DrawLine(vLast, p);
                vLast = p;
            }
        }
    }
}