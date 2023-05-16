using BaseX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeosDocumentImport
{
    /// <summary>
    /// Describes the type of an import option
    /// </summary>
    public enum ConfigType
    {
        /// <summary>
        /// A primitive value
        /// </summary>
        Value,
        /// <summary>
        /// A reference
        /// </summary>
        Reference
    }

    /// <summary>
    /// Attributes fields representing a configuration option
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigAttribute : Attribute
    {
        public readonly string name;
        public readonly ConfigType type;

        /// <summary>
        /// Creates a configuration line in the import configurator
        /// </summary>
        /// <param name="name">Display name of the option</param>
        /// <param name="type">Type of the option</param>
        public ConfigAttribute(string name, ConfigType type)
        {
            this.name = name;
            this.type = type;
        }
    }

    /// <summary>
    /// Converts files into other files,
    /// can be configured using <see cref="ConfigAttribute"/>s
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        /// Checks if Apply can be called
        /// </summary>
        /// <param name="msg">Output for error message if <see langword="false"/> is returned</param>
        /// <returns><see langword="true"/>, if internal configuration is valid</returns>
        bool ValidateConfig(out string msg);

        /// <summary>
        /// Converts a single file into multiple other files
        /// </summary>
        /// <param name="file">To read from</param>
        /// <param name="outputDir">Directory to put output into</param>
        /// <param name="pagePrefix">Name Prefix for generated files</param>
        /// <param name="progress">To keep the user up to date with the conversion progress</param>
        /// <returns>The list of created files (will be imported after function returns)</returns>
        Task<List<string>> Apply(string file, string outputDir, string pagePrefix, IProgressIndicator progress);
    }
}
