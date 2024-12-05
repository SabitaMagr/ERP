﻿namespace NeoErp.Core.Plugins
{
    public abstract class BasePlugin : IPlugin
    {
        /// <summary>
        /// Gets or sets the plugin descriptor
        /// </summary>
        public virtual PluginDescriptor PluginDescriptor { get; set; }

        /// <summary>
        /// Install plugin
        /// </summary>
        public virtual void Install() 
        {
            PluginAreaBootstrapper.MarkPluginAsInstalled(this.PluginDescriptor);
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public virtual void Uninstall() 
        {
            PluginAreaBootstrapper.MarkPluginAsUninstalled(this.PluginDescriptor);
        }

    }
}
