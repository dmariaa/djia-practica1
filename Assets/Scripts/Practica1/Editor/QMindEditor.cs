using System;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

namespace Practica1.Editor
{
    /// <summary>
    /// Editor para castellanizar las etiquetas de las propiedades
    /// del QMind en el editor.
    /// </summary>
    [CustomEditor(typeof(QMind))]
    public class QMindEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        { 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("forgetPreviousLearning"),
                new GUIContent("Olvidar aprendizaje anterior"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("alpha"),
                new GUIContent("QLearning alpha"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gamma"),
                new GUIContent("QLearning gamma"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("epsilon"),
                new GUIContent("QLearning epsilon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("epsilonDecayRate"),
                new GUIContent("Tasa de decremento de epsilon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("epsilonMinimumValue"),
                new GUIContent("Valor mínimo de epsilon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("numberOfEpisodes"),
                new GUIContent("Número de episodios"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loadingPanel"),
                new GUIContent("Panel Loading"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
