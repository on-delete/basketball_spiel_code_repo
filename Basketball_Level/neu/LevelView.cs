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
	//0
	public AudioClip censoredBeep;
	//1
	public AudioClip beepPing;
	//2
	public AudioClip success;
	//3
	public AudioClip ballAufnehmen;
	//4
	public AudioClip ballWerfen;
	//5
	public AudioClip bitteAufstehen;
	//6
	public AudioClip biiteHinsetzen;
	//7

	private List<AudioClip> audioList = new List<AudioClip> ();
	private List<int> audioQueue = new List<int> ();

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
		audioList.Add (ballAufnehmen);
		audioList.Add (ballWerfen);
		audioList.Add (bitteAufstehen);
		audioList.Add (biiteHinsetzen);

		scoreText.text = "Treffer/Versuche: 0/0";
		timeText.text = "Zeit: 0s";

		StartCoroutine (audioQueueProcessor ());
	}

	public void playSound (int sound)
	{
		if (model.isSoundActivated) {
			if (source.isPlaying) {
				audioQueue.Add (sound);
			} else {
				source.PlayOneShot (audioList [sound]);
			}
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

	public void playerNotSitting ()
	{
		StartCoroutine (forcePlayerToSit ());
	}

	public void playerNotStanding ()
	{
		StartCoroutine (forcePlayerToStand ());
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

	IEnumerator audioQueueProcessor ()
	{
		while (true) {
			yield return new WaitForSeconds (0.5f);

			if (audioQueue.Count != 0 && source.isPlaying == false) {
				int actualSound = audioQueue [0];
				audioQueue.RemoveAt (0);
				source.PlayOneShot (audioList [actualSound]);
			}
		}
	}

	IEnumerator forcePlayerToSit ()
	{
		playSound (7);
		while (model.Posture != LevelModel.BodyPose.Sitting) {
			yield return new WaitForSeconds (0.5f);
		}

		levelController.notify ("grab.ball");

		yield return null;
	}

	IEnumerator forcePlayerToStand ()
	{
		playSound (6);
		while (model.Posture != LevelModel.BodyPose.Standing) {
			yield return new WaitForSeconds (0.5f);
		}

		levelController.notify ("throw.ball");

		yield return null;
	}
}
