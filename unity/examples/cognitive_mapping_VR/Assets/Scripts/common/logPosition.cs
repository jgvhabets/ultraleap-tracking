using UnityEngine;
using System.Collections;

public class logPosition : MonoBehaviour {

	private GameObject trackedObject;
	private IEnumerator writePositionCoroutine;		// store name of the coroutine in variable, otherwise each call will be another invoke (and stopping the coroutine will not work!)

	void Start() {

		trackedObject = this.gameObject;
		writePositionCoroutine = writePosition(trackedObject.transform);

		}

	// Initialization
	public void startLogging () {

		logFileHandler.eventFile.write("Position tracking started");
		StartCoroutine(writePositionCoroutine);

	}

	public void endLogging () {

		logFileHandler.eventFile.write("Position tracking stopped");
		StopCoroutine(writePositionCoroutine);

		}

	IEnumerator writePosition (Transform trackedObject) {
		
		// write position to log file every fixedTime
		for (;;) {
			
			// get x,y,z position 
			string px = trackedObject.position.x.ToString("0.000");
			string py = trackedObject.position.y.ToString("0.000");
			string pz = trackedObject.position.z.ToString("0.000");

			// get information about view angles
			string pPitch = trackedObject.eulerAngles.x.ToString("0.000");
			string pJaw = trackedObject.eulerAngles.y.ToString("0.000");
			string pRoll = trackedObject.eulerAngles.z.ToString("0.000");

			logFileHandler.posFile.write(px + "," + py + "," + pz + "," + pPitch  + "," + pJaw + "," + pRoll);

			yield return new WaitForSeconds(.02f);
		}
	}


}
