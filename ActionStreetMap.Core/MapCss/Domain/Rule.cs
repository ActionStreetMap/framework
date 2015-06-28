using System;
using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Core.MapCss.Domain
{
    /// <summary> Represents MapCSS rule for certain model. </summary>
    public class Rule
    {
        private readonly Model _model;

        /// <summary> List of declarations. </summary>
        public Dictionary<string, Declaration> Declarations { get; set; }

        /// <summary> Creates MapCss rule for model. </summary>
        /// <param name="model">Model.</param>
        public Rule(Model model)
        {
            _model = model;
            Declarations = new Dictionary<string, Declaration>(8);
        }

        /// <summary> Checks whether declaration with given name is registered. </summary>
        public bool HasDeclaration(string name)
        {
            return Declarations.ContainsKey(name);
        }

        /// <summary> Checks whether this rule can be used. </summary>
        public bool IsApplicable { get { return Declarations.Count > 0; } }

        /// <summary> Evaluates value for gived qualifier. If not possible, returns default. </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="qualifier">Qualifier: osm tag key</param>
        /// <param name="default">Default value</param>
        /// <returns>Evaluated value</returns>
        public T EvaluateDefault<T>(string qualifier, T @default)
        {
            Assert();

            if (!Declarations.ContainsKey(qualifier))
                return @default;

            var declaration = Declarations[qualifier];

            if (declaration.IsEval)
                return declaration.Evaluator.Walk<T>(_model);

            return (T) Convert.ChangeType(declaration.Value, typeof (T));
        }

        /// <summary> Evaluates value for gived qualifier. </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="qualifier">Qualifier: osm tag key</param>
        /// <returns>Evaluated value</returns>
        public T Evaluate<T>(string qualifier)
        {
            return Evaluate(qualifier, v => (T) Convert.ChangeType(v, typeof (T)));
        }

        /// <summary> Evaluates list of values for gived qualifier. </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="qualifier">Qualifier: osm tag key</param>
        public IEnumerable<T> EvaluateList<T>(string qualifier)
        {
            return EvaluateList(qualifier, v => (T) Convert.ChangeType(v, typeof (T)));
        }

        /// <summary> Evaluates list of values for gived qualifier. </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="qualifier">Qualifier: osm tag key</param>
        /// <param name="converter">Convertrs string to given type</param>
        public IEnumerable<T> EvaluateList<T>(string qualifier, Func<string, T> converter)
        {
            if (!Declarations.ContainsKey(qualifier))
                return Enumerable.Empty<T>();

            Declaration declaration;
            var listDeclaration = Declarations[qualifier] as ListDeclaration;
            if (listDeclaration != null)
            {
                declaration = listDeclaration.Items.First();
                return declaration.Evaluator.Walk<IEnumerable<T>>(_model);
            }
            declaration = Declarations[qualifier];
            return Evaluate(declaration, converter);
        }

        /// <summary> Helper method. </summary>
        private IEnumerable<T> Evaluate<T>(Declaration declaration, Func<string, T> converter)
        {
            yield return declaration.IsEval
                  ? declaration.Evaluator.Walk<T>(_model)
                  : converter(declaration.Value);
        } 

        /// <summary> Evaluates  value for gived qualifier. </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="qualifier">Qualifier: osm tag key</param>
        /// <param name="converter">Convertrs string to given type</param>
        /// <returns>Evaluated value</returns>
        public T Evaluate<T>(string qualifier, Func<string, T> converter)
        {
            Assert();

            if (!Declarations.ContainsKey(qualifier))
                throw new ArgumentException(String.Format(Strings.StyleDeclarationNotFound,
                    qualifier, _model), qualifier);

            var declaration = Declarations[qualifier];

            return declaration.IsEval
                ? declaration.Evaluator.Walk<T>(_model)
                : converter(declaration.Value);
        }

        private void Assert()
        {
            if (!IsApplicable)
                throw new InvalidOperationException(Strings.RuleNotApplicable);
        }
    }
}