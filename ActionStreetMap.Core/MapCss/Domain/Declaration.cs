using System;
using System.Collections.Generic;
using ActionStreetMap.Core.MapCss.Visitors.Eval;

namespace ActionStreetMap.Core.MapCss.Domain
{
    /// <summary> MapCSS declaration (e.g. 'fill-color:red'). </summary>
    public class Declaration
    {
        /// <summary> Gets or sets declaration key. </summary>
        public string Qualifier { get; set; }

        /// <summary> Gets or sets declaration value. </summary>
        public string Value { get; set; }

        /// <summary> Gets or sets whether declaration should be evaluated. </summary>
        public bool IsEval { get; set; }

        /// <summary> Gets or sets evaluator. </summary>
        public ITreeWalker Evaluator { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("[{0}:{1}]:{2}", Qualifier, Value, IsEval);
        }
    }

    /// <summary>  List of declarations. </summary>
    public class ListDeclaration : Declaration
    {
        private readonly List<Declaration> _declarations = new List<Declaration>(4);

        /// <summary> Inner declarations. </summary>
        public List<Declaration> Items
        {
            get
            {
                return _declarations;
            }
        }
    }
}
