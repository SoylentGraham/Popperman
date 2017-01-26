using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class UnityEvent_Bomb : UnityEngine.Events.UnityEvent <Game.Bomb> {}

[System.Serializable]
public class UnityEvent_Player : UnityEngine.Events.UnityEvent <Player> {}


[System.Serializable]
public class UnityEvent_int2 : UnityEngine.Events.UnityEvent <int2> {}


public class Game : MonoBehaviour {

	public class Bomb
	{
		public int		x {	get {return xy.x; } }
		public int		y {	get {return xy.y; } }
		public int2		xy;
		public int		StartFrame;
		public int		Duration;
		public Player	Player;
		public int		Radius;

		//	store flames upon explosion for anim
		public List<int2>	Flames = new List<int2>();
	};

	[InspectorButton("Tick")]
	public bool	_Tick;
	int Frame = 0;


	[Range(1,60)]
	public int		TicksPerSec = 5;
	float			TickCountdown = 0;
	float			TickMs {	get {	return 1000 / (float)TicksPerSec; } }
	//	the time (0...1) remaining before the next tick
	public float	FrameDelta { get	{	return TickCountdown / TickMs;	}	}


	public List<Player>	_Players;
	public List<Bomb>	Bombs = new List<Bomb>();
	public List<Player>	Players
	{
		get
		{
			if ( _Players == null || _Players.Count == 0 )
				_Players = new List<Player>( GameObject.FindObjectsOfType<Player>() );
			return _Players;
		}
	}

	[Header("Game events - eg sound")]
	public UnityEvent_Bomb					OnBombPlaced;
	public UnityEvent_Bomb					OnBombExplode;
	public UnityEvent_Player				OnPlayerDeathExplode;
	public UnityEvent_Player				OnPlayerJoin;
	public UnityEngine.Events.UnityEvent	OnGameFinished;
	public UnityEngine.Events.UnityEvent	OnGameStart;
	public UnityEngine.Events.UnityEvent	OnTickEnd;
	public UnityEvent_int2					OnWallDestroyed;
	

	
	private void Update()
	{
		TickCountdown -= Time.deltaTime;
		if ( TickCountdown < 0 )
		{
			Tick();
			TickCountdown = TickMs / 1000.0f;
		}
	}

	bool WithinDirectionRadius(int2 a,int2 b,int Radius)
	{
		var Deltax = Mathf.Abs( a.x - b.x );
		var Deltay = Mathf.Abs( a.y - b.y );

		if ( Deltay == 0 && Deltax <= Radius )
			return true;

		if ( Deltax == 0 && Deltay <= Radius )
			return true;

		return false;
	}
	
	public Bomb GetBombAt(int2 xy)
	{
		foreach (var Bomb in Bombs) {
			if (Bomb.xy == xy)
				return Bomb;
		}
		return null;
	}

	public Player GetPlayerAt(int2 xy)
	{
		foreach (var Player in Players) {
			if (Player.xy == xy)
				return Player;
		}
		return null;
	}


	PopperMan.Tile GetMapTileAt(int2 xy)
	{
		var map = GameObject.FindObjectOfType<Map>();
		return map [xy];
	}

	void PreExplodeBomb(Bomb bomb,System.Action<Player,Bomb> KillPlayer,System.Action<int2> DestroyTile)
	{
		bomb.Duration = 0;

		System.Func<int2,bool> ProcessExplosionAndContinue = (xy) =>
		{
			//	explode bomb
			var HitBomb = GetBombAt (xy);
			if ( HitBomb!=null )
			{
				//	not already exploded (stop recursion)
				if ( HitBomb.Duration != 0)
				{
					PreExplodeBomb (HitBomb, KillPlayer, DestroyTile);
				}
			}

			//	kill any players we hit
			var HitPlayer = GetPlayerAt (xy);
			if ( HitPlayer )
			{
				KillPlayer( HitPlayer, bomb );
			}

			//	check if we're blocked by the map
			var Tile = GetMapTileAt(xy);
			if ( Tile == PopperMan.Tile.Solid )
			{
				return false;
			}

			//	break wall
			if ( Tile == PopperMan.Tile.Wall )
			{
				bomb.Flames.Add(xy);
				DestroyTile( xy );
				//	stop here
				return false;
			}

			bomb.Flames.Add(xy);
			return true;
		};

		bomb.Flames.Add(bomb.xy);

		//	change of logic. walk explosion to detect different objects, halt the blast on solids
		for (int x = 1;	x <= bomb.Radius;	x++) {
			if (!ProcessExplosionAndContinue (new int2 (bomb.x + x, bomb.y)))
				break;
		}
		for (int x = 1;	x <= bomb.Radius;	x++) {
			if (!ProcessExplosionAndContinue (new int2 (bomb.x - x, bomb.y)))
				break;
		}
		for (int y = 1;	y <= bomb.Radius;	y++) {
			if (!ProcessExplosionAndContinue (new int2 (bomb.x, bomb.y+y)))
				break;
		}
		for (int y = 1;	y <= bomb.Radius;	y++) {
			if (!ProcessExplosionAndContinue (new int2 (bomb.x, bomb.y-y)))
				break;
		}

	}

	bool CanPlayerMoveTo(int2 xy)
	{
		var map = GameObject.FindObjectOfType<Map>();

		if ( GetBombAt(xy) != null )
			return false;

		if ( GetPlayerAt(xy) )
			return false;

		var MapTile = map[xy];
		if ( MapTile != PopperMan.Tile.Floor )
			return false;

		return true;
	}

	bool CanPlaceBombAt(int2 xy,Player Placer)
	{
		var map = GameObject.FindObjectOfType<Map>();

		if ( GetBombAt(xy) != null )
			return false;

		var Player = GetPlayerAt(xy);
		if ( Player && Player != Placer )
			return false;

		var MapTile = map[xy];
		if ( MapTile != PopperMan.Tile.Floor )
			return false;

		return true;
	}


	public void Tick()
	{
		if ( Frame == 0 )
			OnGameStart.Invoke();

		Frame++;

		//	move each player
		var map = GameObject.FindObjectOfType<Map>();

		foreach ( var player in Players )
		{
			System.Action<int2> OnPlaceBomb = (xy) =>
			{
				var bomb = new Bomb();
				bomb.xy = xy;
				bomb.Player = player;
				bomb.StartFrame = Frame;
				bomb.Duration = player.BombDuration;
				bomb.Radius = player.BombRadius;
				Bombs.Add( bomb );

				OnBombPlaced.Invoke(bomb);
			};
			
			if ( !player.Alive )
			{
				if ( player.Input_JoinGame )
				{
					//	todo: be clever at placing them
					player.Alive = true;
					OnPlayerJoin.Invoke(player);
				}
			}
			else
			{
				player.PlaceBomb(map,this,CanPlaceBombAt,OnPlaceBomb);
				player.Move( CanPlayerMoveTo );
			}

			player.ClearInput ();
		}

		var KilledPlayers = new List<Player>();
		System.Action<Player, Bomb> KillPlayer = (p, b) =>
		{
			Pop.AddUnique( KilledPlayers, p );
		};

		var DestroyedTiles = new List<int2>();
		System.Action<int2> DestroyTile = (xy) =>
		{
			Pop.AddUnique( DestroyedTiles, xy );
		};

		//	update bombs
		//	decrease all bomb times. if it pre-explodes, pre-explode others
		foreach ( var bomb in Bombs )
		{
			bomb.Duration--;
			if ( bomb.Duration > 0 )
				continue;

			PreExplodeBomb( bomb, KillPlayer, DestroyTile );
		}


		foreach ( var p in KilledPlayers )
		{
			p.Alive = false;
			OnPlayerDeathExplode.Invoke(p);
		}

		//	check for end of game
		if ( KilledPlayers.Count > 0 )
		{
		}		
		
		foreach ( var xy in DestroyedTiles )
		{
			var OldTile = map[xy];

			var NewTile = PopperMan.Tile.Floor;

			if ( OldTile == PopperMan.Tile.Wall )
			{
				//	randomly generate powerups
				OnWallDestroyed.Invoke(xy);
			}		

			map[xy] = NewTile;
		}		
			

		//	now actually "explode" all those that were triggered. 
		//	restore to players bomb count
		//	kill players
		for (int i = Bombs.Count - 1; i >= 0; i--)
		{ 
			var bomb = Bombs[i];
			if ( bomb.Duration > 0 )
				continue;

			bomb.Player.BombCount++;
			if ( OnBombExplode != null )
				OnBombExplode.Invoke(bomb);
			Bombs.RemoveAt(i);
		}

		OnTickEnd.Invoke ();
	}
}
