using UnityEngine;
using System.Collections;
using System.IO;

public class logFileHandler : MonoBehaviour {

	public static logFile eventFile = new logFile();			// event log file
	public static logFile results = new logFile();        // log file for performance during probe trials
	public static logFile posFile = new logFile();				// position log file

	public static void createParticipantDirectories(string filePath) {
		
		// check if logs folder exists, if not, create it
		if (!Directory.Exists(Application.dataPath + "/logs")) {
			Directory.CreateDirectory(Application.dataPath + "/logs");
			}

		// open up log files
		if (filePath != null)						// if participantInfoUI scene was run before, participant directory and log directory were already created
			{

			Directory.CreateDirectory(filePath);	// create directory for experiment phase

			eventFile.openFile(filePath + "/eventData.csv");
			posFile.openFile(filePath + "/positionData.csv");
			results.openFile(filePath + "/performanceData.csv");
			}
		else													// if the scene is run stand-alone, create non-participant specific directory
			{
			// check if logs folder exists, if not, create it
			if (!Directory.Exists(Application.dataPath + "/logs")) {
				Directory.CreateDirectory(Application.dataPath + "/logs");  
				}

			eventFile.openFile(Application.dataPath + "/logs/debug_eventData.csv");
			posFile.openFile(Application.dataPath + "/logs/debug_positionData.csv");
			results.openFile(Application.dataPath + "/logs/debug_performanceData.csv");
			}
		}

	public static void closeFiles() {
		eventFile.closeFile();
		results.closeFile();
		posFile.closeFile();
		}

	void OnApplicationQuit() {
		eventFile.closeFile();
		results.closeFile();
		posFile.closeFile();
		}

	}
