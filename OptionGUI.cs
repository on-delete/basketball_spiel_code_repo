using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class OptionGUI : MonoBehaviour
{

	private Rect guiWindowRect = new Rect (-180, 100, 140, 280);

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

		if (GUILayout.Button ("Testfall 1")) {
			model.PlayerData = createTestPlayerData (1);
		}

		if (GUILayout.Button ("Testfall 2")) {
			model.PlayerData = createTestPlayerData (2);
		}

		if (GUILayout.Button ("Testfall 3")) {
			model.PlayerData = createTestPlayerData (3);
		}

		GUILayout.Label (label1Text);

		hideWindowClicked = GUILayout.Button ("Optionen ausblenden");
		if (hideWindowClicked) {
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

	private PlayerData createTestPlayerData (int testCase)
	{
		PlayerData data = new PlayerData ();

		if (testCase == 1) {
			data.level = 1;
			model.PlayerLevel = 1;
			
		} else if (testCase == 2) {
			data.level = 1;
			model.PlayerLevel = 1;

			ExerciseData exerciseData1 = new ExerciseData ();
			exerciseData1.attempts = 0;
			exerciseData1.score = 0;
			exerciseData1.countHits = 0;
			exerciseData1.timeStamp = System.DateTime.Now;

			ExerciseData exerciseData2 = new ExerciseData ();
			exerciseData2.attempts = 0;
			exerciseData2.score = 0;
			exerciseData2.countHits = 0;
			exerciseData2.timeStamp = System.DateTime.Now;

			data.exerciseList.Add (exerciseData1);
			data.exerciseList.Add (exerciseData2);
		} else {
			data.level = 2;
			model.PlayerLevel = 2;

			ExerciseData exerciseData1 = new ExerciseData ();
			exerciseData1.attempts = 0;
			exerciseData1.score = 9999999;
			exerciseData1.countHits = 0;
			exerciseData1.timeStamp = System.DateTime.Now;

			ExerciseData exerciseData2 = new ExerciseData ();
			exerciseData2.attempts = 0;
			exerciseData2.score = 9999999;
			exerciseData2.countHits = 0;
			exerciseData2.timeStamp = System.DateTime.Now;

			data.exerciseList.Add (exerciseData1);
			data.exerciseList.Add (exerciseData2);
		}

		return data;
	}
}
