using UnityEngine;
using UE = UnityEngine;
using System.Collections;

public class DebugTest : MonoBehaviour, IDebugContext {
	DefaultDebugSystem debugSystem;

	//public bool ShouldSquelchLog( Dbg.LogType logType, System.Exception exc, string message ) {
	//	return logType == Dbg.LogType.Log;
	//}

	void OnEnable() {
		debugSystem = new DefaultDebugSystem();
		Dbg.DebugSystem = debugSystem;

		Dbg.LogIf( true, this, "I'm logging cause I'm true" );
		Dbg.LogIf( false, this, "I'm not logging cause I'm false" );
		Dbg.Log( this, "Dbg.Log" );
		Dbg.LogWarn( this, "Dbg.LogWarn" );
		Dbg.LogError( this, "Dbg.LogError" );
		Dbg.Assert.IsTrue( true, this, "true should be true" );
		try {
			Dbg.Assert.IsTrue( false, this, "true should not be false" );
		} catch( System.Exception exc ) {
			Dbg.LogError( this, "Dbg.LogError in catch" );
			Dbg.LogExc( this, exc );
		}
		Dbg.Assert.AreEqual( 1, 1, this, "Dbg.Assert.AreEqual" );
		Dbg.Assert.AreNotEqual( 1, 1, this, "Dbg.Assert.AreNotEqual" );
		InvokeRepeating( "Spam", 0.5f, 0.5f );
		InvokeRepeating( "ImportantMessage", 5.0f, 3.0f );
		StartCoroutine( ACoroutine() );
	}

	void Spam() {
		Dbg.Log( this, "Spam" );
	}

	void ImportantMessage() {
		Dbg.LogError( this, "Ah! Something bad happened" );
	}

	void OnGUI() {
		foreach( var log in debugSystem.Logs ) {
			UE.Object obj = log.Object;
			GUILayout.Label( obj == null ? "<null>" : obj.name );
			foreach( var entry in log.Logs ) {
				DrawEntry( entry.Frame, entry.LogType, entry.Message );
			}
		}
	}

	void DrawEntry( int frame, Dbg.LogType logType, string message ) {
		Color color = logType == Dbg.LogType.Log ? Color.white :
			logType == Dbg.LogType.Warning ? Color.yellow :
			Color.red;

		GUILayout.BeginHorizontal();
		GUILayout.Space( 20.0f );

		GUI.color = color;
		GUILayout.Label( string.Format( "{0}:", frame ), GUILayout.MinWidth( 60.0f ) );
		GUILayout.Label( message );
		GUI.color = Color.white;

		GUILayout.EndHorizontal();
	}

	IEnumerator ACoroutine() {
		Debug.Log( "ACoroutine() before" );
		yield return StartCoroutine( NestedCoroutine() );
		Debug.Log( "ACoroutine() after" );
	}

	IEnumerator NestedCoroutine() {
		Debug.Log( "NestedCoroutine() before" );
		yield return StartCoroutine( SuperNestedCoroutine() );
		Debug.Log( "NestedCoroutine() after " );
		yield break;
	}

	IEnumerator SuperNestedCoroutine() {
		Debug.Log( "SuperNestedCoroutine()" );
		yield break;
	}
}
