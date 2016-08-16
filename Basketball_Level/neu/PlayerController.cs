using UnityEngine;
using System.Collections;

public class PlayerController
{

	private IPlayerView playerView;

	private bool isPisaFound;
	private bool isFreezingDtected;

	public PlayerController (PlayerView view)
	{
		playerView = view;
		isPisaFound = false;
		isFreezingDtected = false;
	}

	public void PisaPostureFound ()
	{
		if (!isPisaFound) {
			/*TODO: playerView.playSound ();*/
			playerView.setMeshColorFailure ();
			isPisaFound = true;
		}
	}

	public void PisaPostureCorrected ()
	{
		if (isPisaFound) {
			/*TODO: playerView.stopSound ();*/
			playerView.setMeshColorCorrected ();
			isPisaFound = false;
		}
	}

	public void FreezingDetected ()
	{
		if (!isFreezingDtected) {
			/*TODO: playerView.playSound ();*/
			isFreezingDtected = true;
		}
	}

	public void FreezingCorrected ()
	{
		if (isFreezingDtected) {
			/*TODO: playerView.stopSound ();*/
			isFreezingDtected = false;
		}
	}
}
