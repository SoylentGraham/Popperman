using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Render : MonoBehaviour {

	[System.Serializable]
	public class TickAnimation
	{
		public int				Duration;	//	life in ticks
		public PopperMan.Tile	Tile;
		public int2				xy;
	}


	public Material	MapShader;

	[InspectorButton("UpdateMapShader")]
	public bool		_UpdateMapShader;

	public List<TickAnimation>	Animations = new List<TickAnimation>();

	void Update()
	{
		UpdateMapShader();
	}

	public void AddBombExplodeAnim(Game.Bomb Bomb)
	{
		System.Action<int2> MakeBombAnim = (xy) => {
			var Anim = new TickAnimation ();
			Anim.xy = xy;
			Anim.Tile = PopperMan.Tile.Flame;
			Anim.Duration = 2;
			Animations.Add( Anim );
		};

		foreach (var FlamePos in Bomb.Flames)
		{
			MakeBombAnim ( FlamePos );
		}

	}

	public void UpdateAnimationTick()
	{
		for ( int a=Animations.Count-1;	a>=0;	a-- )
		{
			var Anim = Animations [a];
			Anim.Duration--;

			//	gr: this may have started on this frame, and this end of tick is triggered at start, so let it go to 0
			if (Anim.Duration >= 0)
				continue;

			Animations.RemoveAt(a);
		}
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

			//	check for anims
			foreach (var Anim in Animations) {
				if (Anim.xy != xy)
					continue;
				Tile = Anim.Tile;
			}

			Tiles.Add( (float)Tile );
		}
		MapShader.SetFloatArray("Tiles", Tiles );

		UnityEditor.SceneView.RepaintAll();
	}
}
