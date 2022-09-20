// this creates an editor window with which the different positions for the blocks can be stored and retrieved


#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



public class takeLandmarkScreenshots : EditorWindow {
	public int blockNr;

	private string messageText = " ";

	// - main camera -
	private GameObject mainCamera;                          // mainCamera
	private Transform mainCameraTransform;                  // mainCamera Transform

	private GameObject landMark_source;               // parent gameObject that has all the pointing positions (source) as childs
	private List<GameObject> landmarkIntersections;			// list that will be populated by all the childs of pointPositions gameObject

	// Add menu item named "My Window" to the Window menu
	[MenuItem("Window/Landmark screenshots")]
	public static void ShowWindow() {
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(takeLandmarkScreenshots));
		}

	void OnGUI() {

		EditorGUILayout.Separator();
		EditorGUILayout.Separator();

		if (GUILayout.Button("Take screenshots")) {

			// populate list of waypoints (each is a gameObject) for source positions
			landMark_source = GameObject.Find("waypoints/" + 5.ToString("0") + "_lm/pointingTask_sources");
			landmarkIntersections = new List<GameObject>();
			foreach (Transform child in landMark_source.transform) {
				landmarkIntersections.Add(child.gameObject);
				}
			}

			// go through all the landmarks, place camera

		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Status:", messageText);
		}
	
	}

#endif