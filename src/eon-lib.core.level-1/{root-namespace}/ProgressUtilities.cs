using System;
using System.Threading.Tasks;

namespace Eon {

	public static class ProgressUtilities {

		public static async Task ReportAsync(this IAsyncProgress<IFormattable> progress, string value) {
			if (progress is null)
				throw new ArgumentNullException(paramName: nameof(progress));
			//
			await progress.ReportAsync(value: value.ToFormattableString());
		}

	}

}