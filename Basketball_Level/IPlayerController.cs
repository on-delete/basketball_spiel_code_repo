using UnityEngine;
using System.Collections;

public interface IPlayerController
{
	void PisaPostureFound ();

	void PisaPostureCorrected ();

	void FreezingDetected ();

	void FreezingCorrected ();
}
