using System.IO;
using ActionStreetMap.Core.MapCss.Domain;
using ActionStreetMap.Core.MapCss.Visitors;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.IO;
using Antlr.Runtime;

namespace ActionStreetMap.Core.MapCss
{
    /// <summary>
    ///     Defines provider which returns stylesheet
    /// </summary>
    public interface IStylesheetProvider
    {
        /// <summary>
        ///  Gets stylesheet.
        /// </summary>
        /// <returns>Stylesheet.</returns>
        Stylesheet Get();
    }

    /// <summary>
    ///     Default implementation of IStylesheetProvider
    /// </summary>
    public class StylesheetProvider : IStylesheetProvider, IConfigurable
    {
        private readonly IFileSystemService _fileSystemService;
        private const string PathKey = "mapcss";
        private const string ExprTreeKey = "exprTreeAllowed";

        private string _path;
        private bool _isExprTreeAllowed;
        
        private Stylesheet _stylesheet;

        #region Constructors

        /// <summary>
        ///     Creates stylesheet provider using file system service.
        /// </summary>
        /// <param name="fileSystemService">File system service.</param>
        [Dependency]
        public StylesheetProvider(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

        internal StylesheetProvider(string path, IFileSystemService fileSystemService)
            : this(fileSystemService)
        {
            _path = path;
        }

        internal StylesheetProvider(Stream stream, bool canUseExprTree)
        {
            _stylesheet = Create(stream, canUseExprTree);
        }

        #endregion

        /// <inheritdoc />
        public Stylesheet Get()
        {
            return _stylesheet ?? (_stylesheet = Create());
        }

        private Stylesheet Create()
        {
            using (Stream inputStream = _fileSystemService.ReadStream(_path))
                return Create(inputStream, _isExprTreeAllowed);
        }

        private static Stylesheet Create(Stream stream, bool isExprTreeAllowed)
        {
            var input = new ANTLRInputStream(stream);
            var lexer = new MapCssLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new MapCssParser(tokens);

            var styleSheet = parser.stylesheet();
            var tree = styleSheet.Tree as Antlr.Runtime.Tree.CommonTree;
            // NOTE we cannot use expression trees on some platforms (e.g. web player)
            var visitor = new MapCssVisitor(isExprTreeAllowed);
            return visitor.Visit(tree);
        }

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            _path = configSection.GetString(PathKey, null);
            _isExprTreeAllowed = configSection.GetBool(ExprTreeKey, true);
            _stylesheet = null;
        }
    }
}