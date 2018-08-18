using System;
using System.Collections.Generic;
using System.Text;

namespace VRCModLoader
{
    /// <summary>
    /// Interface for generic mods. Every class that implements this will be loaded if the DLL is placed at
    /// data/Managed/Mods.
    /// </summary>
    public abstract class VRCMod
    {

        /// <summary>
        /// Gets the name of the mod.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the version of the plugin.
        /// </summary>
        public string Version { get; internal set; }

        /// <summary>
        /// Gets the author of the mod.
        /// </summary>
        public string Author { get; internal set; }

        /// <summary>
        /// Gets the download link of the mod.
        /// </summary>
        public string DownloadLink { get; internal set; }
    }
}
