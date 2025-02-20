using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Math;

namespace Game.General
{
    [CustomEditor(typeof(Bresenham))]
    public class BresenhamEditor : Editor
    {
        Vector2 vA = Vector2.zero;
        Vector2 vB = Vector2.one * 10;


        private void OnSceneGUI()
        {
            Vector2Int vMin = new Vector2Int(Mathf.RoundToInt(Mathf.Min(vA.x, vB.x)) - 4,
                                             Mathf.RoundToInt(Mathf.Min(vA.y, vB.y)) - 4);
            Vector2Int vMax = new Vector2Int(Mathf.RoundToInt(Mathf.Max(vA.x, vB.x)) + 4,
                                             Mathf.RoundToInt(Mathf.Max(vA.y, vB.y)) + 4);

            // draw actual line
            Handles.color = Color.red;
            Handles.DrawLine(vA, vB);

            // Bresenhams!
            PlotLine_Bresenham(new Vector2Int(Mathf.FloorToInt(vA.x), Mathf.FloorToInt(vA.y)),
                               new Vector2Int(Mathf.FloorToInt(vB.x), Mathf.FloorToInt(vB.y)));

            // draw grid
            Handles.color = Color.black;
            for (int y = vMin.y; y <= vMax.y; y++) Handles.DrawLine(new Vector2(vMin.x, y), new Vector2(vMax.x, y), 2);
            for (int x = vMin.x; x <= vMax.x; x++) Handles.DrawLine(new Vector2(x, vMin.y), new Vector2(x, vMax.y), 2);

            // move points
            vA = Handles.DoPositionHandle(vA, Quaternion.identity);
            vB = Handles.DoPositionHandle(vB, Quaternion.identity);
        }

        private void PlotLine(Vector2Int vA, Vector2Int vB)
        {
            int dx = vB.x - vA.x;
            int dy = vB.y - vA.y;
            int D = 2 * dy - dx;
            int y = vA.y;

            for (int x = vA.x; x <= vB.x; x++)
            {
                PlotPixel(new Vector2Int(x, y));

                if (D > 0)
                {
                    y = y + 1;
                    D = D - 2 * dx;
                }
                D = D + 2 * dy;
            }
        }

        private void PlotLine_Bresenham(Vector2Int vA, Vector2Int vB)
        {
            Vector3Int vP1 = new Vector3Int(vA.x, vA.y, 0);
            Vector3Int vP2 = new Vector3Int(vB.x, vB.y, 0);

            foreach (Vector3Int p in MathUtil.Bresenham3D(vP1, vP2))
            {
                PlotPixel(new Vector2Int(p.x, p.y));
            }
        }

        private void PlotPixel(Vector2Int p)
        {
            Vector3[] corners = new Vector3[]{
                (Vector3)(Vector2)p + new Vector3(0.0f, 0.0f, 0.0f),
                (Vector3)(Vector2)p + new Vector3(0.0f, 1.0f, 0.0f),
                (Vector3)(Vector2)p + new Vector3(1.0f, 1.0f, 0.0f),
                (Vector3)(Vector2)p + new Vector3(1.0f, 0.0f, 0.0f),
            };
            Handles.color = new Color(1.0f, 0.5f, 0.0f);
            Handles.DrawAAConvexPolygon(corners);
        }
    }
}