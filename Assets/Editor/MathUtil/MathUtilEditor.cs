using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions.Must;

namespace Math
{
    public static class MathUtilEditor
    {
        public static void DrawVector(Vector3 vTarget, Color color, float fWidth = 5.0f)
        {
            DrawVector(Vector3.zero, vTarget, color, fWidth);
        }

        public static void DrawVector(Vector3 vSource, Vector3 vTarget, Color color, float fWidth = 5.0f)
        {
            Vector3 vCamForward = SceneView.currentDrawingSceneView.camera.transform.forward;

            Handles.color = color;
            Handles.DrawLine(vSource, vTarget, fWidth);
            Vector3 vDirection = Vector3.Normalize(vTarget - vSource);
            Vector3 vUp = Vector3.Cross(vDirection, vCamForward).normalized;
            float fArrowSize = Mathf.Min(Vector3.Distance(vSource, vTarget) * 0.075f, 0.4f);
            Handles.DrawLine(vTarget, vTarget - vDirection * fArrowSize + vUp * fArrowSize, fWidth);
            Handles.DrawLine(vTarget, vTarget - vDirection * fArrowSize - vUp * fArrowSize, fWidth);
        }

        public static void DrawGrid(Vector3 vCenter, Vector3 vForward, Vector3 vRight, Color color)
        {
            const int LINE_COUNT = 5;

            Handles.color = color * 0.6f;
            for (int i = 0; i <= LINE_COUNT; ++i)
            {
                Handles.DrawLine(vCenter + vForward * i - vRight * LINE_COUNT, vCenter + vForward * i + vRight * LINE_COUNT);
                Handles.DrawLine(vCenter - vForward * i - vRight * LINE_COUNT, vCenter - vForward * i + vRight * LINE_COUNT);
                Handles.DrawLine(vCenter + vRight * i - vForward * LINE_COUNT, vCenter + vRight * i + vForward * LINE_COUNT);
                Handles.DrawLine(vCenter - vRight * i - vForward * LINE_COUNT, vCenter - vRight * i + vForward * LINE_COUNT);
            }
        }

        public static void DrawPlane(Plane p, Color color, float fSize = 5.0f)
        {
            Handles.color = color;
            Vector3 vUp = Mathf.Abs(Vector3.Dot(p.normal, Vector3.up)) > 0.9f ? Vector3.forward : Vector3.up;
            Vector3 vRight = Vector3.Cross(p.normal, vUp).normalized;
            vUp = Vector3.Cross(p.normal, vRight).normalized;

            Vector3 vCenter = p.normal * -p.distance;
            Vector3[] corners = new Vector3[]
            {
                vCenter - vUp * fSize - vRight * fSize,
                vCenter + vUp * fSize - vRight * fSize,
                vCenter + vUp * fSize + vRight * fSize,
                vCenter - vUp * fSize + vRight * fSize,
            };

            Handles.color = color * 0.5f;
            Handles.DrawAAConvexPolygon(corners);
            
            Handles.color = color;
            for (int i = 0; i < corners.Length; ++i)
            {
                Handles.DrawLine(corners[i], corners[(i + 1) % corners.Length]);
            }
        }
    }
}
