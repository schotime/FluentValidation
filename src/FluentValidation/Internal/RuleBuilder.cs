#region License
// Copyright (c) Jeremy Skinner (http://www.jeremyskinner.co.uk)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://www.codeplex.com/FluentValidation
#endregion

namespace FluentValidation.Internal {
	using System;
	using Validators;
    using System.Linq.Expressions;

	/// <summary>
	/// Builds a validation rule and constructs a validator.
	/// </summary>
	/// <typeparam name="T">Type of object being validated</typeparam>
	/// <typeparam name="TProperty">Type of property being validated</typeparam>
	public class RuleBuilder<T, TProperty> : IRuleBuilderOptions<T, TProperty>, IRuleBuilderInitial<T, TProperty> {
		readonly PropertyRule<T> rule;
	    readonly Func<Type, object> constructor;

		public PropertyRule<T> Rule {
			get { return rule; }
		}

		/// <summary>
		/// Creates a new instance of the <see cref="RuleBuilder{T,TProperty}">RuleBuilder</see> class.
		/// </summary>
        public RuleBuilder(PropertyRule<T> rule, Func<Type, object> constructor)
        {
			this.rule = rule;
		    this.constructor = constructor;
		}

		/// <summary>
		/// Sets the validator associated with the rule.
		/// </summary>
		/// <param name="validator">The validator to set</param>
		/// <returns></returns>
		public IRuleBuilderOptions<T, TProperty> SetValidator(IPropertyValidator validator) {
			validator.Guard("Cannot pass a null validator to SetValidator.");
			rule.AddValidator(validator);
			return this;
		}

		/// <summary>
		/// Sets the validator associated with the rule. Use with complex properties where an IValidator instance is already declared for the property type.
		/// </summary>
		/// <param name="validator">The validator to set</param>
		public IRuleBuilderOptions<T, TProperty> SetValidator(IValidator validator) {
			validator.Guard("Cannot pass a null validator to SetValidator");
			SetValidator(Extensions.InferPropertyValidatorForChildValidator(Rule, validator));
			return this;
		}

        /// <summary>
        /// Sets the validator associated with the rule. Use with complex properties where an IValidator instance is already declared for the property type.
        /// </summary>
        /// <typeparam name="TValidator">The validator to set</typeparam>
        public IRuleBuilderOptions<T, TProperty> SetValidator<TValidator>() where TValidator : IValidator
        {
            var validator = (IValidator)constructor(typeof(TValidator));
            validator.Guard("Cannot pass a null validator to SetValidator");
            SetValidator(Extensions.InferPropertyValidatorForChildValidator(Rule, validator));
            return this;
        }

        /// <summary>
        /// Uses the validator specified to extract the rule
        /// </summary>
        /// <typeparam name="TModel">The model to extract property name</typeparam>
        /// <typeparam name="TValidator">The validator to extract property from</typeparam>
        public IRuleBuilderOptions<T, TProperty> UsingRuleFrom<TModel, TValidator>(Expression<Func<TModel, TProperty>> expression) where TValidator : IValidator<TModel> {
            AddValidators<TValidator>(expression.GetMember().Name);
            return this;
        }

        /// <summary>
        /// Uses the validator specified to extract the rule
        /// </summary>
        /// <typeparam name="TValidator">The validator to extract property from using the same name</typeparam>
        public IRuleBuilderOptions<T, TProperty> UsingRuleFrom<TValidator>() where TValidator : IValidator {
            AddValidators<TValidator>(rule.PropertyName);
            return this;
        }

        /// <summary>
        /// Sets the validator associated with the rule. Use with complex properties where a Property Validator is already declared for the property type.
        /// </summary>
        /// <typeparam name="TValidator">The validator to set</typeparam>
	    public IRuleBuilderOptions<T, TProperty> Using<TValidator>() where TValidator : IPropertyValidator {
            var validator = (IPropertyValidator)constructor(typeof(TValidator));
	        SetValidator(validator);
            return this;
	    }

	    private void AddValidators<TValidator>(string name) where TValidator : IValidator {
            var validator = (IValidator)constructor(typeof(TValidator));
            var descriptor = validator.CreateDescriptor();
            var validators = descriptor.GetValidatorsForMember(name);

            foreach (var val in validators)
            {
                SetValidator(val);
            }
        }

		IRuleBuilderOptions<T, TProperty> IConfigurable<PropertyRule<T>, IRuleBuilderOptions<T, TProperty>>.Configure(Action<PropertyRule<T>> configurator) {
			configurator(rule);
			return this;
		}

		IRuleBuilderInitial<T, TProperty> IConfigurable<PropertyRule<T>, IRuleBuilderInitial<T, TProperty>>.Configure(Action<PropertyRule<T>> configurator) {
			configurator(rule);
			return this;
		}
	}
}