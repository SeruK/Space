using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SA;

public class Localization {
	private Dictionary<string, string> lines;

	public void Load() {
		lines = new Dictionary<string, string>();

		// TODO: Support for different languages etc
		var filePath = Path.Combine( Application.streamingAssetsPath, "en.csv" );
		string line = null;
		using( var file = new StreamReader( filePath ) ) {
			while( ( line = file.ReadLine() ) != null ) {
				string id, val;
				if( ReadCSVLine( line, out id, out val ) ) {
					lines[ id ] = val;
				} 
			}
		}

		DebugUtil.Log( "Read " + lines.Count + " localization line(s)" );
	}

	private bool ReadCSVLine( string source, out string id, out string val ) {
		// TODO: Allow for embedded \"\"

		id = val = null;
		string comment = null;
		if( string.IsNullOrEmpty( source ) ) {
			return false;
		}

		// Comment
		if( source[ 0 ] == '#' ) {
			return false;
		}

		var tokens = source.Split( '\t' );

		if( tokens.Length < 2 ) {
			DebugUtil.LogWarn( "Invalid localization line (\"" + source + "\")" );
			return false;
		}

		id = tokens[ 0 ];
		val = tokens[ 1 ];

		if( tokens.Length > 2 ) {
			comment = tokens[ 2 ];
		}

		if( val.Length < 2 || ( val[ 0 ] != '"' || val[ val.Length - 1 ] != '"' ) ) {
			DebugUtil.LogWarn( "Invalid localization line [" + id + "]. Should begin and end with \"" );
			return false;
		}

		val = val.Substring( 1, val.Length - 2 );
		val = val.Replace( "\\n", System.Environment.NewLine );

		DebugUtil.Log( "Read Line: [{0}] = \"{1}\" ({2})", id, val, comment == null ? "No comment" : comment );

		return true;
	}

	public string Get( string id ) {
		if( lines == null ) {
			DebugUtil.LogError( "Localization not loaded" );
			return null;
		}

		if( string.IsNullOrEmpty( id ) ) {
			return null;
		}

		string res;
		if( lines.TryGetValue( id, out res ) ) {
			return res;
		}

		return null;
	}

	public void Set( string id, string value ) {
		if( lines == null ) {
			DebugUtil.LogError( "Localization not loaded" );
			return;
		}

		if( string.IsNullOrEmpty( id ) || value == null ) {
			DebugUtil.LogError( "Invalid id or value" );
			return;
		}

		if( lines.ContainsKey( id ) ) {
			DebugUtil.LogWarn( "Localization id already existed: " + id + ", overwriting" );
		}

		DebugUtil.Log( string.Format( "Adding Line: [{0}] = \"{1}\"", id, value ) );
		lines[ id ] = value;
	}
}
