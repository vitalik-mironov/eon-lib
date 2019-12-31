#define DO_NOT_USE_OXY_LOGGING_API

using System.Runtime.CompilerServices;
using Eon.Diagnostics;

#if !DO_NOT_USE_OXY_LOGGING_API

using DigitalFlare.Diagnostics.Logging;

#endif

using Eon.Metadata;

namespace Eon.Description {

	public static class DescriptionUtilities {

		[MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
		public static bool IsDisabled(this IDescription description)
			=> (description.EnsureNotNull(nameof(description)).Value as IAbilityOption)?.IsDisabled ?? false;

		public static bool IsAutoActivationEnabled(this IDescription description)
			=> (description.EnsureNotNull(nameof(description)).Value as IAutoActivationOption)?.IsAutoActivationEnabled ?? true;

		// TODO: Put strings into the resources.
		//
		public static void LogDisabilityWarning(this object source, IDescription description, string logMessagePrologue = default, ArgumentPlaceholder<SeverityLevel> severityLevel = default) {
			source.EnsureNotNull(nameof(source));
			description.EnsureNotNull(nameof(description));
			//
#if !DO_NOT_USE_OXY_LOGGING_API
			source
				.IssueWarning(
					messagePrologue: logMessagePrologue,
					message: $"Поскольку описанием компонента отключено его функциональное использование (см. св-во '{nameof(IAbilityOption)}.{nameof(IAbilityOption.IsDisabled)}'), компонент не будет создан, инициализирован, активирован.{Environment.NewLine}\tОписание компонента:{description.FmtStr().GNLI2()}",
					severityLevel: severityLevel.Substitute(value: SeverityLevel.Lowest));
#endif
		}

		// TODO: Put strings into the resources.
		//
		public static void LogAutoActivationDisabilityWarning(this object source, IDescription description, string logMessagePrologue = default, ArgumentPlaceholder<SeverityLevel> severityLevel = default) {
			source.EnsureNotNull(nameof(source));
			description.EnsureNotNull(nameof(description));
			//
#if !DO_NOT_USE_OXY_LOGGING_API
			source
				.IssueWarning(
					messagePrologue: logMessagePrologue,
					message: $"Поскольку описанием компонента его авто-активация не включена (см. св-во '{nameof(IAutoActivationOption)}.{nameof(IAutoActivationOption.IsAutoActivationEnabled)}'), компонент не будет создан, инициализирован и активирован.{Environment.NewLine}\tОписание компонента:{description.FmtStr().GNLI2()}",
					severityLevel: severityLevel.Substitute(value: SeverityLevel.Lowest));
#endif
		}

		public static bool IsSelfOrDescendantOf(this IDescription descendant, IMetadata ancestor)
			=> MetadataUtilities.IsSelfOrDescendantOf(descendant: descendant, ancestor: ancestor);

	}

}

