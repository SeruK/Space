﻿using UnityEngine;
using System.Collections.Generic;
using ItemType = Item.ItemType;
using SimpleJSON;
using System.IO;
using SA;

public class Objective {
	public enum ObjType {
		GetItem,
		KillEntity
	}
	
	public readonly ObjType  ObjectiveType;

	public readonly string   Id;
	public readonly string   TitleId;
	public readonly string   EndConvoId;

	public readonly ItemType ItemType;
	public readonly string   EntityType;

	public readonly int      Amount;

	public readonly Objective[] SubObjectives;

	public Objective( string id, string titleId, string endConvoId, ItemType itemType, int amount, Objective[] subObjectives ) {
		this.Id = id;
		this.TitleId = titleId;
		this.EndConvoId = endConvoId;
		this.ObjectiveType = ObjType.GetItem;
		this.ItemType = itemType;
		this.Amount = amount;
		this.SubObjectives = subObjectives;
	}

	public Objective( string id, string titleId, string endConvoId, string entityType, int amount, Objective[] subObjectives ) {
		this.Id = id;
		this.TitleId = titleId;
		this.EndConvoId = endConvoId;
		this.ObjectiveType = ObjType.KillEntity;
		this.EntityType = entityType;
		this.Amount = amount;
		this.SubObjectives = subObjectives;
	}
}

public class Quest {
	public readonly string TitleId;
	public readonly string DescriptionId;
	public readonly string StartConvoId;
	public readonly Objective[] Objectives;
	public readonly string NextQuestId;

	public Quest( string titleId, string descriptionId, string startConvoId, Objective[] objectives, string nextQuestId ) {
		this.TitleId = titleId;
		this.DescriptionId = descriptionId;
		this.StartConvoId = startConvoId;
		this.Objectives = objectives;
		this.NextQuestId = nextQuestId;
	}
}

public class Quests {
	public class OngoingQuest {
		public readonly string QuestId;
		public readonly Quest Quest;
		public List<string> CompletedObjectivesIds;

		public OngoingQuest( string questId, Quest quest ) {
			this.QuestId = questId;
			this.Quest = quest;
			CompletedObjectivesIds = new List<string>();
		}
	}

	private static readonly string GET_ITEM_FORMAT_ID = "objective_get_item_format";
	private static readonly string KILL_ENTITY_FORMAT_ID = "objective_kill_entity_format";

	public List<OngoingQuest> CurrentQuests {
		get { return currentQuests; }
	}

	public event System.Action<OngoingQuest> OnQuestStarted;
	public event System.Action<OngoingQuest, Objective> OnObjectiveCompleted;
	public event System.Action<OngoingQuest> OnQuestCompleted;

	private Dictionary<string, Quest> quests;
	private List<OngoingQuest> currentQuests;

	public void StartQuest( string id ) {
		if( !quests.ContainsKey( id ) ) {
			SA.Debug.LogWarn( "No quest called " + id + " exists" );
			return;
		}

		SA.Debug.Log( "Starting quest: " + id );
		var newQuest = new OngoingQuest( id, quests[ id ] );
		currentQuests.Add( newQuest );
		if( OnQuestStarted != null ) {
			OnQuestStarted( newQuest );
		}
	}

	public void AquiredItem( ItemType item ) {
		var completedQuests = new List<OngoingQuest>();
		foreach( OngoingQuest quest in currentQuests ) {
			bool completed = TryCompleteQuest( quest, ( Objective objective ) => {
				return objective.ObjectiveType == Objective.ObjType.GetItem &&
				       objective.ItemType == item;
			} );

			if( completed ) {
				completedQuests.Add( quest );
			}
		}
		foreach( var completedQuest in completedQuests ) {
			QuestCompleted( completedQuest );
		}
	}

	private bool TryCompleteQuest( OngoingQuest quest, System.Func<Objective, bool> didCompleteObjective ) {
		bool objectivesLeft = FindIncompleteObjectives( quest, quest.Quest.Objectives, ( Objective foundObjective ) => {
			if( didCompleteObjective( foundObjective ) ) {
				SA.Debug.Log( "Finished objective: " + foundObjective.Id );
				quest.CompletedObjectivesIds.Add( foundObjective.Id );
				if( OnObjectiveCompleted != null ) {
					OnObjectiveCompleted( quest, foundObjective );
				}
				return true;
			}
			return false;
		} );

		return !objectivesLeft;
	}

	private bool FindIncompleteObjectives( OngoingQuest quest, Objective[] objectives, System.Func<Objective, bool> onFound ) {
		bool objectivesLeft = false;

		foreach( var objective in objectives ) {
			if( QuestObjectiveCompleted( quest, objective ) ) {
				objectivesLeft = FindIncompleteObjectives( quest, objective.SubObjectives, onFound ) || objectivesLeft;
				continue;
			}

			if( onFound( objective ) ) {
				objectivesLeft = objectivesLeft || objective.SubObjectives.Length != 0;
			} else {
				objectivesLeft = true;
			}
		}

		return objectivesLeft;
	}

	// TODO: Rework this for less messyness
	public void AllIncompleteObjectives( OngoingQuest quest, Objective[] objectives, ref List<Objective> incompleteObjectives ) {
		foreach( var objective in objectives ) {
			if( QuestObjectiveCompleted( quest, objective ) ) {
				AllIncompleteObjectives( quest, objective.SubObjectives, ref incompleteObjectives );
				continue;
			}
			incompleteObjectives.Add( objective );
		}
	}

	private bool QuestObjectiveCompleted( OngoingQuest quest, Objective objective ) {
		return quest.CompletedObjectivesIds.Exists( ( string completedId ) => { return completedId == objective.Id; } );
	}

	private void QuestCompleted( OngoingQuest quest ) {
		SA.Debug.Log( "Completed quest: " + quest.QuestId );

		string nextId = quest.Quest.NextQuestId;
		currentQuests.Remove( quest );

		if( OnQuestCompleted != null ) {
			OnQuestCompleted( quest );
		}

		StartQuest( nextId );
	}

	public void Load( Localization localization ) {
		quests = new Dictionary<string, Quest>();
		currentQuests = new List<OngoingQuest>();

		var filePath = Path.Combine( Application.streamingAssetsPath, "quests.json" );
		using( var file = new StreamReader( filePath ) ) {
			JSONClass root = JSON.Parse( file.ReadToEnd() ).AsObject;

			foreach( KeyValuePair<string, JSONNode> kvp in root ) {
				string questId = kvp.Key;

				if( quests.ContainsKey( questId ) ) {
					SA.Debug.LogWarn( "Quest " + questId + " already existed" );
					continue;
				}

				JSONClass jsonQuest = kvp.Value.AsObject;

				string generatedTitleId = string.Format( "quest_{0}_title", questId );
				string generatedDescId  = string.Format( "quest_{0}_desc", questId );

				string title = jsonQuest[ "title" ] ?? generatedTitleId;
				string desc  = jsonQuest[ "desc" ] ?? generatedDescId;

				localization.Set( generatedTitleId, title );
				localization.Set( generatedDescId, desc );

				Objective[] objectives = ReadObjectives( localization, jsonQuest[ "objectives" ].AsArray, questId );

				string startConvoId = jsonQuest[ "start_convo" ];
				string nextQuestId = jsonQuest[ "next_quest" ];

				quests[ questId ] = new Quest( generatedTitleId, generatedDescId, startConvoId, objectives, nextQuestId );
			}
		}
	}

	private Objective[] ReadObjectives( Localization localization, JSONArray jsonObjectives, string parentId ) {
		var objectivesList = new List<Objective>();

		int objectiveIdCounter = 0;
		foreach( JSONNode objectiveNode in jsonObjectives ) {
			string objectiveId = string.Format( "{0}_obj{1}", parentId, objectiveIdCounter.ToString().PadLeft( 2, '0' ) );

			JSONClass jsonObjective = objectiveNode.AsObject;
			string objectiveType = jsonObjective[ "objective_type" ];

			JSONNode jsonAmount = jsonObjective[ "amount" ];
			int amount = jsonAmount == null ? 0 : jsonAmount.AsInt;

			string endConvoId = jsonObjective[ "end_convo" ];

			JSONNode jsonSubs = jsonObjective[ "sub_objectives" ];
			Objective[] subObjectives = new Objective[ 0 ];
			if( jsonSubs != null ) {
				subObjectives = ReadObjectives( localization, jsonSubs.AsArray, objectiveId );
			}

			string titleId = string.Format( "{0}_title", objectiveId );
			string title = jsonObjective[ "title" ];

			if( objectiveType == "GetItem" ) {
				ItemType itemType = Item.ItemTypeFromString( jsonObjective[ "item_type" ] );
				string format = localization.Get( GET_ITEM_FORMAT_ID );
				title = title ?? string.Format( format, amount, GetItemName( localization, itemType ) );
				objectivesList.Add( new Objective( objectiveId, titleId, endConvoId, itemType, amount, subObjectives ) );
			} else if( objectiveType == "KillEntity" ) {
				string entityType = jsonObjective[ "entity_type" ];
				string entityName = localization.Get( entityType );
				string format = localization.Get( KILL_ENTITY_FORMAT_ID );
				title = title ?? string.Format( format, amount, entityName );
				objectivesList.Add( new Objective( objectiveId, titleId, endConvoId, entityType, amount, subObjectives ) );
			}

			localization.Set( titleId, title ?? "[OBJECTIVE_TITLE]" );

			++objectiveIdCounter;
		}

		return objectivesList.ToArray();
	}

	private string GetItemName( Localization localization, ItemType itemType ) {
		string name = Item.LocalizedNameId( itemType );
		name = localization.Get( name );
		if( name == null ) {
			name = Item.FallbackName( itemType );
		}
		return name ?? "[UNKNOWN_ITEM]";
	}
}
