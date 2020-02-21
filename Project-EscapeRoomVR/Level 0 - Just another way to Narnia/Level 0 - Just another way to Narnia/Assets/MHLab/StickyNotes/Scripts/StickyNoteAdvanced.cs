using UnityEngine;
using UnityEngine.UI;

namespace MHLab.StickyNotes
{
    [ExecuteInEditMode]
	public class StickyNoteAdvanced : StickyNote
	{
		public Camera MainCamera;
        
		public Canvas MainCanvas;

		public Image MainBackground;
		public Image MainBorder;
		public Text MainText;

		public Image OpenButtonBackground;
		public Image CloseButtonBackground;

		public RectTransform ExpandedSection;

		public Image DescriptionBackground;
		public Image DescriptionBorder;
		public Text DescriptionText;

		public GameObject GoToTargetButton;

		public GameObject AuthorSection;
		public Text AuthorText;
        
		protected override void Start()
		{
			if(MainCamera == null) MainCamera = Camera.allCameras[0];
			ExpandedSection.gameObject.SetActive(false);
			base.Start();
		    DrawGizmo = false;
		}

	    public override void Initialize()
	    {
	        base.Initialize();

	        MainCanvas.worldCamera = MainCamera;
	        SetColors();
	        SetText();
	        SetUI();
	    }

        protected void SetText()
		{
			MainText.text = Title;

			DescriptionText.text = Description;

			AuthorText.text = Author;
		}

		protected void SetColors()
		{
		    var tag = StickyNotesManager.GetTag(this.Tag);
		    if (tag == null) return;

			MainBackground.color = tag.MainColor;
			MainBorder.color = tag.SecondaryColor;
			MainText.color = tag.SecondaryColor;

			OpenButtonBackground.color = tag.SecondaryColor;
			CloseButtonBackground.color = tag.SecondaryColor;

			DescriptionBackground.color = tag.MainColor;
			DescriptionBorder.color = tag.SecondaryColor;
			DescriptionText.color = tag.SecondaryColor;
		}

		protected void SetUI()
		{
			GoToTargetButton.SetActive(Target != null);

			DescriptionBackground.gameObject.SetActive(!string.IsNullOrEmpty(DescriptionText.text));

			AuthorSection.SetActive(!string.IsNullOrEmpty(AuthorText.text));

			if (string.IsNullOrEmpty(Author) && Target == null && string.IsNullOrEmpty(Description))
			{
				CloseButtonBackground.gameObject.SetActive(false);
				OpenButtonBackground.gameObject.SetActive(false);
			}
			else
			{
				CloseButtonBackground.gameObject.SetActive(ExpandedSection.gameObject.activeSelf);
				OpenButtonBackground.gameObject.SetActive(!ExpandedSection.gameObject.activeSelf);
			}
		}

		public void ToggleExpandedSection()
		{
			var status = ExpandedSection.gameObject.activeSelf;

			if (status)
			{
				CloseButtonBackground.gameObject.SetActive(false);
				OpenButtonBackground.gameObject.SetActive(true);
			}
			else
			{
				CloseButtonBackground.gameObject.SetActive(true);
				OpenButtonBackground.gameObject.SetActive(false);
			}

			ExpandedSection.gameObject.SetActive(!status);
		}

		public void MoveCameraOnTarget()
		{
			MainCamera.transform.LookAt(Target);
			MainCamera.transform.position = Vector3.MoveTowards(MainCamera.transform.position, Target.position, 10f);
		}
	}
}