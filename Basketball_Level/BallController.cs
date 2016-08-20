using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{

	private LevelController levelController;
	private LevelModel model;
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
		levelController = GameObject.Find ("App").GetComponent<App> ().LevelController;
		model = GameObject.Find ("App").GetComponent<App> ().LevelModel;
		ballTriggered = false;
		ballFlying = false;
		destroyCoroutine = null;
		ballCollider = GetComponent<SphereCollider> ();
	}

	// Update is called once per frame
	void Update ()
	{
		if (ballTriggered) {
			Vector3 handPosition;

			if (model.BallPosition == LevelModel.ObjectPosition.Left) {
				handPosition = GameObject.FindGameObjectWithTag ("leftHand").transform.position;
			} else {
				handPosition = GameObject.FindGameObjectWithTag ("rightHand").transform.position;
			}

			float radius = Mathf.Max (ballCollider.transform.lossyScale.x, ballCollider.transform.lossyScale.x, ballCollider.transform.lossyScale.x) * ballCollider.radius;

			transform.position = new Vector3 (handPosition.x, handPosition.y - radius, handPosition.z);
		}

		if (ballFlying) {
			if (destroyCoroutine == null) {
				destroyCoroutine = StartCoroutine (destroyRoutine ());
			}
		}
	}

	void OnTriggerEnter (Collider other)
	{
		if (!ballFlying) {
			if (model.BallPosition == LevelModel.ObjectPosition.Left) {
				if (other.tag == "leftHand") {
					levelController.notify ("throw.ball");
					ballTriggered = true;
				}
			} else {
				if (other.tag == "rightHand") {
					levelController.notify ("throw.ball");
					ballTriggered = true;
				}
			}
		} else {
			if (other.tag == "target") {
				levelController.notify ("ball.hit.target");
				Debug.Log ("Treffer!");
			}
		}
	}

	IEnumerator destroyRoutine ()
	{
		yield return new WaitForSeconds (2.5f);
		levelController.notify ("ball.throw.complete");
		Destroy (gameObject);
	}
}
