using UnityEngine;
using System.Collections;

public class fpsMonitor : MonoBehaviour {

	float deltaTime = 0.0f;

	// Update is called once per frame
	void Update () {

		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		float fps = 1.0f / deltaTime;

		if (fps < 59F) {
			logFileHandler.eventFile.write("Warning: FPS drop! " + deltaTime.ToString("0.000") + "ms, " + (fps).ToString("0.000") + "fps");
		}
	}

}
