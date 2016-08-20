using UnityEngine;
using System.Collections;

public interface IPlayerView
{

	void setMeshColorFailure ();

	void setMeshColorCorrected ();

	void playSound (int sound);

	void stopSound ();
}
