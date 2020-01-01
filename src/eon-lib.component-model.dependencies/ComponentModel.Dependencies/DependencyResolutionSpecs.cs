using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Eon.ComponentModel.Dependencies {

	[DebuggerDisplay("{ToString(),nq}")]
	public class DependencyResolutionSpecs
		:Disposable, IDependencyResolutionSpecs {

		Type _dependencyType;

		bool _ensureResolution;

		bool _isNewInstanceRequired;

		IArgsTuple _newInstanceFactoryArgs;

		bool _preventNewInstanceInitialization;

		IDependencyResolutionModel _primaryResolutionModel;

		IDependencyResolutionSelectCriterion _selectCriterion;

		IDisposeRegistry _disposeRegistry;

		// TODO: Put strings into the resources.
		//
		public DependencyResolutionSpecs(
			Type dependencyType,
			bool ensureResolution = false,
			bool isNewInstanceRequired = false,
			IArgsTuple newInstanceFactoryArgs = null,
			bool preventNewInstanceInitialization = false,
			IDependencyResolutionModel primaryResolutionModel = null,
			IDependencyResolutionSelectCriterion selectCriterion = null,
			IDisposeRegistry disposeRegistry = null) {
			//
			dependencyType.EnsureNotNull(nameof(dependencyType));
			if (!isNewInstanceRequired && newInstanceFactoryArgs != null)
				throw
					new ArgumentException(
						message: $"Этот аргумент не применим и должен иметь значение '{((object)null).FmtStr().G()}', когда значение аргумента '{nameof(isNewInstanceRequired)}' равно '{false.FmtStr().G()}'.",
						paramName: nameof(newInstanceFactoryArgs));
			//
			_dependencyType = dependencyType;
			_ensureResolution = ensureResolution;
			_isNewInstanceRequired = isNewInstanceRequired;
			_newInstanceFactoryArgs = newInstanceFactoryArgs;
			_preventNewInstanceInitialization = preventNewInstanceInitialization;
			_primaryResolutionModel = primaryResolutionModel;
			_selectCriterion = selectCriterion;
			_disposeRegistry = disposeRegistry;
		}

		public Type DependencyType
			=> ReadDA(ref _dependencyType);

		public bool EnsureResolution
			=> ReadDA(ref _ensureResolution);

		public bool IsNewInstanceRequired
			=> ReadDA(ref _isNewInstanceRequired);

		public IArgsTuple NewInstanceFactoryArgs
			=> ReadDA(ref _newInstanceFactoryArgs);

		public bool PreventNewInstanceInitialization
			=> ReadDA(ref _preventNewInstanceInitialization);

		public IDependencyResolutionModel PrimaryResolutionModel
			=> ReadDA(ref _primaryResolutionModel);

		public IDependencyResolutionSelectCriterion SelectCriterion
			=> ReadDA(ref _selectCriterion);

		public IDisposeRegistry DisposeRegistry
			=> ReadDA(ref _disposeRegistry);

		// TODO: Put strings into the resources.
		//
		public override string ToString() {
			using (var acquiredBuffer = EonStringBuilderUtilities.AcquireBuffer()) {
				var sb = acquiredBuffer.StringBuilder;
				//
				sb.Append($"{GetType().FmtStr().G()}:");
				sb.Append($"{Environment.NewLine}\tТип зависимости:{(_dependencyType?.AssemblyQualifiedName ?? "-").FmtStr().GNLI2()}");
				sb.Append($"{Environment.NewLine}\tНовый экземпляр зависимости:{_isNewInstanceRequired.FmtStr().YesNo().FmtStr().GNLI2()}");
				sb.Append($"{Environment.NewLine}\tТребуемый конструктор экземпляра зависимости:{(_newInstanceFactoryArgs?.ToString() ?? "-").FmtStr().GNLI2()}");
				sb.Append($"{Environment.NewLine}\tКритерий выбора экземпляра зависимости:{(_selectCriterion?.ToString() ?? "-").FmtStr().GNLI2()}");
				sb.Append($"{Environment.NewLine}\t1-ый обработчик первичной модели разрешения:{(_primaryResolutionModel?.FirstOrDefault()?.ToString() ?? "-").FmtStr().GNLI2()}");
				return sb.ToString();
			}
		}

		protected override void Dispose(bool explicitDispose) {
			_dependencyType = null;
			_primaryResolutionModel = null;
			_selectCriterion = null;
			_newInstanceFactoryArgs = null;
			_disposeRegistry = null;
			//
			base.Dispose(explicitDispose);
		}


	}

}