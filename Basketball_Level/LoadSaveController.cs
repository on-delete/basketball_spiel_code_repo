using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class LoadSaveController
{

	public static void Save (int level, int countHits, int attempts, float score)
	{
		PlayerData data = null;

		BinaryFormatter bf = new BinaryFormatter ();

		data = Load ();

		FileStream file = File.Open (Application.persistentDataPath + "/playerData.dat", FileMode.OpenOrCreate);

		ExerciseData exerciseData = new ExerciseData ();
		exerciseData.timeStamp = System.DateTime.Now;
		exerciseData.countHits = countHits;
		exerciseData.attempts = attempts;
		exerciseData.score = score;

		data.level = level;
		data.exerciseList.Add (exerciseData);

		bf.Serialize (file, data);
		file.Close ();

		/*For testing purpose only!*/
		File.AppendAllText (Application.persistentDataPath + "/playerDataTest.txt", "TimeStamp: " + exerciseData.timeStamp + "; Hits: " + exerciseData.countHits + "; Attempts: " + exerciseData.attempts + "; Score: " + exerciseData.score + "; Level: " + data.level);
		/*For testing purpose only!*/
	}

	public static PlayerData Load ()
	{

		PlayerData data = null;

		BinaryFormatter bf = new BinaryFormatter ();
		if (File.Exists (Application.persistentDataPath + "/playerData.dat")) {
			FileStream file = File.Open (Application.persistentDataPath + "/playerData.dat", FileMode.Open);
			data = (PlayerData)bf.Deserialize (file);
			file.Close ();
		} else {
			data = new PlayerData ();
			data.level = 1;
		}

		return data;
	}
}
