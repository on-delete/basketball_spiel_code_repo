using UnityEngine;
using System.Collections;
using System;

public class LevelController : MonoBehaviour
{

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
	private AudioSource source;
	private bool soundActivated;

	private PlayerData playerData;
	private bool ballGrabbed;
	private bool ballThrown;
	private bool ballHitTarget;
	private int countHits;
	private int attempts;
	private int time;
	private Coroutine startTimerRoutine = null;
	private Coroutine timerRoutine = null;

	private KinectManager kinectManager;

	public bool isBallGrabbed {
		get {
			return ballGrabbed;
		}
		set {
			ballGrabbed = value;
		}
	}

	public bool BallHitTarget {
		get {
			return ballHitTarget;
		}
		set {
			ballHitTarget = value;
		}
	}

	public bool isBallThrown {
		get {
			return ballThrown;
		}
		set {
			ballThrown = value;
		}
	}

	private BodyAnalyzer bodyAnalzyer;

	private LevelStatus levelStatus;

	public LevelStatus lStatus {
		get {
			return levelStatus;
		}
		set {
			levelStatus = value;
		}
	}

	public enum LevelStatus
	{
		None = 0,
		GrabBall,
		ThrowBall,
		ThrowComplete,
		LevelFinished,
		TimeUp,
	};

	protected static LevelController instance = null;

	public static LevelController Instance {
		get {
			return instance;
		}
	}

	private ObjectPosition ballPosition;

	public ObjectPosition BallPosition {
		get {
			return ballPosition;
		}
	}

	public enum ObjectPosition
	{
		Left = 0,
		Right,
	}

	private int playerLevel;

	public int PlayerLevel {
		get {
			return playerLevel;
		}
	}


	// Use this for initialization
	void Start ()
	{
		countHits = 0;
		attempts = 0;
		ballThrown = false;
		ballHitTarget = false;
		instance = this;
		bodyAnalzyer = BodyAnalyzer.Instance;
		levelStatus = LevelStatus.None;
		source = GetComponent<AudioSource> ();
		soundActivated = true;

		playerData = LoadSaveController.Load ();
		playerLevel = playerData.level;

		scoreText.text = "0/0";
		timeText.text = "Zeit: 0s";
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (kinectManager == null) {
			kinectManager = KinectManager.Instance;
		}
		if (levelStatus != LevelStatus.LevelFinished) {
			if (levelStatus == LevelStatus.None) {
				if (bodyAnalzyer.isStartGestureDetected () && bodyAnalzyer.Posture == BodyAnalyzer.BodyPose.Sitting) {
					Debug.Log ("Start Gesture Detected!");
					if (startTimerRoutine == null) {
						startTimerRoutine = StartCoroutine (startTimer ());
					}
				}
			} else if (levelStatus == LevelStatus.GrabBall) {
				if (ballGrabbed) {
					playSound (beepPing);
					levelStatus = LevelStatus.ThrowBall;
				}
			} else if (levelStatus == LevelStatus.ThrowBall) {
				//TODO: Akustische Meldung Werfen
				if (ballThrown) {
					levelStatus = LevelStatus.ThrowComplete;
				}
			} else if (levelStatus == LevelStatus.ThrowComplete) {
				if (ballHitTarget) {
					countHits++;
					playSound (success);
				}
				attempts++;

				scoreText.text = countHits + "/" + attempts;

				if (ballPosition == ObjectPosition.Left) {
					leftBasket.SetActive (false);
					rightBasket.SetActive (true);
					Instantiate (basketballPrefab, new Vector3 (0.24f, 0.52f, 2.21f), Quaternion.Euler (0.0f, 0.0f, 0.0f));
					ballPosition = ObjectPosition.Right;
				} else {
					leftBasket.SetActive (true);
					rightBasket.SetActive (false);
					Instantiate (basketballPrefab, new Vector3 (-0.24f, 0.52f, 2.21f), Quaternion.Euler (0.0f, 0.0f, 0.0f));
					ballPosition = ObjectPosition.Left;
				}
				ballThrown = false;
				ballHitTarget = false;
				ballGrabbed = false;

				playSound (beepPing);

				levelStatus = LevelStatus.GrabBall;
			
			} else if (levelStatus == LevelStatus.TimeUp) {
				StopCoroutine (timerRoutine);

				kinectManager.ClearKinectUsers ();
				kinectManager.avatarControllers.Clear ();

				GameObject ball = GameObject.FindGameObjectWithTag ("basketball");
				if (ball != null) {
					Destroy (ball);
				}
				Debug.Log ("Level finished!");

				decimal score = calculateScore ();
				int newLevel = selectLevel (score);



				endScorePanel.SetActive (true);
				endScoreText.text = "Treffer: " + countHits + "\n" +
				"Versuche: " + attempts + "\n" +
				"Endstand: " + score;

				if (newLevel > playerLevel) {
					endScoreText.text += "\n" + "Level aufgestiegen!";
				} else if (newLevel < playerLevel) {
					endScoreText.text += "\n" + "Level abgestiegen!";
				}

				LoadSaveController.Save (0, countHits, attempts, score);

				levelStatus = LevelStatus.LevelFinished;
			}
		}
	}

	IEnumerator timer ()
	{
		while (true) {
			yield return new WaitForSeconds (1);
			timeText.text = "Zeit: " + ++time + "s";

			if (time == 120) {
				Debug.Log ("test");
				levelStatus = LevelStatus.TimeUp;
			}
		}
	}

	IEnumerator startTimer ()
	{
		for (int i = 3; i > 0; i--) {
			yield return new WaitForSeconds (1);

			playSound (shoortBeep);

			startTimerText.text = "" + i;
		}

		yield return new WaitForSeconds (1);

		startTimerText.text = "LOS";

		playSound (censoredBeep);

		yield return new WaitForSeconds (2);

		startTimerText.text = "";

		playSound (beepPing);

		levelStatus = LevelStatus.GrabBall;
		rightBasket.SetActive (true);
		Instantiate (basketballPrefab, new Vector3 (0.24f, 0.52f, 2.21f), Quaternion.Euler (0.0f, 0.0f, 0.0f));
		ballPosition = ObjectPosition.Right;

		timerRoutine = StartCoroutine (timer ());

		yield return null;
	}

	private void playSound (AudioClip sound)
	{
		if (soundActivated) {
			source.PlayOneShot (sound);
		}
	}

	private decimal calculateScore ()
	{
		if (attempts > 0) {
			return Math.Round ((decimal)(((countHits / attempts) * 2 + attempts) * 100));
		} else {
			return 0;
		}
	}

	private int selectLevel (decimal score)
	{
		if (playerData.exerciseList.Count > 1) {
			int level = 0;
			if (playerLevel == 1 && playerData.exerciseList [playerData.exerciseList.Count - 1].score < score && playerData.exerciseList [playerData.exerciseList.Count - 2].score < score) {
				level = 2;
			} else {
				level = 1;
			}
			if (playerLevel == 2 && playerData.exerciseList [playerData.exerciseList.Count - 1].score > score && playerData.exerciseList [playerData.exerciseList.Count - 2].score > score) {
				level = 1;
			} else {
				level = 2;
			}

			return level;
		} else {
			return playerLevel;
		}
	}
}
