using UnityEngine;
using System.Collections;
using SA;

public class SyllabificationTest : MonoBehaviour {
	void OnEnable() {
		var syll = Syllabificator.CreateFromFile( Application.streamingAssetsPath + "/SyllableList.txt" );
		string toSyllabalize = "to be or not to be, that is the question";
		var res = syll.SyllabalizeString( toSyllabalize );

		string print = toSyllabalize + " = ";
		foreach( var word in res ) {
			print += word.ToString() + " ";
		}

		DebugUtil.Log( print );
	}
}
