using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game.Battlescape
{
    [CustomEditor(typeof(Level))]
    public class LevelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // debug the sight texture
            Level lvl = target as Level;
            EditorGUILayout.ObjectField("Sight Texture", lvl.Sight, typeof(Texture3D), false);
        }

        private void OnSceneGUI()
        {
            if (Application.isPlaying)
            {
                Graphs.EditorGraphUtils.DrawGraph(target as Level);
            }
        }
    }
}