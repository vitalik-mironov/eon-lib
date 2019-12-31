#pragma warning disable CS0168 // Variable is declared but never used

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Eon.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Eon.Prochost.Internal {

	internal static class DebugRunCallback {

#if DEBUG

		public static async Task Callback(RunCallbackState state = default, string[ ] commandLineArgs = default) {
			Console.WriteLine($"Execution ran to:{Environment.NewLine}\t{MethodBase.GetCurrentMethod().FmtStr().GForMemberInfo()}.{Environment.NewLine}{Environment.NewLine}Press any key to continue...");
			Console.WriteLine();
			//
			try {
				if (state is null)
					await TaskUtilities.RunOnDefaultScheduler(factory: P_CallbackAsync, state: commandLineArgs).ConfigureAwait(false);
				else
					await TaskUtilities.RunOnDefaultScheduler(factory: P_CallbackAsync, state: (state, commandLineArgs)).ConfigureAwait(false);
			}
			catch (Exception exception) {
				if (Debugger.IsLogging())
					Debugger.Log(level: 0, category: "Error", message: $"An exception occurred in '{MethodBase.GetCurrentMethod().ToString()}':{Environment.NewLine}{exception.FmtStr().GNLI()}");
				if (Debugger.IsAttached)
					Debugger.Break();
				throw;
			}
		}

		static async Task P_CallbackAsync((RunCallbackState state, string[ ] commandLineArgs) args) {
			args.state.ArgProp($"{nameof(args)}.{nameof(args.state)}").EnsureNotNull();
			//
			// Run cases for debug.
			//
			await Task.CompletedTask;
		}

		static async Task P_CallbackAsync(string[ ] commandLineArgs = default) {
			await Task.CompletedTask;
			// Run cases for debug.
			//
			try {
				//var hostBuilder = new HostBuilder();
				//hostBuilder.UseDefaults(commandLineArgs: commandLineArgs);
				//var host = hostBuilder.Build();
				//var config = host.Services.GetRequiredService<IConfiguration>();
				//
				// ...
			}
			catch (Exception exception) {
				if (Debugger.IsAttached)
					Debugger.Break();
			}
		}

		static void P_DebugSettingsDeserialization(IHost host, IConfiguration config) {
			var instance = config.Deserialize<XClassA>(serviceProvider: host.Services);
			if (Debugger.IsAttached)
				Debugger.Break();
		}

#endif

	}

#if DEBUG

	[DataContract]
	public enum XEnum {

		[EnumMember]
		None = 0,

		[EnumMember]
		First,

		[EnumMember]
		Second

	}

	[DataContract]
	public class XClassA {

		public XClassA() { }

		[DataMember]
		public int Prop1 { get; set; }

		[DataMember]
		public string Prop2 { get; set; }

	}

	[DataContract]
	public class XClassB
		:XClassA {

		public XClassB() { }

		[DataMember]
		public XClassB Prop3 { get; set; }

		[DataMember(IsRequired = true)]
		public byte[ ] Prop4 { get; set; }

		[DataMember]
		public XEnum Prop5 { get; set; }

		[DataMember]
		public TimeSpan? Prop6 { get; set; }

	}

#endif

}

#pragma warning restore CS0168 // Variable is declared but never used