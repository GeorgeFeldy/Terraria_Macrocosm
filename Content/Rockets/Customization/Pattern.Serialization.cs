﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Macrocosm.Common.Utils;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader.IO;

namespace Macrocosm.Content.Rockets.Customization
{
    internal partial class Pattern : TagSerializable
	{ 
		public string ToJSON(bool includeUnlocked = false, bool includeNonModifiableColors = false, bool includeColorFunctions = false) => ToJObject().ToString(Formatting.Indented);

		public JObject ToJObject(bool includeUnlocked = false, bool includeNonModifiableColors = false, bool includeColorFunctions = false)
		{
			JObject jsonObject = new()
			{
				["patternName"] = Name,
				["moduleName"] = ModuleName
			};

			if (includeUnlocked)
			{
				jsonObject["unlocked"] = Unlocked;
				jsonObject["unlockedByDefault"] = UnlockedByDefault;
			}

			JArray colorDataArray = new();
			foreach (var colorData in ColorData)
			{
				JObject colorDataObject = new();

				if (colorData.HasColorFunction)
				{
					if (includeColorFunctions)
					{
						colorDataObject["colorFunction"] = colorData.ColorFunction.Name;
						//colorDataObject["params"] = new JArray(colorData.ColorFunction.Parameters);
					}
					else
					{
						colorDataObject = new JObject();
					}
				}
				else
				{
					if (includeNonModifiableColors || colorData.IsUserModifiable)
					{
						colorDataObject["color"] = colorData.Color.GetHexText();

						if(!colorData.IsUserModifiable)
							colorDataObject["userModifiable"] = false;
					}
					else
					{
						colorDataObject = new JObject();
					}
				}

				colorDataArray.Add(colorDataObject);
			}

			// Remove trailing empty JObjects
			for (int i = colorDataArray.Count - 1; i >= 0; i--)
			{
				if (colorDataArray[i].Type == JTokenType.Object && !colorDataArray[i].HasValues)
					colorDataArray.RemoveAt(i);
				else
					break;
			}
 

			jsonObject["colorData"] = colorDataArray;

			return jsonObject; 
		}

		public static Pattern FromJSON(string json) => FromJObject(JObject.Parse(json));

		public static Pattern FromJObject(JObject patternObject)
		{
			string moduleName = patternObject.Value<string>("moduleName");

			if (!Rocket.DefaultModuleNames.Contains(moduleName))
				throw new SerializationException("Error: Invalid module name.");

			string patternName = patternObject.Value<string>("patternName");

			if (CustomizationStorage.Initialized && !CustomizationStorage.TryGetPattern(moduleName, patternName, out _) )
				throw new SerializationException("Error: Invalid pattern name.");

			bool unlockedByDefault = patternObject.Value<bool>("unlockedByDefault");

			JArray colorDataArray = patternObject.Value<JArray>("colorData");
			List<PatternColorData> colorDatas = new();

			foreach (JObject colorDataObject in colorDataArray.Cast<JObject>())
			{
				string colorHex = colorDataObject.Value<string>("color");
				string colorFunction = colorDataObject.Value<string>("colorFunction");

				if (!string.IsNullOrEmpty(colorFunction))
				{
					JArray parameters = colorDataObject.Value<JArray>("params");

					if (parameters is not null)
						colorDatas.Add(new(ColorFunction.CreateFunctionByName(colorFunction, parameters.ToObjectRecursive<object>())));
					else
						throw new SerializationException("$Error: Color function parameters not specified.");
				}
				else if (!string.IsNullOrEmpty(colorHex))
				{
					bool userModifiable = colorDataObject.Value<bool?>("userModifiable") ?? true;

					if (Utility.TryGetColorFromHex(colorHex, out Color color))
						colorDatas.Add(new(color, userModifiable));
					else
						throw new SerializationException($"Error: Invalid color code: {colorHex}.");
				}
				else
				{
					colorDatas.Add(new());
				}
			}

			return new Pattern(moduleName, patternName, unlockedByDefault, colorDatas.ToArray());
		}

		public Pattern Clone() => DeserializeData(SerializeData());

		public static readonly Func<TagCompound, Pattern> DESERIALIZER = DeserializeData;

		public TagCompound SerializeData()
		{
			TagCompound tag = new()
			{
				["Name"] = Name,
				["ModuleName"] = ModuleName
			};

			// Save the user-changed colors
			for (int i = 0; i < MaxColorCount; i++)
 				if (ColorData[i].IsUserModifiable)
					tag[$"Color{i}"] = ColorData[i].Color;  
 
			return tag;
		}

		public static Pattern DeserializeData(TagCompound tag)
		{
			string name = tag.GetString("Name");
			string moduleName = tag.GetString("ModuleName");

			Pattern originalPatternData = CustomizationStorage.GetPatternReference(moduleName, name);
			Pattern pattern = new(moduleName, name, originalPatternData.UnlockedByDefault, originalPatternData.ColorData);

			// Load the user-changed colors
			for (int i = 0; i < MaxColorCount; i++)
				if (tag.ContainsKey($"Color{i}") && pattern.ColorData[i].IsUserModifiable)
					pattern.ColorData[i] = pattern.ColorData[i].WithUserColor(tag.Get<Color>($"Color{i}"));

			return pattern;
		}
	}
}
