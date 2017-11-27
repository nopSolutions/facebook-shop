using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Misc.FacebookShop.Infrastructure.Cache;
using Nop.Plugin.Misc.FacebookShop.Models;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.FacebookShop.Components
{
    [ViewComponent(Name = "MiscFacebookShopCategoryNavigation")]
    public class MiscFacebookShopCategoryNavigationViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly ICategoryService _categoryService;

        public MiscFacebookShopCategoryNavigationViewComponent(IStoreContext storeContext,
            IWorkContext workContext,
            ICacheManager cacheManager,
            ICategoryService categoryService)
        {
            this._storeContext = storeContext;
            this._workContext = workContext;
            this._cacheManager = cacheManager;
            this._categoryService = categoryService;
        }

        public IViewComponentResult Invoke()
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NAVIGATION_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var model = _cacheManager.Get(cacheKey, () => PrepareCategorySimpleModels(0, null, true).ToList());

            return View("~/Plugins/Misc.FacebookShop/Views/CategoryNavigation.cshtml", model);
        }

        protected IList<CategoryModel> PrepareCategorySimpleModels(int rootCategoryId,
            IList<int> loadSubCategoriesForIds,
            bool validateIncludeInTopMenu)
        {
            var result = new List<CategoryModel>();
            foreach (var category in _categoryService.GetAllCategoriesByParentCategoryId(rootCategoryId))
            {
                if (validateIncludeInTopMenu && !category.IncludeInTopMenu)
                {
                    continue;
                }

                var categoryModel = new CategoryModel
                {
                    Id = category.Id,
                    Name = category.GetLocalized(x => x.Name),
                    SeName = category.GetSeName()
                };

                //load subcategories?
                var loadSubCategories = false;
                if (loadSubCategoriesForIds == null)
                {
                    //load all subcategories
                    loadSubCategories = true;
                }
                else
                {
                    //we load subcategories only for certain categories
                    for (var i = 0; i <= loadSubCategoriesForIds.Count - 1; i++)
                    {
                        if (loadSubCategoriesForIds[i] == category.Id)
                        {
                            loadSubCategories = true;
                            break;
                        }
                    }
                }
                if (loadSubCategories)
                {
                    var subCategories =
                        PrepareCategorySimpleModels(category.Id, loadSubCategoriesForIds, validateIncludeInTopMenu);
                    categoryModel.SubCategories.AddRange(subCategories);
                }
                result.Add(categoryModel);
            }

            return result;
        }
    }
}
