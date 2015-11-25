using UnityEngine;
using System.Collections;

public class TextDisplay : MonoBehaviour
{
	[SerializeField]
	new private GUIText  guiText;
	[SerializeField]
	private float    timePerCharacter = 0.1f;
	[SerializeField]
	private Material glitchMaterial;
	[SerializeField]
	private float    glitchDuration = 0.05f;
	[SerializeField]
	private Vector2  glitchInterval = new Vector2( 0.5f, 1.5f );
	[SerializeField]
	private float    glitchTextOffset = 5.0f;

	public bool TextFullyWritten {
		get {
			return textToDisplay == null ? true : currentTextLength == textToDisplay.Length;
		}
	}

	public bool TextFinishedDisplaying {
		get {
			return TextFullyWritten && clearTextTimer <= 0.0f;
		}
	}

	private string textToDisplay;
	private float  textClearTime = 5.0f;
	// Time until text is cleared
	private float  clearTextTimer;

//	private string lastTextToDisplay;
	private int currentTextLength;
	private float nextTextCharIn;
	private Material displayTextNormalMaterial;
	private float nextTextGlitchIn;
	private float currentTextGlitchTimer;
	private Vector2 currentTextGlitchOffset;

	public void TypeText( string text, Color textColor ) {
		TypeTextThenDisplayFor( text, -1.0f, textColor );
	}

	public void TypeTextThenDisplayFor( string text, float displayFor ) {
		TypeTextThenDisplayFor( text, displayFor, Color.white );
	}

	public void TypeTextThenDisplayFor( string text, float displayFor, Color textColor ) {
		TypeTextThenDisplayFor( text, displayFor, textColor, 0 );
	}

	public void TypeTextThenDisplayFor( string text, float displayFor, Color textColor, int startIndex ) {
		textToDisplay = text;
		textClearTime = displayFor;
		clearTextTimer = -1.0f;
		guiText.color = textColor;
		currentTextLength = startIndex;
	}

	public void ForceFinishCurrentText() {
		if( textToDisplay != null ) {
			guiText.text = textToDisplay;
			currentTextLength = textToDisplay.Length;
		}
	}

	public void ResetText() {
		textToDisplay = "";
	}

	protected void OnEnable() {
		if( guiText != null ) {
			displayTextNormalMaterial = guiText.material;
		}
	}

	protected void OnDisable() {
		if( guiText != null ) {
			guiText.material = displayTextNormalMaterial;
		}
	}

	protected void Update() {
		if( guiText == null ) {
			return;
		}
		
		guiText.pixelOffset = new Vector2( Screen.width / 2.0f, Screen.height - 40.0f ) + currentTextGlitchOffset;
		
//		if( lastTextToDisplay != textToDisplay ) {
//			currentTextLength = 0;
//		}
		
//		lastTextToDisplay = textToDisplay;
		
		if( glitchMaterial != null ) {
			DoGlitching();
		} else {
			guiText.material = displayTextNormalMaterial;
		}
		
		if( string.IsNullOrEmpty( textToDisplay ) ) {
			guiText.text = "";
		} else {
			DoTextAdvancement();
		}
	}

	private void DoGlitching() {
		if( currentTextGlitchTimer > 0.0f ) {
			currentTextGlitchTimer -= Time.deltaTime;
			
			// Reset to normal
			if( currentTextGlitchTimer < 0.0f ) {
				guiText.material = displayTextNormalMaterial;
				currentTextGlitchOffset = Vector2.zero;
			}
		} else {
			guiText.material = displayTextNormalMaterial;
			
			if( nextTextGlitchIn <= 0.0f ) {
				nextTextGlitchIn = Random.Range( glitchInterval.x, glitchInterval.y );
			}
			
			if( nextTextGlitchIn > 0.0f ) {
				nextTextGlitchIn -= Time.deltaTime;
				
				if( nextTextGlitchIn < 0.0f ) {
					currentTextGlitchTimer = glitchDuration;
					guiText.material = glitchMaterial;
					currentTextGlitchOffset = Random.insideUnitCircle * glitchTextOffset;
				}
			}
		}
	}

	private void DoTextAdvancement() {
		if( currentTextLength < textToDisplay.Length ) {
			if( nextTextCharIn > 0.0f ) {
				nextTextCharIn -= Time.deltaTime;
			}
			
			if( nextTextCharIn <= 0.0f ) {
				AdvanceCurrentChar();
				nextTextCharIn = Mathf.Max( 0.0f, timePerCharacter );
				guiText.text = textToDisplay.Substring( 0, currentTextLength );
			}
			
			// Have we finished typing the text?
			if( currentTextLength == textToDisplay.Length ) {
				if( textClearTime > 0.0f ) {
					clearTextTimer = textClearTime;
				}
			}
		} else if( clearTextTimer > 0.0f ) {
			clearTextTimer -= Time.deltaTime;
			if( clearTextTimer <= 0.0f ) {
				ResetText();
			}
		}
	}

	private void AdvanceCurrentChar() {
		do {
			++currentTextLength;
			char c = textToDisplay[ currentTextLength - 1 ];
			if( !char.IsWhiteSpace( c )) {
				break;
			}
		} while ( currentTextLength <= textToDisplay.Length );
	}
}
