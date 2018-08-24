using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRCModLoader
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VRCModInfoAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the mod.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the version of the plugin.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets the author of the mod.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Gets the download link of the mod.
        /// </summary>
        public string DownloadLink { get; }

        public VRCModInfoAttribute(string name, string version, string author, string downloadLink = null)
        {
            Name = name;
            Version = version;
            Author = author;
            DownloadLink = downloadLink;
        }
    }
}
