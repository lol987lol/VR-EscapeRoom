using System;
using UnityEngine;

namespace MHLab.StickyNotes
{
    [Serializable]
    public class StickyNoteTag
	{
		public string Tag;
		public Color MainColor;
		public Color SecondaryColor;
		public string Description;

	    public void SetColor(Color c)
	    {
	        MainColor = new Color(c.r, c.g, c.b);
	        SecondaryColor = CalculateDarkenColor(c, 60);
	    }

	    protected Color CalculateDarkenColor(Color original, int percentage)
	    {
	        float r = original.r - ((original.r * percentage) / 100);
	        float g = original.g - ((original.g * percentage) / 100);
	        float b = original.b - ((original.b * percentage) / 100);

	        return new Color(r, g, b, 1f);
	    }
    }

    [Serializable]
    public class StickyNoteTagsSerializable
    {
        public StickyNoteTag[] Tags;
    }
}
