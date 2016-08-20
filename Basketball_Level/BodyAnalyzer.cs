using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using CircularBuffer;

public class BodyAnalyzer : MonoBehaviour
{

	public GameObject app;
	private LevelModel model;
	private LevelController levelController;
	private PlayerController playerController;
	private LevelView levelView;

	public LevelModel SetModel {
		set {
			model = value;
		}
	}

	public LevelController SetLevelController {
		set {
			levelController = value;
		}
	}

	public PlayerController SetPlayerController {
		set {
			playerController = value;
		}
	}

	public LevelView SetLevelView {
		set {
			levelView = value;
		}
	}

	private KinectManager kinectManager;

	private Dictionary<string, Vector3> bodyJoints = new Dictionary<string, Vector3> ();

	private KinectInterop.HandState lastLeftHandState;
	private KinectInterop.HandState lastRightHandState;
	private LevelModel.BodyPose lastPosture;
	private int isThrown;
	private float throwAngle = Mathf.PI / 3;

	private Vector3 currentHandPosition;
	private Vector3 lastHandPosition = new Vector3 (9999, 9999, 9999);
	private float lastLengthInitToActualPosition;
	private float minThrowVelocity = 0.5f;
	private List<float> velocityList = new List<float> ();

	private LevelModel.LevelStatus lStatus;
	private LevelModel.ObjectPosition ballPosition;
	private int playerLevel;

	private float lastDistanceHandToTarget;

	private Coroutine freezingRoutine;

	private PostureSatus lastPostureStatus;

	private enum PostureSatus
	{
		postureCorrect = 0,
		postureFail,
	}

	// Use this for initialization
	void Start ()
	{
		isThrown = 0;
		lastPosture = LevelModel.BodyPose.Unknown;
	}

	// Update is called once per frame
	void Update ()
	{
		if (kinectManager == null) {
			kinectManager = KinectManager.Instance;
		}/* else if (model == null) {
			model = app.GetComponent<App> ().LevelModel;
		}*/ else {
			if (kinectManager.GetUsersCount () > 0) {
				long userId = kinectManager.GetAllUserIds () [0];
				lStatus = model.lStatus;
				ballPosition = model.BallPosition;
				playerLevel = model.PlayerLevel;

				getBodyJoints (userId);

				model.Posture = getLastPosture (bodyJoints ["hipLeftPos"], bodyJoints ["hipRightPos"], bodyJoints ["kneeLeftPos"], bodyJoints ["kneeRightPos"], bodyJoints ["footLeftPos"], bodyJoints ["footRightPos"], bodyJoints ["shoulderLeftPos"], bodyJoints ["shoulderRightPos"]);
				if (model.Posture != LevelModel.BodyPose.Unknown) {
					lastPosture = model.Posture;
				}

				if (lStatus == LevelModel.LevelStatus.WaitForStartGesture) {
					if (isStartGestureDetected (userId)) {
						levelController.notify ("start.gesture.detected");
					}
				} else {
					if (lStatus == LevelModel.LevelStatus.GrabBall || lStatus == LevelModel.LevelStatus.ThrowBall) {
						if (freezingRoutine == null) {
							freezingRoutine = StartCoroutine (checkFreezing ());
						}
					} else {
						if (freezingRoutine != null) {
							StopCoroutine (freezingRoutine);
							freezingRoutine = null;
						}
					}

					int progress = 0;
					if (lStatus == LevelModel.LevelStatus.GrabBall) {
						progress = calculateProgress ();
					}

					if (lStatus != LevelModel.LevelStatus.None && lStatus != LevelModel.LevelStatus.LevelFinished && model.Posture == LevelModel.BodyPose.Sitting && progress == 0) {
						checkPisaPosture (calculateDriftAngleToSight (bodyJoints ["spineBasePos"], bodyJoints ["spineMidPos"], bodyJoints ["spineShoulderPos"], bodyJoints ["neckPos"]), calculateDriftAngleToFrontAndBack (bodyJoints ["spineBasePos"], bodyJoints ["spineMidPos"], bodyJoints ["spineShoulderPos"], bodyJoints ["neckPos"]));
					}

					if (lStatus == LevelModel.LevelStatus.ThrowBall) {
						/*if (playerLevel == 1) {
							if (model.Posture == LevelModel.BodyPose.Sitting) {
								calculateBallVelocity ();
							}
						} else {
							if (model.Posture == LevelModel.BodyPose.Standing) {
								calculateBallVelocity ();
							}
						}*/
						calculateBallVelocity ();
					}
				}
			}
		}
	}

	public bool isStartGestureDetected (long userId)
	{

		KinectInterop.HandState rightHandState = kinectManager.GetRightHandState (userId);
		KinectInterop.HandState leftHandState = kinectManager.GetLeftHandState (userId);

		if (rightHandState != KinectInterop.HandState.NotTracked ||
		    rightHandState != KinectInterop.HandState.Unknown ||
		    leftHandState != KinectInterop.HandState.NotTracked ||
		    leftHandState != KinectInterop.HandState.Unknown) {
			if (lastLeftHandState == KinectInterop.HandState.Unknown) {
				if (leftHandState == KinectInterop.HandState.Open) {
					lastLeftHandState = KinectInterop.HandState.Open;
					return false;
				}
			} else if (lastLeftHandState == KinectInterop.HandState.Open) {
				if (leftHandState == KinectInterop.HandState.Closed) {
					lastLeftHandState = KinectInterop.HandState.Closed;
					return true;
				}
			}
			if (lastRightHandState == KinectInterop.HandState.Unknown) {
				if (rightHandState == KinectInterop.HandState.Open) {
					lastRightHandState = KinectInterop.HandState.Open;
					return false;
				}
			} else if (lastRightHandState == KinectInterop.HandState.Open) {
				if (rightHandState == KinectInterop.HandState.Closed) {
					lastRightHandState = KinectInterop.HandState.Closed;
					return true;
				}
			}
		} else {
			return false;
		}

		return false;
	}

	private void getBodyJoints (long userId)
	{
		bodyJoints.Clear ();

		bodyJoints.Add ("hipsPos", getJointPosition (userId, KinectInterop.JointType.SpineBase));
		bodyJoints.Add ("kneeLeftPos", getJointPosition (userId, KinectInterop.JointType.KneeLeft));
		bodyJoints.Add ("kneeRightPos", getJointPosition (userId, KinectInterop.JointType.KneeRight));
		bodyJoints.Add ("footLeftPos", getJointPosition (userId, KinectInterop.JointType.AnkleLeft));
		bodyJoints.Add ("footRightPos", getJointPosition (userId, KinectInterop.JointType.AnkleRight));
		bodyJoints.Add ("shoulderRightPos", getJointPosition (userId, KinectInterop.JointType.ShoulderRight));
		bodyJoints.Add ("shoulderLeftPos", getJointPosition (userId, KinectInterop.JointType.ShoulderLeft));
		bodyJoints.Add ("hipRightPos", getJointPosition (userId, KinectInterop.JointType.HipRight));
		bodyJoints.Add ("hipLeftPos", getJointPosition (userId, KinectInterop.JointType.HipLeft));
		bodyJoints.Add ("handRightPos", getJointPosition (userId, KinectInterop.JointType.HandRight));
		bodyJoints.Add ("handLeftPos", getJointPosition (userId, KinectInterop.JointType.HandLeft));
		bodyJoints.Add ("spineShoulderPos", getJointPosition (userId, KinectInterop.JointType.SpineShoulder));
		bodyJoints.Add ("neckPos", getJointPosition (userId, KinectInterop.JointType.Neck));
		bodyJoints.Add ("spineBasePos", getJointPosition (userId, KinectInterop.JointType.SpineBase));
		bodyJoints.Add ("spineMidPos", getJointPosition (userId, KinectInterop.JointType.SpineMid));
		bodyJoints.Add ("elbowRightPos", getJointPosition (userId, KinectInterop.JointType.ElbowRight));
		bodyJoints.Add ("elbowLeftPos", getJointPosition (userId, KinectInterop.JointType.ElbowLeft));
	}

	private void calculateBallVelocity ()
	{
		GameObject ball = GameObject.FindGameObjectWithTag ("basketball");
		if (ball != null && ball.GetComponent<BallController> ().BallTriggered == true) {

			Vector3 shoulderPos;
			Vector3 elbowPos;

			if (ballPosition == LevelModel.ObjectPosition.Left) {
				shoulderPos = bodyJoints ["shoulderLeftPos"];
				elbowPos = bodyJoints ["elbowLeftPos"];
				currentHandPosition = bodyJoints ["handLeftPos"];
			} else {
				shoulderPos = bodyJoints ["shoulderRightPos"];
				elbowPos = bodyJoints ["elbowRightPos"];
				currentHandPosition = bodyJoints ["handRightPos"];
			}
			if (shoulderPos.y < currentHandPosition.y) {

				if (lastHandPosition == new Vector3 (9999, 9999, 9999)) {
					lastHandPosition = currentHandPosition;
				}

				//Debug.Log ("current hand right: " + currentRightHandPosition.x + " " + currentRightHandPosition.y + " " + currentRightHandPosition.z);
				//Debug.Log ("last hand right: " + lastRightHandPosition.x + " " + lastRightHandPosition.y + " " + lastRightHandPosition.z);

				if (currentHandPosition.y > lastHandPosition.y && currentHandPosition.z < lastHandPosition.z) {

					float handVelocity = (currentHandPosition.y - lastHandPosition.y) / Time.deltaTime;
					Debug.Log ("Test: " + handVelocity);
					if (handVelocity > minThrowVelocity && handVelocity < 10) {
						velocityList.Add (handVelocity);
						isThrown = 1;
						//Debug.Log ("Test: " + handVelocity);
					}
				}

				if (isThrown == 1) {
					if (/*handVelocity < minThrowVelocity &&*/ calculateArmDrift (shoulderPos, elbowPos, currentHandPosition) > 150) {
						isThrown = 2;
					}
				}

				if (isThrown == 2) {
					isThrown = 0;

					GameObject target = GameObject.FindGameObjectWithTag ("target");

					float velocityAddedValues = 0;

					for (int i = 0; i < velocityList.Count; i++) {
						if (velocityList [i] <= 10) {
							velocityAddedValues += velocityList [i];
						}
					}

					float avgVelocity = velocityAddedValues / velocityList.Count;

					Vector3 ballVelocity = new Vector3 (0, (avgVelocity * 2), (avgVelocity / Mathf.Tan (throwAngle)));

					ball.GetComponent<BallController> ().BallTriggered = false;
					ball.GetComponent<BallController> ().BallFlying = true;
					ball.transform.position = new Vector3 (0, ball.transform.position.y, ball.transform.position.z);

					Vector3 bestthrowspeed = calculateBestThrowSpeed (ball.transform.position, target.transform.position, 1.0f);
					//Debug.Log ("isThrown: " + isThrown);
					//Debug.Log ("Best throw speed: " + bestthrowspeed.x + " " + bestthrowspeed.y + " " + bestthrowspeed.z);
					//Debug.Log ("caluclated velocity: " + ballVelocity.x + " " + ballVelocity.y + " " + ballVelocity.z);

					ball.GetComponent<Rigidbody> ().useGravity = true;
					ball.GetComponent<Rigidbody> ().velocity = ballVelocity;
					//ball.GetComponent<Rigidbody> ().velocity = new Vector3 (0, bestthrowspeed.y, bestthrowspeed.z);

					velocityList.Clear ();
					lastHandPosition = new Vector3 (9999, 9999, 9999);
				}

				lastHandPosition = currentHandPosition;
			}
		}
	}

	private void checkPisaPosture (decimal driftAngleSight, decimal driftAngleFrontBack)
	{
		if (driftAngleSight > 10 || driftAngleSight < -10 || driftAngleFrontBack > 10 || driftAngleFrontBack < -10) {
			if (lastPostureStatus == PostureSatus.postureCorrect) {
				Debug.Log ("Neigung zu groß!");
				lastPostureStatus = PostureSatus.postureFail;
				playerController.PisaPostureFound ();

			}
		} else if (driftAngleSight <= 10 && driftAngleSight >= -10 && driftAngleFrontBack <= 10 && driftAngleFrontBack >= -10) {
			if (lastPostureStatus == PostureSatus.postureFail) {
				Debug.Log ("Gute Position!");
				lastPostureStatus = PostureSatus.postureCorrect;
				playerController.PisaPostureCorrected ();
			}
		}
	}

	private decimal calculateDriftAngleToSight (Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
	{
		return Math.Round ((decimal)(((calculateArcTan (p1.x, p1.y, p2.x, p2.y) * (p2.y - p1.y) + calculateArcTan (p2.x, p2.y, p3.x, p3.y) * (p3.y - p2.y) + calculateArcTan (p3.x, p3.y, p4.x, p4.y) * (p4.y - p3.y)) /
		((p2.y - p1.y) + (p3.y - p2.y) + (p4.y - p3.y))) * Mathf.Rad2Deg), 2);
	}

	private decimal calculateDriftAngleToFrontAndBack (Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
	{
		return Math.Round ((decimal)(((calculateArcTan (p1.z, p1.y, p2.z, p2.y) * (p2.y - p1.y) + calculateArcTan (p2.z, p2.y, p3.z, p3.y) * (p3.y - p2.y) + calculateArcTan (p3.z, p3.y, p4.z, p4.y) * (p4.y - p3.y)) /
		((p2.y - p1.y) + (p3.y - p2.y) + (p4.y - p3.y))) * Mathf.Rad2Deg), 2);
	}

	private float calculateArcTan (float p1_1, float p1_2, float p2_1, float p2_2)
	{
		return Mathf.Atan ((p2_1 - p1_1) / (p2_2 - p1_2));
	}

	private Vector3 calculateBestThrowSpeed (Vector3 origin, Vector3 target, float timeToTarget)
	{
		// calculate vectors
		Vector3 toTarget = target - origin;
		Vector3 toTargetXZ = toTarget;
		toTargetXZ.y = 0;

		// calculate xz and y
		float y = toTarget.y;
		float xz = toTargetXZ.magnitude;

		// calculate starting speeds for xz and y. Physics forumulase deltaX = v0 * t + 1/2 * a * t * t
		// where a is "-gravity" but only on the y plane, and a is 0 in xz plane.
		// so xz = v0xz * t => v0xz = xz / t
		// and y = v0y * t - 1/2 * gravity * t * t => v0y * t = y + 1/2 * gravity * t * t => v0y = y / t + 1/2 * gravity * t
		float t = timeToTarget;
		float v0y = y / t + 0.5f * Physics.gravity.magnitude * t;
		float v0xz = xz / t;

		// create result vector for calculated starting speeds
		Vector3 result = toTargetXZ.normalized;        // get direction of xz but with magnitude 1
		result *= v0xz;                                // set magnitude of xz to v0xz (starting speed in xz plane)
		result.y = v0y;                                // set y to v0y (starting speed of y plane)

		return result;
	}

	private Vector3 getJointPosition (long userId, KinectInterop.JointType jointType)
	{
		return kinectManager.GetJointPosition (userId, kinectManager.GetJointIndex (jointType));
	}

	private LevelModel.BodyPose getLastPosture (Vector3 hipsLeftPos, Vector3 hipsRightPos, Vector3 kneeLeftPos, Vector3 kneeRightPos, Vector3 footLeftPos, Vector3 footRightPos, Vector3 shoulderLeftPos, Vector3 shoulderRightPos)
	{
		decimal leftLegAngle = calculate3PointAngle (hipsLeftPos, kneeLeftPos, footLeftPos);
		decimal rightLegAngle = calculate3PointAngle (hipsRightPos, kneeRightPos, footRightPos);

		decimal leftBodyAngle = calculate3PointAngle (shoulderLeftPos, hipsLeftPos, footLeftPos);
		decimal rightBodyAngle = calculate3PointAngle (shoulderRightPos, hipsRightPos, footRightPos);

		//leftArmDriftText.text = "Left Leg Drift: " + leftLegAngle;
		//rightArmDriftText.text = "Right Leg Drift: " + rightLegAngle;

		if ((leftLegAngle > 140 && leftLegAngle < 180) || (rightLegAngle > 140 && rightLegAngle < 180)) {
			if (leftBodyAngle > 120 || rightBodyAngle > 120) {
				return LevelModel.BodyPose.Standing;
			} else {
				return LevelModel.BodyPose.Unknown;
			}
		} else if ((leftLegAngle < 100 && leftLegAngle > 20) || (rightLegAngle < 100 && rightLegAngle > 20)) {
			return LevelModel.BodyPose.Sitting;
		} else {
			return LevelModel.BodyPose.Unknown;
		}
	}

	private decimal calculate3PointAngle (Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float alpha = Mathf.Atan (Mathf.Abs (p2.y - p1.y) / Mathf.Abs (p2.z - p1.z)) * Mathf.Rad2Deg;
		float beta = Mathf.Atan (Mathf.Abs (p3.y - p2.y) / Mathf.Abs (p3.z - p2.z)) * Mathf.Rad2Deg;
		return Math.Round ((decimal)(alpha + beta), 2);
	}

	private decimal calculateArmDrift (Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 v1 = new Vector3 (p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
		Vector3 v2 = new Vector3 (p3.x - p2.x, p3.y - p2.y, p3.z - p2.z);

		float skalar = v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;

		float v1length = Vector3.Distance (p1, p2);
		float v2length = Vector3.Distance (p3, p2);

		//float v1length = Mathf.Sqrt (Mathf.Pow (v1.x, 2) + Mathf.Pow (v1.y, 2) + Mathf.Pow (v1.z, 2));
		//float v2length = Mathf.Sqrt (Mathf.Pow (v2.x, 2) + Mathf.Pow (v2.y, 2) + Mathf.Pow (v2.z, 2));

		return Math.Round ((decimal)(Mathf.Acos (skalar / (v1length * v2length)) * Mathf.Rad2Deg), 2);
	}

	private int calculateProgress ()
	{
		int handProgress = 0;

		GameObject ball = GameObject.FindGameObjectWithTag ("basketball");
		if (ball != null) {
			Vector3 targetPosition = ball.transform.position;
			Vector3 handPosition;
			if (ballPosition == LevelModel.ObjectPosition.Left) {
				handPosition = GameObject.FindGameObjectWithTag ("leftHand").transform.position;
			} else {
				handPosition = GameObject.FindGameObjectWithTag ("rightHand").transform.position;
			}

			/*Vector3 vector = new Vector3 ((targetPosition.x - handPosition.x), (targetPosition.y - handPosition.y), (targetPosition.z - handPosition.z));
			lastDistanceHandToTarget = Mathf.Sqrt (Mathf.Pow (vector.x, 2) + Mathf.Pow (vector.y, 2) + Mathf.Pow (vector.z, 2));*/

			lastDistanceHandToTarget = Vector3.Distance (targetPosition, handPosition);

			if (lastDistanceHandToTarget < 0.2) {
				handProgress = 100;
			} else if (lastDistanceHandToTarget < 0.3) {
				handProgress = 90;
			} else if (lastDistanceHandToTarget < 0.4) {
				handProgress = 80;
			} else if (lastDistanceHandToTarget < 0.5) {
				handProgress = 70;
			} else if (lastDistanceHandToTarget < 0.6) {
				handProgress = 60;
			} else if (lastDistanceHandToTarget < 0.7) {
				handProgress = 50;
			} else if (lastDistanceHandToTarget < 0.8) {
				handProgress = 40;
			} else if (lastDistanceHandToTarget < 0.9) {
				handProgress = 30;
			} else if (lastDistanceHandToTarget < 1.0) {
				handProgress = 20;
			} else if (lastDistanceHandToTarget < 1.1) {
				handProgress = 10;
			} else {
				handProgress = 0;
			}

			//Debug.Log ("Progress: " + handProgress + "distance: " + lastDistanceHandToTarget);

			setBallGlowToProgress (handProgress, ball);

			return handProgress;
		} else {
			//progress = 0;
			//resetWorksuitMeshGlowEffect ();

			return 0;
		}

	}

	private void setBallGlowToProgress (int handProgress, GameObject ball)
	{
		Material ballMaterial = ball.GetComponent<MeshRenderer> ().material;

		float colourPercentage = calculateColourPercentage (handProgress);

		//Debug.Log ("Prozent: " + colourPercentage);

		if (handProgress < 50) {
			ballMaterial.SetColor ("_MKGlowColor", new Color32 (180, (byte)colourPercentage, 0, 255));
			ballMaterial.SetFloat ("_MKGlowPower", 0.15f);
			ballMaterial.SetColor ("_MKGlowTexColor", new Color32 (180, (byte)colourPercentage, 0, 255));
			ballMaterial.SetFloat ("_MKGlowTexStrength", 1.68f);
		} else {
			ballMaterial.SetColor ("_MKGlowColor", new Color32 ((byte)colourPercentage, 180, 0, 255));
			ballMaterial.SetFloat ("_MKGlowPower", 0.15f);
			ballMaterial.SetColor ("_MKGlowTexColor", new Color32 ((byte)colourPercentage, 180, 0, 255));
			ballMaterial.SetFloat ("_MKGlowTexStrength", 1.68f);
		}
	}

	private float calculateColourPercentage (int handProgress)
	{
		if (handProgress < 50) {
			return ((float)handProgress / 50.0f) * 180.0f;
		} else {
			return 180.0f - ((((float)handProgress - 50.0f) / 50.0f) * 180.0f);
		}
	}

	IEnumerator checkFreezing ()
	{
		CircularBuffer<Vector3> leftHandPositionBuffer = new CircularBuffer<Vector3> (5);
		CircularBuffer<Vector3> rightHandPositionBuffer = new CircularBuffer<Vector3> (5);
		bool leftHandFreezed;
		bool rightHandFreezed;

		yield return new WaitForSeconds (1.0f);

		while (true) {
			leftHandFreezed = true;
			rightHandFreezed = true;
			leftHandPositionBuffer.PushFront (bodyJoints ["handLeftPos"]);
			rightHandPositionBuffer.PushFront (bodyJoints ["handRightPos"]);

			if (leftHandPositionBuffer.Size == 5 && rightHandPositionBuffer.Size == 5) {
				Vector3 sumPosition = new Vector3 (0, 0, 0);
				for (int i = 0; i < leftHandPositionBuffer.Size; i++) {
					sumPosition.x += leftHandPositionBuffer [i].x;
					sumPosition.y += leftHandPositionBuffer [i].y;
					sumPosition.z += leftHandPositionBuffer [i].z;
				}

				Vector3 avgPosition = new Vector3 (sumPosition.x / 5, sumPosition.y / 5, sumPosition.z / 5);
				for (int i = 0; i < leftHandPositionBuffer.Size; i++) {
					if (Mathf.Abs (Vector3.Distance (leftHandPositionBuffer [i], avgPosition)) > 0.1) {
						leftHandFreezed = false;
						break;
					}
				}

				sumPosition = new Vector3 (0, 0, 0);
				for (int i = 0; i < leftHandPositionBuffer.Size; i++) {
					sumPosition.x += rightHandPositionBuffer [i].x;
					sumPosition.y += rightHandPositionBuffer [i].y;
					sumPosition.z += rightHandPositionBuffer [i].z;
				}

				avgPosition = new Vector3 (sumPosition.x / 5, sumPosition.y / 5, sumPosition.z / 5);
				for (int i = 0; i < rightHandPositionBuffer.Size; i++) {
					if (Mathf.Abs (Vector3.Distance (rightHandPositionBuffer [i], avgPosition)) > 0.1) {
						rightHandFreezed = false;
						break;
					}
				}

				if (leftHandFreezed && rightHandFreezed) {
					playerController.FreezingDetected ();
					Debug.Log ("Freezing!");
				} else {
					playerController.FreezingCorrected ();
					Debug.Log ("Kein Freezing!");
				}

			}

			yield return new WaitForSeconds (1.0f);
		}
	}
}
