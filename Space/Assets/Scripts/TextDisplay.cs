using UnityEngine;
using UE = UnityEngine;
using System;
using System.Text;
using System.Collections;
using SA;

public class TextDisplay : SA.Behaviour
{
	[SerializeField]
	new private UE.UI.Text  guiText;
	[SerializeField]
	private UE.UI.Image     guiBg;
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
			return syllabalizedString != null ? currentSyllLength == syllCount :
			       textToDisplay == null ? true : currentTextLength == textToDisplay.Length;
		}
	}

	public bool TextFinishedDisplaying {
		get {
			return TextFullyWritten && clearTextTimer <= 0.0f;
		}
	}

	private string textToDisplay;
	private string speaker;
	private SyllabalizedWord[] syllabalizedString;
	private int syllCount;
	private Syllabificator syllabificator;
	private int currentSyllLength;
	private float nextSyllTimer;

	private float  textClearTime = 5.0f;
	// Time until text is cleared
	private float  clearTextTimer;

	private int currentTextLength;
	private float nextTextCharIn;
	private Material displayTextNormalMaterial;
	private float nextTextGlitchIn;
	private float currentTextGlitchTimer;
	private Vector2 currentTextGlitchOffset;

	public void Reinitialize( Syllabificator syllabificator ) {
		this.syllabificator = syllabificator;
	}

	public void TypeText( string text, Color textColor ) {
		TypeTextThenDisplayFor( text, -1.0f, textColor );
	}

	public void TypeTextThenDisplayFor( string text, float displayFor ) {
		TypeTextThenDisplayFor( text, displayFor, Color.white );
	}

	public void TypeTextThenDisplayFor( string text, float displayFor, Color textColor ) {
		TypeTextThenDisplayFor( text, displayFor, textColor, syllabalize: false );
	}

	public void TypeTextThenDisplayFor( string text, float displayFor, Color textColor, bool syllabalize ) {
		int speakerIndex = text.IndexOf( ':' );
		if( speakerIndex != -1 ) {
			speaker = text.Substring( 0, speakerIndex );
			text = text.Substring( speakerIndex + 1 ).Trim( ' ', '\t', '\n' );
		}
		textToDisplay = text;

		if( !syllabalize ) {
			syllabalizedString = null;
		} else  {
			syllabalizedString = syllabificator.SyllabalizeString( text );
			syllCount = 0;
			currentSyllLength = 0;
			foreach( SyllabalizedWord word in syllabalizedString ) {
				syllCount += word.Count;
			}
		}

		textClearTime = displayFor;
		clearTextTimer = -1.0f;
		guiText.color = textColor;
		currentTextLength = 0;
	}

	public void ForceFinishCurrentText() {
		if( textToDisplay != null ) {
			guiText.text = textToDisplay;
			currentTextLength = textToDisplay.Length;
		}
		if( syllabalizedString != null ) {
			nextSyllTimer = 0.0f;
			currentSyllLength = syllCount;
			guiText.text = DoWordAdvancement( checkTime: false );
		}
	}

	public void ResetText() {
		textToDisplay = "";
		syllabalizedString = null;
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



		if( glitchMaterial != null ) {
			DoGlitching();
		} else {
			guiText.material = displayTextNormalMaterial;
		}

		if( syllabalizedString == null ) {
			if( string.IsNullOrEmpty( textToDisplay ) ) {
				guiText.text = "";
			} else {
				DoTextAdvancement();
			}
		} else {
			guiText.text = DoWordAdvancement( checkTime: true );
		}

		if( guiBg != null ) {
			guiBg.enabled = !string.IsNullOrEmpty( guiText.text );
		}
	}

	private string DoWordAdvancement( bool checkTime ) {
		if( nextSyllTimer > 0.0f ) {
			nextSyllTimer -= Time.deltaTime;
			return guiText.text;
		}

		System.Text.StringBuilder text = new StringBuilder( "" );
		if( !string.IsNullOrEmpty( speaker ) ) {
			text.AppendFormat( "{0}:\n", speaker );
		}

		if( currentSyllLength < syllCount ) {
			++currentSyllLength;
		}

		int i = 0;
		foreach( SyllabalizedWord word in syllabalizedString ) {
			if( i == currentSyllLength ) {
				break;
			}

			if( word.IsSymbol ) {
				text.Append( word.String );
				++i;
			} else {
				foreach( string syllable in word ) {
					text.Append( syllable );
					if( ++i == currentSyllLength ) {
						nextSyllTimer = syllable.Length * timePerCharacter;
						break;
					}
				}
			}
		}

		if( checkTime && clearTextTimer > 0.0f ) {
			clearTextTimer -= Time.deltaTime;
			if( clearTextTimer <= 0.0f ) {
				ResetText();
				return "";
			}
		}

		return text.ToString();
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
				nextTextGlitchIn = UE.Random.Range( glitchInterval.x, glitchInterval.y );
			}
			
			if( nextTextGlitchIn > 0.0f ) {
				nextTextGlitchIn -= Time.deltaTime;
				
				if( nextTextGlitchIn < 0.0f ) {
					currentTextGlitchTimer = glitchDuration;
					guiText.material = glitchMaterial;
					currentTextGlitchOffset = UE.Random.insideUnitCircle * glitchTextOffset;
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
