////////////////////////////////////////////////////////////////////////////////
//  
// @module Texture Packer Plugin for Unity3D 
// @author Osipov Stanislav lacost.st@gmail.com
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;



public class TPHelper : MonoBehaviour  {

	public bool replaceMaterial = true;
	public bool cloneMaterial = false;

	[HideInInspector] public int atlasID = 0;
	[HideInInspector] public int textureID = 0;

	private TPMeshTexture _meshTexture = null;

	//--------------------------------------
	// INITIALIZE
	//--------------------------------------


	void Awake() {
		if(cloneMaterial) {
			Material m = new Material(GetComponent<Renderer>().material);
			GetComponent<Renderer>().material = m;
		} 
	}

	
	//--------------------------------------
	// PUBLIC METHODS
	//--------------------------------------
	
	public void SwitchAtlas(string atlasPath) {
		meshTexture.atlas = atlasPath;
		if(replaceMaterial) {
			Material m = Resources.Load(atlasPath + "Material") as Material;

			if(gameObject.GetComponent<Renderer>().sharedMaterial != m) {
				gameObject.GetComponent<Renderer>().sharedMaterial = m;
			}
		}
	}
	
	public void SwitchTexture(string textureName) {
		OnTextureChange(textureName);
	}
	

	//--------------------------------------
	// GET / SET
	//--------------------------------------

	public TPMeshTexture meshTexture {
		get {
			if(_meshTexture == null) {
				_meshTexture = GetComponent<TPMeshTexture>();
			}
			return _meshTexture;
		}
	}
	
	//--------------------------------------
	// EVENTS
	//--------------------------------------

	public virtual void OnAtlasChange(string atlasName) {
		string path = TPAtlasesData.getAtlasPath(atlasName);
		SwitchAtlas(path);
	}
	
	
	public string atlasPath {
		get {
			return meshTexture.atlas;
		}
	}
	

	public void SetTexture(string textureName) {
		OnTextureChange(textureName);
	}

	public virtual void OnTextureChange(string textureName) {
		meshTexture.texture = textureName;

		meshTexture.UpdateView();
	}

	public virtual void OnAtlasChangeComplete() {
		OnTextureChange(gameObject.name);
	}
	
	//--------------------------------------
	// PRIVATE METHODS
	//--------------------------------------
	
	//--------------------------------------
	// DESTROY
	//--------------------------------------
}
