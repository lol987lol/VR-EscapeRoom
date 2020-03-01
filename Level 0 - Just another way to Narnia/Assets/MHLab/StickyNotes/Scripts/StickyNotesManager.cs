using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MHLab.StickyNotes
{
	public class StickyNotesManager
	{
		private static Dictionary<string, StickyNoteTag> _tags = new Dictionary<string, StickyNoteTag>();
		private static readonly Dictionary<GUID, StickyNote> _notes = new Dictionary<GUID, StickyNote>();

	    private static bool _alreadyLoaded = false;
	    private static bool _isInitialized = false;

		public static int Count
		{
			get { return _notes.Count; }
		}

		public static Dictionary<GUID, StickyNote> Notes
		{
			get { return _notes; }
		}

	    public static StickyNote[] NotesArray
	    {
	        get { return _notes.Values.ToArray(); }
	    }

        public static Dictionary<string, StickyNoteTag> Tags
	    {
	        get { return _tags; }
	    }

		public static void RegisterNote(StickyNote note)
		{
			if (_notes.ContainsKey(note.Id))
				return;

			_notes.Add(note.Id, note);
		}

		public static void RegisterTag(StickyNoteTag tag)
		{
		    if (string.IsNullOrEmpty(tag.Tag)) return;

			if (_tags.ContainsKey(tag.Tag))
				_tags[tag.Tag] = tag;
			else
				_tags.Add(tag.Tag, tag);

            SaveTags();
		}

		public static void UnregisterNote(StickyNote note)
		{
			if (_notes.ContainsKey(note.Id))
			{
				_notes.Remove(note.Id);
			}
		}

		public static void UnregisterTag(StickyNoteTag tag)
		{
		    if (_tags.ContainsKey(tag.Tag))
		    {
		        foreach (var stickyNote in Notes)
		        {
		            var note = stickyNote.Value;
		            if (note.Tag == tag.Tag)
		                note.Tag = "Root";
		        }
		        _tags.Remove(tag.Tag);
		    }

		    SaveTags();
        }

		public static string[] GetTags()
		{
		    if (_tags.Count == 0)
		    {
		        LoadTags();
		    }

            return _tags.Keys.ToArray();
		}

	    public static StickyNoteTag[] GetTagsArray()
	    {
	        if (_tags.Count == 0)
	        {
	            LoadTags();
	        }

	        return _tags.Values.ToArray();
        }

		public static StickyNoteTag GetTag(string key)
		{
		    if (_tags.Count == 0)
		    {
                LoadTags();
		    }

			if (_tags.ContainsKey(key))
			{
				return _tags[key];
			}

		    return null;
		}

	    public static void LoadTagsFromFile()
	    {
	        if (!File.Exists("StickyNotesDatabase/tags.json"))
	        {
	            SaveTags();
	        }

            var content = File.ReadAllText("StickyNotesDatabase/tags.json");
	        var tags = JsonUtility.FromJson<StickyNoteTagsSerializable>(content);

	        foreach (var stickyNoteTag in tags.Tags)
	        {
	            if (string.IsNullOrEmpty(stickyNoteTag.Tag) || stickyNoteTag.Tag.Trim() == string.Empty) continue;

	            stickyNoteTag.SetColor(stickyNoteTag.MainColor);
	            if (_tags.ContainsKey(stickyNoteTag.Tag))
	                _tags[stickyNoteTag.Tag] = stickyNoteTag;
                else
	                _tags.Add(stickyNoteTag.Tag, stickyNoteTag);
	        }
        }

	    public static void LoadTags(bool forced = false)
		{
		    if (_alreadyLoaded && !forced) return;

            _tags.Clear();

		    if (!_tags.ContainsKey("Root"))
		    {
		        var tag = new StickyNoteTag()
		        {
		            Description = "The root tag",
		            Tag = "Root"
		        };
		        tag.SetColor(Color.white);

		        _tags.Add("Root", tag);
		    }

            if (!File.Exists("StickyNotesDatabase/tags.json"))
			{
			    SaveTags();
            }

		    LoadTagsFromFile();

            _alreadyLoaded = true;
		}

		public static void SaveTags()
		{
			Directory.CreateDirectory("StickyNotesDatabase/");
		    var tags = _tags.Values.ToArray();

            var serializable = new StickyNoteTagsSerializable();
		    serializable.Tags = tags;

            var content = JsonUtility.ToJson(serializable, true);
			File.WriteAllText("StickyNotesDatabase/tags.json", content);

            LoadTags();
		}

	    public static void Initialize()
	    {
            Debug.Log(Path.Combine(Application.dataPath, "MHLab/StickyNotes/Textures/stickynotes_gizmo_icon.png"));
	        if (!File.Exists(Path.Combine(Application.dataPath, "Gizmos/stickynotes_gizmo_icon.png")))
	        {
	            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Gizmos"));
                File.Copy(Path.Combine(Application.dataPath, "MHLab/StickyNotes/Textures/stickynotes_gizmo_icon.png"), Path.Combine(Application.dataPath, "Gizmos/stickynotes_gizmo_icon.png"));
                AssetDatabase.Refresh();
	        }

	        _isInitialized = true;
	    }
	}
}
