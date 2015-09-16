using UnityEngine;
using System.Collections;
using SA;

public class SyllabificationTest : MonoBehaviour {
	public string toSyllabalize = "Must give us pause. There's the respect";

	void OnEnable() {
		var syll = Syllabificator.CreateFromFile( Application.streamingAssetsPath + "/SyllableList.txt" );
		var res = syll.SyllabalizeString( toSyllabalize );

		string print = toSyllabalize + " =";
		string wl = "";
		int syllablesFound = 0;
		foreach( var word in res ) {
			print += " ";
			for( int i = 0; i < word.Count; ++i ) {
				wl += word.IsCharacter ? "-" : "w";
				print += ( syllablesFound % 2 == 0 ? word[ i ] : word[ i ].ToUpper() );
				if( !word.IsCharacter ) { ++syllablesFound; }
			}
		}

		DebugUtil.Log( print );
		DebugUtil.Log( wl );
	}
}
