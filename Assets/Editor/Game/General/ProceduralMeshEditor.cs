using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.General
{
    [CustomEditor(typeof(ProceduralMesh), true)]
    public class ProceduralMeshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(20);
            if (GUILayout.Button("Update Mesh"))
            {
                ProceduralMesh pm = target as ProceduralMesh;
                pm.UpdateMesh();
            }
        }
    }
}