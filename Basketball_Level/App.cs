using UnityEngine;
using System.Collections;

public class App : MonoBehaviour
{

	public LevelView levelView;
	public PlayerView playerView;
	public BodyAnalyzer bodyAnalyzer;
	public OptionGUI optionGui;

	private PlayerController playerController;
	private LevelController levelController;
	private LevelModel levelModel;

	public PlayerController PlayerController {
		get {
			return playerController;
		}
	}

	public LevelController LevelController {
		get {
			return levelController;
		}
	}

	public LevelModel LevelModel {
		get {
			return levelModel;
		}
	}

	void Start ()
	{
		levelModel = new LevelModel ();

		playerController = new PlayerController (playerView, levelModel);

		levelController = ScriptableObject.CreateInstance<LevelController> ();
		levelController.init (levelModel, levelView);

		optionGui.SetModel = levelModel;

		levelView.SetLevelController = levelController;
		levelView.SetModel = levelModel;

		bodyAnalyzer.SetLevelController = levelController;
		bodyAnalyzer.SetModel = levelModel;
		bodyAnalyzer.SetPlayerController = playerController;
	}
}
