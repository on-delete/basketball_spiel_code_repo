using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{

	private LevelController levelController;
	private Coroutine destroyCoroutine;
	private SphereCollider ballCollider;

	private bool ballTriggered;

	public bool BallTriggered {
		get {
			return ballTriggered;
		}
		set {
			ballTriggered = value;
		}
	}

	private bool ballFlying;

	public bool BallFlying {
		get {
			return ballFlying;
		}
		set {
			ballFlying = value;
		}
	}

	// Use this for initialization
	void Start ()
	{
		ballTriggered = false;
		ballFlying = false;
		destroyCoroutine = null;
		ballCollider = GetComponent<SphereCollider> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (levelController != null) {
			if (ballTriggered) {
				Vector3 handPosition;

				if (levelController.BallPosition == LevelController.ObjectPosition.Left) {
					handPosition = GameObject.FindGameObjectWithTag ("leftHand").transform.position;
				} else {
					handPosition = GameObject.FindGameObjectWithTag ("rightHand").transform.position;
				}
		
				float radius = Mathf.Max (ballCollider.transform.lossyScale.x, ballCollider.transform.lossyScale.x, ballCollider.transform.lossyScale.x) * ballCollider.radius;

				transform.position = new Vector3 (handPosition.x, handPosition.y - radius, handPosition.z);
			}

			if (ballFlying) {
				if (destroyCoroutine == null) {
					levelController.isBallGrabbed = false;
					destroyCoroutine = StartCoroutine (destroyRoutine ());
				}
			}
		} else {
			levelController = LevelController.Instance;
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if (!ballFlying) {
			if (levelController != null) {
				if (levelController.BallPosition == LevelController.ObjectPosition.Left) {
					if (other.tag == "leftHand") {
						levelController.isBallGrabbed = true;
						ballTriggered = true;
					}
				} else {
					if (other.tag == "rightHand") {
						levelController.isBallGrabbed = true;
						ballTriggered = true;
					}
				}
			} else {
				levelController = LevelController.Instance;
			}
		} else {
			if (other.tag == "target") {
				levelController.BallHitTarget = true;
				Debug.Log ("Treffer!");
			}
		}
	}

	IEnumerator destroyRoutine ()
	{
		yield return new WaitForSeconds (2.5f);
		levelController.isBallThrown = true;
		Destroy (gameObject);
	}
}
