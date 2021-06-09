using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Replay.Lib;

namespace Replay.Lib
{
	/// <summary>
	/// Replaces <see cref="HarmonyPatch"/> and prevents mod crashing from having no class specified in the game's code.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
	public class AdofaiPatchAttribute : HarmonyPatch
	{
		public AdofaiPatchAttribute(string patchId, string className, string methodName, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodName = methodName;
		}

		public AdofaiPatchAttribute(string patchId, string className, string methodName, int minVersion = -1, int maxVersion = -1, params Type[] argumentTypes)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodName = methodName;
			info.argumentTypes = argumentTypes;
		}

		public AdofaiPatchAttribute(string patchId, string className, string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodName = methodName;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		public AdofaiPatchAttribute(string patchId, string className, MethodType methodType, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodType = methodType;
		}

		public AdofaiPatchAttribute(string patchId, string className, MethodType methodType, int minVersion = -1, int maxVersion = -1, params Type[] argumentTypes)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodType = methodType;
			info.argumentTypes = argumentTypes;
		}

		public AdofaiPatchAttribute(string patchId, string className, MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(typeof(ADOBase));
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodType = methodType;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		public AdofaiPatchAttribute(string patchId, Type assemblyType, string className, string methodName, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodName = methodName;
		}

		public AdofaiPatchAttribute(string patchId, Type assemblyType, string className, string methodName, int minVersion = -1, int maxVersion = -1, params Type[] argumentTypes)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodName = methodName;
			info.argumentTypes = argumentTypes;
		}

		public AdofaiPatchAttribute(string patchId, Type assemblyType, string className, string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodName = methodName;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		public AdofaiPatchAttribute(string patchId, Type assemblyType, string className, MethodType methodType, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodType = methodType;
		}

		public AdofaiPatchAttribute(string patchId, Type assemblyType, string className, MethodType methodType, int minVersion = -1, int maxVersion = -1, params Type[] argumentTypes)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodType = methodType;
			info.argumentTypes = argumentTypes;
		}

		public AdofaiPatchAttribute(string patchId, Type assemblyType, string className, MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Assembly.GetAssembly(assemblyType);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodType = methodType;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		public AdofaiPatchAttribute(string patchId, string assemblyName, string className, string methodName, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodName = methodName;
		}

		public AdofaiPatchAttribute(string patchId, string assemblyName, string className, string methodName, int minVersion = -1, int maxVersion = -1, params Type[] argumentTypes)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodName = methodName;
			info.argumentTypes = argumentTypes;
		}

		public AdofaiPatchAttribute(string patchId, string assemblyName, string className, string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodName = methodName;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		public AdofaiPatchAttribute(string patchId, string assemblyName, string className, MethodType methodType, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodType = methodType;
		}

		public AdofaiPatchAttribute(string patchId, string assemblyName, string className, MethodType methodType, int minVersion = -1, int maxVersion = -1, params Type[] argumentTypes)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodType = methodType;
			info.argumentTypes = argumentTypes;
		}

		public AdofaiPatchAttribute(string patchId, string assemblyName, string className, MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations, int minVersion = -1, int maxVersion = -1)
		{
			PatchId = patchId;
			ClassName = className;
			Assembly = Misc.GetAssemblyByName(assemblyName);
			MinVersion = minVersion;
			MaxVersion = maxVersion;
			info.declaringType = Assembly?.GetType(className);
			info.methodType = methodType;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		private void ParseSpecialArguments(Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			if (argumentVariations == null || argumentVariations.Length == 0)
			{
				info.argumentTypes = argumentTypes;
				return;
			}

			if (argumentTypes.Length < argumentVariations.Length)
			{
				throw new ArgumentException("argumentVariations contains more elements than argumentTypes",
					"argumentVariations");
			}

			List<Type> list = new List<Type>();
			for (int i = 0; i < argumentTypes.Length; i++)
			{
				Type type = argumentTypes[i];
				switch (argumentVariations[i])
				{
					case ArgumentType.Ref:
					case ArgumentType.Out:
						type = type.MakeByRefType();
						break;
					case ArgumentType.Pointer:
						type = type.MakePointerType();
						break;
				}

				list.Add(type);
			}
		}


		public string PatchId { get; set; }

		public string ClassName { get; set; }

		public string MethodName { get; set; }

		public int MinVersion { get; set; }

		public int MaxVersion { get; set; }

		public Assembly Assembly { get; set; }

		public bool IsEnabled { get; set; }
	}
}
