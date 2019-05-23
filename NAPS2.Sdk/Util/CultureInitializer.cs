using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NAPS2.Config.Experimental;
using NAPS2.Lang.Resources;
using NAPS2.Logging;

namespace NAPS2.Util
{
    /// <summary>
    /// A helper to set the thread culture based on user and app configuration.
    /// </summary>
    public class CultureInitializer
    {
        private readonly ConfigProvider<CommonConfig> configProvider;

        public CultureInitializer(ConfigProvider<CommonConfig> configProvider)
        {
            this.configProvider = configProvider;
        }

        public void InitCulture()
        {
            var cultureId = configProvider.Get(c => c.Culture);
            if (!string.IsNullOrWhiteSpace(cultureId))
            {
                try
                {
                    var culture = new CultureInfo(cultureId);
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;
                    MiscResources.Culture = culture;
                    SettingsResources.Culture = culture;
                }
                catch (CultureNotFoundException e)
                {
                    Log.ErrorException("Invalid culture.", e);
                }
            }
        }
    }
}