using UnityEngine;
using System.Collections;
using SA;

public class SyllabificationTest : MonoBehaviour {
	void OnEnable() {
		var syll = Syllabificator.CreateFromFile( Application.streamingAssetsPath + "/SyllableList.txt" );
		string toSyllabalize = "to be or not to be, that is the question";
		var res = syll.SyllabalizeString( toSyllabalize );

		string print = toSyllabalize + " =";
		int syllablesFound = 0;
		foreach( var word in res ) {
			print += " ";
			for( int i = 0; i < word.Count; ++i ) {
				print += ( syllablesFound % 2 == 0 ? word[ i ] : word[ i ].ToUpper() );
				++syllablesFound;
			}
		}

		DebugUtil.Log( print );
	}
}
