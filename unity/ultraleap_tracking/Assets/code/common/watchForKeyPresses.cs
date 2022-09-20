using UnityEngine;
using System.Collections;

public class watchForKeyPresses : MonoBehaviour {

	/* class to watch for key presses
    (c) Johannes Achtzehn, 07|2022, Charite - Universitaetsmedizin Berlin, johannes.achtzehn@charite.de
    */

	private GameObject handModels;
	private GameObject main_camera;
	private bool loggin_enabled;

	void Start() {

		handModels = GameObject.Find("HandModels");
		main_camera = GameObject.Find("Main Camera");
		loggin_enabled = false;

	}

	// Update is called once per frame
	void Update () {

        // if escape key is detected, close the application
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			logfile_handler.event_file.write("Application quit");
			Application.Quit();
		}

		if (Input.GetKeyDown(KeyCode.Space))
        {

			if (loggin_enabled)	// disable logging
			{ 
				logfile_handler.event_file.write("Tracking disabled");
				handModels.GetComponent<log_hand_position>().enabled = false;    // enable hand tracking
				loggin_enabled = false;
				main_camera.GetComponent<Camera>().backgroundColor = new Color(155f / 255f, 88f / 255f, 82f / 255f);
			}
			else   // enable logging
			{
				
				handModels.GetComponent<log_hand_position>().enabled = true;    // enable hand tracking
				logfile_handler.event_file.write("Tracking enabled");
				loggin_enabled = true;
				main_camera.GetComponent<Camera>().backgroundColor = new Color(82f / 255f, 155f / 255f, 109f / 255f);
			}
		}
		

	}
}
