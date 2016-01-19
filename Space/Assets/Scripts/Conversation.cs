using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using SA;

public class ConversationCharacter {
	public readonly string Id;
	public readonly Color TextColor;

	public ConversationCharacter( string id, UnityEngine.Color textColor ) {
		this.Id = id;
		this.TextColor = textColor;
	}
}

public class ConversationAnswer {
	public readonly string ContentId;
	public readonly string Destination;

	public ConversationAnswer( string contentId, string destination ) {
		this.ContentId = contentId;
		this.Destination = destination;
	}
}

public class ConversationEntry {
	public readonly ConversationCharacter Talker;
	public readonly string ContentId;
	public readonly ConversationAnswer[] Answers;

	public ConversationEntry( ConversationCharacter talker, string contentId, ConversationAnswer[] answers ) {
		this.Talker = talker;
		this.ContentId = contentId;
		this.Answers = answers;
	}
}

public class Conversation {
	public readonly ConversationEntry[] Entries;
	public readonly bool Pauses;

	public Conversation( ConversationEntry[] entries, bool pauses ) {
		this.Entries = entries;
		this.Pauses = pauses;
	}
}

public class Conversations {
	private static readonly string UNKNOWN_TALKER_ID = "char_unknown";

	public Dictionary<string, Conversation> Convos;
	private Dictionary<string, ConversationCharacter> characters;
	private Dictionary<string, ConversationEntry> tagToEntry;

	public void Load( Localization localization ) {
		characters = new Dictionary<string, ConversationCharacter>();

		characters.Add( UNKNOWN_TALKER_ID, new ConversationCharacter( UNKNOWN_TALKER_ID, Color.white ) );

		Convos = new Dictionary<string, Conversation>();
		tagToEntry = new Dictionary<string, ConversationEntry>();

		var filePath = Path.Combine( Application.streamingAssetsPath, "conversations.json" );
		using( var file = new StreamReader( filePath ) ) {
			JSONNode root = JSON.Parse( file.ReadToEnd() );
			JSONClass jsonCharacters = root[ "characters" ].AsObject;

			foreach( KeyValuePair<string, JSONNode> kvp in jsonCharacters ) {
				string id = kvp.Key;
				JSONNode jsonCharacter = kvp.Value;
				string colorString = jsonCharacter[ "text_color" ];
				Color textColor = Util.ParseColorString( colorString, Color.white );
				characters[ id ] = new ConversationCharacter( id, textColor );
			}

			var aliases = new Dictionary<string, ConversationCharacter>();
			JSONClass jsonAliases = root[ "aliases" ].AsObject;
			foreach( KeyValuePair<string, JSONNode> aliasKvp in jsonAliases ) {
				aliases[ aliasKvp.Value ] = characters[ aliasKvp.Key ];
			}

			JSONClass conversations = root[ "conversations" ].AsObject;

			foreach( KeyValuePair<string, JSONNode> kvp in conversations ) {
				string convoId = kvp.Key;
				JSONClass jsonConvo = kvp.Value.AsObject;

				JSONNode jsonPause = jsonConvo[ "pause" ];
				bool pauses = jsonPause == null ? false : jsonPause.AsBool;

				var entriesList = new List<ConversationEntry>();
				JSONArray jsonEntries = jsonConvo[ "entries" ].AsArray;
				int entryCounter = 0;
				foreach( JSONNode jsonEntry in jsonEntries ) {
					ConversationCharacter talker = null;
					string generatedId = null;
					string tag = null;
					List<ConversationAnswer> answers = new List<ConversationAnswer>();
					JSONArray answersNode = null;

					foreach( KeyValuePair<string, JSONNode> entryKvp in jsonEntry.AsObject ) {
						if( entryKvp.Key == "tag" ) {
							tag = entryKvp.Value;
							continue;
						} else if( entryKvp.Key == "answers" ) {
							answersNode = entryKvp.Value.AsArray;
							continue;
						}

						if( aliases.ContainsKey( entryKvp.Key ) ) {
							talker = aliases[ entryKvp.Key ];
						} else if( characters.ContainsKey( entryKvp.Key ) ) {
							talker = characters[ entryKvp.Key ];
						} else {
							SA.Debug.LogWarn( "Unknown conversation talker @" + convoId + "[" + entryCounter + "] : " + entryKvp.Key );
							talker = characters[ UNKNOWN_TALKER_ID ];
						}

						generatedId = string.Format( "conv_{0}_{1}",  convoId, entryCounter.ToString().PadLeft( 4, '0' ) );

						localization.Set( generatedId, entryKvp.Value );

						++entryCounter;
					}

					if( answersNode != null ) {
						int answerId = 0;
						foreach( JSONNode node in answersNode ) {
							string answerContentId = string.Format( "{0}_ans_{1}", generatedId, answerId );
							string content = null;
							string dest = null;

							JSONArray answerArray = node.AsArray;
							if( answerArray == null ) {
								// No destination
								content = node;
							} else {
								content = answerArray[ 0 ];
								dest = answerArray[ 1 ];
							}

							localization.Set( answerContentId, content );
							answers.Add( new ConversationAnswer( answerContentId, dest ) );
							++answerId;
						}
					}

					var entry = new ConversationEntry( talker, generatedId, answers.ToArray() );

					if( !string.IsNullOrEmpty( tag ) ) {
						if( tagToEntry.ContainsKey( tag ) ) {
							SA.Debug.LogError( "Duplicate tag {0} in {1}", tag, convoId );
						}
						SA.Debug.Log( "Tagging {0} as {1}", generatedId, tag );
						tagToEntry[ tag ] = entry;
					}

					entriesList.Add( entry );
				}

				Convos.Add( convoId, new Conversation( entriesList.ToArray(), pauses ) );
			}
		}
	}
}
