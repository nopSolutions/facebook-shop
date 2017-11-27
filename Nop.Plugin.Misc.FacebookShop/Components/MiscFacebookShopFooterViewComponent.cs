using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.FacebookShop.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.FacebookShop.Components
{
    [ViewComponent(Name = "MiscFacebookShopFooter")]
    public class MiscFacebookShopFooterViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;

        public MiscFacebookShopFooterViewComponent(IStoreContext storeContext)
        {
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke()
        {
            var model = new FooterModel
            {
                StoreName = _storeContext.CurrentStore.GetLocalized(x => x.Name)
            };

            return View("~/Plugins/Misc.FacebookShop/Views/Footer.cshtml", model);
        }
    }
}
