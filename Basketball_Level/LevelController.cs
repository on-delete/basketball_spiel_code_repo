using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public class LevelController : ScriptableObject, ILevelController
{

	private LevelModel model;
	private ILevelView levelView;

	private PlayerData playerData;

	private int countHits;
	private int attempts;
	private bool hit = false;
	private int hitsInRowCounter = 0;
	private int pointsPerHit = 50;
	private float endScore = 0;

	public void init (LevelModel newModel, LevelView view)
	{
		model = newModel;
		levelView = view;
		model.lStatus = LevelModel.LevelStatus.WaitForStartGesture;
		model.isSoundActivated = true;

		playerData = LoadSaveController.Load ();
		model.PlayerData = playerData;
		model.PlayerLevel = playerData.level;
		model.BallPosition = LevelModel.ObjectPosition.Left;
	}

	public void notify (string command)
	{
		switch (command) {
		case "start.gesture.detected":
			{
				levelView.startGame ();
				break;
			}
		case "grab.ball":
			{
				if (model.Posture != LevelModel.BodyPose.Sitting) {
					levelView.playerNotSitting ();
				} else {
					levelView.playSound (2);
					levelView.playSound (4);
					model.lStatus = LevelModel.LevelStatus.GrabBall;

					if (model.BallPosition == LevelModel.ObjectPosition.Left) {
						levelView.instantiateRightBasket ();
					} else {
						levelView.instantiateLeftBasket ();
					}
				}
				break;
			}
		case "throw.ball":
			{
				if (model.PlayerLevel == 2 && model.Posture != LevelModel.BodyPose.Standing) {
					levelView.playerNotStanding ();
				} else {

					levelView.playSound (2);
					levelView.playSound (5);
					model.lStatus = LevelModel.LevelStatus.ThrowBall;
				}
				break;
			}
		case "ball.throw.complete":
			{
				++attempts;
				if (hit) {
					if (hitsInRowCounter <= 5) {
						hitsInRowCounter++;
					}
					endScore += pointsPerHit * Mathf.Pow (2, hitsInRowCounter);
				} else {
					hitsInRowCounter = 0;
				}
				hit = false;
				model.lStatus = LevelModel.LevelStatus.GrabBall;
				string scoreText = "Punkte: " + endScore;
				levelView.setScoreText (scoreText);
				notify ("grab.ball");
				break;
			}

		case "ball.hit.target":
			{
				++countHits;
				hit = true;
				levelView.playSound (3);
				break;
			}

		case"time.up":
			{
				levelView.stopGame ();

				KinectManager kinectManager = KinectManager.Instance;
				kinectManager.ClearKinectUsers ();
				kinectManager.avatarControllers.Clear ();

				GameObject ball = GameObject.FindGameObjectWithTag ("basketball");
				if (ball != null) {
					Destroy (ball);
				}

				Debug.Log ("Level finished!");

				List<ExerciseData> sortedList = model.PlayerData.exerciseList.OrderByDescending (o => o.score).ToList ();
				int newLevel = selectLevel (endScore);

				float highscore = 0;

				if (sortedList.Count > 0) {
					highscore = sortedList [0].score;
				}

				string endScoreText = "Treffer: " + countHits + "\n" +
				                      "Versuche: " + attempts + "\n" +
				                      "Endstand: " + endScore + "\n" + "\n" +
				                      "Highscore: " + highscore;

				if (newLevel > model.PlayerLevel) {
					endScoreText += "\n" + "Level aufgestiegen!";
				} else if (newLevel < model.PlayerLevel) {
					endScoreText += "\n" + "Level abgestiegen!";
				} else {
					endScoreText += "\n" + "Keine Leveländerung!";
				}

				levelView.setEndScoreText (endScoreText);

				LoadSaveController.Save (newLevel, countHits, attempts, endScore);

				model.lStatus = LevelModel.LevelStatus.LevelFinished;

				AudioSource[] audioSources = GameObject.FindObjectsOfType<AudioSource> ();
				if (audioSources.Count () > 0) {
					for (int i = 0; i < audioSources.Count (); i++) {
						audioSources [i].Stop ();
					}
				}

				break;
			}
		default:
			{
				break;
			}
		}
	}

	private int selectLevel (float newScore)
	{
		if (model.PlayerData.exerciseList.Count > 1) {
			List<ExerciseData> sortedList = model.PlayerData.exerciseList.OrderByDescending (o => o.timeStamp).ToList ();

			int level = 0;
			if (model.PlayerLevel == 1) {
				if (sortedList [sortedList.Count - 1].score < newScore && sortedList [sortedList.Count - 2].score < newScore) {
					level = 2;
				} else {
					level = 1;
				}
			} else {
				if (sortedList [sortedList.Count - 1].score > newScore && sortedList [sortedList.Count - 2].score > newScore) {
					level = 1;
				} else {
					level = 2;
				}
			}

			return level;
		} else {
			return model.PlayerLevel;
		}
	}
}
