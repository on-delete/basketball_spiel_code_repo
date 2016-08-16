﻿using UnityEngine;
using System.Collections;

public class App : MonoBehaviour
{

	public LevelView levelView;
	public PlayerView playerView;
	public BodyAnalyzer2 bodyAnalyzer;

	private PlayerController playerController;
	private LevelController2 levelController;
	private LevelModel levelModel;

	public PlayerController PlayerController {
		get {
			return playerController;
		}
	}

	public LevelController2 LevelController2 {
		get {
			return levelController;
		}
	}

	public LevelModel LevelModel {
		get {
			return levelModel;
		}
	}

	// Use this for initialization
	void Start ()
	{
		playerController = new PlayerController (playerView);

		levelModel = new LevelModel ();
		levelController = new LevelController2 (levelModel, levelView);

		bodyAnalyzer.SetLevelController = levelController;
		bodyAnalyzer.SetModel = levelModel;
		bodyAnalyzer.SetPlayerController = playerController;

		levelView.SetLevelController = levelController;
		levelView.SetModel = levelModel;
	}
}