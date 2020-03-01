using UnityEditor;
using UnityEngine;

namespace MHLab.StickyNotes
{
    [CustomEditor(typeof(StickyNote), true)]
    public class StickyNoteEditor : Editor
    {
        private bool _colorsFoldout = false;
        private bool _targetFoldout = false;

        private bool _debugFoldout = false;
        private bool _debugMainFoldout = false;
        private bool _debugDescriptionFoldout = false;
        private bool _debugTargetFoldout = false;
        private bool _debugAuthorFoldout = false;

        private int _tagsIndex;

        public override void OnInspectorGUI()
        {
            var note = (StickyNote) target;
            bool isAdvanced = target is StickyNoteAdvanced;

            var tagObj = serializedObject.FindProperty("Tag");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Current Tag");
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(tagObj.stringValue);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("Available Tags");

            var tags = StickyNotesManager.GetTags();

            int counter = 0;

            while(counter < tags.Length)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < 4; j++)
                {
                    if (counter >= tags.Length) break;

                    EditorGUILayout.BeginVertical();
                    var tag = tags[counter];
                    if (GUILayout.Button(tag))
                    {
                        tagObj.stringValue = tag;
                    }
                    EditorGUILayout.EndVertical();
                    counter++;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Author"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Title"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Go to Note"))
            {
                Selection.activeGameObject = note.gameObject;
                SceneView.FrameLastActiveSceneView();
            }

            EditorGUILayout.EndVertical();
            if (isAdvanced)
            {
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("Expand"))
                {
                    var advanced = (StickyNoteAdvanced) note;
                    advanced.ToggleExpandedSection();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginVertical();
            if (note.Target != null)
            {
                if (GUILayout.Button("Go to Target"))
                {
                    Selection.activeGameObject = note.Target.gameObject;
                    SceneView.FrameLastActiveSceneView();
                }
            }
            else
            {
                if (GUILayout.Button("No Target"))
                {
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            _targetFoldout = EditorGUILayout.Foldout(_targetFoldout, "Target", true);
            if (_targetFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Target"));
            }

            if (isAdvanced)
            {
                _debugFoldout = EditorGUILayout.Foldout(_debugFoldout, "Debug Properties", true);
                if (_debugFoldout)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("MainCamera"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("MainCanvas"));

                    _debugMainFoldout = EditorGUILayout.Foldout(_debugMainFoldout, "Main", true);
                    if (_debugMainFoldout)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("MainBackground"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("MainBorder"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("MainText"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OpenButtonBackground"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("CloseButtonBackground"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ExpandedSection"));
                    }

                    _debugDescriptionFoldout = EditorGUILayout.Foldout(_debugDescriptionFoldout, "Description", true);
                    if (_debugDescriptionFoldout)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("DescriptionBackground"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("DescriptionBorder"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("DescriptionText"));
                    }

                    _debugTargetFoldout = EditorGUILayout.Foldout(_debugTargetFoldout, "Target", true);
                    if (_debugTargetFoldout)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("GoToTargetButton"));
                    }

                    _debugAuthorFoldout = EditorGUILayout.Foldout(_debugAuthorFoldout, "Author", true);
                    if (_debugAuthorFoldout)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("AuthorSection"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("AuthorText"));
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}