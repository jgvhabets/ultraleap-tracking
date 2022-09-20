// this creates an editor window with which the different positions for the blocks can be stored and retrieved


#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;



public class TourAssetPositionSelection : EditorWindow {
	public int blockNr;

	private string messageText = " ";

	// Add menu item named "My Window" to the Window menu
	[MenuItem("Window/Tour asset position selection")]
	public static void ShowWindow() {
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(TourAssetPositionSelection));
		}

	void OnGUI() {
		GUILayout.Label("Base settings", EditorStyles.boldLabel);
		string[] options = new string[] { "One", "Two", "Three", "Four" };
		blockNr = EditorGUILayout.Popup("Select block nr: ", blockNr, options);
		
		EditorGUILayout.Separator();

		GUILayout.Label("Choose action", EditorStyles.boldLabel);

		if (GUILayout.Button("Find and select all objects")) {

			GameObject[] movingObjects = findGameObjects();		// find all the gameobjects with specific tag (including inactive ones!)

			messageText = "Found " + movingObjects.Length.ToString() + " GameObjects";

			Selection.objects = movingObjects;

			}

		if (GUILayout.Button("Save positions")) {

			if (EditorUtility.DisplayDialog("Confirm action", "You are about to SAVE positions for block: " + (blockNr + 1).ToString(), "Yes", "No")) {
				GameObject[] movingObjects = findGameObjects();     // find all the gameobjects with specific tag (including inactive ones!)
				messageText = "Saving object positions and visibility for block: " + (blockNr + 1).ToString();
	
				foreach (GameObject obj in movingObjects) {

					// save position and rotation
					Vector3 currentPosition = obj.transform.position;
					Quaternion currentRotation = obj.transform.rotation;

					obj.GetComponent<blockPositions>().positionList[blockNr] = currentPosition;
					obj.GetComponent<blockPositions>().rotationList[blockNr] = currentRotation;

					}
				}
			else {
				messageText = "Aborted action";
				}
			}

		if (GUILayout.Button("Load positions")) {

			if (EditorUtility.DisplayDialog("Confirm action", "You are about to LOAD positions for block: " + (blockNr + 1).ToString(), "Yes", "No")) {

				messageText = "Loading object positions and visibility for block: " + (blockNr + 1).ToString();
				GameObject[] movingObjects = findGameObjects();     // find all the gameobjects with specific tag (including inactive ones!)

				foreach (GameObject obj in movingObjects) {

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
			else {
				messageText = "Aborted action";
				}
			}

		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Status:", messageText);
		}

	private GameObject[] findGameObjects() {
		// using a bit of a detour to find all the gameObjects with specified tags (FindGameObjectsWithTag() will only find active gameObjects!!!)

		GameObject parentGameObject = GameObject.Find("city/movingProps");
		List<GameObject> movingObjects_list = new List<GameObject>();

		foreach (Transform child in parentGameObject.transform) {
			movingObjects_list.Add(child.gameObject);
			}

		return movingObjects_list.ToArray();
		}

	}

#endif