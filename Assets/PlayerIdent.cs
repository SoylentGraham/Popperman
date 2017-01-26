using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]
public class PlayerIdent : MonoBehaviour {

	public RectTransform	IdentCanvas;
	public Text				IdentCanvasText
	{
		get
		{
			return IdentCanvas.GetComponentInChildren<Text>();
		}
	}
	public Player player
	{
		get
		{
			return GetComponent<Player>();
		}

	}



	public void SetText(string NewIdent)
	{
		var text = IdentCanvasText;
		if ( !text )
			return;
		text.text = NewIdent;
	}


	void Update ()
	{
		var Renderer = GameObject.FindObjectOfType<Render>();
		var CanvasPos = Renderer.GetCanvasTileUv( player.xy );
		var rt = IdentCanvas.GetComponent<RectTransform>();
		var Pos3 = rt.localPosition;
		Pos3.x = CanvasPos.x;
		Pos3.y = CanvasPos.y;
		rt.localPosition = Pos3;
	}
}
