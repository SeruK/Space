using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {
	public Entity Player;
	public SmoothFollow CameraController;

	public GUIText  DisplayText;
	public float    DisplayTextWait;
	public Material DisplayTextGlitchMaterial;
	public float    DisplayTextGlitchDuration;
	public Vector2  DisplayTextGlitchRandom;
	public float    DisplayTextGlitchOffset;

	public string TextToDisplay;
	private string lastTextToDisplay;
	private int currentTextLength;
	private float nextTextCharIn;
	private Material displayTextNormalMaterial;
	private float nextTextGlitchIn;
	private float currentTextGlitchTimer;
	private Vector2 currentTextGlitchOffset;

	protected void OnEnable() {
		if( DisplayText != null ) {
			displayTextNormalMaterial = DisplayText.material;
		}
	}

	protected void Update() {
		InputPlayer();
		UpdateDisplayText();
	}

	protected void InputPlayer() {
		if( Player == null ) {
			return;
		}

		Player.RequestedHorizontalSpeed = Input.GetKey( KeyCode.LeftArrow ) ? -1.0f :
			Input.GetKey( KeyCode.RightArrow ) ? 1.0f : 0.0f;
		Player.RequestedJump = Input.GetKey( KeyCode.UpArrow );
		if( Input.GetKey( KeyCode.DownArrow ) && CameraController != null ) {
			CameraController.extraCameraOffset = new Vector3( 0.0f, 2.0f, 0.0f );
		} else {
			CameraController.extraCameraOffset = Vector3.zero;
		}
	}

	protected void UpdateDisplayText() {
		if( DisplayText == null ) {
			return;
		}

		DisplayText.pixelOffset = new Vector2( Screen.width / 2.0f, Screen.height - 40.0f ) + currentTextGlitchOffset;
		
		if( lastTextToDisplay != TextToDisplay ) {
			currentTextLength = 0;
		}
		
		lastTextToDisplay = TextToDisplay;

		if( DisplayTextGlitchMaterial != null ) {
			if( currentTextGlitchTimer > 0.0f ) {
				currentTextGlitchTimer -= Time.deltaTime;

				// Reset to normal
				if( currentTextGlitchTimer < 0.0f ) {
					DisplayText.material = displayTextNormalMaterial;
					currentTextGlitchOffset = Vector2.zero;
				}
			} else {
				DisplayText.material = displayTextNormalMaterial;

				if( nextTextGlitchIn <= 0.0f ) {
					nextTextGlitchIn = Random.Range( DisplayTextGlitchRandom.x, DisplayTextGlitchRandom.y );
				}

				if( nextTextGlitchIn > 0.0f ) {
					nextTextGlitchIn -= Time.deltaTime;

					if( nextTextGlitchIn < 0.0f ) {
						currentTextGlitchTimer = DisplayTextGlitchDuration;
						DisplayText.material = DisplayTextGlitchMaterial;
						currentTextGlitchOffset = Random.insideUnitCircle * DisplayTextGlitchOffset;
					}
				}
			}
		} else {
			DisplayText.material = displayTextNormalMaterial;
		}
		
		if( string.IsNullOrEmpty( TextToDisplay ) ) {
			DisplayText.text = "";
		} else {
			if( currentTextLength < TextToDisplay.Length ) {
				if( nextTextCharIn > 0.0f ) {
					nextTextCharIn -= Time.deltaTime;
				}
				
				if( nextTextCharIn <= 0.0f ) {
					do {
						++currentTextLength;
						char c = TextToDisplay[ currentTextLength - 1 ];
						if( !char.IsWhiteSpace( c )) {
							break;
						}
					} while ( currentTextLength <= TextToDisplay.Length );
					nextTextCharIn = Mathf.Max( 0.0f, DisplayTextWait );
					DisplayText.text = TextToDisplay.Substring( 0, currentTextLength );
				}
			}
		}
	}
}
