﻿using UnityEngine;
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
		//model.PlayerLevel = playerData.level;
		model.PlayerLevel = 1;
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

				List<ExerciseData> sortedList = playerData.exerciseList.OrderByDescending (o => o.score).ToList ();
				int newLevel = selectLevel (endScore);

				string endScoreText = "Treffer: " + countHits + "\n" +
				                      "Versuche: " + attempts + "\n" +
				                      "Endstand: " + endScore + "\n" + "\n" +
				                      "Highscore: " + sortedList [0].score;

				if (newLevel > model.PlayerLevel) {
					endScoreText += "\n" + "Level aufgestiegen!";
				} else if (newLevel < model.PlayerLevel) {
					endScoreText += "\n" + "Level abgestiegen!";
				}

				levelView.setEndScoreText (endScoreText);

				LoadSaveController.Save (0, countHits, attempts, endScore);

				model.lStatus = LevelModel.LevelStatus.LevelFinished;

				break;
			}
		default:
			{
				break;
			}
		}
	}

	/*private decimal calculateScore ()
	{




		if (attempts > 0) {
			//return Math.Round ((decimal)(((countHits / attempts) * 2 + attempts) * 100));
			return Math.Round ((decimal)(((countHits / attempts) * (attempts / 60)) * 1000));
		} else {
			return 0;
		}
	}*/

	private int selectLevel (float score)
	{
		if (playerData.exerciseList.Count > 1) {
			List<ExerciseData> sortedList = playerData.exerciseList.OrderByDescending (o => o.timeStamp).ToList ();

			int level = 0;
			if (model.PlayerLevel == 1) {
				if (sortedList [playerData.exerciseList.Count - 1].score < score && sortedList [playerData.exerciseList.Count - 2].score < score) {
					level = 2;
				} else {
					level = 1;
				}
			} else {
				if (sortedList [playerData.exerciseList.Count - 1].score > score && sortedList [playerData.exerciseList.Count - 2].score > score) {
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
