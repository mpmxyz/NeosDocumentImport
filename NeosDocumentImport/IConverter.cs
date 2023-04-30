using BaseX;
using System;
using System.Collections.Generic;

namespace NeosDocumentImport
{
    public enum ConfigType
    {
        Value,
        Reference
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigAttribute : Attribute
    {
        public readonly string name;
        public readonly ConfigType type;

        public ConfigAttribute(string name, ConfigType type)
        {
            this.name = name;
            this.type = type;
        }
    }
    public interface IConverter
    {
        bool ValidateConfig();
        List<string> Apply(string file, string outputDir, string pagePrefix, IProgressIndicator progress);
    }
}
