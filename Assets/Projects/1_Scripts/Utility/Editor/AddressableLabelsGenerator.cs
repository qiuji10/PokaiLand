using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace PokaiLand.Utility.Editor
{
    public static class AddressableLabelsGenerator
    {
        [MenuItem("Tools/Generate Addressables Labels Enum")]
        public static void GenerateAddressableLabels()
        {
            string outputPath = Path.Combine(Application.dataPath, "Projects", "1_Scripts", "Utility", "Enum", "AddressableLabels.cs");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? Application.dataPath);

            List<string> labels = GetAllAddressableLabels();
            GenerateLabelsEnum(outputPath, labels);

            AssetDatabase.Refresh();
            Debug.Log($"Generated Addressable Labels Enum: {labels.Count} labels");
        }

        private static List<string> GetAllAddressableLabels()
        {
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            return settings.groups
                .SelectMany(g => g.entries)
                .Select(e => e.labels)
                .SelectMany(l => l)
                .Distinct()
                .OrderBy(l => l)
                .ToList();
        }

        private static void GenerateLabelsEnum(string outputPath, List<string> labels)
        {
            using StreamWriter writer = new StreamWriter(outputPath);
            writer.WriteLine("using System;");
            writer.WriteLine();
            writer.WriteLine("namespace PokaiLand.Enum");
            writer.WriteLine("{");
            writer.WriteLine("    [Flags]");
            writer.WriteLine("    public enum EAddressableLabels");
            writer.WriteLine("    {");

            for (int i = 0; i < labels.Count; i++)
            {
                string sanitizedLabel = SanitizeLabel(labels[i]);
                writer.WriteLine($"        {sanitizedLabel} = 1 << {i},");
            }

            writer.WriteLine("    }");
            writer.WriteLine("}");
        }

        private static string SanitizeLabel(string label)
        {
            return string.Concat(label.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)))
                .Replace(".", "")
                .Replace("/", "");
        }
    }
}