//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : GaffManager.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GaffManager : MonoBehaviour
{
	public bool isShowing = false;

	void OnGUI()
    {
		if (isShowing)
		{
			if(GUILayout.Button("Close"))
			{
				isShowing = false;
			}
			ToggleGaffOnScreen(isShowing);
		}
		else
		{
			if(GUILayout.Button("Press for gafff menu"))
            {
				isShowing = true;
			}
		}
    }
	public void ToggleGaffOnScreen(bool onOff)
    {
		if (onOff)
		{
			GUI.BeginGroup(new Rect(Vector2.zero, new Vector2(Screen.width * .75f, Screen.height * .75f)));
			Color bgColor = Color.grey;
			bgColor.a = .5f;
			GUI.backgroundColor = bgColor;
			if(GUILayout.Button("Free Spin"))
            {

            }
			if (GUILayout.Button("Overlay Spin"))
			{

			}
			GUI.EndGroup();
		}
		isShowing = true;
	}

	public void SelectStops()
    {

	}

	public void AutoPlay()
    {

	}



	//public void SetReels(Reel[] rReelConfiguration)
 //   {

	//}

	//public void SetMultiSpinReels(List<Reel[]> rReelConfiguration)
 //   {

	//}
}
