using System;

namespace MagicEastern.ADOExt
{
    internal class ConfigAddedEventArgs : EventArgs
    {
        public string ConfigName;
        public IServiceProvider ServiceProvider;
    }

    internal delegate void ConfigAddedHandler(object sender, ConfigAddedEventArgs args);

    public class DBServiceManagerBuilder
    {
        internal event ConfigAddedHandler ConfigAdded;

        public void AddConfig(string name, IServiceProvider serviceProvider)
        {
            var args = new ConfigAddedEventArgs
            {
                ConfigName = name,
                ServiceProvider = serviceProvider
            };
            ConfigAdded(this, args);
        }
    }
}
