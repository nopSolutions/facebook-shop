using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.FacebookShop.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.FacebookShop.Components
{
    [ViewComponent(Name = "RenderCategoryLine")]
    public class RenderCategoryLineViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var data = ViewComponentContext.Arguments;
            var model = new RenderCategoryLineModel
            {
                Category = data["category"] as CategoryModel,
                Level = Convert.ToInt32(data["level"]),
                LastItem = Convert.ToBoolean(data["lastItem"])
            };
            return View("~/Plugins/Misc.FacebookShop/Views/_RenderCategoryLine.cshtml", model);
        }
    }
}
