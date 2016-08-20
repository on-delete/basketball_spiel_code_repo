using UnityEngine;
using System.Collections;

public class PlayerController
{

	private IPlayerView playerView;
	private LevelModel model;

	private bool isPisaFound;
	private bool isFreezingDtected;

	public PlayerController (PlayerView view, LevelModel newModel)
	{
		model = newModel;
		playerView = view;
		isPisaFound = false;
		isFreezingDtected = false;
	}

	public void PisaPostureFound ()
	{
		if (!isPisaFound) {
			if (model.Posture == LevelModel.BodyPose.Sitting) {
				playerView.playSound (0);
			} else {
				playerView.playSound (1);
			}
			playerView.setMeshColorFailure ();
			isPisaFound = true;
		}
	}

	public void PisaPostureCorrected ()
	{
		if (isPisaFound) {
			playerView.stopSound ();
			playerView.setMeshColorCorrected ();
			isPisaFound = false;
		}
	}

	public void FreezingDetected ()
	{
		if (!isFreezingDtected) {
			playerView.playSound (2);
			isFreezingDtected = true;
		}
	}

	public void FreezingCorrected ()
	{
		if (isFreezingDtected) {
			playerView.stopSound ();
			isFreezingDtected = false;
		}
	}
}
