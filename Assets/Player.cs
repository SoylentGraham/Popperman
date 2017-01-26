using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class UnityEvent_String : UnityEngine.Events.UnityEvent <string> {}


[ExecuteInEditMode]
public class Player : MonoBehaviour {

	const string	IdentAlphabet = "!ABCDEFGHIJKLMNOPQRSTUVWYZ0123456789";

	public int		JoystickIndex = 0;
	public int		JoystickButton_First = 0;

	public int		this[PopperMan.NesPadJoystickButton Button]	{	get { return JoystickButton_First + (int)Button; }	}

	public bool		EnableKeyboardInput = true;

	public PopperMan.Direction	Input_Direction = PopperMan.Direction.None;
	public bool					Input_Bomb = false;
	public bool					Input_JoinGame = false;
	public bool					Input_ChangeIdent = false;

	public string				Ident = "?";

	[Range(0,20)]
	public int		x = 1;

	[Range(0,20)]
	public int		y = 1;

	public int2 xy
	{
		get
		{
			return new int2(x,y);
		}
	}


	[Range(0,4)]
	public int		BombCount = 1;
	[Range(1,10)]
	public int		BombDuration = 10;
	[Range(1,10)]
	public int		BombRadius = 2;

	public bool		Alive = false;

	[Header("Gameplay sounds etc")]
	public UnityEngine.Events.UnityEvent	OnBump;
	public UnityEngine.Events.UnityEvent	OnPlaceBombFailed;
	public UnityEngine.Events.UnityEvent	OnPlayerMoved;
	public UnityEvent_String				OnIdentChanged;


	public void ChangeIdent()
	{
		var Index = IdentAlphabet.IndexOf(Ident);
		Index = (Index+1) % IdentAlphabet.Length;
		Ident = "" + IdentAlphabet[Index];
		OnIdentChanged.Invoke( Ident );
	}


	void SetDirectionIfKey(PopperMan.Direction Direction,PopperMan.NesPadJoystickButton Button)
	{
		SetDirectionIfKey (Direction, PopperMan.GetJoystickButtonName (JoystickIndex, this[Button] ));
	}

	void SetDirectionIfKey(PopperMan.Direction Direction,string KeyName)
	{
		if (!Input.GetKey (KeyName))
			return;
		Input_Direction = Direction;
	}

	void SetDirectionIfKey(PopperMan.Direction Direction,KeyCode KeyName)
	{
		if (!Input.GetKey (KeyName))
			return;
		Input_Direction = Direction;
	}

	public void ClearInput()
	{
		Input_Direction = PopperMan.Direction.None;
		Input_Bomb = false;
		Input_JoinGame = false;
	}

	void Start()
	{
		Ident = "" + IdentAlphabet[ Random.Range(0,IdentAlphabet.Length-1)];
		OnIdentChanged.Invoke( Ident );
	}

	void Update()
	{
		//	we OR the inputs, as they're only used on a tick, we store it until
		SetDirectionIfKey( PopperMan.Direction.Up, PopperMan.NesPadJoystickButton.Up );
		SetDirectionIfKey( PopperMan.Direction.Down, PopperMan.NesPadJoystickButton.Down );
		SetDirectionIfKey( PopperMan.Direction.Left, PopperMan.NesPadJoystickButton.Left );
		SetDirectionIfKey( PopperMan.Direction.Right, PopperMan.NesPadJoystickButton.Right );

		Input_Bomb |= Input.GetKeyDown (PopperMan.GetJoystickButtonName (JoystickIndex, this[PopperMan.NesPadJoystickButton.A] ));
		Input_JoinGame |= Input.GetKeyDown (PopperMan.GetJoystickButtonName (JoystickIndex, this[PopperMan.NesPadJoystickButton.Start] ));
		Input_ChangeIdent |= Input.GetKeyDown (PopperMan.GetJoystickButtonName (JoystickIndex, this[PopperMan.NesPadJoystickButton.Select] ));
		
		if ( EnableKeyboardInput ) 
		{
			SetDirectionIfKey( PopperMan.Direction.Up, KeyCode.UpArrow );
			SetDirectionIfKey( PopperMan.Direction.Down, KeyCode.DownArrow );
			SetDirectionIfKey( PopperMan.Direction.Left, KeyCode.LeftArrow );
			SetDirectionIfKey( PopperMan.Direction.Right, KeyCode.RightArrow );

			Input_Bomb |= Input.GetKeyDown (KeyCode.Space);
			Input_JoinGame |= Input.GetKeyDown (KeyCode.Return);
		}

		if ( Input_ChangeIdent )
		{
			ChangeIdent();
			Input_ChangeIdent = false;
		}
	}

	public void Move(System.Func<int2,bool> CanMoveTo)
	{
		if ( Input_Direction == PopperMan.Direction.None )
			return;

		var NewPos = PopperMan.Move( new int2(x,y), Input_Direction );
		if ( !CanMoveTo( NewPos ) )
		{
			OnBump.Invoke();
			return;
		}

		x = NewPos.x;
		y = NewPos.y;
		OnPlayerMoved.Invoke();
	}


	public void PlaceBomb(Map map,Game game,System.Func<int2,Player,bool> CanPlaceBombAt,System.Action<int2> PlaceBomb)
	{
		if ( !Input_Bomb )
			return;

		if ( BombCount == 0 )
		{
			OnPlaceBombFailed.Invoke();
			return;
		}

		if ( !CanPlaceBombAt(xy,this) )
		{
			OnPlaceBombFailed.Invoke();
			return;
		}

		//	place bomb!
		PlaceBomb(xy);
		BombCount--;
		Input_Bomb = false;
	}

}
