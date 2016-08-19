using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerView : MonoBehaviour, IPlayerView
{
	private Coroutine resetTimerRoutine = null;
	private Material worksuitMeshMaterial;
	private static Color32 red = new Color32 (180, 0, 0, 255);
	private static Color32 green = new Color32 (0, 180, 0, 255);
	private static Color32 white = new Color32 (255, 255, 255, 255);

	List<AudioClip> audioList = new List<AudioClip> ();
	private List<int> audioQueue = new List<int> ();

	private AudioSource source;

	void Start ()
	{
		worksuitMeshMaterial = GameObject.FindWithTag ("playerMesh").GetComponent<SkinnedMeshRenderer> ().material;
		source = GetComponent<AudioSource> ();

		StartCoroutine (audioQueueProcessor ());
	}

	public void setMeshColorFailure ()
	{
		worksuitMeshMaterial.SetColor ("_MKGlowColor", red);
		worksuitMeshMaterial.SetFloat ("_MKGlowPower", 0.15f);
		worksuitMeshMaterial.SetColor ("_MKGlowTexColor", red);
		worksuitMeshMaterial.SetFloat ("_MKGlowTexStrength", 1.68f);
		if (resetTimerRoutine != null) {
			StopCoroutine (resetTimerRoutine);
			resetTimerRoutine = null;
		}
	}

	public void setMeshColorCorrected ()
	{
		worksuitMeshMaterial.SetColor ("_MKGlowColor", green);
		worksuitMeshMaterial.SetFloat ("_MKGlowPower", 0.15f);
		worksuitMeshMaterial.SetColor ("_MKGlowTexColor", green);
		worksuitMeshMaterial.SetFloat ("_MKGlowTexStrength", 1.68f);
		resetTimerRoutine = StartCoroutine (resetWorksuitGlowEffectTimer ());
	}

	IEnumerator resetWorksuitGlowEffectTimer ()
	{
		yield return new WaitForSeconds (3);

		resetWorksuitMeshGlowEffect ();

		yield return null;
	}

	private void resetWorksuitMeshGlowEffect ()
	{
		worksuitMeshMaterial.SetColor ("_MKGlowColor", white);
		worksuitMeshMaterial.SetFloat ("_MKGlowPower", 0.0f);
		worksuitMeshMaterial.SetColor ("_MKGlowTexColor", white);
		worksuitMeshMaterial.SetInt ("_MKGlowTexStrength", 0);
	}

	public void playSound (int sound)
	{
		if (source.isPlaying) {
			audioQueue.Add (sound);
		} else {
			source.PlayOneShot (audioList [sound]);
		}
	}

	public void stopSound ()
	{
		source.Stop ();
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
}
