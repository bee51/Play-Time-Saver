using System;
using System.Reflection;
using Game.Scripts.LevelManagement;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using YeappGame.Utilities;

namespace Game.Scripts.Play.Editor
{
    public class SaveSystemEditor : EditorWindow
    {
        public GameObject selectedObject;


        private Rect _objectArea;

        

        [MenuItem("Editors/Play Time Saver Editor")]
        private static void ShowWindow()
        {
            var window = GetWindow<SaveSystemEditor>();
            window.titleContent = new GUIContent("Playing Time Save Window");
            window.minSize = new Vector2(200, 200);
            window.Show();
        }

        private void OnGUI()
        {
            DrawArea();
            CreateArea();
        }

        private void CreateArea()
        {
            GUILayout.BeginArea(_objectArea);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Selected Object Name ");

            selectedObject = (GameObject) EditorGUILayout.ObjectField(selectedObject, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
            EditorGUILayout.Space(50);
            if (Selection.activeObject!=null)
            {
                selectedObject = Selection.activeGameObject.gameObject;
                
            }

            if (selectedObject != null)
            {
                GetAllComponents();
            }
        }

        private void DrawArea()
        {
            _objectArea.x = 0;
            _objectArea.y = 10;
            _objectArea.height = Screen.height - 50;
            _objectArea.width = Screen.width / 2;
        }


        private void GetAllComponents()
        {
            Component[] components = selectedObject.GetComponents(typeof(Component));
            foreach (Component component in components)
            {
                GUILayout.BeginHorizontal();
                var newComponentObjects = component;
                newComponentObjects =
                    (Component) EditorGUILayout.ObjectField(newComponentObjects, typeof(Component), true);
                if (Application.isPlaying)
                {
                    if (GUILayout.Button("Save Current Component"))
                    {
                        GetCopyOf(SaveCurrentData(component.name, newComponentObjects),newComponentObjects);
                        Debug.Log("Data saved");
                    }
                }

                GUILayout.EndHorizontal();
            }
        }

        private Component SaveCurrentData(string objectName, Component current)
        {
            #region Integrate Level System
            //it is for level prefab ındex
            var currentLevelIndex = LevelManager.Instance.CurrentLevelIdx;
            //Todo: it is for level prefabs it is change with level prefabs 
            var playingLevelPrefab = LevelSettings.Current.sortedLevelPrefabs[currentLevelIndex];
            #endregion
            

            var currentObject = playingLevelPrefab.transform.Find(objectName).gameObject;
            var type = current.GetType();
            var currentComponent = currentObject.GetComponent(type) as Component;

            return currentComponent;
        }

        public static T GetCopyOf<T>(Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                 BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch
                    {
                    } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }

            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }

            return comp as T;
        }
    }
}