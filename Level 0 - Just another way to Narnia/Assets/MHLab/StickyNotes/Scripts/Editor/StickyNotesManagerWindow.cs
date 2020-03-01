using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityScript.Steps;

namespace MHLab.StickyNotes
{
	public class StickyNotesManagerWindow : EditorWindow
	{
		private static StickyNotesManagerWindow _window;

		protected static StickyNotesManagerWindow Window
		{
			set { _window = value; }
			get
			{
				if (_window == null)
				{
                    StickyNotesManager.Initialize();
					StickyNotesManager.LoadTags();
					_window = EditorWindow.GetWindow<StickyNotesManagerWindow>(false, "StickyNotes Manager");
				}

				return _window;
			}
		}

		#region GUI Settings

		private static float _descriptionHeight = 60;
		private static float _margin = 10f;
		private static float _innerMargin = 10f;
		private static float _buttonMargin = 5f;
		private static float _borderWidth = 15f;
		private static float _noteHeight = 35;
		private static float _buttonHeight = 20;
		private static float _buttonPerRow = 4;
	    private static float _tagHeight = 90 +_buttonHeight + _buttonMargin;
	    private static float _tagStringHeight = 20;
	    private static float _tagDescriptionHeight = 45;

        private static Vector2 _currentScrollPosition;

	    private static bool _menuNotesToggle = true;
	    private static bool _menuTagsToggle = false;
	    private static bool _menuOldNotesToggle = true;
	    private static bool _menuOldTagsToggle = false;

	    private static Color _originalColor;
	    private static Color _pickedColor;
		#endregion

		[MenuItem("GameObject/StickyNotes/Create advanced note", false, 21)]
		[MenuItem("Window/DevKit/StickyNotes/Create advanced note")]
		public static void CreateStickyNote()
		{
		    StickyNotesManager.Initialize();
            StickyNotesManager.LoadTags();
			var note = (GameObject)GameObject.Instantiate(Resources.Load("StickyNotes/StickyNote"));
		    var stickyNote = note.GetComponent<StickyNoteAdvanced>();
		    stickyNote.Tag = "Root";
			if (Selection.activeGameObject != null)
			{
				note.transform.parent = Selection.activeTransform;
				note.transform.position = Selection.activeTransform.position;
			}
		}

	    [MenuItem("GameObject/StickyNotes/Create note", false, 20)]
	    [MenuItem("Window/DevKit/StickyNotes/Create note")]
	    public static void CreateStickyNoteSimple()
	    {
	        StickyNotesManager.Initialize();
            StickyNotesManager.LoadTags();
	        var note = (GameObject)GameObject.Instantiate(Resources.Load("StickyNotes/StickyNoteSimple"));
	        var stickyNote = note.GetComponent<StickyNote>();
	        stickyNote.Tag = "Root";
            if (Selection.activeGameObject != null)
	        {
	            note.transform.parent = Selection.activeTransform;
	            note.transform.position = Selection.activeTransform.position;
	        }
	    }

        [MenuItem("Window/DevKit/StickyNotes/Manager")]
		public static void OpenManager()
        {
            StickyNotesManager.Initialize();
            StickyNotesManager.LoadTags();
            _window = EditorWindow.GetWindow<StickyNotesManagerWindow>(false, "StickyNotes Manager");
		}

		public void Update()
		{
			Repaint();
		}

		public void OnGUI()
		{
		    EditorStyles.label.wordWrap = true;
		    float currentWritingHeight = _margin;

		    _originalColor = GUI.contentColor;

            float menuButtonWidth = (Window.position.width - (_margin * 2)) / 2;


		    _menuNotesToggle = GUI.Toggle(new Rect(_margin, currentWritingHeight, menuButtonWidth, _buttonHeight), _menuNotesToggle, "Notes", "Button");
		    _menuTagsToggle = GUI.Toggle(new Rect(menuButtonWidth, currentWritingHeight, menuButtonWidth, _buttonHeight), _menuTagsToggle, "Tags", "Button");

		    CheckMenu();

		    currentWritingHeight += _buttonHeight + _margin;

		    if (_menuNotesToggle)
		    {
                RenderNotesMenu(currentWritingHeight);
		    }

		    if (_menuTagsToggle)
		    {
                RenderTagsMenu(currentWritingHeight);
		    }

			GUI.contentColor = _originalColor;
		}

	    private static void CheckMenu()
	    {
	        if (_menuNotesToggle != _menuOldNotesToggle)
	        {
	            if (_menuNotesToggle)
	            {
	                _menuTagsToggle = false;
	            }
	            _menuOldNotesToggle = _menuNotesToggle;
	        }

	        if (_menuTagsToggle != _menuOldTagsToggle)
	        {
	            if (_menuTagsToggle)
	            {
	                _menuNotesToggle = false;
	            }
	            _menuOldTagsToggle = _menuTagsToggle;
	        }
	    }

	    private static void RenderNotesMenu(float currentWritingHeight)
	    {
            EditorGUI.LabelField(new Rect(_margin, currentWritingHeight, Window.position.width - (_margin * 2), _descriptionHeight),
                "Here you can manage all your notes around the scene. You can have a clear view of them, select a specific one, delete them, expand them. In few words: you can have a lot of fun with them! :)");
            currentWritingHeight += _descriptionHeight;

	        float width = (Window.position.width - _margin * 2) / 4;

	        if (GUI.Button(new Rect(_margin, currentWritingHeight, width, _buttonHeight), "Refresh"))
	        {
	            StickyNotesManager.LoadTags(true);
	        }

	        if (GUI.Button(new Rect(_margin + width, currentWritingHeight, width, _buttonHeight), "Delete all"))
	        {
	            foreach (var stickyNote in StickyNotesManager.NotesArray)
	            {
                    StickyNotesManager.UnregisterNote(stickyNote);
                    GameObject.DestroyImmediate(stickyNote.gameObject);
	            }
	        }

	        if (GUI.Button(new Rect(_margin + width * 2, currentWritingHeight, width, _buttonHeight), "Enable all"))
	        {
	            foreach (var stickyNote in StickyNotesManager.Notes)
	            {
	                var note = stickyNote.Value;
	                note.gameObject.SetActive(true);
	            }
	        }

            if (GUI.Button(new Rect(_margin + width * 3, currentWritingHeight, width, _buttonHeight), "Disable all"))
	        {
	            foreach (var stickyNote in StickyNotesManager.Notes)
	            {
	                var note = stickyNote.Value;
	                note.gameObject.SetActive(false);
	            }
            }

	        currentWritingHeight += _buttonHeight + _margin * 2;

            if (StickyNotesManager.Count < 1)
	        {
	            EditorGUI.LabelField(new Rect(_margin, currentWritingHeight, Window.position.width - (_margin * 2), _descriptionHeight), "No notes found in the scene.");
	            return;
	        }

            _currentScrollPosition = GUI.BeginScrollView(
                new Rect(0, currentWritingHeight, Window.position.width, 6 * (_noteHeight + _buttonMargin + _buttonHeight + _margin * 2)),
                _currentScrollPosition,
                new Rect(0, currentWritingHeight, Window.position.width,
                    StickyNotesManager.Count * (_noteHeight + _buttonMargin + _buttonHeight + _margin * 2)), false, false
            );
            try
            {
                foreach (var stickyNote in StickyNotesManager.Notes)
                {
                    var tag = StickyNotesManager.GetTag(stickyNote.Value.Tag);
                    EditorGUI.DrawRect(new Rect(0 + _margin, currentWritingHeight, Window.position.width - (_margin * 2), _noteHeight),
                        tag.MainColor);
                    EditorGUI.DrawRect(new Rect(0 + _margin, currentWritingHeight, _borderWidth, _noteHeight),
                        tag.SecondaryColor);

                    GUI.contentColor = tag.SecondaryColor;
                    EditorGUI.LabelField(
                        new Rect(0 + _margin + _borderWidth + _innerMargin, currentWritingHeight + _innerMargin,
                            Window.position.width - (_margin * 2) - (_innerMargin * 2), _noteHeight - _innerMargin), stickyNote.Value.Title);

                    currentWritingHeight += _buttonMargin + _noteHeight;

                    bool isAdvanced = stickyNote.Value is StickyNoteAdvanced;

                    width = (Window.position.width - _margin * 2) / (_buttonPerRow - ((!isAdvanced) ? 1 : 0));

                    GUI.contentColor = _originalColor;
                    if (GUI.Button(new Rect(_margin, currentWritingHeight, width - (_buttonMargin / (_buttonPerRow - 1)), _buttonHeight), "To Note"))
                    {
                        Selection.activeGameObject = stickyNote.Value.gameObject;
                        SceneView.FrameLastActiveSceneView();
                    }

                    if (GUI.Button(new Rect(width + _margin, currentWritingHeight, width - (_buttonMargin / (_buttonPerRow - 1)), _buttonHeight), "Delete"))
                    {
                        GameObject.DestroyImmediate(stickyNote.Value.gameObject);
                    }

                    if (isAdvanced)
                    {
                        if (GUI.Button(new Rect(width * 2 + _margin, currentWritingHeight,  width - (_buttonMargin / (_buttonPerRow - 1)), _buttonHeight), "Expand"))
                        {
                            var advanced = (StickyNoteAdvanced) stickyNote.Value;
                            advanced.ToggleExpandedSection();
                        }
                    }

                    if (GUI.Button(new Rect(width * (3 - ((!isAdvanced) ? 1 : 0)) + _margin, currentWritingHeight, width - (_buttonMargin / (_buttonPerRow - 1)), _buttonHeight), "To Target"))
                    {
                        if (stickyNote.Value.Target != null)
                        {
                            Selection.activeGameObject = stickyNote.Value.Target.gameObject;
                            SceneView.FrameLastActiveSceneView();
                        }
                    }

                    currentWritingHeight += _margin * 2 + _buttonHeight;
                }
            }
            catch (InvalidOperationException)
            { }

            GUI.EndScrollView();
        }

	    private static void RenderTagsMenu(float currentWritingHeight)
	    {
	        EditorGUI.LabelField(new Rect(_margin, currentWritingHeight, Window.position.width - (_margin * 2), _descriptionHeight),
	            "Here you can manage all your tags for your notes. Tags are useful to group together notes in the same category!");
	        currentWritingHeight += _descriptionHeight;

	        if (GUI.Button(new Rect(_margin, currentWritingHeight, Window.position.width - (_margin * 2), _buttonHeight), "Save all changes"))
	        {
                StickyNotesManager.SaveTags();
	        }

	        currentWritingHeight += _buttonHeight + _margin;

            if (GUI.Button(new Rect(_margin, currentWritingHeight, Window.position.width - (_margin * 2), _buttonHeight), "Add new tag"))
	        {
	            var tag = new StickyNoteTag()
	            {
                    Description = "",
                    Tag = "Temp " + (StickyNotesManager.Tags.Count + 1)
	            };
	            tag.SetColor(Color.white);
                StickyNotesManager.RegisterTag(tag);
	        }

            currentWritingHeight += _buttonHeight + _margin;
	        float windowWidth = Window.position.width - _margin * 2;

            _currentScrollPosition = GUI.BeginScrollView(
                new Rect(0, currentWritingHeight, Window.position.width, Window.position.height - currentWritingHeight), //6 * (_noteHeight + _buttonMargin + _buttonHeight + _margin * 2)),
                _currentScrollPosition,
                new Rect(0, currentWritingHeight, windowWidth, StickyNotesManager.Tags.Count * (_tagHeight + _margin)), 
                false, true
            );

            try
            {
                var tags = StickyNotesManager.GetTagsArray();
                foreach (var tag in tags)
                {
                    if (tag.Tag == "Root") continue;

                    _pickedColor = tag.MainColor;

                    EditorGUI.DrawRect(new Rect(0 + _margin, currentWritingHeight, windowWidth - (_margin * 2), _tagHeight),
                        tag.MainColor);
                    
                    tag.Tag = EditorGUI.TextField(
                        new Rect(0 + _margin + _innerMargin, currentWritingHeight + _innerMargin,
                            ((windowWidth - (_margin * 2) - (_innerMargin * 2)) / 2) - _buttonMargin, _tagStringHeight), tag.Tag);
                    
                    _pickedColor = EditorGUI.ColorField( new Rect( 
                        ((windowWidth - (_margin * 2) - (_innerMargin * 2)) / 2) + _buttonMargin + _margin + _innerMargin,
                        currentWritingHeight + _innerMargin,
                        ((windowWidth - (_margin * 2) - (_innerMargin * 2)) / 2) - _buttonMargin,
                        _tagStringHeight), _pickedColor);
                    tag.SetColor(_pickedColor);

                    tag.Description = EditorGUI.TextArea(new Rect(0 + _margin + _innerMargin, currentWritingHeight + _innerMargin + _tagStringHeight + _buttonMargin,
                        (windowWidth - (_margin * 2) - (_innerMargin * 2)), _tagDescriptionHeight), tag.Description);

                    float width = (windowWidth - _margin * 2) / _buttonPerRow;

                    if (GUI.Button(new Rect(width * 3 + _margin, currentWritingHeight + _tagStringHeight + _innerMargin * 2 + _buttonMargin + _tagDescriptionHeight, width - (_buttonMargin / (_buttonPerRow - 1)), _buttonHeight), "Delete"))
                    {
                        StickyNotesManager.UnregisterTag(tag);
                    }

                    currentWritingHeight += _buttonMargin + _tagHeight;
                }
            }
            catch (InvalidOperationException)
            { }

            GUI.EndScrollView();
        }
	}
}