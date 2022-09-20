/*
 DZNE UI Sample Project
 
This class is writing a text file for each participant with unique parameters ID+Name+Age.

1. Setup UI Canvas.
2. Create a UI Panel. Add all texts, input fields and buttons as part of panel. Panel is expandable. 
3. You can add new fields by creating new or just duplicating old ones and setting their position from editor inspector.
4. Add this script to Main Camera/ Any game object you want. 
5. Submit button = Assign script and the function which should be called when button is clicked from inspector button properties.
6. The user log files will be saved in Assets/Logs folder. You can change the path or folder name from script and project tab.


 Asema Hassan
 06.10.2015*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class getParticipantInfo : MonoBehaviour {

	[SerializeField] private string sceneToLoad = "mainExperiment";         // which scene to load

	public static Dictionary<string,string> UserProfileDict = new Dictionary<string,string> ();
	List<string> keysList = null;

	// keep track of current participant info, parameters used to create unique file names
	string _ID= null;
	string _Name = null;

	// path of current user file
	public static string filePath_subj = null;  // path to participant folder (inside this folder, pointing and tour folders will be created)
    string fileName;							// name of participant log file

    bool hasEmptyField;							// bool to detect if a field is empty

    private EventSystem system;					// Eventsystem to allow for tab between input fields

	// get player gameobject and scripts attached to enable/disable the different scripts (start tour or pointing task)
	private controlExperiment controlExperimentScript;

	// loading text
	private Text loadingText;

    private void Start() {
        system = EventSystem.current;												// get current eventSystem
		loadingText = GameObject.Find("Canvas/loadingText").GetComponent<Text>();   // get loading text

		DontDestroyOnLoad(this.gameObject);		// do not destroy this camera on level load (so that it can disable/enable tour or pointing task script)
    }

    // Call this method when the user hit submit button
    public void ReadAllDataFromFields() {

        UserProfileDict = new Dictionary<string, string>();

        // Find all fields in scene with following tags and store them in hashtable
        GameObject[] allTextFields = FindObsWithTag("Text_Fields");     // as keys
        GameObject[] allInputFields = FindObsWithTag("Input_Fields");   // as values

        hasEmptyField = false;

        if (allTextFields != null && allInputFields != null) {
            // Just make sure you have equal number of text fields and input fields
            for (int i = 0; i < allTextFields.Length; i++) {
                GameObject oneField = allTextFields[i].gameObject;
                GameObject userInput = allInputFields[i].gameObject.transform.FindChild("Text").gameObject;
                Text userText = userInput.GetComponent<Text>();
                string fieldValue = userText.text;

				// detect if any field is empty
				if (fieldValue == "") {
					hasEmptyField = true;
					loadingText.text = ("Empty fields!");
					}

				UserProfileDict.Add(oneField.name, fieldValue);
                }
            }

		// start tour or pointing task, depending on which button has been pressed
		if (hasEmptyField == false) {

			CreateParticipantFileAndWriteData();	// write participant data and create path

			StartCoroutine(loadScene());				// load scene

			}
        else
            {
            Debug.LogError("Empty fields!");
            }
	    
    }

	private IEnumerator loadScene() {

		AsyncOperation loadMainScene = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);           // load main scene

		while (!loadMainScene.isDone) {
			float progress = loadMainScene.progress * 100f;
			loadingText.text = ("Loading scene: " + progress.ToString("0.00") + "% / 100%");
			yield return new WaitForSeconds(Time.deltaTime);
			}

		Destroy(this.gameObject);	// destroy the main camera of ui

		}
			

	// this function is called after submit button has been pressed and no fields are empty
	private void CreateParticipantFileAndWriteData(){
		
		// Acquire keys and sort them.
		keysList = UserProfileDict.Keys.ToList ();
		keysList.Sort ();

		// Pick ID, Name and Age from Dictionary and create file with parameters, assuming the order of parameters is same from UI Canvas 1. ID, 2.Name, 3. Gender , 4. Age
		foreach (string key in keysList) {
			string[] keyText = key.Split ('.');
			string nameOfKey = keyText [1];
			string valueOfKey =  UserProfileDict [key];

			if (String.Equals(nameOfKey,"Number"))
				_ID = valueOfKey;
			else if(String.Equals(nameOfKey,"Name"))
				_Name = valueOfKey;
			}
			
		filePath_subj = getFilePath (_ID,_Name);

        fileName = filePath_subj + "/participantData.csv";

		// If file doesnot exist create new and write header once.
		if (!File.Exists (fileName)) {
			// Loop through keys and write data.
			foreach (string key in keysList) {
				string[] keyText = key.Split ('.');
                File.AppendAllText (fileName, System.String.Format (keyText [1] + "," + UserProfileDict [key] + Environment.NewLine));
			}
            File.AppendAllText(fileName, System.String.Format("Current day and time,{0}", DateTime.Now));	// write current date and time into file as well
		}
	}

	// Customised method to get objects by tag and sort them in order
	private GameObject[] FindObsWithTag( string tag ){
		GameObject[] foundObs = GameObject.FindGameObjectsWithTag(tag);
		Array.Sort( foundObs, CompareObNames );
		return foundObs;
	}

	// Compare game objects name and sort by order
	private int CompareObNames( GameObject x, GameObject y ){
		return x.name.CompareTo( y.name );
	}

	// Following method is used to retrieve the relative path as device platform
	private string getFilePath(string id, string name){

        DateTime cDT = DateTime.Now;
        string partPath = Application.dataPath + "/logs/" + id + "_" + name + "_" + cDT.ToString("dd") + "_" + cDT.ToString("MM") + "_" + cDT.ToString("yyyy") + "_" + 
            cDT.ToString("HH") + "_" + cDT.ToString("mm");
		
		// create general directory with subj name etc + individual directories for tour and pointing
		if (!Directory.Exists(partPath))
            {
            Directory.CreateDirectory(partPath);
			}

        return partPath;
	}

    private void Update()
        {
        if (system.currentSelectedGameObject == null || !Input.GetKeyDown(KeyCode.Tab))
            return;

        Selectable current = system.currentSelectedGameObject.GetComponent<Selectable>();
        if (current == null)
            return;

        bool up = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Selectable next = up ? current.FindSelectableOnUp() : current.FindSelectableOnDown();

        // We are at the end or the beginning, go to either, depends on the direction we are tabbing in
        // The previous version would take the logical 0 selector, which would be the highest up in your editor hierarchy
        // But not certainly the first item on your GUI, or last for that matter
        // This code tabs in the correct visual order
        if (next == null)
            {
            next = current;

            Selectable pnext;
            if (up) while ((pnext = next.FindSelectableOnDown()) != null) next = pnext;
            else while ((pnext = next.FindSelectableOnUp()) != null) next = pnext;
            }

        // Simulate Inputfield MouseClick
        InputField inputfield = next.GetComponent<InputField>();
        if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(system));

        // Select the next item in the taborder of our direction
        system.SetSelectedGameObject(next.gameObject);
        }

    }