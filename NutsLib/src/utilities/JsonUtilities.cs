﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Vintagestory.API.Common;

namespace NutsLib;

/// <summary>
/// Uses System.Text.Json to manipulate jsons.
/// TODO: add replacement of text with variants.
/// </summary>
public static class JsonUtilities
{
    /// <summary>
    /// Attempt to deserialize a type from a json object.
    /// </summary>
    public static T? Get<T>(this JsonObject jObject, string path, T? defaultValue = default)
    {
        if (jObject.TryGetPropertyValue(path, out JsonNode? node) && node != null)
        {
            if (node is JsonObject obj)
            {
                T? objectValue = obj.Deserialize<T>() ?? defaultValue;
                return objectValue;
            }

            if (node is JsonArray arr)
            {
                T? arrayValue = arr.Deserialize<T>() ?? defaultValue;
                return arrayValue;
            }

            T desValue = node.GetValue<T>();
            return desValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// Do a full copy of a node, as a new object.
    /// </summary>
    public static JsonNode Copy(this JsonNode node)
    {
        // https://stackoverflow.com/questions/71000221/how-to-duplicate-a-jsonnode-jsonobject-jsonarray
        return JsonNode.Parse(node.ToJsonString())!;
    }

    /// <summary>
    /// Do a full copy of a node, as a new object.
    /// </summary>
    public static JsonObject Copy(this JsonObject node)
    {
        // https://stackoverflow.com/questions/71000221/how-to-duplicate-a-jsonnode-jsonobject-jsonarray
        return (JsonObject)JsonNode.Parse(node.ToJsonString())!;
    }

    /// <summary>
    /// Do a full copy of a node, as a new object.
    /// </summary>
    public static JsonArray Copy(this JsonArray node)
    {
        // https://stackoverflow.com/questions/71000221/how-to-duplicate-a-jsonnode-jsonobject-jsonarray
        return (JsonArray)JsonNode.Parse(node.ToJsonString())!;
    }

    /// <summary>
    /// Checks for "Variants" and "VariantOverrides" keys in a JsonObject.
    /// Replaces the "code" with it's new code. Merges every variant override that matches regex onto the variant.
    /// Executes a delegate for each new JsonObject, with "Variants" and "VariantOverrides" removed.
    /// variants: {
    ///     "color": [ "red", "green", "blue" ],
    ///     "weight": [ "1", "2", "3" ]
    /// }
    /// </summary>
    public static void ForEachVariant(JsonObject jsonObject, Action<JsonObject> action)
    {
        List<JsonObject> list = [];

        string originalCode = jsonObject["Code"]?.GetValue<string>() ?? throw new Exception($"Code missing from json {jsonObject}!");

        if (jsonObject["Variants"] is JsonObject variants)
        {
            JsonObject variantOverrides = jsonObject["VariantOverrides"] as JsonObject ?? [];
            jsonObject.Remove("Variants");
            jsonObject.Remove("VariantOverrides");

            string[][] variantGroups = new string[variants.Count][];
            int index = 0;
            foreach (KeyValuePair<string, JsonNode?> group in variants)
            {
                if (group.Value is not JsonArray array) throw new Exception($"Expected variant array for {originalCode}!");
                variantGroups[index] = array.Select(x => x?.GetValue<string>() ?? throw new Exception($"Unknown variant string for {originalCode}!")).ToArray();
                index++;
            }

            string[] variantNames = variants
                .Select(kvp => kvp.Key)
                .ToArray();

            string[][] combinations = variantGroups.Aggregate(
                Enumerable.Repeat(new string[] { "" }, 1), // Start with a single empty combination.
                (acc, current) => acc.SelectMany(a => current, (a, c) => a.Append(c).ToArray()) // Combine each element of the previous result with the current array.
            )
            .ToArray();

            // Instead create an array of arrays of strings.

            foreach (string[] variant in combinations)
            {
                //JsonObject newObject = jsonObject.Copy();
                string jsonCode = jsonObject.ToJsonString();

                string variantString = string.Join("-", variant);

                // Replace all variant wild cards.
                for (int i = 0; i < variantNames.Length; i++)
                {
                    string variantName = variantNames[i];
                    string variantValue = variant[i + 1];
                    jsonCode = jsonCode.Replace($"{{{variantName}}}", variantValue);
                }

                // Parse back into new object.
                JsonObject newObject = (JsonObject)JsonNode.Parse(jsonCode)!;

                string newCode = $"{originalCode}{variantString}";
                newObject["Code"] = newCode;

                // For each variant override, if the new code matches the regex, merge it onto the json.
                foreach (KeyValuePair<string, JsonNode?> variantOverride in variantOverrides)
                {
                    // Only merge objects.
                    if (variantOverride.Value is not JsonObject variantObject) continue;

                    // By default merge arrays
                    bool mergeArrays = variantObject["MergeArrays"]?.GetValue<bool>() ?? false;

                    if (Regex.IsMatch(newCode, GetSpecialRegex(variantOverride.Key)))
                    {
                        Merge(newObject, variantOverride.Value!, mergeArrays);
                    }
                }

                // Remove this key if it was merged onto the new object.
                newObject.Remove("MergeArrays");

                list.Add(newObject);
            }
        }
        else
        {
            jsonObject.Remove("Variants");
            jsonObject.Remove("VariantOverrides");

            list.Add(jsonObject);
        }

        // Execute delegate.
        foreach (JsonObject objectVariant in list)
        {
            action(objectVariant);
        }
    }

    /// <summary>
    /// Same as ForEachVariant but does not set the Code.
    /// </summary>
    public static void ForEachVariantNoCode(JsonObject jsonObject, Action<JsonObject> action)
    {
        List<JsonObject> list = [];

        if (jsonObject["Variants"] is JsonObject variants)
        {
            JsonObject variantOverrides = jsonObject["VariantOverrides"] as JsonObject ?? [];
            jsonObject.Remove("Variants");
            jsonObject.Remove("VariantOverrides");

            string[][] variantGroups = new string[variants.Count][];
            int index = 0;
            foreach (KeyValuePair<string, JsonNode?> group in variants)
            {
                if (group.Value is not JsonArray array) throw new Exception($"Expected variant array!");
                variantGroups[index] = array.Select(x => x?.GetValue<string>() ?? throw new Exception($"Unknown variant string!")).ToArray();
                index++;
            }

            string[] variantNames = variants
                .Select(kvp => kvp.Key)
                .ToArray();

            string[][] combinations = variantGroups.Aggregate(
                Enumerable.Repeat(new string[] { "" }, 1), // Start with a single empty combination.
                (acc, current) => acc.SelectMany(a => current, (a, c) => a.Append(c).ToArray()) // Combine each element of the previous result with the current array.
            )
            .ToArray();

            // Instead create an array of arrays of strings.

            foreach (string[] variant in combinations)
            {
                //JsonObject newObject = jsonObject.Copy();
                string jsonCode = jsonObject.ToJsonString();

                string variantString = string.Join("-", variant);

                // Replace all variant wild cards.
                for (int i = 0; i < variantNames.Length; i++)
                {
                    string variantName = variantNames[i];
                    string variantValue = variant[i + 1];
                    jsonCode = jsonCode.Replace($"{{{variantName}}}", variantValue);
                }

                // Parse back into new object.
                JsonObject newObject = (JsonObject)JsonNode.Parse(jsonCode)!;

                string newCode = $"{variantString}";

                // Remove dash at beginning of newCode if present.
                if (newCode.StartsWith("-")) newCode = newCode[1..];

                // For each variant override, if the new code matches the regex, merge it onto the json.
                foreach (KeyValuePair<string, JsonNode?> variantOverride in variantOverrides)
                {
                    // Only merge objects.
                    if (variantOverride.Value is not JsonObject variantObject) continue;

                    // By default merge arrays
                    bool mergeArrays = variantObject["MergeArrays"]?.GetValue<bool>() ?? false;

                    if (Regex.IsMatch(newCode, GetSpecialRegex(variantOverride.Key)))
                    {
                        Merge(newObject, variantOverride.Value!, mergeArrays);
                    }
                }

                // Remove this key if it was merged onto the new object.
                newObject.Remove("MergeArrays");

                list.Add(newObject);
            }
        }
        else
        {
            jsonObject.Remove("Variants");
            jsonObject.Remove("VariantOverrides");

            list.Add(jsonObject);
        }

        // Execute delegate.
        foreach (JsonObject objectVariant in list)
        {
            action(objectVariant);
        }
    }

    /// <summary>
    /// Gets special regex used in code. * represents wildcards and | is or.
    /// Example: "*-phyllite|*-halite-*".
    /// </summary>
    public static string GetSpecialRegex(string originalRegex)
    {
        return "^(" + string.Join("|", originalRegex.Split('|').Select(m => Regex.Escape(m).Replace("\\*", ".*"))) + ")$";
    }

    /// <summary>
    /// Recursively handle extensions and return a JsonObject with the extends value removed.
    /// </summary>
    public static JsonObject HandleExtends(JsonObject jsonObject, ICoreAPI api)
    {
        // Try to get the extends key.
        jsonObject.TryGetPropertyValue("Extends", out JsonNode? extendsValue);
        if (extendsValue != null)
        {
            // Check if the extends value is a string.
            if (extendsValue is JsonValue extendsString)
            {
                IAsset? assetToExtend = api.Assets.TryGet(extendsString.GetValue<string>());
                if (assetToExtend != null)
                {
                    // Parse the asset to extend.
                    string assetText = assetToExtend.ToText();
                    if (JsonNode.Parse(assetText) is JsonObject assetObject)
                    {
                        JsonObject extendedObject = HandleExtends(assetObject, api);

                        // Merge this onto the jsonObject.
                        Merge(jsonObject, extendedObject, true);
                    }
                }
            }
        }

        // Remove the extends key.
        jsonObject.Remove("Extends");

        return jsonObject;
    }

    /// <summary>
    /// Merges 2 JsonNodes together.
    /// </summary>
    public static JsonNode Merge(this JsonNode jsonBase, JsonNode jsonMerge, bool combineArrays)
    {
        switch (jsonBase)
        {
            case JsonObject jsonBaseObj when jsonMerge is JsonObject jsonMergeObj:
                {
                    KeyValuePair<string, JsonNode?>[] mergeNodesArray = jsonMergeObj.ToArray();

                    foreach (KeyValuePair<string, JsonNode?> prop in mergeNodesArray)
                    {
                        jsonBaseObj[prop.Key] = jsonBaseObj[prop.Key] switch
                        {
                            JsonObject jsonBaseChildObj when prop.Value is JsonObject jsonMergeChildObj => jsonBaseChildObj.Merge(jsonMergeChildObj, combineArrays),
                            JsonArray jsonBaseChildArray when prop.Value is JsonArray jsonMergeChildArray => jsonBaseChildArray.Merge(jsonMergeChildArray, combineArrays),
                            _ => prop.Value!.Copy()
                        };
                    }
                    break;
                }
            case JsonArray jsonBaseArray when jsonMerge is JsonArray jsonMergeArray:
                {
                    if (!combineArrays)
                    {
                        // If not merging arrays, clear the base array so it gets replaced.
                        jsonBaseArray.Clear();
                    }

                    JsonNode?[] mergeNodesArray = jsonMergeArray.ToArray();

                    foreach (JsonNode? mergeNode in mergeNodesArray)
                    {
                        if (mergeNode == null) continue;
                        jsonBaseArray.Add(mergeNode.Copy());
                    }
                    break;
                }
            default:
                throw new ArgumentException($"The JsonNode type [{jsonBase.GetType().Name}] is incompatible for merging with the target/base " + $"type [{jsonMerge.GetType().Name}]; merge requires the types to be the same.");
        }

        return jsonBase;
    }
}