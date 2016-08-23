using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{

	public GameObject hourglassImage;

	public void startExercise ()
	{
		hourglassImage.SetActive (true);
		SceneManager.LoadScene (1, LoadSceneMode.Single);
	}

	public void close ()
	{
		Application.Quit ();
	}
}
