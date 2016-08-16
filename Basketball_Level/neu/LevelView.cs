using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelView : MonoBehaviour, ILevelView
{
	private LevelController2 levelController;
	private LevelModel model;

	public LevelController2 SetLevelController {
		set {
			levelController = value;
		}
	}

	public LevelModel SetModel {
		set {
			model = value;
		}
	}

	public GameObject basketballPrefab;
	public GameObject endScorePanel;
	public GameObject leftBasket;
	public GameObject rightBasket;
	public GUIText timeText;
	public GUIText startTimerText;
	public GUIText scoreText;
	public GUIText endScoreText;

	public AudioClip shoortBeep;
	public AudioClip censoredBeep;
	public AudioClip beepPing;
	public AudioClip success;

	List<AudioClip> audioList = new List<AudioClip> ();

	private AudioSource source;

	private int time;
	private int maxTime = 60;

	private Coroutine startTimerRoutine = null;
	private Coroutine timerRoutine = null;

	void Start ()
	{
		source = GetComponent<AudioSource> ();

		audioList.Add (shoortBeep);
		audioList.Add (censoredBeep);
		audioList.Add (beepPing);
		audioList.Add (success);

		scoreText.text = "0/0";
		timeText.text = "Zeit: 0s";
	}

	public void playSound (int sound)
	{
		if (model.isSoundActivated) {
			source.PlayOneShot (audioList [sound]);
		}
	}

	public void setStartTimerText (string text)
	{
		startTimerText.text = text;
	}

	public void setTimerText (string text)
	{
		timeText.text = text;
	}

	public void setScoreText (string text)
	{
		scoreText.text = text;
	}

	public void setEndScoreText (string text)
	{
		endScorePanel.SetActive (true);
		endScoreText.text = text;
	}

	public void instantiateRightBasket ()
	{
		leftBasket.SetActive (false);
		rightBasket.SetActive (true);
		Instantiate (basketballPrefab, new Vector3 (0.24f, 0.52f, 2.21f), Quaternion.Euler (0.0f, 0.0f, 0.0f));
		model.BallPosition = LevelModel.ObjectPosition.Right;
	}

	public void instantiateLeftBasket ()
	{
		leftBasket.SetActive (true);
		rightBasket.SetActive (false);
		Instantiate (basketballPrefab, new Vector3 (-0.24f, 0.52f, 2.21f), Quaternion.Euler (0.0f, 0.0f, 0.0f));
		model.BallPosition = LevelModel.ObjectPosition.Left;
	}

	public void startGame ()
	{
		if (startTimerRoutine == null) {
			Debug.Log ("Start Gesture Detected!");
			startTimerRoutine = StartCoroutine (startTimer ());
		}
	}

	public void stopGame ()
	{
		if (timerRoutine != null) {
			StopCoroutine (timerRoutine);
			timerRoutine = null;
		}
	}

	IEnumerator startTimer ()
	{
		for (int i = 3; i > 0; i--) {
			yield return new WaitForSeconds (1);

			playSound (0);

			setStartTimerText ("" + i);
		}

		yield return new WaitForSeconds (1);

		setStartTimerText ("LOS");

		playSound (1);

		yield return new WaitForSeconds (2);

		setStartTimerText ("");

		timerRoutine = StartCoroutine (timer ());

		levelController.notify ("grab.ball");

		yield return null;
	}

	IEnumerator timer ()
	{
		while (true) {
			yield return new WaitForSeconds (1);

			string timeText = "Zeit: " + ++time + "s";

			setTimerText (timeText);

			if (time == maxTime) {
				levelController.notify ("time.up");
			}
		}
	}
}
