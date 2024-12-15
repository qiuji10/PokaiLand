using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace PokaiLand.Utility.Editor
{
    public static class AddressableLabelsGenerator
    {
        private const int MAX_BITS = 32; // For uint
        private const string HASH_SEED = "PokaiLand"; // Consistent seed for hash generation

        [MenuItem("Tools/Generate Addressables Labels Enum")]
        public static void GenerateAddressableLabels()
        {
            string outputPath = Path.Combine(Application.dataPath, "Projects", "1_Scripts", "Utility", "Enum", "AddressableLabels.cs");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? Application.dataPath);

            List<string> labels = GetAllAddressableLabels();
            Dictionary<string, uint> labelValues = GenerateDeterministicValues(labels);
            GenerateLabelsEnum(outputPath, labelValues);

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

        private static Dictionary<string, uint> GenerateDeterministicValues(List<string> labels)
        {
            var labelValues = new Dictionary<string, uint>();
            var usedPositions = new HashSet<int>();

            foreach (var label in labels)
            {
                string sanitizedLabel = SanitizeLabel(label);
                int position = GetDeterministicPosition(sanitizedLabel, usedPositions);
                
                if (position >= 0 && position < MAX_BITS)
                {
                    labelValues[sanitizedLabel] = 1u << position;
                    usedPositions.Add(position);
                }
                else
                {
                    Debug.LogError($"Could not generate unique position for label: {label}. Too many labels or hash collision.");
                }
            }

            return labelValues;
        }

        private static int GetDeterministicPosition(string label, HashSet<int> usedPositions)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(HASH_SEED + label);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                uint hash = BitConverter.ToUInt32(hashBytes, 0);

                // Try to find an unused position
                for (int i = 0; i < MAX_BITS; i++)
                {
                    int position = (int)(hash % MAX_BITS);
                    if (!usedPositions.Contains(position))
                    {
                        return position;
                    }
                    hash = (hash >> 1) | (hash << 31); // Rotate hash
                }
            }

            return -1; // No available position found
        }

        private static void GenerateLabelsEnum(string outputPath, Dictionary<string, uint> labelValues)
        {
            using StreamWriter writer = new StreamWriter(outputPath);
            writer.WriteLine("using System;");
            writer.WriteLine();
            writer.WriteLine("namespace PokaiLand.Enum");
            writer.WriteLine("{");
            writer.WriteLine("    [Flags]");
            writer.WriteLine("    public enum EAddressableLabels : uint");
            writer.WriteLine("    {");

            foreach (var kvp in labelValues.OrderBy(x => x.Key))
            {
                writer.WriteLine($"        {kvp.Key} = {kvp.Value}u, // 0x{kvp.Value:X8}");
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