using UnityEngine;
using System.Collections;

public interface ILevelView
{
	void playSound (int sound);

	void setStartTimerText (string text);

	void setTimerText (string text);

	void setScoreText (string text);

	void setEndScoreText (string text);

	void instantiateRightBasket ();

	void instantiateLeftBasket ();

	void startGame ();

	void stopGame ();
}
