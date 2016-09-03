using UnityEngine;
using System.Collections;

public class LevelModel
{

	private BodyPose posture;

	public BodyPose Posture {
		get {
			return posture;
		}
		set {
			posture = value;
		}
	}

	public enum BodyPose
	{
		Unknown = 0,
		Sitting,
		Standing,
	}

	private ObjectPosition ballPosition;

	public ObjectPosition BallPosition {
		get {
			return ballPosition;
		}
		set {
			ballPosition = value;
		}
	}

	public enum ObjectPosition
	{
		Left = 0,
		Right,
	}

	private bool ballGrabbed;

	public bool isBallGrabbed {
		get {
			return ballGrabbed;
		}
		set {
			ballGrabbed = value;
		}
	}

	private bool ballThrown;

	public bool isBallThrown {
		get {
			return ballThrown;
		}
		set {
			ballThrown = value;
		}
	}

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
		WaitForStartGesture,
		GrabBall,
		ThrowBall,
		LevelFinished,
	};

	private int playerLevel;

	public int PlayerLevel {
		get {
			return playerLevel;
		}
		set {
			playerLevel = value;
		}
	}

	private PlayerData playerData;

	public PlayerData PlayerData {
		get {
			return playerData;
		}
		set {
			playerData = value;
		}
	}

	private bool soundActivated;

	public bool isSoundActivated {
		get {
			return soundActivated;
		}
		set {
			soundActivated = value;
		}
	}
}
