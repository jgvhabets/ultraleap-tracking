using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class main : MonoBehaviour {

    /* class control flow of paradigm
    This script will run at the beginning and control other scripts/gameobjects
    First Start() will run and call track_hands()
    (c) Johannes Achtzehn, 07|2022, Charite - Universitaetsmedizin Berlin, johannes.achtzehn@charite.de
    */

	#region Input and global variables

	//// --- Global variables ---		
	public static List<string> header_items;    // will be used to store all header items, it is public so other function can access it
	public static string nan_string;		// this will be used in the logging if a hand is not detected
	#endregion

	// use this for initialization
	void Awake() {

		DateTime cDT = DateTime.Now;    // get the current date and time

		// create paths for the log files
		string logfile_path = Application.dataPath + "/log_files/" + 
			cDT.ToString("dd") + "_" + cDT.ToString("MM") + "_" + cDT.ToString("yyyy") + "_" +
			cDT.ToString("HH") + "_" + cDT.ToString("mm") + "_" + cDT.ToString("ss");
		logfile_handler.createParticipantDirectories(logfile_path);

		// write headers
		List<string> coordinates = new List<string> { "x", "y", "z" };
		List<string> fingers = new List<string> { "thumb", "index", "middle", "ring", "pinky" };
		List<string> joints = new List<string> { "metacarp", "interphal_prox", "interphal_dist", "tip" };
		List<string> additional_positions = new List<string> { "palm", "pinch_position", "pinch_pred_position"};
		List<string> additional_floats = new List<string> { "is_pinching", "pinch_strength", "pinch_distance", "grab_strength", "confidence" };
		
		string header = "global_time,program_time,delta_time,";
		List<string> header_items = new List<string>();

		// write header for fingers and joints
		foreach (string finger in fingers)
        {
			foreach (string joint in joints)
            {
				foreach(string coord in coordinates)
                {
					header += finger + "_" + joint + "_" + coord + ",";
					header_items.Add(finger + "_" + joint + "_" + coord);
					nan_string += "nan" + ",";
				}
            }
        }

		// write header for other positions
		foreach (string additional_position in additional_positions)
        {
			foreach (string coord in coordinates)
            {
				header += additional_position + "_" + coord + ",";
				header_items.Add(additional_position + "_" + coord);
				nan_string += "nan" + ",";
			}
        }

		// write header for floats
		foreach (string additional_float in additional_floats)
		{
			header += additional_float + ",";
			header_items.Add(additional_float);
			nan_string += "nan" + ",";
		}


		header = header.Remove(header.Length - 1);				// hack to remove the last comma from the header
		nan_string = nan_string.Remove(nan_string.Length - 1);	// hack to remove the last comma from the string of nans

		logfile_handler.pos_file_lh.writeHeader(header);
		logfile_handler.pos_file_rh.writeHeader(header);

		logfile_handler.event_file.writeHeader("global_time,program_time,event");

		StartCoroutine(track_hands());								// enable hands and start to track

		}

	IEnumerator track_hands() {

		yield return new WaitForSeconds(.5f);						// wait for a small time before starting to stabilize frame rate
		logfile_handler.event_file.write("Program started");

	}
}
