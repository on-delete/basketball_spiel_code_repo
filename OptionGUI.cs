using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class OptionGUI : MonoBehaviour
{

	[Tooltip ("GUI-Window rectangle in screen coordinates (pixels).")]
	private Rect guiWindowRect = new Rect (-180, 100, 140, 220);

	[Tooltip ("Whether the window is currently invisible or not.")]
	public bool hiddenWindow = false;

	private bool hideWindowClicked = false;
	private bool isSoundActivated = true;
	private  int selected = 0;
	string[] options = new string[] {
		"Level 1", "Level 2",
	};

	private string label1Text = string.Empty;
	private string label2Text = string.Empty;

	private LevelModel model;

	public LevelModel SetModel {
		set {
			model = value;
		}
	}

	private void ShowGuiWindow (int windowID)
	{
		GUILayout.BeginVertical ();

		GUILayout.Space (30);

		isSoundActivated = GUILayout.Toggle (isSoundActivated, "Sounds aktivieren");
		model.isSoundActivated = this.isSoundActivated;

		selected = GUILayout.SelectionGrid (selected, options, 1);
		model.PlayerLevel = selected + 1;

		GUILayout.Space (10);

		if (GUILayout.Button ("Übung beenden")) {
			Destroy (GameObject.Find ("KinectManager"));
			SceneManager.LoadScene (0, LoadSceneMode.Single);
		}

		GUILayout.Label (label1Text);

		hideWindowClicked = GUILayout.Button ("Optionen ausblenden");
		if (hideWindowClicked) {
			//label2Text = "Hiding options window...";
			HideWindow (hideWindowClicked);
		}
		
		GUILayout.Label (label2Text);
		GUILayout.EndVertical ();
		
		// Make the window draggable.
		GUI.DragWindow ();
	}

	
	void OnGUI ()
	{
		if (!hiddenWindow) {
			Rect windowRect = guiWindowRect;
			if (windowRect.x < 0)
				windowRect.x += Screen.width;
			if (windowRect.y < 0)
				windowRect.y += Screen.height;
			
			guiWindowRect = GUI.Window (1, windowRect, ShowGuiWindow, "Options");
		}
	}

	// hide options window
	private void HideWindow (bool hideWin)
	{
		if (hideWin) {
			hiddenWindow = true;
		}
	}
}
