using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class controlExperiment : MonoBehaviour {

	#region Input and global variables
	// --- input variables ---
	[Header("Experiment parameters")]
	[SerializeField] public bool showEncoding = true;                    // show tours?
	[SerializeField] public bool showPointing = true;                    // show pointing task?
	[SerializeField] public bool showDistJudgement = true;               // show dist Judgement? (needs to be public so that pointing task can also start dist judgement
	[SerializeField] public bool showLastProbeTour = true;               // show one last probe tour at the end?
	[SerializeField] public int numLm = 5;                              // how many landmarks

	// --- Global variables ---
	private encoding encodingScript;
	private pointingTask pointingScript;
	private distJudgement distanceJudgementScript;

	private fadeInOut blackScreen;                                  // fadeInOut of black background screen
	private fadeInOut blueBackground;                               // fadeInOut for blue background
	private fadeInOut expEndText;									// exp end text

	#endregion

	// Use this for initialization
	void Start() {

		blackScreen = GameObject.Find("UI_elements/blackScreen").GetComponent<fadeInOut>();             // now we can use the functions of fadeInOut
		blueBackground = GameObject.Find("UI_elements/blueBackground").GetComponent<fadeInOut>();       // now we can use the functions of fadeInOut
		expEndText = GameObject.Find("UI_elements/expEndText").GetComponent<fadeInOut>();               // now we can use the functions of fadeInOut

		encodingScript = gameObject.GetComponent<encoding>();
		pointingScript = gameObject.GetComponent<pointingTask>();
		distanceJudgementScript = gameObject.GetComponent<distJudgement>();

		StartCoroutine(main());						// start main routine 

		}

	IEnumerator main() {

		yield return new WaitForSeconds(2f);                        // wait for a while before starting to stabilize frame rate

		// -- start encoding phase --
		if (showEncoding) {
			logFileHandler.createParticipantDirectories(getParticipantInfo.filePath_subj + "/encoding");
			yield return StartCoroutine(encodingScript.startEncoding());
			logFileHandler.closeFiles();
		}

		// -- pointing phase --
		if (showPointing) {
			logFileHandler.createParticipantDirectories(getParticipantInfo.filePath_subj + "/pointing");
			yield return StartCoroutine(pointingScript.startPointing());
			logFileHandler.closeFiles();
			} 

		// -- distance judgement --
		if (showDistJudgement) {
			logFileHandler.createParticipantDirectories(getParticipantInfo.filePath_subj + "/distJudgement");
			yield return StartCoroutine(distanceJudgementScript.startDistJudgement());
			logFileHandler.closeFiles();
			}

		// -- one more tour
		if (showLastProbeTour) {
			logFileHandler.createParticipantDirectories(getParticipantInfo.filePath_subj + "/probeTour_end");	// create another directory to store information about the last tour
			yield return StartCoroutine(encodingScript.startSingleProbeTour());
			logFileHandler.closeFiles();
			}

		// -- show last text
		blackScreen.FadeToOpaque();
		blueBackground.FadeToOpaque();
		expEndText.FadeToOpaque();

		yield return new WaitForSeconds(5);

		Application.Quit();

		}
}
