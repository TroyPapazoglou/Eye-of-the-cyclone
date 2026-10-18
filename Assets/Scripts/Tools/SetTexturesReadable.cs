using System.IO;
using UnityEditor;
using UnityEngine;

//public class SetTexturesReadable : EditorWindow
//{
//    [MenuItem("Tools/Make Textures Readable")]
//    public static void MakeTexturesReadable()
//    {
//        // Define the path to your target folder
//        string folderPath = "Assets/Resources/images";

//        // Check if the folder actually exists
//        if (!Directory.Exists(folderPath))
//        {
//            Debug.LogError($"The folder path '{folderPath}' does not exist!");
//            return;
//        }

//        // Find all files in the folder and its subfolders
//        string[] filePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
//        int changedCount = 0;

//        foreach (string filePath in filePaths)
//        {
//            // Load the importer for this specific asset file
//            TextureImporter textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;

//            // If the file is a texture and doesn't have Read/Write enabled yet
//            if (textureImporter != null && !textureImporter.isReadable)
//            {
//                // Enable Read/Write
//                textureImporter.isReadable = true;

//                // Save the changes and force Unity to reimport the image
//                textureImporter.SaveAndReimport();
//                changedCount++;
//            }
//        }

//        Debug.Log($"Success! Successfully enabled Read/Write on {changedCount} textures.");
//    }
//}
