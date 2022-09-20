// BatchRename.cs
// Unity Editor extension that allows batch renaming for GameObjects in Hierarchy
// Via Alan Thorn (TW: @thorn_alan)

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;

public class BatchRename : ScriptableWizard {

	/// <summary>
	/// Parent GameObject
	/// </summary>
	public GameObject parent_GameObj;


	/// <summary>
	/// Start count
	/// </summary>
	public int StartNumber = 0;

	/// <summary>
	/// Increment
	/// </summary>
	public int Increment = 1;

	/// <summary>
	/// Base name
	/// </summary>
	public string BaseName = "";

	[MenuItem("Edit/Batch Rename...")]
	static void CreateWizard() {
		ScriptableWizard.DisplayWizard("Batch Rename", typeof(BatchRename), "Rename");
		}

	/// <summary>
	/// Called when the window first appears
	/// </summary>
	void OnEnable() {
		helpString = "Put all the gameObjects in one parent and paste that parent here:";
		}


	/// <summary>
	/// Rename
	/// </summary>
	void OnWizardCreate() {

		// Current Increment
		int PostFix = StartNumber;
		foreach (Transform child in parent_GameObj.transform) {
			child.gameObject.name = BaseName + PostFix.ToString("0");
			PostFix += Increment;
			}

		}
	}

#endif
