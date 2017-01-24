using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Render : MonoBehaviour {

	public Material	MapShader;

	[InspectorButton("UpdateMapShader")]
	public bool		_UpdateMapShader;


	void Update()
	{
		UpdateMapShader();
	}


	public void UpdateMapShader()
	{
		var game = GameObject.FindObjectOfType<Game>();
		var map = GameObject.FindObjectOfType<Map>();

		MapShader.SetInt("Width", map.Width );
		MapShader.SetInt("Height", map.Height );
		
		var Tiles = new List<float>();
		for ( int i=0;	i<map.Width*map.Height;	i++ )
		{
			var xy = map.GetMapXy(i);
			var Tile = game.GetTile(xy);
			Tiles.Add( (float)Tile );
		}
		MapShader.SetFloatArray("Tiles", Tiles );

		UnityEditor.SceneView.RepaintAll();
	}
}
