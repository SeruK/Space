using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// TODO: Case-insensitive
namespace SA {
	public static class SyllabificationStringExtensions {
		public static int CountOccurrencesOf( this String str, char c ) {
			int count = 0, length = str.Length;
			for( int i = 0; i < length; ++i ) {
				if( str[ i ] == c ) ++count;
			}
			return count;
		}
	}

	// abandon = a·ban·don
	// indices describes: a[b]an[d]on
	public class SyllabalizedWord : IEnumerable<string> {
		private readonly string fullString;
		private readonly int[]  indices;
		private readonly bool   isSymbol;

		public string String { get { return fullString; } }
		public int    Count { get { return indices.Length + 1; } }
		public bool   IsSymbol { get { return isSymbol; } }
		public string SyllabalizedString {
			get { return string.Join( "·", GetSyllables() ); }
		}

		public override string ToString () {
			return SyllabalizedString;
		}
		public string this[ int i ] {
			get { return GetSyllable( i ); }
		} 

		public SyllabalizedWord( string fullString, int[] indices ) : this( fullString, indices, false ) {}
	
		public SyllabalizedWord( string fullString, int[] indices, bool isSymbol ) {
			// An empty string has no syllables or reason for existing
			SA.Debug.Assert( !string.IsNullOrEmpty( fullString ) );
			this.fullString  = fullString;
			this.indices     = indices == null ? new int[ 0 ] : (int[])indices.Clone();
			this.isSymbol    = isSymbol;
		}

		public SyllabalizedWord( SyllabalizedWord word, string fullString ) : this( fullString, word.indices, word.isSymbol )  {}

		public string GetSyllable( int i ) {
			SA.Debug.Assert( i < Count );
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

		public IEnumerator<string> GetEnumerator() {
			for( int i = 0; i < Count; ++i ) {
				yield return GetSyllable( i );
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	public class Syllabificator {
		private Dictionary<string, SyllabalizedWord> database;

		private Syllabificator( Dictionary<string, SyllabalizedWord> db ) {
			this.database = db;
		}

		public static Syllabificator CreateFromFile( string absolutePath ) {
			var database = new Dictionary<string, SyllabalizedWord>();
			
			using( var reader = new StreamReader( absolutePath ) ) {
				string line = null;
				while( ( line = reader.ReadLine() ) != null ) {
					// abandon=a·ban·don
					string[] items = line.Split( '=' );
					SA.Debug.Assert( items.Length == 2 );
					string fullString = items[ 0 ];
					string syllabalizedString = items[ 1 ];
					
					database[ fullString ] = ResolveSyllabalizedWord( fullString, syllabalizedString );
				}
			}
			
			return new Syllabificator( database );
		}

		public SyllabalizedWord[] SyllabalizeString( string str ) {
			string[] words = str.Trim().Split( ' ' );
			var syllabalizedList = new List<SyllabalizedWord>();

			for( int i = 0; i < words.Length; ++i ) {
				string word = words[ i ];
				string[] tokens = TokenizeLettersNonLetters( word );
				var postWords = new List<SyllabalizedWord>();
				if( tokens.Length > 1 ) {
					word = "";
					foreach( string token in tokens ) {
						bool tokenIsWord = char.IsLetter( token[ 0 ] );
						if( tokenIsWord ) {
							word += token; 
						} else {
							// Haven't found word yet, add character token as word
							if( word == "" ) {
								// Special case: 'tis
								if( token == "'" ) {
									word += token;
								} else {
									syllabalizedList.Add( new SyllabalizedWord( token, null, true ) );
								}
							} else {
								// Special cases: heart-ache, it's
								if( token == "-" || token == "'" ) {
									word += token;
								} else {
									postWords.Add( new SyllabalizedWord( token, null, true ) );
								}
							}
						}
					}
				}
				string key = word.ToLower();
				if( database.ContainsKey( key ) ) {
					syllabalizedList.Add( new SyllabalizedWord( database[ key ], word ) );
				} else {
					SyllabalizedWord resolved = ResolveByRules( word );
					if( resolved != null ) {
						syllabalizedList.Add( resolved );
					} else {
						SA.Debug.Log( "Unable to resolve word: \"" + word + "\", adding as-is" );
						syllabalizedList.Add( new SyllabalizedWord( word, null ) );
					}
				}

				foreach( var postWord in postWords ) {
					syllabalizedList.Add( postWord );
				}
			}

			return syllabalizedList.ToArray();
		}

		private static string[] TokenizeLettersNonLetters( string str ) {
			SA.Debug.Assert( !string.IsNullOrEmpty( str ) );
			var list = new List<string>();
			int start = 0;
			bool isWord = char.IsLetter( str[ 0 ] );
			for( int i = 1; i < str.Length; ++i ) {
				if( isWord && !char.IsLetter( str[ i ] ) ) {
					list.Add( str.Substring( start, i - start ) );
					start = i;
					isWord = false;
				} else if( !isWord && char.IsLetter( str[ i ] ) ) {
					list.Add( str.Substring( start, i - start ) );
					start = i;
					isWord = true;
				}
			}
			list.Add( str.Substring( start, str.Length - start ) );
			return list.ToArray();
		}

		private static SyllabalizedWord ResolveSyllabalizedWord( string fullString, string syllabalizedString ) {
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
			
			return new SyllabalizedWord( fullString, indices );
		}

		// RULES
		// determines if a prefix or suffix could be
		// replaced to create a word (f.ex. "dicks"
		// would use the same syllabification)
		// the rules are returned in pairs of
		// "replaceWith" / "reinsertAfter" strings.
		// TODO: Test with both prefix and suffix

		private SyllabalizedWord ResolveByRules( string word ) {
			bool isPrefix = true;
			// Try to match prefixes
			for( int prefixLen = 1; ( prefixLen <= 3 && prefixLen < word.Length ); ++prefixLen ) {
				string prefix = word.Substring( 0, prefixLen );
				string[] rules = GetPrefixRules( prefix );
				var resolvedWord = TryResolveWithRules( word, rules, isPrefix, prefix );
				if( resolvedWord != null ) {
					return resolvedWord;
				}
			}

			isPrefix = false;
			// Try to match suffixes
			for( int suffixLen = 1; ( suffixLen <= 5 && suffixLen < word.Length ); ++suffixLen ) {
				string suffix = word.Substring( word.Length - suffixLen );
				string[] rules = GetSuffixRules( suffix );
				var resolvedWord = TryResolveWithRules( word, rules, isPrefix, suffix );
				if( resolvedWord != null ) {
					return resolvedWord;
				}
			}

			return null;
		}

		private SyllabalizedWord TryResolveWithRules( string word, string[] rules, bool isPrefix, string toReplace ) {
			if( rules == null ) {
				return null;
			}
			for( int i = 0; i < rules.Length; i += 2 ) {
				string replaceWith = rules[ i ];
				string reinsert    = rules[ i + 1 ];
				SyllabalizedWord resolvedWord = TryResolveWithRule( word, isPrefix, toReplace, replaceWith, reinsert );
				if( resolvedWord != null ) {
					return resolvedWord;
				}
			}
			return null;
		}

		private SyllabalizedWord TryResolveWithRule( string word, bool isPrefix, string toReplace, string replaceWith, string reinsert ) {
			string key = null;
			if( isPrefix ) {
				key = string.Format( "{0}{1}", replaceWith, word.Remove( 0, toReplace.Length ) );
			} else {
				key = string.Format( "{0}{1}", word.Remove( word.Length - toReplace.Length ), replaceWith ); 
			}
			key = key.ToLower();
			if( !database.ContainsKey( key ) ) {
				return null;
			}
			SyllabalizedWord existingWord = database[ key ];
			// TODO: Cache? Rules are to avoid wasting memory, but common words might be worthwhile
			string syllabalizedString = existingWord.SyllabalizedString;
			if( isPrefix ) {
				if( replaceWith.Length != 0 ) {
					syllabalizedString =  syllabalizedString.Remove( replaceWith.Length );
				}
				syllabalizedString = reinsert + syllabalizedString;
			} else {
				if( replaceWith.Length != 0 ) {
					syllabalizedString = syllabalizedString.Remove( syllabalizedString.Length - replaceWith.Length );
				}
				syllabalizedString += reinsert;
			}
			SyllabalizedWord newWord = ResolveSyllabalizedWord( word, syllabalizedString );

			return newWord;
		}

		private string[] GetPrefixRules( string prefix ) {
			switch( prefix ) {
			case "'": return new string[] { "", "'" };
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

		private string[] GetSuffixRules( string suffix ) {
			switch( suffix ) {
				// Ownership
			case "'s": return new string[]{ "", "'s" };
			case "'re": return new string[]{ "", "'re" };
			case "s'": return new string[]{ "", "s'" };
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
	}
}