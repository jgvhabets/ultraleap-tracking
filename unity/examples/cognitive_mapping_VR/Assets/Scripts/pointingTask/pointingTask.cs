using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class pointingTask : MonoBehaviour {

	#region Input and global variables
	// --- input variables ---
	[Header("General parameters")]
	[SerializeField] private bool debugConsole = false;              // should everything that is written to log file also appear in editor console?

	// --- global variables --

	private int numLm;
	private List<GameObject> pointSources;                  // list that will be populated by all the childs of pointPositions gameObject
	private List<GameObject> pointTargets;                  // list that will be populated by all the childs of pointPositions gameObject
	private GameObject pointing_sourceParent;				// parent gameObject that has all the pointing positions (source) as childs
	private GameObject pointing_targetParent;				// parent gameObject that has all the pointing positions (target) as childs
	
	// - main camera -
	private GameObject mainCamera;							// mainCamera
	private Transform mainCameraTransform;					// mainCamera Transform
	private followPlayer _followPlayer;                     // mainCamera follow player script

    // -- on-screen display --
    private fadeInOut blackScreen;                          // fadeInOut of black background screen
	private fadeInOut blueBackground;                       // fadeInOut for blue background
	private fadeInOut crosshair;
	private fadeInOut probeIntroText;
	private fadeInOut cross_iti;                                    // cross for ITI phases

	private List<fadeInOut> lmImages = new List<fadeInOut>();   // list for images of landmarks

	private GameObject cityParent;                                      // will be used to enable or disable city (to reduce GPU load during distractor/rest phase)

	private static readonly System.Random rnd = new System.Random();    // instance of the random class to draw random elements, static readonly so that randomness is preserved

	#endregion

	public IEnumerator startPointing() {

		GameObject.Find("UI_elements/stdImages").SetActive(false);      // disable STD images from tour/probe/std

		logFileHandler.eventFile.debug = debugConsole;					// enable debug (everything that is written into log file will be written into debug console as well)
		logFileHandler.eventFile.writeHeader("Time,Event");				// write header

		logFileHandler.results.debug = debugConsole;
		logFileHandler.results.writeHeader("Time,TrialNr,Source,Target,CorrResp,GivenResp,Error,RT");

        #region Init Scripts and GameObjects
        mainCamera = GameObject.Find("mainCamera/Camera (eye)");                                        // main camera 
        mainCameraTransform = GameObject.Find("mainCamera").GetComponent<Transform>();                  // transform of main camera
		_followPlayer = GameObject.Find("mainCamera").GetComponent<followPlayer>();                    // main camera follow player script
		_followPlayer.enabled = false;                                                                  // disable follow player script upon initialization

        // --- find scripts for fadeInOut ---
        blackScreen = GameObject.Find("UI_elements/blackScreen").GetComponent<fadeInOut>();						// now we can use the functions of fadeInOut
		blueBackground = GameObject.Find("UI_elements/blueBackground").GetComponent<fadeInOut>();				// now we can use the functions of fadeInOut
		crosshair = GameObject.Find("UI_elements/crosshair").GetComponent<fadeInOut>();							// now we can use the functions of fadeInOut
		probeIntroText = GameObject.Find("UI_elements/introductionText_pointing/0").GetComponent<fadeInOut>();  // now we can use the functions of fadeInOut
		cross_iti = GameObject.Find("UI_elements/ITI_cross").GetComponent<fadeInOut>();

		numLm = gameObject.GetComponent<controlExperiment>().numLm;

		// populate list of waypoints (each is a gameObject) for source positions
		pointing_sourceParent = GameObject.Find("waypoints/" + numLm.ToString("0") + "_lm/pointingTask_sources");
		pointSources = new List<GameObject>();
		foreach (Transform child in pointing_sourceParent.transform) {
			pointSources.Add(child.gameObject);
			}

		// populate list of waypoints (each is a gameObject) for target positions
		pointing_targetParent = GameObject.Find("waypoints/" + numLm.ToString("0") + "_lm/pointingTask_targets");
		pointTargets = new List<GameObject>();
		foreach (Transform child in pointing_targetParent.transform) {
			pointTargets.Add(child.gameObject);
			}

		// populate list of landmark fadeInOut objects (pictures)
		for (int i = 0; i < numLm; i++) {
			lmImages.Add(GameObject.Find("UI_elements/pointingImages/lm_" + i.ToString("0")).GetComponent<fadeInOut>());;
			}

		cityParent = GameObject.Find("city");   // city parent gameobject

		#endregion
		logFileHandler.eventFile.write("Pointing started");
		
		yield return StartCoroutine(main());     // start main routine 

		}

	#region Procedual function to control the experiment
	// main contains calls to all the subfunctions that run during the experiment
	IEnumerator main() {

        cityParent.SetActive(true);

		yield return new WaitForSeconds(1f);            // wait for a while before starting to stabilize frame rate

		yield return StartCoroutine(introduction());    // start introduction

		// -- init pointing sequence as jagged array {source landmark, target landmark}
		int[][] pointSequence = {
                    new int[] {0,1},
                    new int[] {2,3},
                    new int[] {0,3},
                    new int[] {1,3},
                    new int[] {4,2},
                    new int[] {1,4},
                    new int[] {4,0},
                    new int[] {0,4},
                    new int[] {3,1},
                    new int[] {1,2},
                    new int[] {3,4},
                    new int[] {2,0},
                    new int[] {4,3},
                    new int[] {2,1},
                    new int[] {0,2},
                    new int[] {4,1},
                    new int[] {3,0},
                    new int[] {1,0},
                    new int[] {2,4},
                    new int[] {3,2}
                };	

        GameObject.Find("mainCamera").GetComponent<logPosition>().startLogging();

		int trialNr = 0;	// keep track of trial nr.

		foreach (int[] trial in pointSequence) {

			logFileHandler.eventFile.write("Starting pointing trial " + trialNr.ToString("0"));

			yield return StartCoroutine(pointingTrial(trialNr, trial[0], trial[1]));

			trialNr++;		// increase trial nr. by one
		}

        GameObject.Find("mainCamera").GetComponent<logPosition>().endLogging(); // end logging

	}

	IEnumerator introduction() {

		logFileHandler.eventFile.write("Starting introduction");

		blueBackground.FadeToOpaque();
		probeIntroText.FadeToOpaque();

		while (!(Input.GetKey(KeyCode.UpArrow))) {
			yield return new WaitForSeconds(Time.deltaTime);
			}

		blueBackground.FadeToClear();
		probeIntroText.FadeToClear();
		yield return new WaitForSeconds(probeIntroText.fadeSpeed);
		}

	IEnumerator pointingTrial(int trialNr, int lmSource, int lmTarget) {

		// set start position from source and assign correct rotation to look at landmark
		Vector3 newPos = new Vector3(pointSources[lmSource].transform.position.x, 1.8f, pointSources[lmSource].transform.position.z);
		Quaternion newRot = Quaternion.Euler(0, pointSources[lmSource].transform.rotation.eulerAngles.y, 0);

		mainCameraTransform.position = newPos;
		mainCameraTransform.rotation = newRot;

		// fade out black screen
		blackScreen.FadeToClear();
		logFileHandler.eventFile.write("Fade to clear started");
		yield return new WaitForSeconds(blackScreen.fadeSpeed);       
		logFileHandler.eventFile.write("Fade to clear ended");

		// fade in crosshair and picture of target
		crosshair.FadeToOpaque();
		lmImages[lmTarget].FadeToOpaque();
		logFileHandler.eventFile.write("Crosshair and image fading started");
		yield return new WaitForSeconds(crosshair.fadeSpeed);
		logFileHandler.eventFile.write("Crosshair image fading ended");

		logFileHandler.eventFile.write("Enable rotation");

        // wait for participant to make judgement
        // waiting for user input
        double reactionTime = 0f;
		double startTime = globalTimer.paradigmTime;
		while (!(Input.GetKey(KeyCode.UpArrow))) {

			yield return new WaitForSeconds(Time.deltaTime);
			reactionTime = globalTimer.paradigmTime - startTime;
			}

        logFileHandler.eventFile.write("Participant logged response");
		logFileHandler.eventFile.write("Disable rotation");

		// get accuracy
		float responseAngle = mainCamera.GetComponent<Transform>().rotation.eulerAngles.y;
		logFileHandler.eventFile.write("Given response: " + responseAngle.ToString("0.000"));
        Debug.Log(responseAngle);

        Vector3 relativePos = pointTargets[lmTarget].transform.position - pointSources[lmSource].transform.position;	// calculate difference vector between two positions
		float correctAngle = Quaternion.LookRotation(relativePos).eulerAngles.y;										// let unity calculate pointing angle to that vector --> this is the correct angle
        Debug.Log(correctAngle);

        logFileHandler.eventFile.write("Correct angle: " + correctAngle.ToString("0.000"));

		float errorAngle = Mathf.DeltaAngle(correctAngle, responseAngle);
        Debug.Log(errorAngle);

        logFileHandler.results.write(trialNr.ToString("0") + "," + lmSource.ToString("0") + "," + lmTarget.ToString("0") + "," 
			+ correctAngle.ToString("0.000") +  "," + responseAngle.ToString("0.000") + "," + errorAngle.ToString("0.000") + "," + reactionTime.ToString("0.000"));
		
		// fade out black screen
		blackScreen.FadeToOpaque();
		crosshair.FadeToClear();
		lmImages[lmTarget].FadeToClear();
		logFileHandler.eventFile.write("Fade to opaque");
		yield return new WaitForSeconds(blackScreen.fadeSpeed);
		logFileHandler.eventFile.write("Fade to opaque ended");

		yield return new WaitForSeconds(.5f);                               // wait a short time before ITI

		// display ITI
		float rndDisplayTime = (float)(2d + rnd.NextDouble() * 4d);         // gives a random time for the ITI interval between 2 and 6 seconds
		cross_iti.FadeToOpaque();
		logFileHandler.eventFile.write("ITI started, time: " + rndDisplayTime.ToString("0.000"));
		yield return new WaitForSeconds(rndDisplayTime);
		logFileHandler.eventFile.write("ITI ended");
		cross_iti.FadeToClear();

		}
	#endregion

	#region Additional helper methods
	#endregion
	}