using System.Collections.Generic;
using System.IO;

namespace SA {
	public static class SyllabificationStringExtensions {
		public static int CountOccurrencesOf( this System.String str, char c ) {
			int count = 0, length = str.Length;
			for( int i = 0; i < length; ++i ) {
				if( str[ i ] == c ) ++count;
			}
			return count;
		}
	}

	// abandon = a·ban·don
	// indices describes: a[b]an[d]on
	public class SyllabalizedString {
		private string fullString;
		private int[]  indices;

		public string String { get { return fullString; } }
		public int    Count { get { return indices.Length + 1; } }

		public string this[ int i ] {
			get { return GetSyllable( i ); }
		} 

		public SyllabalizedString( string fullString, int[] indices ) {
			// An empty string has no syllables or reason for existing
			DebugUtil.Assert( !string.IsNullOrEmpty( fullString ) );
			this.fullString = fullString;
			this.indices    = indices ?? new int[ 0 ];
		}

		public string GetSyllable( int i ) {
			DebugUtil.Assert( i < Count );
			// i == 1
			// a [b] a n [d] o n
			//   1        4
			// start     next
			int startIndex = i == 0 ? 0 : indices[ i - 1 ];
			int nextIndex  = i == Count - 1 ? fullString.Length : indices[ i ];
			return fullString.Substring( startIndex, nextIndex - startIndex );
		}

		public string[] GetSyllables() {
			var ret = new string[ Count ];
			for( int i = 0; i < Count; ++i ) {
				ret[ i ] = GetSyllable( i );
			}
			return ret;
		}
	}

	public class Syllabificator {
		private Dictionary<string, SyllabalizedString> database;

		private Syllabificator( Dictionary<string, SyllabalizedString> db ) {
			this.database = db;
		}

		public SyllabalizedString SyllabalizeString( string str ) {
			return database[ str ];
		}

		public static Syllabificator CreateFromFile( string absolutePath ) {
			var database = new Dictionary<string, SyllabalizedString>();

			using( var reader = new StreamReader( absolutePath ) ) {
				string line = null;
				while( ( line = reader.ReadLine() ) != null ) {
					// abandon=a·ban·don
					string[] items = line.Split( '=' );
					DebugUtil.Assert( items.Length == 2 );
					string fullString = items[ 0 ];
					string syllabalizedString = items[ 1 ];

					// res = scan - found
					//
					//       a · ban · don
					// scan  0 1 234 5 678
					// found 0 0 111 1 222
					// res     1     4
					//
					// indices position in full string:
					// a[b]an[d]on
					int[] indices = new int[ syllabalizedString.CountOccurrencesOf( '·' ) ];
					int scan = 0;
					int found = 0;
					while( scan < syllabalizedString.Length ) {
						if( syllabalizedString[ scan ] == '·' ) {
							indices[ found ] = scan - found;
							++found;
						} 
						++scan;
					}

					database[ fullString ] = new SyllabalizedString( fullString, indices );
				}
			}

			return new Syllabificator( database );
		}
	}
}