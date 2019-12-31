using System;
using System.Diagnostics;
using System.Threading;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Diagnostics {

	public static class ProcessUtilities {

		#region Nested types

		sealed class P_CurrentProcessInfo {

			public readonly int Id;

			public readonly string Name;

			public DateTimeOffset? StartTime;

			public string MainModuleFileName;

			internal P_CurrentProcessInfo(int processId, string processName, DateTimeOffset? processStartTime, string mainModuleFileName) {
				Id = processId;
				Name = processName;
				StartTime = processStartTime;
				MainModuleFileName = mainModuleFileName;
			}

		}

		#endregion

		static P_CurrentProcessInfo __CurrentProcessInfo;

		static P_CurrentProcessInfo P_GetCurrentProcessInfo() {
			var result = itrlck.Get(ref __CurrentProcessInfo);
			if (result is null) {
				using (var currentProcess = Process.GetCurrentProcess()) {
					DateTime? processStartTime;
					try {
						processStartTime = currentProcess.StartTime;
					}
					catch {
						processStartTime = null;
					}
					result =
						new P_CurrentProcessInfo(
							processId: currentProcess.Id,
							processName: currentProcess.ProcessName,
							processStartTime: processStartTime == null ? (DateTimeOffset?)null : new DateTimeOffset(processStartTime.Value),
							mainModuleFileName: currentProcess.MainModule?.FileName);
				}
				result = Interlocked.CompareExchange(ref __CurrentProcessInfo, result, null) ?? result;
			}
			return result;
		}

		public static int CurrentProcessId
			=> P_GetCurrentProcessInfo().Id;

		public static string CurrentProcessName
			=> P_GetCurrentProcessInfo().Name;

		public static DateTimeOffset? CurrentProcessStartTime
			=> P_GetCurrentProcessInfo().StartTime;

		public static string CurrentProcessMainModuleFileName
			=> P_GetCurrentProcessInfo().MainModuleFileName;

	}

}