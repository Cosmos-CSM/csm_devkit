namespace CSM_DevKit.Common;

/// <summary>
///     Provides constants for [CSM DevKit]
/// </summary>
static public class Constants {


    /// <summary>
    ///     Stores FileTemplates names.
    /// </summary>
    public struct FileTemplates {

        /// <summary>
        ///     Stores constants related to C# Migration Designer file template.
        /// </summary>
        public struct CSharpMigrationDesigner {

            /// <summary>
            ///     Resource file name to get C# Migration Desginer file template.
            /// </summary>
            public const string FileName = "CSharp_Migration_Designer.txt";

            /// <summary>
            ///     File template replacement token for [Namespace].
            /// </summary>
            public const string NamespaceToken = "NAMESPACE_TOKEN";

            /// <summary>
            ///     File template remplacement token for generation [DATE]
            /// </summary>
            public const string DateToken = "DATE_TOKEN";

            /// <summary>
            ///     File template replacement token for [DatabaseContext] type.
            /// </summary>
            public const string DatabaseContextToken = "DB_CTX_TOKEN";

            /// <summary>
            ///     File template replacement token for [DatabaseProvider] value.
            /// </summary>
            public const string DatabaseProviderToken = "DB_PROVIDER_TOKEN";
        }
    }
}
