using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System;

public class distJudgement : MonoBehaviour {

	// ATTENTION: works only for 5 landmarks (hardcoded right now) //

	#region Input and global variables
	// --- input variables ---
	[Header("General parameters")]
	[SerializeField]
	private bool debugConsole = false;                      // should everything that is written to log file also appear in editor console?

	// - main camera -
	private GameObject mainCamera;                          // mainCamera
	private Transform mainCameraTransform;                  // mainCamera Transform
	
	// -- on-screen display --
	private fadeInOut blackScreen;                          // fadeInOut of black background screen
	private fadeInOut blueBackground;                       // fadeInOut for blue background
	private fadeInOut introText;                            // intro text
	private fadeInOut img0_txt;								// img text
	private fadeInOut img1_txt;                             // img text
	private fadeInOut cross_iti;                            // cross for ITI phases

	// -- Distance judgement task --
	private RawImage img_0;                                 // image plcaeholder 0
	private RawImage img_1;                                 // image placeholder 1

	// --- global variables --
	private int numLm;
	private List<GameObject> landmarks;                     // list that will be populated by all the childs of pointPositions gameObject
	private GameObject landmarkParent;                      // parent gameObject that has all the pointing positions (source) as childs
	private float[,] distMatrix = new float[5, 5];          // distance matrix that store all the distances between landmarks

	private List<GameObject> pointSources;                  // list that will be populated by all the childs of pointPositions gameObject
	private List<GameObject> pointTargets;                  // list that will be populated by all the childs of pointPositions gameObject
	private GameObject pointing_sourceParent;               // parent gameObject that has all the pointing positions (source) as childs
	private GameObject pointing_targetParent;               // parent gameObject that has all the pointing positions (target) as childs

	private static readonly System.Random rnd = new System.Random();    // instance of the random class to draw random elements, static readonly so that randomness is preserved

	#endregion

	public IEnumerator startDistJudgement() {

		logFileHandler.eventFile.debug = debugConsole;                  // enable debug (everything that is written into log file will be written into debug console as well)
		logFileHandler.eventFile.writeHeader("Time,Event");             // write header

		logFileHandler.results.debug = debugConsole;
		logFileHandler.results.writeHeader("Time,TrialNr,Source,Target_0,Target_1,CorrResp,GivenResp,Correct,RT");

		#region Init Scripts and GameObjects
		blackScreen = GameObject.Find("UI_elements/blackScreen").GetComponent<fadeInOut>();             // now we can use the functions of fadeInOut
		blueBackground = GameObject.Find("UI_elements/blueBackground").GetComponent<fadeInOut>();       // now we can use the functions of fadeInOut
		introText = GameObject.Find("UI_elements/distJudgement/introText").GetComponent<fadeInOut>();   // now we can use the functions of fadeInOut
		img0_txt = GameObject.Find("UI_elements/distJudgement/0_text").GetComponent<fadeInOut>();       // now we can use the functions of fadeInOut
		img1_txt = GameObject.Find("UI_elements/distJudgement/1_text").GetComponent<fadeInOut>();       // now we can use the functions of fadeInOut
		cross_iti = GameObject.Find("UI_elements/ITI_cross").GetComponent<fadeInOut>();

		mainCamera = GameObject.Find("mainCamera");                                 // main camera
		mainCameraTransform = mainCamera.GetComponent<Transform>();                 // transform of main camera

		numLm = gameObject.GetComponent<controlExperiment>().numLm;

		// init image placeholders
		img_0 = GameObject.Find("UI_elements/distJudgement/0").GetComponent<RawImage>();
		img_1 = GameObject.Find("UI_elements/distJudgement/1").GetComponent<RawImage>();
		img_0.CrossFadeAlpha(0f, 0f, false);
		img_1.CrossFadeAlpha(0f, 0f, false);

		// populate list of waypoints (each is a gameObject) for tour
		landmarkParent = GameObject.Find("waypoints/" + numLm.ToString("0") + "_lm/pointingTask_targets");
		landmarks = new List<GameObject>();
		foreach (Transform child in landmarkParent.transform) {
			landmarks.Add(child.gameObject);
			}

		// calculate distance matrix between all the landmarks
		for (int rows = 0; rows < 5; rows++) {
			for (int columns = 0; columns < 5; columns++) {
				distMatrix[rows, columns] = Vector3.Distance(landmarks[columns].transform.position, landmarks[rows].transform.position);
				}
			}

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

		#endregion

		logFileHandler.eventFile.write("Distance judgement started");

		yield return StartCoroutine(main());     // start main routine 

		}

	#region Procedual function to control the experiment
	IEnumerator main() {

		blackScreen.FadeToOpaque();

		yield return new WaitForSeconds(1f);					// wait for a while before starting to stabilize frame rate

		yield return StartCoroutine(introduction());			// start introduction

		int[][] distJudgeSequence = generateTrialSequence();	// generate randomized sequence

		int trialNr = 0;    // keep track of trial nr.

		foreach (int[] trial in distJudgeSequence) {

			logFileHandler.eventFile.write("Starting dist judgement trial " + trialNr.ToString("0"));
			logFileHandler.eventFile.write("Source: " + trial[0].ToString("0") +  "; Landmarks: " + trial[1].ToString("0") + "," + trial[2].ToString("0"));

			yield return StartCoroutine(distJudgementTrial(trialNr, trial));

			trialNr++;      // increase trial nr. by one
			}

		img_0.CrossFadeAlpha(0f, 0f, false);
		img_1.CrossFadeAlpha(0f, 0f, false);

		img0_txt.FadeToClear();
		img1_txt.FadeToClear();

		}

	IEnumerator introduction() {

		logFileHandler.eventFile.write("Starting introduction");

		blueBackground.FadeToOpaque();
		introText.FadeToOpaque();

		while (!(Input.GetKey(KeyCode.UpArrow))) {
			yield return new WaitForSeconds(Time.deltaTime);
			}

		blueBackground.FadeToClear();
		introText.FadeToClear();
		yield return new WaitForSeconds(introText.fadeSpeed);

		}
	IEnumerator distJudgementTrial(int trialNr, int[] trial) {

		// find out correct answer
		int correct_lm = findCorrectLandmark(trial[0], trial[1], trial[2]);                 // return the correct LANDMARK NR (0 - 4)

		// set the participant at the source landmark
		// set start position from source and assign correct rotation to look at landmark
		Vector3 newPos = new Vector3(pointSources[trial[0]].transform.position.x, 1.8f, pointSources[trial[0]].transform.position.z);
		Quaternion newRot = Quaternion.Euler(0, pointSources[trial[0]].transform.rotation.eulerAngles.y, 0);

		mainCameraTransform.position = newPos;
		mainCameraTransform.rotation = newRot;

		// fade out black screen
		blackScreen.FadeToClear();
		logFileHandler.eventFile.write("Fade to clear started");
		yield return new WaitForSeconds(blackScreen.fadeSpeed);
		logFileHandler.eventFile.write("Fade to clear ended");

		// find out which button is the correct one to press
		List<int> targets_list = trial.Skip(1).Take(2).ToList();

		int corrResp = targets_list.IndexOf(correct_lm);

		// load images
		img_0.texture = (Texture2D)Resources.Load("Images/distJudge/" + "lm_" + trial[1].ToString("0"));
		img_1.texture = (Texture2D)Resources.Load("Images/distJudge/" + "lm_" + trial[2].ToString("0"));

		// display images
		img_0.CrossFadeAlpha(1f, blackScreen.fadeSpeed, false);
		img_1.CrossFadeAlpha(1f, blackScreen.fadeSpeed, false);

		img0_txt.FadeToOpaque();
		img1_txt.FadeToOpaque();

		double reactionTime = 0f;
		double startTime = globalTimer.paradigmTime;
		while (!(Input.GetKey(KeyCode.Keypad1) || Input.GetKey(KeyCode.Keypad2))) {
			yield return new WaitForSeconds(Time.deltaTime);
			reactionTime = globalTimer.paradigmTime - startTime;
			}

		// which button has been pressed?]
		int givenResp = 9;
		if (Input.GetKey(KeyCode.Keypad1)) {
			logFileHandler.eventFile.write("Given response: 0 ");
			givenResp = 0;
			}
		else if (Input.GetKey(KeyCode.Keypad2)) {
			logFileHandler.eventFile.write("Given response: 1 ");
			givenResp = 1;
			}

		logFileHandler.eventFile.write("Correct response: Button " + corrResp.ToString("0") + " Landmark " + correct_lm.ToString("0"));

		int trialCorrect = 0;
		if (givenResp == corrResp) {
			trialCorrect = 1;
			}

		logFileHandler.results.write(trialNr.ToString("0") + "," + trial[0].ToString("0") + "," + trial[1].ToString("0") + "," + trial[2].ToString("0") + "," + corrResp.ToString("0") +
			"," + givenResp.ToString("0") + "," + trialCorrect.ToString("0") + "," + reactionTime.ToString("0.000"));

		// hide images
		img_0.CrossFadeAlpha(0f, .05f, false);
		img_1.CrossFadeAlpha(0f, .05f, false);

		img0_txt.FadeToClear();
		img1_txt.FadeToClear();

		// fade out black screen
		blackScreen.FadeToOpaque();
		logFileHandler.eventFile.write("Fade to clear started");
		yield return new WaitForSeconds(blackScreen.fadeSpeed);
		logFileHandler.eventFile.write("Fade to clear ended");

		yield return new WaitForSeconds(.5f);								// wait a short time before ITI
		// display ITI
		float rndDisplayTime = (float)(2d + rnd.NextDouble() * 4d);         // gives a random time for the ITI interval between 2 and 6 seconds
		cross_iti.FadeToOpaque();
		logFileHandler.eventFile.write("ITI started, time: " + rndDisplayTime.ToString("0.000"));
		yield return new WaitForSeconds(rndDisplayTime);
		logFileHandler.eventFile.write("ITI ended");
		cross_iti.FadeToClear();

		}



	#endregion


	#region Help functions

	private int[][] generateTrialSequence() {

		int[][] possibleTrials =					// list of all possible combinations
			{
			new int[] {0,1,2},
			new int[] {0,1,3},
			new int[] {0,1,4},
			new int[] {0,2,3},
			new int[] {0,2,4},
			new int[] {0,3,4},
			new int[] {1,0,2},
			new int[] {1,0,3},
			new int[] {1,0,4},
			new int[] {1,2,3},
			new int[] {1,2,4},
			new int[] {1,3,4},
			new int[] {2,0,1},
			new int[] {2,0,3},
			new int[] {2,0,4},
			new int[] {2,1,3},
			new int[] {2,1,4},
			new int[] {2,3,4},
			new int[] {3,0,1},
			new int[] {3,0,2},
			new int[] {3,0,4},
			new int[] {3,1,2},
			new int[] {3,1,4},
			new int[] {3,2,4},
			new int[] {4,0,1},
			new int[] {4,0,2},
			new int[] {4,0,3},
			new int[] {4,1,2},
			new int[] {4,1,3},
			new int[] {4,2,3}
			};


		List<int[]> sequenceList = new List<int[]>();			// create list of int[] to be able to shuffle them around

		// go through each trial and shuffle only the last two items (possible targets)
		for (int i = 0; i < possibleTrials.Length; i++) {

			List<int> currentTrial_targets = possibleTrials[i].Skip(1).Take(2).ToList();	// convert current int[] of trial to list to be able to shuffle

			currentTrial_targets.Shuffle();                                                 // shuffle

			int[] currentTrial = new int[] { possibleTrials[i][0], currentTrial_targets[0], currentTrial_targets[1] };

			// add this to the list of trials again
			sequenceList.Add(currentTrial);              // add trial to sequencelist
			}

		sequenceList.Shuffle();             // randomize the whole list

		return sequenceList.ToArray();

		}

	private int findCorrectLandmark(int source, int target_0, int target_1) {

		int correctLm = 0;
		// find distance between source and the two targets
		float distance_0 = Vector2.Distance(new Vector2(pointSources[source].transform.position.x, pointSources[source].transform.position.z), new Vector2(pointTargets[target_0].transform.position.x, pointTargets[target_0].transform.position.z));
		float distance_1 = Vector2.Distance(new Vector2(pointSources[source].transform.position.x, pointSources[source].transform.position.z), new Vector2(pointTargets[target_1].transform.position.x, pointTargets[target_1].transform.position.z));

		logFileHandler.eventFile.write("Comparing distances between landmarks: " + target_0.ToString() + ", " + target_1.ToString() + ", " + (distance_0 - distance_1).ToString("0.000"));

		if (distance_0 < distance_1) {
			correctLm = target_0;
			}
		else {
			correctLm = target_1;
			}

		return correctLm;

		}

	#endregion
	}
