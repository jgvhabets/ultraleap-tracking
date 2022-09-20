using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class fadeInOut : MonoBehaviour {

	public float fadeSpeed = 1.5f;
	[SerializeField] private Component toFade;
	[SerializeField] private float initialAlpha = 0f;
	[SerializeField] private float clearAlpha = 0f;
	[SerializeField] private float obliqueAlpha = 1;

	// Use this for initialization
	void Awake () {

		// check if component is Image or RawImage, otherwise through error
		if (toFade.GetType() != typeof(Image) && toFade.GetType() != typeof(RawImage) && toFade.GetType() != typeof(Text)) {
			Debug.LogError("--> FadeInOut script has been given something other than Image or RawImage, other types currently not supported.");
			}
		
		// set initial alpha
		if (toFade.GetType() == typeof(Image)) {
			Image fadeImg = (Image)toFade;
			fadeImg.CrossFadeAlpha(initialAlpha, 0f, false);
			}
		else if (toFade.GetType() == typeof(RawImage)) {
			RawImage fadeImg = (RawImage)toFade;
			fadeImg.CrossFadeAlpha(initialAlpha, 0f, false);
			}
		else if (toFade.GetType() == typeof(Text)){
			Text fadeImg = (Text)toFade;
			fadeImg.CrossFadeAlpha(initialAlpha, 0f, false);
			}
		}

	public void FadeToClear() {
		// find out which kind of component is attached to the gameobject to be faded in order to fade it
		if (toFade.GetType() == typeof(Image)) {
			Image fadeImg = (Image)toFade;
			fadeImg.CrossFadeAlpha(clearAlpha, fadeSpeed, false);
			}
		else if (toFade.GetType() == typeof(RawImage)) {
			RawImage fadeImg = (RawImage)toFade;
			fadeImg.CrossFadeAlpha(clearAlpha, fadeSpeed, false);
			}
		else if (toFade.GetType() == typeof(Text)) {
			Text fadeImg = (Text)toFade;
			fadeImg.CrossFadeAlpha(clearAlpha, fadeSpeed, false);
			}
		}

	public void FadeToOpaque() {
		// find out which kind of component is attached to the gameobject to be faded in order to fade it
		if (toFade.GetType() == typeof(Image)) {
			Image fadeImg = (Image)toFade;
			fadeImg.CrossFadeAlpha(obliqueAlpha, fadeSpeed, false);
			}
		else if (toFade.GetType() == typeof(RawImage)) {
			RawImage fadeImg = (RawImage)toFade;
			fadeImg.CrossFadeAlpha(obliqueAlpha, fadeSpeed, false);
			}
		else if (toFade.GetType() == typeof(Text)) {
			Text fadeImg = (Text)toFade;
			fadeImg.CrossFadeAlpha(obliqueAlpha, fadeSpeed, false);
			}
		}

	public void ChangeColor(Color targetColor) {
		// find out which kind of component is attached to the gameobject to be faded in order to fade it
		if (toFade.GetType() == typeof(Image)) {
			Image fadeImg = (Image)toFade;
			fadeImg.CrossFadeColor(targetColor, 0f, false, false);
			}
		else if (toFade.GetType() == typeof(RawImage)) {
			RawImage fadeImg = (RawImage)toFade;
			fadeImg.CrossFadeColor(targetColor, 0f, false, false);
			}
		else if (toFade.GetType() == typeof(Text)) {
			Text fadeImg = (Text)toFade;
			fadeImg.CrossFadeColor(targetColor, 0f, false, false);
			}
		}
}
