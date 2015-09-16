using UnityEngine;
using System.Collections;
using SA;

public class SyllabificationTest : MonoBehaviour {
	void OnEnable() {
		var syll = Syllabificator.CreateFromFile( Application.streamingAssetsPath + "/SyllableList.txt" );
		var res = syll.SyllabalizeString( "abandon" );
		string print = res.String + " = ";
		for( int i = 0; i < res.Count; ++i ) {
			print += res[ i ] + "·";
		}

		DebugUtil.Log( string.Join( "·", res.GetSyllables() ) );
	}
}
