using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SA;

public class SyllabificationTest : MonoBehaviour {
	[SerializeField]
	private GUIText textBox;
	[SerializeField]
	private float letterDelay;
	[SerializeField]
	private float commaDelay;
	[SerializeField]
	private float punctuationDelay;
	[SerializeField]
	private float colonDelay;
	[SerializeField]
	private int normalTextSize;
	[SerializeField]
	private int bigTextSize;

	private string boxText = "";
	private IEnumerator coroutine;

	void OnEnable() {
		var syll = Syllabificator.CreateFromFile( Application.streamingAssetsPath + "/SyllableList.txt" );
		if( coroutine != null ) {
			StopCoroutine( coroutine );
		}
		coroutine = ReadLines( syll );
		StartCoroutine( coroutine );
	}

	void OnDisable() {
		if( coroutine != null ) {
			StopCoroutine( coroutine );
		}
	}

	void Update() {
		Rect screenRect = textBox.GetScreenRect();
		float yOffset = 0.0f;
		if( screenRect.height > Screen.height / 2.0f ) {
			yOffset = screenRect.height - ( Screen.height / 2.0f );
		}

		textBox.pixelOffset = new Vector2( Screen.width / 2.0f, Screen.height - 20.0f + yOffset );
		textBox.fontSize = normalTextSize;
	}

	IEnumerator ReadLines( Syllabificator syll ) {
		var lines = new List<SyllabalizedWord[]>();
		using (var reader = new StreamReader( Application.streamingAssetsPath + "/lines.txt" )) {
			string line = null;
			while (( line = reader.ReadLine() ) != null) {
				var res = syll.SyllabalizeString( line );
				lines.Add( res );
			}
		}

		boxText = "";

		return WritePerSyllable( lines );
	}

	IEnumerator WritePerSyllable( List<SyllabalizedWord[]> lines ) {
		foreach( SyllabalizedWord[] line in lines ) {
			for( int i = 0; i < line.Length; ++i ) {
				var word = line[ i ];
				if( word.IsSymbol ) {
					float delay = letterDelay;
					if( word.String == "," ) {
						delay = commaDelay;
					} else if( word.String == "." || word.String == "?" || word.String == "!" ) {
						delay = punctuationDelay;
					} else if( word.String == ":" || word.String == ";" ) {
						delay = commaDelay;
					}
					AppendGUIText( word.String, false );
					yield return new WaitForSeconds( delay );
					continue;
				}
				
				AppendGUIText( " ", false );
				
				foreach( string syllable in word ) {
					AppendGUIText( syllable, true );
					yield return new WaitForSeconds( letterDelay * (float)syllable.Length );
				}
			}
			AppendGUIText( "\n", false );
		}
	}

	string openElement =
		"<color=aqua>"
			//"<b>"
			//string.Format( "<size={0}>", bigTextSize )
			;
	string closeElement =
		"</color>"
			//"</b>";
			//"</size>"
			;

	void ReplaceGUIText( string oldStr, string newStr ) {
		int old = boxText.LastIndexOf( oldStr );
		boxText = boxText.Remove( old );
		boxText += newStr;
		textBox.text = boxText;
	}

	void AppendGUIText( string str, bool bold ) {
		boxText = boxText.Replace( openElement, "" );
		boxText = boxText.Replace( closeElement, "" );
		boxText += bold ? string.Format( "{0}{1}{2}", openElement, str, closeElement ) : str;
		textBox.text = boxText;
	}

	void Syllabalize( Syllabificator syll, string toSyllabalize ) {
		var res = syll.SyllabalizeString( toSyllabalize );

		if( res.Length != 10 && res.Length != 11 ) {
			DebugUtil.Log( "Probably not proper verse" );
		}

		string print = toSyllabalize + " = ";
		string wl = "";
		int syllablesFound = 0;
		foreach( var word in res ) {
			if( !word.IsSymbol ) {
				print += " ";
			}
			for( int i = 0; i < word.Count; ++i ) {
				wl += word.IsSymbol ? "-" : "w";
				print += word[ i ];//( syllablesFound % 2 == 0 ? word[ i ] : word[ i ].ToUpper() );
				if( !word.IsSymbol ) { ++syllablesFound; }
			}
		}
		
		DebugUtil.Log( print );
	}
//
//	protected void OnGUI() {
//		float inset = 30.0f;
//		var textRect = new Rect( inset, inset, Screen.width - inset, Screen.height - inset );
//		GUI.TextArea( textRect, boxText );
//	}
}
