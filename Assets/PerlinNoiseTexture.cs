using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PerlinNoiseTexture : MonoBehaviour {

	public RenderTexture	NoiseTexture;

	void Update ()
	{
		var w = NoiseTexture.width;
		var h = NoiseTexture.height;

		var Noise = new Texture2D( w, h, TextureFormat.RGBAFloat, false );
		for ( int x=0;	x<w;	x++ )
		{
			for ( int y=0;	y<h;	y++ )
			{
				var r = Mathf.PerlinNoise( x/(float)w, y/(float)h );
				var g = Random.Range(0,1.0f);
				var b = Random.Range(0,1.0f);
				var a = 1.0f;
				Noise.SetPixel( x, y, new Color(r,g,b,a) );
			}
		}
		Noise.Apply();

		Graphics.Blit( Noise, NoiseTexture );
	
		this.enabled = false;	
	}
	
}
