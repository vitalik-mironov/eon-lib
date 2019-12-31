using System;
using System.Globalization;
using System.Threading.Tasks;

using Eon.Context;
using Eon.Text;

namespace Eon.Description {

	public class XAppInstanceIdTemplateSubstitution
		:ITextTemplateVarSubstitution {

		#region Static members

		/// <summary>
		/// Value: 'name'.
		/// </summary>
		public static readonly string NameToken = "name";

		/// <summary>
		/// Value: 'version'.
		/// </summary>
		public static readonly string VersionToken = "version";

		/// <summary>
		/// Value: 'pid'.
		/// </summary>
		public static readonly string PidToken = "pid";

		/// <summary>
		/// Value: 'no-pid'.
		/// </summary>
		public static readonly string MissingPidSubstitution = "no-pid";

		#endregion

		readonly string _name;

		readonly string _version;

		readonly string _pid;

		public XAppInstanceIdTemplateSubstitution(string name, string version, string pid = default) {
			//
			name.Arg(nameof(name)).EnsureNotNull().EnsureNotEmptyOrWhiteSpace();
			version.Arg(nameof(version)).EnsureNotNull().EnsureNotEmptyOrWhiteSpace();
			pid.Arg(nameof(pid)).EnsureNotEmptyOrWhiteSpace();
			//
			_name = name;
			_version = version;
			_pid = pid;
		}

		public string Name
			=> _name;

		public string Version
			=> _version;

		public string Pid
			=> _pid;

		public virtual string Substitute(in TextTemplateVar var, CultureInfo culture, IContext ctx = null) {
			if (var.Var.EqualsOrdinalCI(otherString: NameToken))
				return _name;
			else if (var.Var.EqualsOrdinalCI(otherString: VersionToken))
				return _version;
			else if (var.Var.EqualsOrdinalCI(otherString: PidToken)) {
				if (_pid is null)
					return MissingPidSubstitution;
				else
					return _pid;
			}
			else
				throw new NotSupportedException(message: $"Specified template variable is not supported.{Environment.NewLine}\tVariable:{var.Var.FmtStr().GNLI2()}").SetErrorCode(code: GeneralErrorCodes.Operation.NotSupported);
		}

		public async Task<string> SubstituteAsync(TextTemplateVar var, CultureInfo culture, IContext ctx = null) {
			ctx.ThrowIfCancellationRequested();
			//
			await Task.CompletedTask;
			//
			return Substitute(var: var, culture: culture, ctx: ctx);
		}

	}

}
