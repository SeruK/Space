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
	public class SyllabalizedWord {
		private string fullString;
		private int[]  indices;

		public string String { get { return fullString; } }
		public int    Count { get { return indices.Length + 1; } }
		public override string ToString () {
			return string.Join( "·", GetSyllables() );
		}

		public string this[ int i ] {
			get { return GetSyllable( i ); }
		} 

		public SyllabalizedWord( string fullString, int[] indices ) {
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
		private Dictionary<string, SyllabalizedWord> database;

		private Syllabificator( Dictionary<string, SyllabalizedWord> db ) {
			this.database = db;
		}

		public SyllabalizedWord[] SyllabalizeString( string str ) {
			string[] words = str.Trim().Split( ' ' );
			var syllabalizedList = new List<SyllabalizedWord>();

			foreach( string word in words ) {
				if( database.ContainsKey( word ) ) {
					syllabalizedList.Add( database[ word ] );
				} else {
					// Resolve by rules
				}
			}

			return syllabalizedList.ToArray();
		}

		public static Syllabificator CreateFromFile( string absolutePath ) {
			var database = new Dictionary<string, SyllabalizedWord>();

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

					database[ fullString ] = new SyllabalizedWord( fullString, indices );
				}
			}

			return new Syllabificator( database );
		}

		private SyllabalizedWord ResolveByRules( string word ) {


			return null;
		}

		private string[] MatchSuffix( string suffix ) {
			switch( suffix ) {
				// Plural
			case "ses": return new string[]{ "se", "s·es", "sis", "ses" };
			case "zes": return new string[]{ "ze", "z·es" };
			case "s": return new string[]{ "", "s" };
				// -ed
			case "ded": return new string[]{ "d", "d·ed" };
			case "ted": return new string[]{ "t", "t·ed" };
			case "mmed": return new string[]{ "m", "mmed" };
			case "gged": return new string[]{ "g", "gged" };
			case "pped": return new string[]{ "p", "pped" };
			case "nned": return new string[]{ "n", "nned" };
			case "tted": return new string[]{ "t", "t·ted" };
			case "ed": return new string[]{ "", "ed" };
				// -er
			case "mmier": return new string[]{ "mmy", "m·mi·er" };
			case "ggier": return new string[]{ "g", "g·gi·er" };
			case "ppier": return new string[]{ "p", "p·pi·er" };
			case "ier": return new string[]{
					"e", "i·er",// ache --> achier
					"ey", "i·er",
					"y", "i·er"
				};
			case "gger": return new string[]{ "g", "g·ger" };
			case "rrer": return new string[]{ "r", "r·rer" };
			case "ler": return new string[]{ "l", "ler" };
			case "der": return new string[]{ "d", "der" };
			case "dders": return new string[]{ "d", "d·ders" };
			case "er": return new string[]{ 
					"e", "er",
					"", "er"
				};
				// -ed
			case "ied": return new string[]{ "y", "ied" };
				// -es
			case "es": return new string[]{ "", "es" };
			case "iness": return new string[]{ "e", "i·ness" }; // ache --> achiness
			case "mmies": return new string[]{ "m", "m·mies" };
			case "ggies": return new string[]{ "g", "g·gies" };
			case "nnies": return new string[]{ "n", "n·nies" };
			case "ies": return new string[]{ "y", "ies" }; // cry --> cries
			case "ves": return new string[]{ "f", "ves" }; // thied --> thieves
				// -ous
			case "ous": return new string[]{ "", "·ous" };
				// -est
			case "mmiest": return new string[]{ "mmy", "m·mi·est" };
			case "iest": return new string[]{
					"e", "i·est", // ache --> achiest
					"ey", "i·est",
					"y", "i·est" // zesty --> zestiest
				};
			case "est": return new string[]{
					"", "·est",
					"e", "·est"
				};
				//-ing
			case "tting": return new string[]{ "t", "t·ting" };
			case "ming": return new string[]{ "", "·ming" };
			case "ding": return new string[]{ "", "·ding" };
			case "ring": return new string[]{ "", "·ring" };
			case "ning": return new string[]{ "", "·ning" };
			case "ging": return new string[]{ "", "·ging" };
			case "ing": return new string[]{
					"", "·ing",
					"e", "·ing"
				};
				// -ish
			case "ish": return new string[]{ "", "·ish" };
				// -ist
			case "ist": return new string[]{
					"e", "·ist",
					"y", "·ist"
				};
				// -y
			case "ddy": return new string[]{ "dd", "d·dy" };
			case "dly": return new string[]{ "d", "d·ly" }; // hard --> hardly
			case "ggy": return new string[]{ "g", "·ggy" };
			case "bby": return new string[]{ "", "·bby" };
			case "ly": return new string[]{ "", "ly" };
			case "y": return new string[]{
					"", "·y",
					"e", "·y"
				};
				// misc
			case "tion": return new string[]{ "", "·tion" };
			case "men": return new string[]{ "man", "men" };
			case "sat": return new string[]{ "sit", "sat" };
			case "ae": return new string[]{ "a", "ae" };
			case "ese": return new string[]{ "a", "ese" };
			case "up": return new string[]{ "", "·up" };
			case "able": return new string[]{ "e", "·a·ble" };
			case "ible": return new string[]{ "e", "·i·ble" };
			case "th": return new string[]{ "", "th" };
			case "ncy": return new string[]{ "nt", "·n·cy" }; // stringent --> stringency
			case "or": return new string[]{ "", "··or" };
			case "ial": return new string[]{ "y", "·i·al" };
			case "al": return new string[]{ "", "·al" }; // logic --> logical
			case "d": return new string[]{ "", "·d" }; // plague --> plagued
			case "a": return new string[]{ "um", "·a" };
			case "i": return new string[]{
					"", "·i",
					"us", "i"
				};
			case "l": return new string[]{ "", "·l" };
			case "x": return new string[]{ "", "·x" };
			}

			return null;
		}

		private string[] MatchPrefix( string prefix ) {
			switch( prefix ) {
			case "co": return new string[] { "", "co·" };
			case "by": return new string[] { "", "bi·" };
			case "de": return new string[] { "", "de·" };
			case "non": return new string[] { "", "non·" };
			case "un": return new string[] { "", "un·" };
			case "up": return new string[] { "", "up·" };
			case "pre": return new string[] { "", "pre·" };
			case "mis": return new string[] { "", "mis·" };
			case "re": return new string[] { "", "re·" };
			}
			return null;
		}
	}
}