using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class encoding : MonoBehaviour {

	#region Input and global variables
	// --- input variables ---
	[Header("General parameters")]
	[SerializeField] private bool debugConsole = false;				// should everything that is written to log file also appear in editor console?
	[SerializeField] public bool showIntro = true;					// show the introduction?
	[SerializeField] public bool showTours = true;                  // show tours?
	[SerializeField] public bool showProbeTours = true;             // show probe tours?
	[SerializeField] public bool showProbeTrials = true;			// show probe trials?	
	[SerializeField] public bool showDistractor_or_Rest = true;     // show distractor or rest phase?	
	[SerializeField] public bool reverseLastTour = false;			// reverse the direction of last tour?			

	[Header("Encoding parameters")]
	[SerializeField] private Vector3 startPos;						// starting position
	public int minimumCycles;										// how many sets of tours
	public int numTours_inEachCycle;								// how many tours in each set
	public int objectPositionIndex;									// which position configuration should be loaded?
	public List<float> agentSpeeds = new List<float>();				// speed settings
	[SerializeField] private bool waitAtLm = false;					// wait at landmark?
	[SerializeField] private float waitTimeAtLm = 5f;				// wait time in seconds at landmarks
	[SerializeField] private float probeTourReactionTime = 6f;		// reaction times for trials during probe tour	

	[Header("Probe trial parameters")]
	[SerializeField] private float distanceProbeTrials = 10f;       // distance to center of intersection in probe trials
	[SerializeField] private float agentSpeed = 7f;                 // speed of navMeshAgent
	[SerializeField] private float waitBeforeMoveTime = 1f;			// before moving to intersection, wait a short time
	[SerializeField] private float probeTrialReactionTime = 4f;     // reaction times for probe trials
	[SerializeField] private int lmProbeTrials_tourDir = 1;         // how many times should be tour direction probed PER BLOCK at landmarks
	[SerializeField] private int lmProbeTrials_diffDir = 1;         // how many times should be different direction probed PER BLOCK at landmarks
	[SerializeField] private int intermProbeTrials_tourDir = 2;     // how many trials should be tour direction probed PER BLOCK at intermediate intersections
	[SerializeField] private int intermProbeTrials_diffDir = 2;     // how many times should be different direction probed PER BLOCK at intermediate intersections
	[SerializeField] private Vector3 startPos_reverse; 

	[Header("Distractor/rest parameters")]
	[SerializeField] private float numSTDTrials;                    // how many std trials
	[SerializeField] private float stdReactionTime = 30f;           // reaction time for STD trials

	// --- global variables ---

	private int numLm;										// how many landmarks, this will be obtained by controlExperiment
	
	// -- navigation --
	private List<GameObject> waypoints_tour;                // list of all the waypoints for the tour
	private List<GameObject> waypoints_probe_tour;			// waypoints for the probe tour
	private List<GameObject> waypoints_probe;               // list of all the waypoints for the probe trials
	private float lmSpeed;                                  // speed of navAgent for each landmark
	private UnityEngine.AI.NavMeshPath path;                               // NavMesh path that is calculated for every waypoint
	private UnityEngine.AI.NavMeshAgent playerNavAgent;                    // NavMesh agent 
	private GameObject parent_waypoints_tour;               // parent GameObject of all the waypoints for the tour
	private GameObject parent_waypoints_probe_tour;			// parten GameObject of all waypoints for the probe tour
	private GameObject parent_waypoints_probe;              // parent GameObject of all the waypoints for the probe trials
	private GameObject probeIntersectionSignal;				// cylinder that signals the intersection for the probe trials

	// - main camera --
	private GameObject mainCamera;                          // mainCamera
	private Transform mainCameraTransform;                  // mainCamera Transform
	private followPlayer _followPlayer;                     // mainCamera follow player script
	
	// -- on-screen display --
	private fadeInOut blackScreen;                                  // fadeInOut of black background screen
	private fadeInOut blueBackground;                               // fadeInOut for blue background
	private List<fadeInOut> introTexts = new List<fadeInOut>();     // list for different texts during introduction of the whole experiment
	private fadeInOut tourIntroText;                                // text before the second run of encoding (tour through city)
	private fadeInOut probeTourIntroText;                           // probe tour intro text
	private fadeInOut probeTourMissText;                            // when participants have made an error on the probe tour trials
	private fadeInOut lastProbeTourIntroText;						// intro for last probe tour after cognitive mapping tasks
	private fadeInOut betweenTourText;                              // text between the tours
	private fadeInOut probeIntroText;                               // text before the probe trial phase
	private fadeInOut restIntroText;                                // text before the rest phase
	private fadeInOut restEndText;									// text at the end of the rest phase
	private fadeInOut stdIntroText;                                 // text before the spot-the-difference trials
	private fadeInOut cross_iti;                                    // cross for ITI phases
	private List<fadeInOut> introLmImages = new List<fadeInOut>();  // list containing images of landmarks during introduction
	private List<fadeInOut> introLmTxts = new List<fadeInOut>();    // list containing texts for landmarks during introduction

	// -- arrow background and the individual arrows --
	private fadeInOut arrowBG;
	private fadeInOut arrow_l;
	private fadeInOut arrow_r;
	private fadeInOut arrow_t;

	// -- Probe trial performance --
	private bool completedProbeTour;								// bool that indicates that participants have succesfully completed probeTour phase

	// -- STD task --
	private RawImage std_l;											// image plcaeholder on the left
	private RawImage std_r;											// image placeholder on the right
	private fadeInOut sameDiffText_same;							// Text showing "same" for STD task
	private fadeInOut sameDiffText_diff;							// Text showing "diff" for STD task
	private int[] std_diffTrials = new int[] {1, 6, 54, 9, 57, 39, 5, 0, 44, 11, 38, 53, 52, 12, 27, 59, 47, 45, 20, 4,
		29, 33, 36, 35, 55, 32, 2, 3, 23, 26};	// images that are different from one another
	
	// -- pointing task --
	private pointingTask pointingTaskScript;

	// -- UI elements --
	private GameObject UI_introductionText_all;

	// -- List of trial orders --
	private int[][] probeTrialOrder_all;

	private static readonly System.Random rnd = new System.Random();    // instance of the random class to draw random elements, static readonly so that randomness is preserved

	private GameObject cityParent;										// will be used to enable or disable city (to reduce GPU load during distractor/rest phase)
	#endregion

	// Initialization of whole encoding phase
	public IEnumerator startEncoding() {

		logFileHandler.eventFile.debug = debugConsole;								// enable debug (everything that is written into log file will be written into debug console as well)
		logFileHandler.eventFile.writeHeader("Time,Event");							// write header

		logFileHandler.results.debug = debugConsole;
		logFileHandler.results.writeHeader("Probe tour trials");
		logFileHandler.results.writeHeader("Time,TrialNr,Waypoint,CorrResp,GivenResp,Correct,RT");

		logFileHandler.posFile.writeHeader("Time,x,y,z,pitch,jaw,roll");			// write header for position tracking file

		#region Init Scripts and GameObjects
		// init scripts and other gameObjects
		playerNavAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();              // get navigation agent
		playerNavAgent.enabled = false;                                             // stop agent so player can be moved to start position

		mainCamera = GameObject.Find("mainCamera");                                 // main camera
		mainCameraTransform = mainCamera.GetComponent<Transform>();                 // transform of main camera
		_followPlayer = mainCamera.GetComponent<followPlayer>();                    // main camera follow player script
		_followPlayer.enabled = false;                                              // disable follow player script upon initialization
		probeIntersectionSignal = GameObject.Find("mainCamera/positionSignal");     // cylinder that appears and disappears during probe tours/trials

		probeIntersectionSignal.GetComponent<Renderer>().enabled = false;			// disable probeIntersectionSignal


		UI_introductionText_all = GameObject.Find("UI_elements/introductionText_tour_probe");   // has all the UI elements inside it

		// --- find scripts for fadeInOut ---
		blackScreen = GameObject.Find("UI_elements/blackScreen").GetComponent<fadeInOut>();             // now we can use the functions of fadeInOut
		blueBackground = GameObject.Find("UI_elements/blueBackground").GetComponent<fadeInOut>();       // now we can use the functions of fadeInOut
		cross_iti = GameObject.Find("UI_elements/ITI_cross").GetComponent<fadeInOut>();
		arrowBG = GameObject.Find("UI_elements/arrowBG").GetComponent<fadeInOut>();
		arrow_l = GameObject.Find("UI_elements/arrow_l").GetComponent<fadeInOut>();
		arrow_r = GameObject.Find("UI_elements/arrow_r").GetComponent<fadeInOut>();
		arrow_t = GameObject.Find("UI_elements/arrow_t").GetComponent<fadeInOut>();

		tourIntroText = GameObject.Find("UI_elements/tourIntroText").GetComponent<fadeInOut>();
		probeTourIntroText = GameObject.Find("UI_elements/probeTourIntroText").GetComponent<fadeInOut>();
		probeTourMissText = GameObject.Find("UI_elements/probeTourMissText").GetComponent<fadeInOut>();
		lastProbeTourIntroText = GameObject.Find("UI_elements/lastProbeTourIntroText").GetComponent<fadeInOut>();
		betweenTourText = GameObject.Find("UI_elements/betweenTourText").GetComponent<fadeInOut>();
		probeIntroText = GameObject.Find("UI_elements/probeIntroText").GetComponent<fadeInOut>();
		stdIntroText = GameObject.Find("UI_elements/stdIntroText").GetComponent<fadeInOut>();
		restIntroText = GameObject.Find("UI_elements/restIntroText").GetComponent<fadeInOut>();
		restEndText = GameObject.Find("UI_elements/restEndText").GetComponent<fadeInOut>();

		numLm = gameObject.GetComponent<controlExperiment>().numLm;

		// populate list of waypoints (each is a gameObject) for tour
		parent_waypoints_tour = GameObject.Find("waypoints/" + numLm.ToString("0") + "_lm/tour");
		waypoints_tour = new List<GameObject>();
		foreach (Transform child in parent_waypoints_tour.transform) {
			waypoints_tour.Add(child.gameObject);
			}

		// populate list of waypoints (each is a gameObject) for probe tour
		parent_waypoints_probe_tour = GameObject.Find("waypoints/" + numLm.ToString("0") + "_lm/probe_tour");
		waypoints_probe_tour = new List<GameObject>();
		foreach (Transform child in parent_waypoints_probe_tour.transform) {
			waypoints_probe_tour.Add(child.gameObject);
			}

		// populate list of waypoints (each is a gameObject) for probe trials
		parent_waypoints_probe = GameObject.Find("waypoints/" + numLm.ToString("0") + "_lm/probeTrials");
		waypoints_probe = new List<GameObject>();
		foreach (Transform child in parent_waypoints_probe.transform) {
			waypoints_probe.Add(child.gameObject);
			}

		// populate list of landmark images and texts for introduction
		for (int i = 0; i < numLm; i++) {
			introLmImages.Add(GameObject.Find("UI_elements/introductionText_tour_probe/lm_" + i.ToString("0") + "_pic").GetComponent<fadeInOut>());
			introLmTxts.Add(GameObject.Find("UI_elements/introductionText_tour_probe/lm_" + i.ToString("0") + "_txt").GetComponent<fadeInOut>());
			}

		// populate list of introduction texts
		for (int i = 0; i < 2; i++) {
			introTexts.Add(GameObject.Find("UI_elements/introductionText_tour_probe/" + i.ToString("0")).GetComponent<fadeInOut>());
			}

		std_l = GameObject.Find("UI_elements/stdImages/left").GetComponent<RawImage>();
		std_r = GameObject.Find("UI_elements/stdImages/right").GetComponent<RawImage>();
		std_l.CrossFadeAlpha(0f, 0f, false);
		std_r.CrossFadeAlpha(0f, 0f, false);
		sameDiffText_same = GameObject.Find("UI_elements/sameDiffText_same").GetComponent<fadeInOut>();
		sameDiffText_diff = GameObject.Find("UI_elements/sameDiffText_diff").GetComponent<fadeInOut>();

		probeTrialOrder_all = generateProbeTrialList();		// generate list of probe trials

		// -- check if input variables make sense --
		if (waypoints_tour[0] == null || waypoints_probe[0] == null) {
			Debug.LogError("--> No waypoints for tour or probe trials specified!");
			}

		// number of tours
		if (minimumCycles == 0 && numTours_inEachCycle == 0) {
			Debug.LogError("--> Number of tours is invalid");
			}

		cityParent = GameObject.Find("city");   // city parent gameobject
		#endregion

		logFileHandler.eventFile.write("Experiment started");
		yield return StartCoroutine(main());     // start main routine 

		}

	// Initialization of just single probe tour
	public IEnumerator startSingleProbeTour() {

		logFileHandler.eventFile.debug = debugConsole;                              // enable debug (everything that is written into log file will be written into debug console as well)
		logFileHandler.eventFile.writeHeader("Time,Event");                         // write header

		logFileHandler.results.debug = debugConsole;
		logFileHandler.results.writeHeader("Probe tour trials");
		logFileHandler.results.writeHeader("Time,TrialNr,Waypoint,CorrResp,GivenResp,Correct,RT");

		logFileHandler.posFile.writeHeader("Time,x,y,z,pitch,jaw,roll");            // write header for position tracking file

		#region Init Scripts and GameObjects
		// init scripts and other gameObjects
		playerNavAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();					// get navigation agent
		playerNavAgent.enabled = false;                                             // stop agent so player can be moved to start position

		mainCamera = GameObject.Find("mainCamera");                                 // main camera
		mainCameraTransform = mainCamera.GetComponent<Transform>();                 // transform of main camera
		_followPlayer = mainCamera.GetComponent<followPlayer>();                    // main camera follow player script
		_followPlayer.enabled = false;                                              // disable follow player script upon initialization
		probeIntersectionSignal = GameObject.Find("mainCamera/positionSignal");     // cylinder that appears and disappears during probe tours/trials

		probeIntersectionSignal.GetComponent<Renderer>().enabled = false; 

		UI_introductionText_all = GameObject.Find("UI_elements/introductionText_tour_probe");   // has all the UI elements inside it

		// --- find scripts for fadeInOut ---
		blackScreen = GameObject.Find("UI_elements/blackScreen").GetComponent<fadeInOut>();             // now we can use the functions of fadeInOut
		blueBackground = GameObject.Find("UI_elements/blueBackground").GetComponent<fadeInOut>();       // now we can use the functions of fadeInOut
		cross_iti = GameObject.Find("UI_elements/ITI_cross").GetComponent<fadeInOut>();
		arrowBG = GameObject.Find("UI_elements/arrowBG").GetComponent<fadeInOut>();
		arrow_l = GameObject.Find("UI_elements/arrow_l").GetComponent<fadeInOut>();
		arrow_r = GameObject.Find("UI_elements/arrow_r").GetComponent<fadeInOut>();
		arrow_t = GameObject.Find("UI_elements/arrow_t").GetComponent<fadeInOut>();

		tourIntroText = GameObject.Find("UI_elements/tourIntroText").GetComponent<fadeInOut>();
		probeTourIntroText = GameObject.Find("UI_elements/probeTourIntroText").GetComponent<fadeInOut>();
		probeTourMissText = GameObject.Find("UI_elements/probeTourMissText").GetComponent<fadeInOut>();

		if (reverseLastTour) {
			lastProbeTourIntroText = GameObject.Find("UI_elements/lastProbeTourIntroText_reversed").GetComponent<fadeInOut>();
			}
		else {
			lastProbeTourIntroText = GameObject.Find("UI_elements/lastProbeTourIntroText").GetComponent<fadeInOut>();
			}

		betweenTourText = GameObject.Find("UI_elements/betweenTourText").GetComponent<fadeInOut>();
		probeIntroText = GameObject.Find("UI_elements/probeIntroText").GetComponent<fadeInOut>();
		stdIntroText = GameObject.Find("UI_elements/stdIntroText").GetComponent<fadeInOut>();
		restIntroText = GameObject.Find("UI_elements/restIntroText").GetComponent<fadeInOut>();
		restEndText = GameObject.Find("UI_elements/restEndText").GetComponent<fadeInOut>();

		numLm = gameObject.GetComponent<controlExperiment>().numLm;

		// populate list of waypoints (each is a gameObject) for tour
		parent_waypoints_tour = GameObject.Find("waypoints/" + numLm.ToString("0") + "_lm/tour");
		waypoints_tour = new List<GameObject>();
		foreach (Transform child in parent_waypoints_tour.transform) {
			waypoints_tour.Add(child.gameObject);
			}

		// populate list of waypoints (each is a gameObject) for probe tour
		if (reverseLastTour) {
			parent_waypoints_probe_tour = GameObject.Find("waypoints/" + numLm.ToString("0") + "_lm/probe_tour_reversed");
			}
		else {
			parent_waypoints_probe_tour = GameObject.Find("waypoints/" + numLm.ToString("0") + "_lm/probe_tour");
			}
		waypoints_probe_tour = new List<GameObject>();
		foreach (Transform child in parent_waypoints_probe_tour.transform) {
			waypoints_probe_tour.Add(child.gameObject);
			}

		// populate list of waypoints (each is a gameObject) for probe trials
		parent_waypoints_probe = GameObject.Find("waypoints/" + numLm.ToString("0") + "_lm/probeTrials");
		waypoints_probe = new List<GameObject>();
		foreach (Transform child in parent_waypoints_probe.transform) {
			waypoints_probe.Add(child.gameObject);
			}

		probeTrialOrder_all = generateProbeTrialList();     // generate list of probe trials

		// -- check if input variables make sense --
		if (waypoints_tour[0] == null || waypoints_probe[0] == null) {
			Debug.LogError("--> No waypoints for tour or probe trials specified!");
			}

		// number of tours
		if (minimumCycles == 0 && numTours_inEachCycle == 0) {
			Debug.LogError("--> Number of tours is invalid");
			}

		cityParent = GameObject.Find("city");   // city parent gameobject
		#endregion

		// -- show small intro
		blackScreen.FadeToOpaque();
		blueBackground.FadeToOpaque();
		lastProbeTourIntroText.FadeToOpaque();

		while (!(Input.GetKey(KeyCode.UpArrow))) {
			yield return new WaitForSeconds(Time.deltaTime);
			}

		blueBackground.FadeToClear();
		lastProbeTourIntroText.FadeToClear();

		yield return new WaitForSeconds(blackScreen.fadeSpeed);

		mainCamera.GetComponent<logPosition>().startLogging();                      // start position logging

		if (reverseLastTour) {
			yield return StartCoroutine(probeTour(false, false, true));                 // start probe tour
			}
		else {
			yield return StartCoroutine(probeTour(false, false, false));                 // start probe tour
			}

		mainCamera.GetComponent<logPosition>().endLogging();						// start position logging
		}

	// main contains calls to all the subfunctions that run during the experiment
	#region Procedual functions that control the experiment
	IEnumerator main() {

		yield return new WaitForSeconds(1f);                        // wait for a while before starting to stabilize frame rate

		// -- introduction to whole experiment --
		if (showIntro == true) {
			yield return StartCoroutine(introduction());            // start general introduction
			}

		UI_introductionText_all.SetActive(false);                   // disable gameObject to save memory

		completedProbeTour = false;									// set the bool for completing probeTour to false    
		                     
		// -- first tour phase (2 cycles) --
		for (int cycle = 0; cycle < minimumCycles; cycle++) {

			logFileHandler.eventFile.write("Starting cycle " + cycle.ToString("0"));

			if (showTours == true) {

				mainCamera.GetComponent<logPosition>().startLogging();					// enable position logging
																						
				findAndPlaceMovingGameObjects(objectPositionIndex);                     // load object positions according to current tour set (block)

				// tours through environment
				if (cycle == 0) {
					yield return StartCoroutine(tour(numTours_inEachCycle, false));		// start first set of tours without introduction
					}
				else {
					yield return StartCoroutine(tour(numTours_inEachCycle, true));		// start first set of tours	with short introduction
					}
				}

			// -- probe tour --
			if (showProbeTours == true) {

				if (cycle == minimumCycles - 1) {
					yield return StartCoroutine(probeTour(true, true, false));          // start probe trials with tracking performance (last probe tour, so track performance now)
					}
				else {
					yield return StartCoroutine(probeTour(false, true, false));          // start probe trials
					}
				}
			}

		// -- second tour phase (1 cycle, only if they made a mistake) --
		if (showProbeTours) {
			
			while (!completedProbeTour) {       // if they made a mistake in the last cycle, completedProbeTour will be false -> they will go into another learning cycle of one tour

				logFileHandler.eventFile.write("Last probe tour not completed successfully -> entering another learning cycle");

				// -- show small intro 

				blackScreen.FadeToOpaque();
				blueBackground.FadeToOpaque();
				probeTourMissText.FadeToOpaque();

				while (!(Input.GetKey(KeyCode.UpArrow))) {
					yield return new WaitForSeconds(Time.deltaTime);
					}

				blackScreen.FadeToClear();
				blueBackground.FadeToClear();
				probeTourMissText.FadeToClear();

				if (showTours) {
					yield return StartCoroutine(tour(1, false));             // start one tour
					}

				if (showProbeTours) {
					yield return StartCoroutine(probeTour(true, true, false));           // start probe trials
					}
				}
			}

		mainCamera.GetComponent<logPosition>().endLogging();			// disable position logging

		// -- rest or task phase --
		if (showDistractor_or_Rest == true) {


			cityParent.SetActive(false);        // disable city environment
			
			// rest
			if (getParticipantInfo.UserProfileDict["5.Group"] == "R") {     // start rest period

				logFileHandler.eventFile.write("Rest phase intro text");
				

				//show short introduction
				blackScreen.FadeToOpaque();
				blueBackground.FadeToOpaque();
				restIntroText.FadeToOpaque();
				yield return new WaitForSeconds(restIntroText.fadeSpeed);

				while (!(Input.GetKey(KeyCode.UpArrow))) {
					yield return new WaitForSeconds(Time.deltaTime);
					}

				restIntroText.FadeToClear();
				blueBackground.FadeToClear();

				logFileHandler.eventFile.write("Rest phase started");
				yield return new WaitForSeconds(numSTDTrials * stdReactionTime);
				logFileHandler.eventFile.write("Rest phase ended");

				restEndText.FadeToOpaque();

				while (!(Input.GetKey(KeyCode.UpArrow))) {
					yield return new WaitForSeconds(Time.deltaTime);
					}
				restEndText.FadeToClear();
				}

			// STD
			else {
				yield return StartCoroutine(spotTheDifference());       // start spot the difference task
				}
			}

		cityParent.SetActive(true);                                     // enable city environment
		yield return new WaitForSeconds(1);								// wait 1 second before proceeding
		}

	IEnumerator introduction() {

		logFileHandler.eventFile.write("Starting introduction");

		blueBackground.FadeToOpaque();

		// display first introduction screen
		logFileHandler.eventFile.write("General introduction text");
		introTexts[0].FadeToOpaque();
		while (!(Input.GetKey(KeyCode.UpArrow))) {
			yield return new WaitForSeconds(Time.deltaTime);
			}
		introTexts[0].FadeToClear();
		yield return new WaitForSeconds(introTexts[0].fadeSpeed);        // wait for a while before starting the tour

		// show landmarks in a random fashion
		List<int> landmarkSequence = Enumerable.Range(0, numLm).ToList();   // create list from 0 -> number of landmarks
		landmarkSequence.Shuffle();     // shuffle that list

		// go through list of landmarks in a random order and present image + text
		for (int i = 0; i < landmarkSequence.Count; i++) {

			logFileHandler.eventFile.write("Introduction text for lm " + landmarkSequence[i].ToString("0"));

			introLmImages[landmarkSequence[i]].FadeToOpaque();
			introLmTxts[landmarkSequence[i]].FadeToOpaque();

			while (!(Input.GetKey(KeyCode.UpArrow))) {
				yield return new WaitForSeconds(Time.deltaTime);
				}

			introLmImages[landmarkSequence[i]].FadeToClear();
			introLmTxts[landmarkSequence[i]].FadeToClear();
			yield return new WaitForSeconds(introLmImages[landmarkSequence[i]].fadeSpeed);        // wait for fading before continue
			}

		// show last intro screen before tour starts
		logFileHandler.eventFile.write("Last introduction text before tour");
		introTexts[1].FadeToOpaque();
		while (!(Input.GetKey(KeyCode.UpArrow))) {
			yield return new WaitForSeconds(Time.deltaTime);
			}
		introTexts[1].FadeToClear();
		yield return new WaitForSeconds(introTexts[0].fadeSpeed);        // wait for a while before starting the tour
		
		blueBackground.FadeToClear();
		}

	IEnumerator tour(int nrOfTours, bool tourIntro) {

		// -- small introduction for second run
		if (tourIntro == true) {

			logFileHandler.eventFile.write("Showing small introduction for second run of tour");

			blueBackground.FadeToOpaque();
			tourIntroText.FadeToOpaque();
			
			while (!(Input.GetKey(KeyCode.UpArrow))) {
				yield return new WaitForSeconds(Time.deltaTime);
				}

			blueBackground.FadeToClear();
			tourIntroText.FadeToClear();

			yield return new WaitForSeconds(blueBackground.fadeSpeed);
			}

		// --- tours throught the environemnt ---
		for (int tourNr = 0; tourNr < nrOfTours; tourNr++) {

			// -- on the second tour, give short intro text
			if (tourNr > 0) {
				betweenTourText.FadeToOpaque();

				logFileHandler.eventFile.write("Waiting between tours");

				yield return new WaitForSeconds(3f);

				betweenTourText.FadeToClear();
				yield return new WaitForSeconds(betweenTourText.fadeSpeed);
				}

			logFileHandler.eventFile.write("Starting tour " + tourNr.ToString("0"));

			logFileHandler.eventFile.write("Setting start position");

			// set start position and rotation
			Quaternion startRot = Quaternion.Euler(0, 0, 0);
			this.gameObject.transform.position = startPos;
			this.gameObject.transform.rotation = startRot;
			mainCameraTransform.position = startPos;
			mainCameraTransform.rotation = startRot;

			blackScreen.FadeToClear();
			logFileHandler.eventFile.write("Fade to clear started");
			yield return new WaitForSeconds(blackScreen.fadeSpeed);        // wait for a while before starting the tour
			logFileHandler.eventFile.write("Fade to clear ended");

			_followPlayer.enabled = true;                                           // enable the camera to follow player (navAgent)
			playerNavAgent.enabled = true;                                          // enable nav Agent

			// go through list of landmarks
			for (int wpNr = 0; wpNr < waypoints_tour.Count; wpNr = wpNr + 1) {
		
				float distanceToWp = 100f;                                          // reset distance value

				GameObject waypoint = waypoints_tour[wpNr];                         // get next landmark as GameObj

				path = new UnityEngine.AI.NavMeshPath();                                           // reset path

				playerNavAgent.CalculatePath(waypoint.transform.position, path);    // calculate path

				// get properties of next landmarks and make adjustments accordingly
				// agent speed setting
				if (waypoint.GetComponent<lmProps>().speedSetting == 1) {
					lmSpeed = agentSpeeds[0];
					}
				else if (waypoint.GetComponent<lmProps>().speedSetting == 2) {
					lmSpeed = agentSpeeds[1];
					}
				else {
					lmSpeed = agentSpeeds[2];
					}

				bool isLm = waypoint.GetComponent<lmProps>().waitForLm;             // is next waypoint a landmark?

				playerNavAgent.speed = lmSpeed;             // set speed
				playerNavAgent.autoBraking = isLm;          // set auto braking

				if (wpNr == waypoints_tour.Count - 1)       // on the last waypoint, enable auto braking (even though it is not set as a landmark) so that nav agent is stopping
					{
					playerNavAgent.autoBraking = true;
					}

				logFileHandler.eventFile.write("Start to waypoint " + wpNr.ToString("0") + "; speed: " + lmSpeed.ToString("0"));

				// if the player should first rotate towards the next waypoint, do so
				if (waypoint.GetComponent<lmProps>().rotateFirst) {
					logFileHandler.eventFile.write("Rotating to next waypoint");

					Quaternion angleToNextWp = Quaternion.LookRotation(waypoint.transform.position - this.transform.position);			// get the rotation towards the next waypoint

					// wait until camera follows and then start to move
					while (Mathf.Abs(Mathf.DeltaAngle(angleToNextWp.eulerAngles.y, mainCameraTransform.rotation.eulerAngles.y)) > 15f) {
						this.transform.rotation = Quaternion.Slerp(this.transform.rotation, angleToNextWp, Time.deltaTime * 3f);         // slerp the player to that direction
						yield return new WaitForSeconds(Time.deltaTime);
						}

					logFileHandler.eventFile.write("Rotation stopped");
					}

				playerNavAgent.SetPath(path);       // go to waypoint

				// wait for agent to reach waypoint, do something depending on lm or just waypoint
				// if lm or last waypoint, wait for camera to reach waypoint and wait x seconds
				if (isLm == true || wpNr == waypoints_tour.Count - 1) {
					while (distanceToWp > .05f) {
						distanceToWp = Vector2.Distance(new Vector2(mainCameraTransform.position.x, mainCameraTransform.position.z), new Vector2(waypoint.transform.position.x, waypoint.transform.position.z));
						yield return new WaitForSeconds(Time.deltaTime);
						}

					if (wpNr == waypoints_tour.Count - 1) {
						logFileHandler.eventFile.write("Last waypoint reached");
						}
					else if (waitAtLm) {
						logFileHandler.eventFile.write("Waiting at landmark");
						yield return new WaitForSeconds(waitTimeAtLm);
						}
					}
				else // else, it is a waypoint in between, wait for navAgent to reach destination and continue
					{
					while (distanceToWp > 1f) {
						distanceToWp = Vector2.Distance(new Vector2(playerNavAgent.transform.position.x, playerNavAgent.transform.position.z), new Vector2(waypoint.transform.position.x, waypoint.transform.position.z));
						yield return new WaitForSeconds(Time.deltaTime);
						}
					logFileHandler.eventFile.write("Waypoint " + wpNr.ToString("0") + " reached");
					}

				}

			playerNavAgent.ResetPath();                 // clear current path of navagent (otherwise it will start immediatley next tour)
			playerNavAgent.enabled = false;             // disable navAgent so it can be repositioned to start
			yield return new WaitForSeconds(1F);        // wait before fading to black
			blackScreen.FadeToOpaque();
			logFileHandler.eventFile.write("Fade to black started");
			yield return new WaitForSeconds(blackScreen.fadeSpeed);
			logFileHandler.eventFile.write("Fade to black ended");

			logFileHandler.eventFile.write("Ending tour " + tourNr.ToString("0"));
			yield return new WaitForSeconds(.2F);        // wait before next tour starts
			_followPlayer.enabled = false;              // disable follow function of main camera
			}

		}

	public IEnumerator probeTour(bool trackPerformance, bool showIntro, bool reversed) {			// public because at the end we want to have one additional probe tour --> we need to selectively call this function

		bool madeError = false;		// keeps track if participant made an error
		int probeTrialNr = 0;       // keeps track of how many probe trials there were in this cycle

		// -- introduction
		if (showIntro) {
			logFileHandler.eventFile.write("Showing introduction for probe tour");

			blueBackground.FadeToOpaque();
			probeTourIntroText.FadeToOpaque();

			while (!(Input.GetKey(KeyCode.UpArrow))) {
				yield return new WaitForSeconds(Time.deltaTime);
				}

			blueBackground.FadeToClear();
			probeTourIntroText.FadeToClear();

			yield return new WaitForSeconds(blueBackground.fadeSpeed);
			}

		// -- start tour
		logFileHandler.eventFile.write("Starting probe tour");

		logFileHandler.eventFile.write("Setting start position");

		// set start position and rotation
		if (!reversed) {
			Quaternion startRot = Quaternion.Euler(0, 0, 0);
			gameObject.transform.position = startPos;
			gameObject.transform.rotation = startRot;
			mainCameraTransform.position = startPos;
			mainCameraTransform.rotation = startRot;
			}
		else {
			Quaternion startRot = Quaternion.Euler(0, 0, 0);
			gameObject.transform.position = startPos_reverse;
			gameObject.transform.rotation = startRot;
			mainCameraTransform.position = startPos_reverse;
			mainCameraTransform.rotation = startRot;
			}

		blackScreen.FadeToClear();
		logFileHandler.eventFile.write("Fade to clear started");
		yield return new WaitForSeconds(blackScreen.fadeSpeed);					// wait for a while before starting the tour
		logFileHandler.eventFile.write("Fade to clear ended");

		_followPlayer.enabled = true;                                           // enable the camera to follow player (navAgent)
		playerNavAgent.enabled = true;                                          // enable nav Agent

		// go through list of landmark
		for (int wpNr = 0; wpNr < waypoints_probe_tour.Count; wpNr++) {

			float distanceToWp_camera = 100f;										// reset distance value
			float distanceToWp_navAgent = 100f;
			float angleDifference_camera_navAgent = 100f;							// reset angular distance value

			GameObject waypoint = waypoints_probe_tour[wpNr];                   // get next landmark as GameObj
			bool isLm = waypoint.GetComponent<lmProps>().waitForLm;             // is next waypoint a landmark?
			bool isProbe = false;                                               // is the next waypoint a probe waypoint? default is false

			if (waypoint.GetComponent<probeProps>() != null) {
				isProbe = true;													// if probeProbs is attached to waypoint, it is a probe waypoint
				}

			path = new UnityEngine.AI.NavMeshPath();                                           // reset path

			playerNavAgent.CalculatePath(waypoint.transform.position, path);    // calculate path

			// get properties of next landmarks and make adjustments accordingly
			// agent speed setting
			if (waypoint.GetComponent<lmProps>().speedSetting == 1) {
				lmSpeed = agentSpeeds[0];
				}
			else if (waypoint.GetComponent<lmProps>().speedSetting == 2) {
				lmSpeed = agentSpeeds[1];
				}
			else {
				lmSpeed = agentSpeeds[2];
				}

			playerNavAgent.speed = lmSpeed;             // set speed

			if (isProbe || isLm || wpNr == waypoints_probe_tour.Count - 1) {		// if waypoint is either a landmark, a probe waypoint or the last waypoint
				playerNavAgent.autoBraking = true;							// set auto braking
				}
		
			logFileHandler.eventFile.write("Start to waypoint " + wpNr.ToString("0") + "; speed: " + lmSpeed.ToString("0"));

			// if the player should first rotate towards the next waypoint, do so
			if (waypoint.GetComponent<lmProps>().rotateFirst) {
				logFileHandler.eventFile.write("Rotating to next waypoint");

				Quaternion angleToNextWp = Quaternion.LookRotation(waypoint.transform.position - this.transform.position);          // get the rotation towards the next waypoint

				// wait until camera follows and then start to move
				while (Mathf.Abs(Mathf.DeltaAngle(angleToNextWp.eulerAngles.y, mainCameraTransform.rotation.eulerAngles.y)) > 15f) {
					this.transform.rotation = Quaternion.Slerp(this.transform.rotation, angleToNextWp, Time.deltaTime * 3f);         // slerp the player to that direction
					yield return new WaitForSeconds(Time.deltaTime);
					}

				logFileHandler.eventFile.write("Rotation stopped");
				}

			playerNavAgent.SetPath(path);       // go to waypoint
			
			// wait for agent to reach waypoint, do something depending on lm, probe waypoint or just intermediate waypoint
			// if lm or last waypoint, wait for camera to reach waypoint and wait x seconds
			if (isLm || isProbe|| wpNr == waypoints_probe_tour.Count - 1) {

				// if next a rotation is required before showing the arrows for probe, check if navAgent has reached landmark yet and rotate the navAgent towards the intersection
				// this way, the camera will also rotate towards the center of the intersection
				if (isProbe && waypoint.GetComponent<probeProps>().rotateFirst) {
					
					while (distanceToWp_navAgent > .05f) {
						distanceToWp_navAgent = Vector2.Distance(new Vector2(gameObject.transform.position.x, this.gameObject.transform.position.z), new Vector2(waypoint.transform.position.x, waypoint.transform.position.z));
						yield return new WaitForSeconds(Time.deltaTime);
						}

					StartCoroutine(rotateNavAgentTowardsTarget(waypoint.transform.rotation));
					}

				// wait for camera to reach landmark
				while (distanceToWp_camera > .05f) {
					distanceToWp_camera = Vector2.Distance(new Vector2(mainCameraTransform.position.x, mainCameraTransform.position.z), new Vector2(waypoint.transform.position.x, waypoint.transform.position.z));
					yield return new WaitForSeconds(Time.deltaTime);
				}

				// if a rotation is needed, wait for it to complete
				while (angleDifference_camera_navAgent > 1f) {
					angleDifference_camera_navAgent = Quaternion.Angle(mainCameraTransform.rotation, gameObject.transform.rotation);
					yield return new WaitForSeconds(Time.deltaTime);
					}

				// depending on what type of waypoint, choose action
				if (wpNr == waypoints_probe_tour.Count - 1) {							// waypoint is last one
					logFileHandler.eventFile.write("Last waypoint reached");
					}
				else if (isLm && waitAtLm) {												// waypoint is a landmark
					logFileHandler.eventFile.write("Waiting at landmark");
					yield return new WaitForSeconds(waitTimeAtLm);
					}

				else if (isProbe) {                                                          // waypoint is a probe tour trial point
					logFileHandler.eventFile.write("Probe tour trial waypoint reached");
					
					// get possible directions at current probe waypoint
					List<char> currentProbeDirections = new List<char>();
					currentProbeDirections = waypoint.GetComponent<probeProps>().directions;

					probeIntersectionSignal.GetComponent<Renderer>().enabled = true;		// enable signal cylinder

					// display different direction arrows depending on settings
					arrowBG.ChangeColor(new Color(1f, 1f, 1f));		// re-change bg color back to white
					arrowBG.FadeToOpaque();							// background for arrows

					if (currentProbeDirections.Contains('l')) {
						arrow_l.FadeToOpaque();
						}
					if (currentProbeDirections.Contains('r')) {
						arrow_r.FadeToOpaque();
						}
					if (currentProbeDirections.Contains('t')) {
						arrow_t.FadeToOpaque();
						}

					logFileHandler.eventFile.write("Fade of directions panel started");
					yield return new WaitForSeconds(arrowBG.fadeSpeed);
					logFileHandler.eventFile.write("Fade of directions panel ended");

					KeyCode correctButton = waypoint.GetComponent<probeProps>().correctButton;						// get the correct button for this trial
					KeyCode givenButton = KeyCode.Return;                                                           // create variable for given response
					int trialCorrect = 0;                                                                           // is trial correct (for performance log file)

					// waiting for user input
					double reactionTime = 0f;
					double startTime = globalTimer.paradigmTime;
					while (!((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))) && reactionTime < probeTourReactionTime) {
						yield return new WaitForSeconds(Time.deltaTime);
						reactionTime = globalTimer.paradigmTime - startTime;
						}

					// which button has been pressed?
					if (Input.GetKey(KeyCode.UpArrow)) {
						logFileHandler.eventFile.write("Up arrow pressed");
						givenButton = KeyCode.UpArrow;
						if (correctButton == KeyCode.UpArrow) {
							logFileHandler.eventFile.write("Correct button was pressed!");
							trialCorrect = 1;
							}
						}
					else if (Input.GetKey(KeyCode.LeftArrow)) {
						logFileHandler.eventFile.write("Left arrow pressed");
						givenButton = KeyCode.LeftArrow;
						if (correctButton == KeyCode.LeftArrow) {
							logFileHandler.eventFile.write("Correct button was pressed!");
							trialCorrect = 1;
							}
						}
					else if (Input.GetKey(KeyCode.RightArrow)) {
						logFileHandler.eventFile.write("Right arrow pressed");
						givenButton = KeyCode.RightArrow;
						if (correctButton == KeyCode.RightArrow) {
							logFileHandler.eventFile.write("Correct button was pressed!");
							trialCorrect = 1;
							}
						}
					else {
						logFileHandler.eventFile.write("No button pressed");
						}

					// change color of BG depending on correctness of answer
					if (trialCorrect == 1) {
						arrowBG.ChangeColor(new Color(.5f, .9f, .5f));
						}
					else {
						arrowBG.ChangeColor(new Color(1f, .5f, .5f));
						}

					yield return new WaitForSeconds(2f);        // wait a second before continuing

					fadeArrowsToClear();                        // fade arrows and background

					probeIntersectionSignal.GetComponent<Renderer>().enabled = false;

					if (trackPerformance) {
						if (trialCorrect == 0) {                    // if they made an error, set the global variable to true
							madeError = true;
							}
						}

					// write to performance log file
					logFileHandler.results.write(probeTrialNr.ToString("0") + "," + wpNr.ToString("0") + "," + correctButton.ToString() + "," + givenButton.ToString() + "," + trialCorrect.ToString("0") + "," + reactionTime.ToString("0.000"));
					probeTrialNr++;

					}
				}
			else // else, it is a waypoint in between, wait for navAgent to reach destination and continue
				{
				while (distanceToWp_camera > 2f) {
					distanceToWp_camera = Vector2.Distance(new Vector2(playerNavAgent.transform.position.x, playerNavAgent.transform.position.z), new Vector2(waypoint.transform.position.x, waypoint.transform.position.z));
					yield return new WaitForSeconds(Time.deltaTime);
					}
				logFileHandler.eventFile.write("Waypoint " + wpNr.ToString("0") + " reached");
				}
			}

		if (trackPerformance) {
			if (madeError) {
				logFileHandler.eventFile.write("Participant made an error -> probe tour not successful");
				completedProbeTour = false;
				}
			else {
				logFileHandler.eventFile.write("Participant made no error -> probe tour successful");
				completedProbeTour = true;
				}
			}

		playerNavAgent.ResetPath();                 // clear current path of navagent (otherwise it will start immediatley next tour)
		playerNavAgent.enabled = false;             // disable navAgent so it can be repositioned to start
		yield return new WaitForSeconds(1f);        // wait before fading to black
		blackScreen.FadeToOpaque();
		logFileHandler.eventFile.write("Fade to black started");
		yield return new WaitForSeconds(blackScreen.fadeSpeed);
		logFileHandler.eventFile.write("Fade to black ended");

		logFileHandler.eventFile.write("Ending probe tour ");
		yield return new WaitForSeconds(.2f);		// wait before next tour starts
		_followPlayer.enabled = false;              // disable follow function of main camera

		}

	IEnumerator spotTheDifference() {

		cityParent.SetActive(false);        // disable city environment

		logFileHandler.results.writeHeader("Distractor task");
		logFileHandler.results.writeHeader("Time,ImageNr,Correct");

		int[] STDsequence = generateSTDTrialList();	// generate list

		// -- give short introduction
		logFileHandler.eventFile.write("Waiting for participant to start spot the difference");

		blueBackground.FadeToOpaque();
		stdIntroText.FadeToOpaque();

		while (!(Input.GetKey(KeyCode.UpArrow))) {
			yield return new WaitForSeconds(Time.deltaTime);
			}

		stdIntroText.FadeToClear();
		blueBackground.FadeToClear();
		yield return new WaitForSeconds(blueBackground.fadeSpeed);

		logFileHandler.eventFile.write("Starting spot the difference run");

		for (int listIdx = 0; listIdx < numSTDTrials; listIdx++) {

			int stdTrialNr = STDsequence[listIdx];

			logFileHandler.eventFile.write("Starting spot the difference trial " + listIdx.ToString());
			logFileHandler.eventFile.write("Picture number " + stdTrialNr.ToString());
			std_l.texture = (Texture2D)Resources.Load("Images/std/" + stdTrialNr.ToString("0") + "_l");
			std_r.texture = (Texture2D)Resources.Load("Images/std/" + stdTrialNr.ToString("0") + "_r");

			std_l.CrossFadeAlpha(1f, 0.05f, false);
			std_r.CrossFadeAlpha(1f, 0.05f, false);

			// define correct button based on list
			KeyCode correctButton = KeyCode.Return;
			if( Array.IndexOf(std_diffTrials, stdTrialNr) != -1) {	// different trial
				correctButton = KeyCode.RightArrow;
				}
			else {
				correctButton = KeyCode.LeftArrow;					// same trial
				}

			sameDiffText_same.FadeToOpaque();
			sameDiffText_diff.FadeToOpaque();

			logFileHandler.eventFile.write("Waiting for participant response");

			// waiting for user input
			double reactionTime = 0f;
			double startTime = globalTimer.paradigmTime;
			while (!((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))) && reactionTime < stdReactionTime) {
				yield return new WaitForSeconds(Time.deltaTime);
				reactionTime = globalTimer.paradigmTime - startTime;
				}

			// which button has been pressed?
			int trialCorrect = 0;                           // is trial correct (for performance log file)
			if (Input.GetKey(KeyCode.LeftArrow)) {
				logFileHandler.eventFile.write("Left arrow pressed");
				if (correctButton == KeyCode.LeftArrow) {
					logFileHandler.eventFile.write("Correct button was pressed!");
					trialCorrect = 1;
					}
				else {
					logFileHandler.eventFile.write("Wrong button was pressed!");
					}
				sameDiffText_same.ChangeColor(new Color(.5f, .75f, 1f));
				}
			else if (Input.GetKey(KeyCode.RightArrow)) {
				logFileHandler.eventFile.write("Right arrow pressed");
				if (correctButton == KeyCode.RightArrow) {
					logFileHandler.eventFile.write("Correct button was pressed!");
					trialCorrect = 1;
					}
				else {
					logFileHandler.eventFile.write("Wrong button was pressed!");
					}
				sameDiffText_diff.ChangeColor(new Color(.5f, .75f, 1f));
				}
			else {
				logFileHandler.eventFile.write("No button pressed");
				}

			// is reaction time smaller than maximum reaction time? if so, wait remaining time
			if (reactionTime < stdReactionTime) {
				logFileHandler.eventFile.write("Participant pressed button before end of std trial reaction time, waiting for " + (stdReactionTime - reactionTime).ToString("0.00") + " seconds");        
				yield return new WaitForSeconds((float)(stdReactionTime - reactionTime));			// wait remaining time	
				}

			sameDiffText_diff.ChangeColor(new Color(1f, 1f, 1f));
			sameDiffText_same.ChangeColor(new Color(1f, 1f, 1f));

			sameDiffText_same.FadeToClear();
			sameDiffText_diff.FadeToClear();
			std_l.CrossFadeAlpha(0f, 0.05f, false);
			std_r.CrossFadeAlpha(0f, 0.05f, false);

			logFileHandler.results.write(stdTrialNr.ToString() + "," + trialCorrect.ToString());

			// fade blackscreen and arrows
			blackScreen.FadeToOpaque();

			logFileHandler.eventFile.write("Fade to black started");
			yield return new WaitForSeconds(blackScreen.fadeSpeed);
			logFileHandler.eventFile.write("Fade to black ended");

			// display ITI
			float rndDisplayTime = .5f;						// display iti for .5s
			cross_iti.FadeToOpaque();
			logFileHandler.eventFile.write("ITI started, time: " + rndDisplayTime.ToString("0.000"));
			yield return new WaitForSeconds(rndDisplayTime);
			logFileHandler.eventFile.write("ITI ended");
			cross_iti.FadeToClear();

			}

		std_l.CrossFadeAlpha(0f, 0f, false);
		std_r.CrossFadeAlpha(0f, 0f, false);
		}

	IEnumerator probeTrials(int probeTrialList) {

		// -- give short introduction
		probeIntroText.FadeToOpaque();
		blueBackground.FadeToOpaque();

		logFileHandler.eventFile.write("Waiting for participant to start probe trials");
		while (!(Input.GetKey(KeyCode.UpArrow))) {
			yield return new WaitForSeconds(Time.deltaTime);
			}

		probeIntroText.FadeToClear();
		blueBackground.FadeToClear();
		yield return new WaitForSeconds(probeIntroText.fadeSpeed);

		List<int> probeTrialOrder_block = new List<int>(probeTrialOrder_all[probeTrialList]);

		// --- probe trials ---

		for (int probeTrialNr = 0; probeTrialNr < probeTrialOrder_block.Count; probeTrialNr++) {

			int probeTrialwP = probeTrialOrder_block[probeTrialNr];       // get the current waypoint location from the desired order list of probe trials 

			// get transform of current waypoint
			Transform currentProbeWaypointTransform = waypoints_probe[probeTrialwP].transform;

			// get possible directions at current probe waypoint
			List<char> currentProbeDirections = new List<char>();
			currentProbeDirections = waypoints_probe[probeTrialwP].GetComponent<probeProps>().directions;

			logFileHandler.eventFile.write("Starting probe trial nr: " + probeTrialNr.ToString("0"));
			logFileHandler.eventFile.write("Starting probe trial of waypoint: " + probeTrialwP.ToString("0"));

			// move camera and navMeshAgent to start of probe trial
			logFileHandler.eventFile.write("Setting start position of probe waypoint");
			mainCameraTransform.rotation = currentProbeWaypointTransform.rotation;
			mainCameraTransform.position = new Vector3(currentProbeWaypointTransform.position.x, 1.8f, currentProbeWaypointTransform.position.z);
			this.transform.position = new Vector3(currentProbeWaypointTransform.position.x, 1.8f, currentProbeWaypointTransform.position.z);
			this.transform.rotation = currentProbeWaypointTransform.rotation;

			// move camera and navagent some meters back
			if (!waypoints_probe[probeTrialwP].GetComponent<probeProps>().startCloser) {		// if startCloser bool is not set, use the distance specified in editor
			mainCameraTransform.Translate(new Vector3(0f, 0f, -distanceProbeTrials));
			this.transform.Translate(new Vector3(0f, 0f, -distanceProbeTrials));
				}
			else {
				mainCameraTransform.Translate(new Vector3(0f, 0f, -distanceProbeTrials + 4f));	// if startCloser is set, start a bit close to the center of the intersection (to avoid weird navagent behavior)
				this.transform.Translate(new Vector3(0f, 0f, -distanceProbeTrials + 4f));
				}

			// fade blackscreen
			blackScreen.FadeToClear();
			logFileHandler.eventFile.write("Fade to black started");
			yield return new WaitForSeconds(blackScreen.fadeSpeed);
			logFileHandler.eventFile.write("Fade to black ended");

			// move them to the center of the intersection
			playerNavAgent.enabled = true;													// disable navAgent so it can be repositioned to start
			path = new UnityEngine.AI.NavMeshPath();														// reset path
			playerNavAgent.CalculatePath(currentProbeWaypointTransform.position, path);		// calculate path

			yield return new WaitForSeconds(waitBeforeMoveTime);
			playerNavAgent.speed = agentSpeed;							// set speed
			playerNavAgent.autoBraking = true;							// enable auto braking
			_followPlayer.enabled = true;								// let the camera follow

			logFileHandler.eventFile.write("Moving to intersection");
			playerNavAgent.SetPath(path);								// go to waypoint

			float distanceToWp = distanceProbeTrials;
			while (distanceToWp > .05f) {
				distanceToWp = Vector2.Distance(new Vector2(mainCameraTransform.position.x, mainCameraTransform.position.z), new Vector2(currentProbeWaypointTransform.position.x, currentProbeWaypointTransform.position.z));
				yield return new WaitForSeconds(Time.deltaTime);
				}
			logFileHandler.eventFile.write("Intersection reached");

			playerNavAgent.ResetPath();                 // clear current path of navagent (otherwise it will start immediatley next tour)
			playerNavAgent.enabled = false;             // disable navAgent so it can be repositioned to start
			_followPlayer.enabled = false;              // disable following of camera

			// display different direction arrows depending on settings
			arrowBG.ChangeColor(new Color(1f, 1f, 1f));                 // change color of arrow BG back to white
			arrowBG.FadeToOpaque();										// background for arrows

			if (currentProbeDirections.Contains('l')) {
				arrow_l.FadeToOpaque();
				}
			if (currentProbeDirections.Contains('r')) {
				arrow_r.FadeToOpaque();
				}
			if (currentProbeDirections.Contains('t')) {
				arrow_t.FadeToOpaque();
				}

			logFileHandler.eventFile.write("Fade of directions panel started");
			yield return new WaitForSeconds(arrowBG.fadeSpeed);
			logFileHandler.eventFile.write("Fade of directions panel ended");

			KeyCode correctButton = waypoints_probe[probeTrialwP].GetComponent<probeProps>().correctButton; // get the correct button for this trial
			KeyCode givenButton = KeyCode.Return;                                                           // create variable for given response
			int trialCorrect = 0;                                                                           // is trial correct (for performance log file)

			// waiting for user input
			double reactionTime = 0f;
			double startTime = globalTimer.paradigmTime;
			while (!((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))) && reactionTime < probeTrialReactionTime) {
				yield return new WaitForSeconds(Time.deltaTime);
				reactionTime = globalTimer.paradigmTime - startTime;
				}

			// which button has been pressed?
			if (Input.GetKey(KeyCode.UpArrow)) {
				logFileHandler.eventFile.write("Up arrow pressed");
				givenButton = KeyCode.UpArrow;
				if (correctButton == KeyCode.UpArrow) {
					logFileHandler.eventFile.write("Correct button was pressed!");
					trialCorrect = 1;
					}
				}
			else if (Input.GetKey(KeyCode.LeftArrow)) {
				logFileHandler.eventFile.write("Left arrow pressed");
				givenButton = KeyCode.LeftArrow;
				if (correctButton == KeyCode.LeftArrow) {
					logFileHandler.eventFile.write("Correct button was pressed!");
					trialCorrect = 1;
					}
				}
			else if (Input.GetKey(KeyCode.RightArrow)) {
				logFileHandler.eventFile.write("Right arrow pressed");
				givenButton = KeyCode.RightArrow;
				if (correctButton == KeyCode.RightArrow) {
					logFileHandler.eventFile.write("Correct button was pressed!");
					trialCorrect = 1;
					}
				}
			else {
				logFileHandler.eventFile.write("No button pressed");
				}

			// is reaction time smaller than maximum reaction time? if so, wait remaining time
			if (reactionTime < probeTrialReactionTime) {
				logFileHandler.eventFile.write("Participant pressed button before end of probe trial reaction time, waiting for " + (probeTrialReactionTime - reactionTime).ToString("0.00") + " seconds");
				arrowBG.ChangeColor(new Color(.5f, .75f, 1f));                                      // change BG color of arrows         
				yield return new WaitForSeconds((float)(probeTrialReactionTime - reactionTime));    // wait remaining time	
				}

			fadeArrowsToClear();

			// write to performance log file
			logFileHandler.results.write(probeTrialNr.ToString("0") + "," + probeTrialwP.ToString("0") + "," + correctButton.ToString() + "," + givenButton.ToString() + "," + trialCorrect.ToString("0"));

			// fade blackscreen and arrows
			blackScreen.FadeToOpaque();

			logFileHandler.eventFile.write("Fade to black started");
			yield return new WaitForSeconds(blackScreen.fadeSpeed);
			logFileHandler.eventFile.write("Fade to black ended");

			yield return new WaitForSeconds(.5f);                               // wait a short time before ITI

			// display ITI
			float rndDisplayTime = (float)(2d + rnd.NextDouble() * 4d);         // gives a random time for the ITI interval between 2 and 6 seconds
			cross_iti.FadeToOpaque();
			logFileHandler.eventFile.write("ITI started, time: " + rndDisplayTime.ToString("0.000"));
			yield return new WaitForSeconds(rndDisplayTime);
			logFileHandler.eventFile.write("ITI ended");
			cross_iti.FadeToClear();
			logFileHandler.eventFile.write("Ending probe trial nr: " + probeTrialNr.ToString("0"));

			_followPlayer.rotation = true;                  // re-enable rotation following
			playerNavAgent.enabled = false;                 // disable navAgent
			_followPlayer.enabled = false;                  // disable follow function of main camera
			}
		}
	#endregion

	// --- some ressource functions go here ---

	#region Additional helper methods

	IEnumerator rotateNavAgentTowardsTarget(Quaternion target) {

		Quaternion startRot = gameObject.transform.rotation;
	
		for (float t = 0f; t < 1f; t+= 1 * Time.deltaTime) {
			gameObject.transform.rotation = Quaternion.Lerp(startRot, target, t);
			yield return null;
			}
		}

	void findAndPlaceMovingGameObjects(int blockNr) {

		// using a bit of a detour to find all the gameObjects with specified tags (FindGameObjectsWithTag() will only find active gameObjects!!!
		GameObject parentGameObject = GameObject.Find("city/movingProps");
		List<GameObject> movingObjects_list = new List<GameObject>();

		foreach (Transform child in parentGameObject.transform) {
			movingObjects_list.Add(child.gameObject);
			}

		movingObjects_list.ToArray();	// convert List<> to array

		// now go through each of the gameobjects and set their position/visibility
		foreach (GameObject obj in movingObjects_list) {

			// load position and rotation
			Vector3 newPosition = obj.GetComponent<blockPositions>().positionList[blockNr];
			Quaternion newRotation = obj.GetComponent<blockPositions>().rotationList[blockNr];

			// set
			obj.transform.position = newPosition;
			obj.transform.rotation = newRotation;

			// load visibility
			obj.SetActive(obj.GetComponent<blockPositions>().visibility[blockNr]);
			}
		}

	void fadeArrowsToClear() {
	
		arrowBG.FadeToClear();
		arrow_l.FadeToClear();
		arrow_r.FadeToClear();
		arrow_t.FadeToClear();
		}

	private int[][] generateProbeTrialList() {

		// init lists of different kinds of probe landmarks (lm, intermediate, same, diff directions...)
		List<int> lmTour = new List<int>();			// at landmarks, in tour direction
		List<int> lmDiff = new List<int>();			// at landmarks, in different direction
		List<int> intermTour = new List<int>();		// between landmarks, in tour direction
		List<int> intermDiff = new List<int>();		// between landmarks, in different direction

		int blockLength = lmProbeTrials_tourDir * numLm + lmProbeTrials_diffDir * numLm + intermProbeTrials_tourDir + intermProbeTrials_diffDir;	// how long will each block be?

		int[][] probeTrialSequence = new int[minimumCycles][];  // init jagged array for whole probe trial sequence ([blocks],[sequence for block])

		for (int i = 0; i < probeTrialSequence.Length; i++) {
			probeTrialSequence[i] = new int[blockLength];		// init array inside jagged array for each block
			}

		// generate lists of different kinds of probe landmarks (lm, intermediate, same, diff directions...)
		for (int i = 0; i < waypoints_probe.Count; i++) {
			// if a landmark and in tour direction
			if (waypoints_probe[i].GetComponent<probeProps>().isTourDirection && waypoints_probe[i].GetComponent<probeProps>().isLmIntersection) {
				lmTour.Add(i);
				}
			// if landmark but other direction
			else if (waypoints_probe[i].GetComponent<probeProps>().isTourDirection == false && waypoints_probe[i].GetComponent<probeProps>().isLmIntersection) {
				lmDiff.Add(i);
				}
			// if intermediate and in tour direction
			else if (waypoints_probe[i].GetComponent<probeProps>().isTourDirection && waypoints_probe[i].GetComponent<probeProps>().isLmIntersection == false) {
				intermTour.Add(i);
				}
			else {
				intermDiff.Add(i);
				}
			}

		// now go through the list of all waypoints and put them into according list
		// iterate through blocks
		for (int blockNr = 0; blockNr < minimumCycles; blockNr++) {

			List<int> blockSequence = new List<int>();    // init block sequence

			// add lm probe trials in tour direction
			for (int i = 0; i < lmProbeTrials_tourDir; i++) {
				blockSequence.AddRange(lmTour);
				}

			for (int i = 0; i < lmProbeTrials_diffDir; i++) {
				blockSequence.AddRange(lmDiff);
				}

			for (int i = 0; i < intermProbeTrials_tourDir; i++) {

				bool waypointFound = false;		// bool to indicate if a suitable waypoint has been found

				while (!waypointFound) {
					int waypointNr = intermTour[rnd.Next(intermTour.Count)];

					if (blockSequence.Contains(waypointNr)) {
						waypointFound = false;
						}
					else {
						blockSequence.Add(waypointNr);
						waypointFound = true;
						}
					}
				}

			for (int i = 0; i < intermProbeTrials_diffDir; i++) {

				bool waypointFound = false;     // bool to indicate if a suitable waypoint has been found

				while (!waypointFound) {
					int waypointNr = intermDiff[rnd.Next(intermDiff.Count)];

					if (blockSequence.Contains(waypointNr)) {
						waypointFound = false;
						}
					else {
						blockSequence.Add(waypointNr);
						waypointFound = true;
						}
					}
				}

			// randomly shuffle the list until criteria have been met
			bool sequenceFound = false;

			while (!sequenceFound) {

				bool consecutiveFound = false;
				blockSequence.Shuffle();

				// go through the blocksequence and check for consecutive probe trial waypoint numbers
				for(int i = 0; i < blockSequence.Count - 1; i++ ) {

					// check if current probe trial number is -1 or +1 of the next one (consecutive)
					if (blockSequence[i] == blockSequence[i + 1] + 1 || blockSequence[i] == blockSequence[i + 1] - 1) {
						consecutiveFound = true;	// if next element is same or consecutive landmark, set the flag true
						}
					}

				// if no consecutive element have been found, set sequenceFound to true
				if (!consecutiveFound) {
					sequenceFound = true;
					}
				}
						
			probeTrialSequence[blockNr] = blockSequence.ToArray();		// add blocksequence to probetrial sequence as array

			}

		return probeTrialSequence;
		}	

	private int[] generateSTDTrialList () {

		List<int> stdTrialSequence = new List<int>();	// temporary list containing the sequence for std trials

		// fill the list with according number of trials
		for (int listIdx = 0; listIdx < numSTDTrials; listIdx++) {
			stdTrialSequence.Add(listIdx);
			}

		stdTrialSequence.Shuffle();     // shuffle the list

		return stdTrialSequence.ToArray();
		}
	#endregion
}