#if TRG_NETSTANDARD1_5 || TRG_SL5
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using DigitalFlare.BusinessApp.Data;
using DigitalFlare.Reflection;
using DigitalFlare.Text;
using rt = DigitalFlare.Resources.XResource.XResourceTreeManager;

namespace DigitalFlare.Data {

	public partial class CsvDataTableParser {

		#region Nested types

		//[DebuggerNonUserCode]
		//[DebuggerStepThrough]
		sealed class P_RowTypeEmit {

			static readonly string __DynamicAssemblyNamePrefix = string.Format("{0}.{1}", typeof(Row).Namespace, "GeneratedCsvDataTableRowTypes");

			static readonly string __DynamicAssemblyRootNamespacePrefix = string.Format("{0}.{1}", typeof(Row).Namespace, "GeneratedCsvDataTableRowTypes");

			readonly object _syncRoot = new object();

			AssemblyBuilder _assemblyBuilder;

			ModuleBuilder _moduleBuilder;

			internal P_RowTypeEmit() { }

			public Type EmitType(string typeName, string[ ] columnNames) {
				if (string.IsNullOrEmpty(typeName))
					throw new ArgumentException(rt.Format(typeof(string), "CanNotNullOrEmpty"), "typeName");
				if (columnNames == null)
					throw new ArgumentNullException("columnNames");
				if (columnNames.Length < 1)
					throw new ArgumentException(rt.Format(typeof(Array), "CanNotEmpty"), "columnNames");
				lock (_syncRoot) {
					// Assembly.
					//
					if (_assemblyBuilder == null) {
#if TRG_SL5
						_assemblyBuilder =
							AppDomain
							.CurrentDomain
							.DefineDynamicAssembly(
								name: new AssemblyName(string.Format("{0}_{1}", __DynamicAssemblyNamePrefix, Guid.NewGuid().ToString("n").Substring(0, 8))),
								access: AssemblyBuilderAccess.Run);
#else
						_assemblyBuilder =
							AssemblyBuilder
							.DefineDynamicAssembly(
								name: new AssemblyName(string.Format("{0}_{1}", __DynamicAssemblyNamePrefix, Guid.NewGuid().ToString("n").Substring(0, 8))),
								access: AssemblyBuilderAccess.Run);
#endif
					}
					// Module.
					//
					if (_moduleBuilder == null)
						_moduleBuilder =
							_assemblyBuilder
							.DefineDynamicModule("main");
					// Type.
					//
					var type =
						_moduleBuilder
						.DefineType(string.Format("{0}.{1}_{2}", __DynamicAssemblyRootNamespacePrefix, typeName, Guid.NewGuid().ToString("n").Substring(0, 10)), TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable | TypeAttributes.AnsiClass, typeof(Row));
					// Create metadata fields.
					//
					var metadataFields = new Dictionary<string, FieldBuilder>(StringComparer.Ordinal);
					for (var i = 0; i < columnNames.Length; i++) {
						var field = type.DefineField(string.Format(CultureInfo.InvariantCulture, "C{0:x8}Property", columnNames[ i ].GetHashCode()), typeof(FieldProperty<string>), FieldAttributes.Private | FieldAttributes.NotSerialized | FieldAttributes.Static | FieldAttributes.InitOnly);
						metadataFields.Add(columnNames[ i ], field);
					}
					// Create type initializer.
					//
					var iLGen = type.DefineTypeInitializer().GetILGenerator();
					iLGen.BeginScope();
					for (var i = 0; i < columnNames.Length; i++) {
						iLGen.Emit(OpCodes.Ldc_I4, i);
						iLGen.Emit(OpCodes.Ldstr, columnNames[ i ]);
						iLGen.Emit(OpCodes.Call, typeof(Row).GetMethod("CreateColumnMetadataField", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic));
						iLGen.Emit(OpCodes.Stsfld, metadataFields[ columnNames[ i ] ]);
					}
					iLGen.Emit(OpCodes.Ret);
					iLGen.EndScope();
					// Create properties.
					//
					for (var i = 0; i < columnNames.Length; i++) {
						var property = type.DefineProperty(string.Format(CultureInfo.InvariantCulture, "C{0:x8}", columnNames[ i ].GetHashCode()), PropertyAttributes.None, typeof(string), Type.EmptyTypes);
						var propertyGetter = type.DefineMethod("get_" + property.Name, MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.SpecialName, CallingConventions.HasThis, typeof(string), Type.EmptyTypes);
						iLGen = propertyGetter.GetILGenerator();
						iLGen.BeginScope();
						var localVar0 = iLGen.DeclareLocal(typeof(string));
						iLGen.Emit(OpCodes.Ldarg_0);
						iLGen.Emit(OpCodes.Ldsfld, metadataFields[ columnNames[ i ] ]);
						iLGen.Emit(OpCodes.Call, typeof(Row).GetMethod("GetValue", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic));
						iLGen.Emit(OpCodes.Stloc_0);
						iLGen.Emit(OpCodes.Ldloc_0);
						iLGen.Emit(OpCodes.Ret);
						iLGen.EndScope();
						property.SetGetMethod(propertyGetter);
					}
					// Crete ctor.
					//
					var ctor = type.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, CallingConventions.HasThis, new Type[ ] { typeof(ICsvDataTable), typeof(IEnumerable<string>) });
					ctor.DefineParameter(1, ParameterAttributes.In, "table");
					ctor.DefineParameter(2, ParameterAttributes.In, "values");
					iLGen = ctor.GetILGenerator();
					iLGen.BeginScope();
					iLGen.Emit(OpCodes.Ldarg_0);
					iLGen.Emit(OpCodes.Ldarg_1);
					iLGen.Emit(OpCodes.Ldarg_2);
					iLGen.Emit(OpCodes.Call, typeof(Row).FindConstructor(new Type[ ] { typeof(ICsvDataTable), typeof(IEnumerable<string>) }));
					iLGen.Emit(OpCodes.Ret);
					iLGen.EndScope();
#if TRG_NETFRAMEWORK || TRG_SL5
					return type.CreateType();
#else
					return type.CreateTypeInfo().AsType();
#endif
				}
			}

		}

		#endregion

		#region Static members

		static readonly Dictionary<Guid, Type> __RowTypeRepository = new Dictionary<Guid, Type>();

		static readonly P_RowTypeEmit __RowTypeGenerator = new P_RowTypeEmit();

		static Type P_GenerateRowType(Dictionary<string, int> columnNamesOrder, IComparer<string> columnNameComparer) {
			columnNamesOrder.EnsureNotNull(nameof(columnNamesOrder));
			columnNameComparer.EnsureNotNull(nameof(columnNameComparer));
			var columnNamesSorted =
				columnNamesOrder
				.OrderBy(i => i.Key, columnNameComparer)
				.Select(i => i.Key)
				.ToArray();
			if (columnNamesSorted.Length < 1)
				throw new ArgumentException(rt.Format(typeof(Array), "CanNotEmpty"), nameof(columnNamesOrder));
			//
			string typeName;
			Guid typeSgnatureGuid;
			using (var acquiredBuffer = StringBuilderUtilities.AcquireBuffer()) {
				var sb = acquiredBuffer.StringBuilder;
				for (var i = 0; i < columnNamesSorted.Length; i++) {
					sb.Append('[');
					sb.Append(columnNamesSorted[ i ]);
					sb.Append(']');
					if (i != columnNamesSorted.Length - 1)
						sb.Append(',');
				}
				var sha1 = default(SHA1);
				try {
#if TRG_NETFRAMEWORK || TRG_SL5
					sha1 = new SHA1Managed();
#else
					sha1 = SHA1.Create();
#endif
					var guidBytes = new byte[ 16 ];
					sha1.Initialize();
					Array.Copy(sha1.ComputeHash(Encoding.Unicode.GetBytes(sb.ToString())), 0, guidBytes, 0, guidBytes.Length);
					typeName = "CsvRow_" + (typeSgnatureGuid = new Guid(guidBytes)).ToString("n");
				} finally {
					(sha1 as IDisposable)?.Dispose();
				}
			}
			Type result;
			if (!__RowTypeRepository.TryGetValue(typeSgnatureGuid, out result)) {
				Array.Sort(columnNamesSorted, (x, y) => columnNamesOrder[ x ].CompareTo(columnNamesOrder[ y ]));
				lock (__RowTypeRepository) {
					if (!__RowTypeRepository.TryGetValue(typeSgnatureGuid, out result))
						__RowTypeRepository.Add(typeSgnatureGuid, result = __RowTypeGenerator.EmitType(typeName, columnNamesSorted));
				}
			}
			return result;
		}

#endregion

	}

}
#endif