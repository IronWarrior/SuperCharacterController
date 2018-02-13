using UnityEngine;
using System.Collections;

public class PlayerInputController : MonoBehaviour {

    public PlayerInput Current;
	public string jumpButton = "Jump";
	public string moveAxisX = "Horizontal";
	public string moveAxisY = "Vertical";
	public bool useMouseLook = true;
	public string mouseInputAxisX = "Mouse X";
	public string mouseInputAxisY = "Mouse Y";
	public bool useRightStickInput = true;
	public string rightStickInputAxisX = "RightH";
	public string rightStickInputAxisY = "RightV";
	
    public Vector2 RightStickMultiplier = new Vector2(3, -1.5f);

	// Use this for initialization
	void Start () {
        Current = new PlayerInput();
	}

	// Update is called once per frame
	void Update () {
        
        // Retrieve our current WASD or Arrow Key input
        // Using GetAxisRaw removes any kind of gravity or filtering being applied to the input
        // Ensuring that we are getting either -1, 0 or 1
        Vector3 moveInput = new Vector3(Input.GetAxisRaw( moveAxisX ), 0, Input.GetAxisRaw( moveAxisY ));

		Vector2 mouseInput = Vector2.zero;
		if( useMouseLook ) {
			mouseInput = new Vector2(Input.GetAxis( mouseInputAxisX ), Input.GetAxis( mouseInputAxisY ));
		}
        

        Vector2 rightStickInput = Vector2.zero;
		if( useRightStickInput ) {
			rightStickInput = new Vector2(Input.GetAxisRaw( rightStickInputAxisX ), Input.GetAxisRaw( rightStickInputAxisY ));
		}
			

        // pass rightStick values in place of mouse when non-zero
        mouseInput.x = rightStickInput.x != 0 ? rightStickInput.x * RightStickMultiplier.x : mouseInput.x;
        mouseInput.y = rightStickInput.y != 0 ? rightStickInput.y * RightStickMultiplier.y : mouseInput.y;

        bool jumpInput = Input.GetButtonDown( jumpButton );

        Current = new PlayerInput()
        {
            MoveInput = moveInput,
            MouseInput = mouseInput,
            JumpInput = jumpInput
        };
	}
}

public struct PlayerInput
{
    public Vector3 MoveInput;
    public Vector2 MouseInput;
    public bool JumpInput;
}
