using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

using Eon.Collections;
using Eon.Data.Filters.Criteria.Operators;
using Eon.Reflection;
using Eon.Runtime.Serialization;

using Newtonsoft.Json;

using itrlck = Eon.Threading.InterlockedUtilities;

namespace Eon.Data.Filters.Criteria {

	[DataContract]
	public sealed class TypeNameCriterion
		:AsReadOnlyValidatableBase, ICriterion, IAsReadOnly<TypeNameCriterion> {

		#region Nested types

		sealed class P_LeftOperand
			:ICriterionOperand {

			readonly TypeNameCriterion _criterion;

			internal P_LeftOperand(TypeNameCriterion criterion) {
				criterion.EnsureNotNull(nameof(criterion));
				//
				_criterion = criterion;
			}

			public Type TypeConstraint
				=> typeof(string);

			// TODO: Put strings into the resources.
			//
			public Expression BuildExpression(Expression input = default) {
				if (input is null)
					input = Expression.Parameter(type: typeof(Type), name: "type");
				var nameQualification = _criterion.NameQualification;
				PropertyInfo prop;
				switch (nameQualification) {
					case TypeNameQualification.Type:
						prop = typeof(Type).GetTypeInfo().GetProperty(name: nameof(Type.Name));
						break;
					case TypeNameQualification.Namespace:
						prop = typeof(Type).GetTypeInfo().GetProperty(name: nameof(Type.FullName));
						break;
					case TypeNameQualification.Assembly:
						prop = typeof(Type).GetTypeInfo().GetProperty(name: nameof(Type.AssemblyQualifiedName));
						break;
					default:
						throw
							new NotSupportedException(message: $"Значение '{nameQualification}' типа '{typeof(TypeNameQualification)}' не поддерживается.");
				}
				return Expression.Property(expression: input, property: prop);
			}

		}

		sealed class P_RightOperand
			:ICriterionOperand {

			readonly TypeNameCriterion _criterion;

			internal P_RightOperand(TypeNameCriterion criterion) {
				criterion.EnsureNotNull(nameof(criterion));
				//
				_criterion = criterion;
			}

			public Type TypeConstraint
				=> typeof(string);

			public Expression BuildExpression(Expression input = default) {
				if (input is null)
					return Expression.Constant(value: _criterion.Operand, type: typeof(string));
				else
					return input;
			}

		}

		#endregion

		#region Static & constant members

		public static readonly IReadOnlyList<IStringCriterionOperator> DefinedOperators =
			new ListReadOnlyWrap<IStringCriterionOperator>(
				list:
				new IStringCriterionOperator[ ] {
					CriterionOperatorBase.StringContains,
					CriterionOperatorBase.StringNotContains,
					CriterionOperatorBase.StringMatchRegex,
					CriterionOperatorBase.StringNotMatchRegex,
					CriterionOperatorBase.StringEquals,
					CriterionOperatorBase.StringNotEquals,
					CriterionOperatorBase.StringStartsWith,
					CriterionOperatorBase.StringEndsWith,
			});

		public static readonly IStringCriterionOperator DefaultOperator = CriterionOperatorBase.StringEquals;

		#endregion

		TypeNameQualification _nameQualification;

		IStringCriterionOperator _operator;

		string _operand;

		P_LeftOperand _left;

		P_RightOperand _right;

		Expression _builtExpression;

		Func<Type, bool> _compiled;

		public TypeNameCriterion()
			: this(nameQualification: default) { }

		public TypeNameCriterion(TypeNameQualification nameQualification = default, IStringCriterionOperator @operator = default, string operand = default, bool isReadOnly = default)
			: base(isReadOnly: isReadOnly) {
			//
			_nameQualification = nameQualification;
			_operator = @operator;
			_operand = operand;
			_left = null;
			_right = null;
			_builtExpression = null;
		}

		public TypeNameCriterion(TypeNameCriterion other, bool isReadOnly = default)
			: this(
					nameQualification: other.EnsureNotNull(nameof(other)).Value.NameQualification,
					@operator: other.Operator,
					operand: other.Operand,
					isReadOnly: isReadOnly) {
			//
			if (isReadOnly && other.IsReadOnly) {
				_left = other._left;
				_right = other._right;
				_builtExpression = other._builtExpression;
			}
		}

		[JsonConstructor]
		TypeNameCriterion(SerializationContext context)
			: base(ctx: context) { }

		[DataMember(Order = 0, IsRequired = false, EmitDefaultValue = true)]
		public TypeNameQualification NameQualification {
			get => _nameQualification;
			set {
				EnsureNotReadOnly();
				_nameQualification = value;
			}
		}

		[DataMember(Order = 1, IsRequired = false, EmitDefaultValue = true)]
		public IStringCriterionOperator Operator {
			get => _operator;
			set {
				EnsureNotReadOnly();
				_operator = value;
			}
		}

		[DataMember(Order = 2, IsRequired = false, EmitDefaultValue = true)]
		public string Operand {
			get => _operand;
			set {
				EnsureNotReadOnly();
				_operand = value;
			}
		}

		IReadOnlyList<ICriterionOperator> ICriterion.DefinedOperators
			=> DefinedOperators;

		ICriterionOperator ICriterion.DefaultOperator
			=> DefaultOperator;

		P_LeftOperand P_Left
			=> itrlck.UpdateIfNull(ref _left, factory: () => new P_LeftOperand(criterion: this));

		// TODO: Put strings into the resources.
		//
		ICriterionOperand ICriterion.Left {
			get => P_Left;
			set => throw new InvalidOperationException(message: $"Свойство '{nameof(ICriterion.Left)}' недоступно для редактирования.{Environment.NewLine}\tОбъект:{this.FmtStr().GNLI2()}");
		}

		bool ICriterion.IsLeftReadOnly
			=> true;

		ICriterionOperator ICriterion.Operator {
			get => Operator;
			set {
				var @operator =
					value
					.Arg(nameof(value))
					.EnsureOfType<ICriterionOperator, IStringCriterionOperator>()
					.Value;
				//
				Operator = @operator;
			}
		}

		P_RightOperand P_Right
			=> itrlck.UpdateIfNull(ref _right, factory: () => new P_RightOperand(criterion: this));

		// TODO: Put strings into the resources.
		//
		ICriterionOperand ICriterion.Right {
			get => P_Right;
			set => throw new InvalidOperationException(message: $"Свойство '{nameof(ICriterion.Right)}' недоступно для редактирования.{Environment.NewLine}\tОбъект:{this.FmtStr().GNLI2()}");
		}

		bool ICriterion.IsRightReadOnly
			=> true;

		protected override void CreateReadOnlyCopy(out AsReadOnlyValidatableBase readOnlyCopy)
			=> readOnlyCopy = new TypeNameCriterion(other: this, isReadOnly: true);

		public new TypeNameCriterion AsReadOnly() {
			if (IsReadOnly)
				return this;
			else
				return new TypeNameCriterion(other: this, isReadOnly: true);
		}

		// TODO: Put strings into the resources.
		//
		protected override void OnValidate() {
			var @operator =
				Operator
				.ArgProp(nameof(Operator))
				.EnsureNotNull()
				.EnsureValid()
				.Value;
			if (!DefinedOperators.Contains(@operator))
				throw new EonException(message: $"Свойство '{nameof(Operator)}' имеет недопустимое значение. Указанный оператор не определен для данного критерия.{Environment.NewLine}\tОператор:{@operator.FmtStr().GNLI2()}");
		}

		public Expression BuildExpression(Expression input = default) {
			bool userInput;
			if (input is null) {
				input = Expression.Parameter(type: typeof(Type), name: "type");
				userInput = false;
			}
			else
				userInput = true;
			//
			var readOnlyVersion = AsReadOnly();
			readOnlyVersion.Validate();
			Expression result;
			if (ReferenceEquals(this, readOnlyVersion) && !userInput && !((result = itrlck.Get(ref _builtExpression)) is null))
				return result;
			else {
				result =
					readOnlyVersion
					.Operator
					.BuildExpression(
						left: readOnlyVersion.P_Left.BuildExpression(input: input),
						right: readOnlyVersion.P_Right.BuildExpression(input: Expression.Constant(value: readOnlyVersion.Operand, type: typeof(string))));
				if (ReferenceEquals(this, readOnlyVersion) && !userInput)
					itrlck.UpdateIfNull(ref _builtExpression, result);
				return result;
			}
		}

		public Func<Type, bool> Compile() {
			var readOnlyVersion = AsReadOnly();
			readOnlyVersion.Validate();
			Func<Type, bool> result;
			if (ReferenceEquals(this, readOnlyVersion) && !((result = itrlck.Get(ref _compiled)) is null))
				return result;
			else {
				var typeParameter = Expression.Parameter(type: typeof(Type), name: "type");
				result =
					Expression
					.Lambda<Func<Type, bool>>(body: readOnlyVersion.BuildExpression(input: typeParameter), parameters: typeParameter)
					.Compile();
				if (ReferenceEquals(this, readOnlyVersion))
					itrlck.UpdateIfNull(ref _compiled, result);
				return result;
			}
		}

	}

}