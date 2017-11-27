using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.FacebookShop
{
    public class FacebookShopPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public FacebookShopPlugin(IWebHelper webHelper)
        {
            this._webHelper = webHelper;
        }

        #endregion
        
        #region Methods
        
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/MiscFacebookShop/Configure";
        }

        #endregion
    }
}
