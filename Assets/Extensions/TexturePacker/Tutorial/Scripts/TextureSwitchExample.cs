using UnityEngine;
using System.Collections;

public class TextureSwitchExample : MonoBehaviour {
	
	public TPHelper helper;
	public string AtlasPath1 = "example";
	public string AtlasPath2 = "TutorialAtlas";
	
	
	
	void OnGUI() {
		
		if(GUI.Button(new Rect(20, 20, 140, 50), "Set Texture 1")) {
			if(!helper.atlasPath.Equals(AtlasPath1))  {
				helper.SwitchAtlas(AtlasPath1);
			}
			
			helper.OnTextureChange("f_share");
		}
		
		
		if(GUI.Button(new Rect(20, 90, 140, 50), "Set Texture 2")) {
			if(!helper.atlasPath.Equals(AtlasPath1))  {
				helper.SwitchAtlas(AtlasPath1);
			}
			

			helper.OnTextureChange("t_share");
			
		}
		
		if(GUI.Button(new Rect(20, 160, 140, 50), "Set Texture 3")) {
			if(!helper.atlasPath.Equals(AtlasPath1))  {
				helper.SwitchAtlas(AtlasPath1);
			}
			
			helper.OnTextureChange("b_share");
		}
		
		
		if(GUI.Button(new Rect(20, 240, 200, 50), "Set Texture 3 (another atlas)")) {
			if(!helper.atlasPath.Equals(AtlasPath2))  {
				helper.SwitchAtlas(AtlasPath2);
			}
			
			helper.OnTextureChange("fireball_0001");
		}
		
		
	}
}
