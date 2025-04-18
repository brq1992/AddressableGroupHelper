using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace AddressableAssetTool
{
    public class DirectoryTreeGeneratorEditor : Editor
    {
        [MenuItem("Tools/AddressableAssetManager/Generate Directory Tree")]
        static void GenerateDirectoryTree()
        {
            var selectedObject = Selection.activeObject;

            if (selectedObject != null)
            {
                string path = AssetDatabase.GetAssetPath(selectedObject);
                if (Directory.Exists(path))
                {
                    Node rootNode = BuildTree(path, "");
                    List<string> lines = new List<string>();
                    rootNode.GenerateTreeText(lines, "");

                    string outputPath = Path.Combine(Application.dataPath, "DirectoryTree.txt");
                    File.WriteAllLines(outputPath, lines);
                    com.igg.core.IGGDebug.Log("Directory tree generated at: " + outputPath);

                    OpenWithNotepadPlusPlus(outputPath);
                }
                else
                {
                    com.igg.core.IGGDebug.LogError("Selected object is not a folder.");
                }
            }
            else
            {
                com.igg.core.IGGDebug.LogError("No folder selected.");
            }
        }

        static Node BuildTree(string path, string indent)
        {
            Node node = new Node(Path.GetFileName(path));

            foreach (var directory in Directory.GetDirectories(path))
            {
                node.AddChild(BuildTree(directory, indent + "|   "));
            }

            foreach (var file in Directory.GetFiles(path, "*.asset"))
            {
                string assetPath = file.Replace("\\", "/");
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                if (asset is AddressableAssetRule)
                {
                    node.AddChild(new Node(Path.GetFileName(file)));
                }
            }

            return node;
        }

        class Node
        {
            public string Name;
            public List<Node> Children = new List<Node>();

            public Node(string name)
            {
                Name = name;
            }

            public void AddChild(Node child)
            {
                Children.Add(child);
            }

            public void GenerateTreeText(List<string> lines, string indent)
            {
                lines.Add(indent + "|___" + Name);
                foreach (var child in Children)
                {
                    child.GenerateTreeText(lines, indent + "|   ");
                }
            }
        }

        static void OpenWithNotepadPlusPlus(string filePath)
        {
            string notepadPlusPlusPath = @"C:\Program Files\Notepad++\notepad++.exe";

            if (File.Exists(notepadPlusPlusPath))
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(notepadPlusPlusPath)
                {
                    Arguments = filePath
                };
                System.Diagnostics.Process.Start(startInfo);
            }
            else
            {
                com.igg.core.IGGDebug.LogError("Notepad++ not found at: " + notepadPlusPlusPath);
            }
        }
    }
}