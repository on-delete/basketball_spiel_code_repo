using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour {

	public GameObject hourglassImage;

	Scene scene;

	void Start(){
		scene = SceneManager.GetActiveScene();
	}

	public void startExercise(){
		hourglassImage.SetActive(true);
		SceneManager.LoadScene(1, LoadSceneMode.Single);
	}
	
	public void close(){
		Application.Quit();
	}
}
