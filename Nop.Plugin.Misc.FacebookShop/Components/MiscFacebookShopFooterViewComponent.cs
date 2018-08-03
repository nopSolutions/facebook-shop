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
        private readonly ILocalizationService _localizationService;

        public MiscFacebookShopFooterViewComponent(IStoreContext storeContext,
            ILocalizationService localizationService)
        {
            this._storeContext = storeContext;
            this._localizationService = localizationService;
        }

        public IViewComponentResult Invoke()
        {
            var model = new FooterModel
            {
                StoreName = _localizationService.GetLocalized(_storeContext.CurrentStore, x => x.Name)
            };

            return View("~/Plugins/Misc.FacebookShop/Views/Footer.cshtml", model);
        }
    }
}
