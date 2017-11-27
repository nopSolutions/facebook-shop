using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.FacebookShop
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //home page
            routeBuilder.MapRoute("Plugin.Misc.FacebookShop.Index",
                 "facebook/shop/",
                 new { controller = "MiscFacebookShop", action = "Index" });

            //category page
            routeBuilder.MapRoute("Plugin.Misc.FacebookShop.Category",
                 "facebook/shop/category/{categoryId}/",
                 new { controller = "MiscFacebookShop", action = "Category" },
                 new { categoryId = @"\d+" });

            //search page
            routeBuilder.MapRoute("Plugin.Misc.FacebookShop.ProductSearch",
                 "facebook/shop/search/",
                 new { controller = "MiscFacebookShop", action = "Search" });
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
