using UnityEngine;
using System.Collections;
using System.IO;
using SA;

public class SyllabificationTest : MonoBehaviour {
	void OnEnable() {
		var syll = Syllabificator.CreateFromFile( Application.streamingAssetsPath + "/SyllableList.txt" );

		using (var reader = new StreamReader( Application.streamingAssetsPath + "/lines.txt" )) {
			string line = null;
			while (( line = reader.ReadLine() ) != null) {
				Syllabalize( syll, line );
			}
		}
				

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
}
