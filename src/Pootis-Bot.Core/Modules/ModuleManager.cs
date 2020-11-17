﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Pootis_Bot.Core;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.PackageDownloader;

namespace Pootis_Bot.Modules
{
	public sealed class ModuleManager : IDisposable
	{
		private readonly ModuleLoadContext loadContext;

		private readonly List<IModule> modules;

		private readonly string modulesDirectory;
		private readonly string assembliesDirectory;

		public ModuleManager(string modulesDir, string assembliesDir)
		{
			modulesDirectory = $"{Bot.ApplicationLocation}/{modulesDir}";
			assembliesDirectory = $"{Bot.ApplicationLocation}/{assembliesDir}";
			modules = new List<IModule>();
			loadContext = new ModuleLoadContext(modulesDirectory, assembliesDirectory);
		}

		public void Dispose()
		{
			foreach (IModule module in modules)
			{
				Logger.Info("Shutting down module {@ModuleName}...", module.GetModuleInfo().ModuleName);
				module.Dispose();
			}
		}

		public void LoadModules()
		{
			//Make sure the modules directory exists
			if (!Directory.Exists(modulesDirectory))
				Directory.CreateDirectory(modulesDirectory);

			//Get all dlls in the directory
			string[] dlls = Directory.GetFiles(modulesDirectory, "*.dll");
			foreach (string dll in dlls)
			{
				Assembly loadedAssembly = LoadModule(dll);
				LoadModulesInAssembly(loadedAssembly);
			}
		}

		private Assembly LoadModule(string dllPath)
		{
			return loadContext.LoadFromAssemblyPath(dllPath);
		}

		private void LoadModulesInAssembly(Assembly assembly)
		{
			foreach (Type type in assembly.GetTypes().Where(x => x.IsClass && x.IsPublic))
			{
				if (!typeof(IModule).IsAssignableFrom(type)) continue;

				if (!(Activator.CreateInstance(type) is IModule module)) continue;

				ModuleInfo moduleInfo = module.GetModuleInfo();
				VerifyModuleNuGetPackages(moduleInfo);

				Logger.Info("Loaded module {@Module} version {@Version}", moduleInfo.ModuleName,
					moduleInfo.ModuleVersion.ToString());

				module.Init();
				modules.Add(module);
			}
		}

		private void VerifyModuleNuGetPackages(ModuleInfo moduleInfo)
		{
			if (!Directory.Exists(assembliesDirectory))
				Directory.CreateDirectory(assembliesDirectory);

			NuGetPackageResolver packageResolver = new NuGetPackageResolver("netstandard2.1", $"{Bot.ApplicationLocation}/Packages/");

			foreach (ModuleNuGetPackage nuGetPackage in moduleInfo.NuGetPackages)
			{
				//The assembly already exists, it should be safe to assume that other dlls that it requires exist as well
				if(File.Exists($"{assembliesDirectory}/{nuGetPackage.AssemblyName}.dll")) continue;

				Logger.Info("Downloading NuGet packages for {@ModuleName}...", moduleInfo.ModuleName);

				//Download the packages and extract them
				List<string> dlls = packageResolver.DownloadPackage(nuGetPackage.PackageId, nuGetPackage.PackageVersion).GetAwaiter()
					.GetResult();

				foreach (string dll in dlls)
				{
					string dllName = Path.GetFileName(dll);
					string destination = $"{assembliesDirectory}/{dllName}";

					//The dll already exists, we don't need to copy it again
					if(File.Exists(destination))
						continue;

					//The required dll exists in the root
					if(File.Exists($"{Bot.ApplicationLocation}/{dllName}"))
						continue;

					File.Copy(dll, destination, true);
				}
			}

			packageResolver.Dispose();
		}
	}
}