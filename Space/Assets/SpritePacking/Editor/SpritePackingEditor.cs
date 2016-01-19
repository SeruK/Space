using UnityEngine;
using UE = UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Diagnostics;

public static class SpritePackingEditor {
	[MenuItem( "Assets/Custom/Pack Sprites" )]
	private static void MenuGenerate() {
		Generate();
	}

	private static string SPRITE_PACKING_ROOT = "SpritePacking";
	private static string SPRITE_PACKER_PATH = "texturepacker.jar";
	private static string SPRITE_PACKER_CMD = "/C java -jar \"{0}\" \"{1}\" \"{2}\" \"{3}\"";

	private static void Generate() {
		GenerateDirectory( inRootDir: "{0}/{1}".Fmt( SPRITE_PACKING_ROOT, "Tilesets" ),
		                   outRootDir: "Resources/Tilesets",
		                   createSubdirs: true,
		                   label: "Tileset" );
	}

	private static void GenerateDirectory( string inRootDir, string outRootDir, bool createSubdirs, string label ) {
		string rootAbsPath = GetAssetPath( inRootDir );

		AssetDatabase.StartAssetEditing();
		foreach( var texturesFolder in Directory.GetDirectories( rootAbsPath ) ) {
			string name = Path.GetFileName( texturesFolder );
			UE.Debug.Log( "Generating atlas in directory: " + name );
			
			string inputDir = string.Format( "{0}/{1}", inRootDir, name );
			string outputDir = createSubdirs ? string.Format( "{0}/{1}", outRootDir, name ) : outRootDir;
			RunSpritePacker( GetAssetPath( inputDir ), GetAssetPath( outputDir ), name );
			
			// TODO: This hangs the editor, hack around it
			//string atlasFilePath = GetAssetPath( "{0}/{1}.atlas" ).Fmt( outputDir, name );
			//// Append .txt so that it can be loaded as a TextAsset
			//File.Move( atlasFilePath, atlasFilePath + ".txt" );

			string atlasAssetPath = "Assets/{0}/{1}.atlas".Fmt( outputDir, name );
			AssetDatabase.ImportAsset( atlasAssetPath, ImportAssetOptions.ForceUpdate );
			//if( !string.IsNullOrEmpty( label ) ) {
			//	var atlasAsset = AssetDatabase.LoadAssetAtPath<TextAsset>( atlasAssetPath );
			//	AssetDatabase.SetLabels( atlasAsset, new string[] { label } );
			//}
		}
		AssetDatabase.StopAssetEditing();
	}

	private static void RunSpritePacker( string inputDir, string outputDir, string packFileName ) {
		string cmd = SPRITE_PACKER_CMD.Fmt( GetPath( SPRITE_PACKER_PATH ), inputDir, outputDir, packFileName );
		UE.Debug.Log( "Running cmd: " + cmd );
		Process process = Process.Start( "cmd.exe", cmd );
		process.WaitForExit();
	}

	private static string GetPath( string relative ) {
		return GetAssetPath( string.Format( "{0}/{1}", SPRITE_PACKING_ROOT, relative ) );
	}

	// TODO: Move this
	private static string GetAssetPath( string relative ) {
		return string.Format( "{0}/{1}", Application.dataPath, relative );
	}
}
