using UnityEngine;
using System.Collections;

public class MeshAnimationControllerExample : MonoBehaviour {

	public TPMeshAnimation anim;
	
	
	
	
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
	}
}
