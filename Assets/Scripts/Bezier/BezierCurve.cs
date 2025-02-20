using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bezier
{
    [ExecuteInEditMode]
    public class BezierCurve : MonoBehaviour
    {
        [System.Serializable]
        public class ControlPoint
        {
            public Vector3          m_vPosition;
            public Vector3          m_vTangent;
            public float            m_fDistance;
        }

        [SerializeField]
        public List<ControlPoint>   m_points = new List<ControlPoint>();

        [SerializeField] 
        public bool                 m_bClosed;

        private ControlPoint        m_closedPoint = new ControlPoint();
        private float               m_fLength;

        #region Properties

        public bool IsEmpty => m_points.Count == 0;

        public float Length => m_fLength;

        public ControlPoint FirstPoint => !IsEmpty ? m_points[0] : null;

        public ControlPoint LastPoint => !IsEmpty ? m_points[m_points.Count - 1] : null;

        public IEnumerable<ControlPoint> Points
        {
            get
            {
                foreach (ControlPoint cp in m_points)
                {
                    yield return cp;
                }

                if (m_bClosed)
                {
                    yield return m_closedPoint;
                }
            }
        }

        #endregion

        private void OnEnable()
        {
            UpdateDistances();
        }

        public Pose GetPoseAtDistance(float fDistance)
        {
            if (m_points.Count == 0)
            {
                throw new System.Exception("The Bezier Curve is empty");
            }

            if (fDistance <= 0.0f)
            {
                return new Pose 
                { 
                    position = FirstPoint.m_vPosition, 
                    rotation = Quaternion.LookRotation(FirstPoint.m_vTangent.normalized) 
                };
            }
            else if(fDistance >= Length)
            {
                return new Pose
                {
                    position = LastPoint.m_vPosition,
                    rotation = Quaternion.LookRotation(LastPoint.m_vTangent.normalized)
                };
            }

            for (int i = 1; i < m_points.Count; i++)
            {
                ControlPoint A = m_points[i - 1];
                ControlPoint B = m_points[i];

                if (/*fDistance >= A.m_fDistance &&*/ fDistance < B.m_fDistance)
                {
                    float fBlend = Mathf.InverseLerp(A.m_fDistance, B.m_fDistance, fDistance);
                    return new Pose
                    {
                        position = GetPosition(A, B, fBlend),
                        rotation = Quaternion.LookRotation(GetForward(A, B, fBlend))
                    };
                }
            }

            // loop back to start
            if (m_bClosed && fDistance <= m_fLength)
            {
                float fBlend = Mathf.InverseLerp(LastPoint.m_fDistance, m_closedPoint.m_fDistance, fDistance);
                return new Pose
                {
                    position = GetPosition(LastPoint, m_closedPoint, fBlend),
                    rotation = Quaternion.LookRotation(GetForward(LastPoint, m_closedPoint, fBlend))
                };
            }

            // this should never happen
            throw new System.Exception("Oh the horror...");
        }

        public void UpdateDistances()
        {
            if (m_points.Count == 0)
            {
                return;
            }

            m_fLength = 0.0f;
            m_points[0].m_fDistance = 0.0f;
            ControlPoint prev = m_points[0];
            foreach(ControlPoint cp in Points) 
            {
                if (cp != prev)
                {
                    cp.m_fDistance = prev.m_fDistance + CalculateDistance(prev, cp);
                }
            }
            m_fLength = LastPoint.m_fDistance;

            if (m_bClosed)
            {
                m_closedPoint.m_vPosition = FirstPoint.m_vPosition;
                m_closedPoint.m_vTangent = FirstPoint.m_vTangent;
                m_fLength += CalculateDistance(LastPoint, FirstPoint);
            }
        }

        public static float CalculateDistance(ControlPoint A, ControlPoint B)
        {
            const float SEGMENT_DISTANCE = 0.25f;

            Vector3 p0 = A.m_vPosition;
            Vector3 p1 = A.m_vPosition + A.m_vTangent;
            Vector3 p2 = B.m_vPosition - B.m_vTangent;
            Vector3 p3 = B.m_vPosition;

            float fRoughDistance = Vector3.Distance(p0, p1) +
                                   Vector3.Distance(p1, p2) +
                                   Vector3.Distance(p2, p3);

            int iSegmentCount = Mathf.RoundToInt(fRoughDistance / SEGMENT_DISTANCE);

            // draw curve
            Vector3 vLastPosition = A.m_vPosition;
            float fCurveDistance = 0.0f;
            for (int i = 0; i <= iSegmentCount; ++i)
            {
                float f = i / (float)iSegmentCount;
                Vector3 vPosition = GetPosition(A, B, f);
                fCurveDistance += Vector3.Distance(vLastPosition, vPosition);
                vLastPosition = vPosition;
            }

            return fCurveDistance;
        }

        public static Vector3 GetPosition(ControlPoint A, ControlPoint B, float t)
        {
            Vector3 p0 = A.m_vPosition;
            Vector3 p1 = A.m_vPosition + A.m_vTangent;
            Vector3 p2 = B.m_vPosition - B.m_vTangent;
            Vector3 p3 = B.m_vPosition;

            float fOneMinusT = 1.0f - t;

            return p0 * fOneMinusT * fOneMinusT * fOneMinusT +
                   p1 * 3.0f * fOneMinusT * fOneMinusT * t + 
                   p2 * 3.0f * fOneMinusT * t * t +
                   p3 * t * t * t;
        }

        public static Vector3 GetForward(ControlPoint A, ControlPoint B, float t)
        {
            Vector3 v1 = A.m_vPosition;
            Vector3 v2 = A.m_vPosition + A.m_vTangent;
            Vector3 v3 = B.m_vPosition - B.m_vTangent;
            Vector3 v4 = B.m_vPosition;

            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            Vector3 vDir = 3f * oneMinusT * oneMinusT * (v2 - v1) +
                           6f * oneMinusT * t * (v3 - v2) +
                           3f * t * t * (v4 - v3);

            return vDir.normalized;
        }
    }
}