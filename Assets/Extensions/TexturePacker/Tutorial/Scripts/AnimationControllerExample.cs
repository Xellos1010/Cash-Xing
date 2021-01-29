using UnityEngine;
using System.Collections;

public class AnimationControllerExample : MonoBehaviour {

	public TPSpriteAnimation anim;
	

	void Start() {
		anim.OnEnterFrame += OnEnterFrame;
		anim.OnAnimationComplete += OnAnimationComplete;
		anim.OnFadeAnimationComplete += OnFadeAnimationComplete;
	}

	
	void OnGUI() {
		Rect rect = new Rect(10, 10, 130, 40);
		
		
		if(GUI.Button(rect, "Play")) {
			anim.Play();
		}
		
		rect.y += 60;
		if(GUI.Button(rect, "Stop")) {
			anim.Stop();

		}
		
		rect.y += 60;
		if(GUI.Button(rect, "GoToAndPlay 2")) {
			anim.GoToAndPlay(2);
		}
		
		rect.y += 60;
		if(GUI.Button(rect, "GoToAndStop 2")) {
			anim.GoToAndStop(2);
		}

		rect.y += 60;
		if(GUI.Button(rect, "Fade Out")) {
			anim.AnimateSpriteOpacity(anim.opacity, 0f, 0.5f);
		}

		rect.y += 60;
		if(GUI.Button(rect, "Fade In")) {
			anim.AnimateSpriteOpacity(anim.opacity, 1f, 0.5f);
		}


	}

	void OnEnterFrame (int index) {
		//Debug.Log("Fame index: " + index);
	}

	void OnAnimationComplete (){
		Debug.Log("OnAnimationComplete");
	}

	void OnFadeAnimationComplete (){
		Debug.Log("OnFadeComplete");
	}
}
