using UnityEngine;
using System.Collections;
using System.IO;

public class logfile_handler : MonoBehaviour {

    /* class to open and close log files
    (c) Johannes Achtzehn, 07|2022, Charite - Universitaetsmedizin Berlin, johannes.achtzehn@charite.de
    */

	public static logfile event_file = new logfile();	            // event log file
	public static logfile pos_file_lh = new logfile();              // position log file for left hand
	public static logfile pos_file_rh = new logfile();              // position log file for left hand

	public static void createParticipantDirectories(string filePath) {
		
		// check if logs folder exists, if not, create it
		if (!Directory.Exists(filePath)) {
			Directory.CreateDirectory(filePath);
			}

		event_file.openFile(filePath + "/event_data.csv");
		pos_file_lh.openFile(filePath + "/position_data_lh.csv");
		pos_file_rh.openFile(filePath + "/position_data_rh.csv");
		
		}

	public static void closeFiles() {
		event_file.closeFile();
		pos_file_lh.closeFile();
		pos_file_rh.closeFile();
	}

	void OnApplicationQuit() {
		event_file.closeFile();
		pos_file_lh.closeFile();
		pos_file_rh.closeFile();
	}

	}
