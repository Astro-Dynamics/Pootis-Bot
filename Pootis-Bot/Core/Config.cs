﻿using System;
using System.IO;
using Newtonsoft.Json;
using Pootis_Bot.Entities;

namespace Pootis_Bot.Core
{
	/// <summary>
	/// Stores config for the bot!
	/// </summary>
	public static class Config
	{
		private const string ConfigFolder = "Resources";
		private const string ConfigFile = "config.json";

		private const string ConfigVersion = "4";

		public static readonly ConfigFile bot = new ConfigFile();

		static Config()
		{
			if (!Directory.Exists(ConfigFolder)) //Creates the Resources folder if it doesn't exist.
				Directory.CreateDirectory(ConfigFolder);

			//If the config.json file doesn't exist it create a new one.
			if (!File.Exists(ConfigFolder + "/" + ConfigFile)) 
			{
				bot.ConfigVersion = ConfigVersion;
				AddHelpModuleDefaults();

				SaveConfig();

				Global.Log("Config.json was created. Is this your first time running?", ConsoleColor.Yellow);
			}
			else
			{
				string json =
					File.ReadAllText(ConfigFolder + "/" + ConfigFile); //If it does exist then it continues like normal.
				bot = JsonConvert.DeserializeObject<ConfigFile>(json);

				if (string.IsNullOrWhiteSpace(bot.ConfigVersion) || (bot.ConfigVersion != ConfigVersion))
				{
					bot.ConfigVersion = ConfigVersion;
					SaveConfig();
					Global.Log("Updated config to version " + ConfigVersion, ConsoleColor.Yellow);
				}
			}
		}

		/// <summary>
		/// Saves the config! DUH!
		/// </summary>
		public static void SaveConfig()
		{
			string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
			File.WriteAllText(ConfigFolder + "/" + ConfigFile, json);
		}

		/// <summary>
		/// Adds the default help modules
		/// </summary>
		public static void AddHelpModuleDefaults()
		{
			ConfigFile.HelpModule basic = new ConfigFile.HelpModule
			{
				Group = "Basic"
			};
			basic.Modules.Add("BasicCommands");
			basic.Modules.Add("Misc");

			bot.HelpModules.Add(basic);

			ConfigFile.HelpModule utils = new ConfigFile.HelpModule
			{
				Group = "Utils"
			};
			utils.Modules.Add("Utils");

			bot.HelpModules.Add(utils);

			ConfigFile.HelpModule fun = new ConfigFile.HelpModule
			{
				Group = "Fun"
			};
			fun.Modules.Add("GiphySearch");
			fun.Modules.Add("GoogleSearch");
			fun.Modules.Add("YoutubeSearch");
			fun.Modules.Add("TronaldDump");

			bot.HelpModules.Add(fun);

			ConfigFile.HelpModule audio = new ConfigFile.HelpModule
			{
				Group = "Audio"
			};
			audio.Modules.Add("Music");

			bot.HelpModules.Add(audio);
		}
	}
}