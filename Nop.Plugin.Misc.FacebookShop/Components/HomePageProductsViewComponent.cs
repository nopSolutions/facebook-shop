using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.FacebookShop.Models;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.FacebookShop.Components
{
    [ViewComponent(Name = "HomePageProducts")]
    public class HomePageProductsViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;

        public HomePageProductsViewComponent(IAclService aclService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext)
        {
            this._aclService = aclService;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._pictureService = pictureService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._productService = productService;
            this._storeContext = storeContext;
            this._storeMappingService = storeMappingService;
            this._taxService = taxService;
            this._urlRecordService = urlRecordService;
            this._workContext = workContext;
        }

            public IViewComponentResult Invoke()
        {
            var products = _productService.GetAllProductsDisplayedOnHomePage();
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

            var model = PrepareProductOverviewModels(products).ToList();
            return View("~/Plugins/Misc.FacebookShop/Views/HomePageProducts.cshtml", model);
        }

        protected IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException("products");

            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    Name = _localizationService.GetLocalized(product, x => x.Name),
                    ShortDescription = _localizationService.GetLocalized(product, x => x.ShortDescription),
                    FullDescription = _localizationService.GetLocalized(product, x => x.FullDescription),
                    SeName = _urlRecordService.GetSeName(product),
                };
                //price
                if (preparePriceModel)
                {
                    #region Prepare product price

                    var priceModel = new ProductOverviewModel.ProductPriceModel();

                    switch (product.ProductType)
                    {
                        case ProductType.GroupedProduct:
                        {
                            #region Grouped product

                            var associatedProducts = _productService.GetAssociatedProducts(product.Id, _storeContext.CurrentStore.Id);

                            switch (associatedProducts.Count)
                            {
                                case 0:
                                {
                                    //no associated products
                                    priceModel.OldPrice = null;
                                    priceModel.Price = null;
                                    priceModel.DisableBuyButton = true;
                                    priceModel.DisableWishlistButton = true;
                                    priceModel.AvailableForPreOrder = false;
                                }
                                    break;
                                default:
                                {
                                    //we have at least one associated product
                                    priceModel.DisableBuyButton = true;
                                    priceModel.DisableWishlistButton = true;
                                    priceModel.AvailableForPreOrder = false;

                                    if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                                    {
                                        //find a minimum possible price
                                        decimal? minPossiblePrice = null;
                                        Product minPriceProduct = null;
                                        foreach (var associatedProduct in associatedProducts)
                                        {
                                            //calculate for the maximum quantity (in case if we have tier prices)
                                            var tmpPrice = _priceCalculationService.GetFinalPrice(associatedProduct,
                                                _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);
                                            if (!minPossiblePrice.HasValue || tmpPrice < minPossiblePrice.Value)
                                            {
                                                minPriceProduct = associatedProduct;
                                                minPossiblePrice = tmpPrice;
                                            }
                                        }
                                        if (minPriceProduct != null && !minPriceProduct.CustomerEntersPrice)
                                        {
                                            if (minPriceProduct.CallForPrice)
                                            {
                                                priceModel.OldPrice = null;
                                                priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
                                            }
                                            else if (minPossiblePrice.HasValue)
                                            {
                                                //calculate prices
                                                decimal taxRate;
                                                decimal finalPriceBase = _taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, out taxRate);
                                                decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                                priceModel.OldPrice = null;
                                                priceModel.Price = String.Format(_localizationService.GetResource("Products.PriceRangeFrom"), _priceFormatter.FormatPrice(finalPrice));

                                            }
                                            else
                                            {
                                                //Actually it's not possible (we presume that minimalPrice always has a value)
                                                //We never should get here
                                                Debug.WriteLine("Cannot calculate minPrice for product #{0}", product.Id);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //hide prices
                                        priceModel.OldPrice = null;
                                        priceModel.Price = null;
                                    }
                                }
                                    break;
                            }

                            #endregion
                        }
                            break;
                        default:
                        {
                            #region Simple product

                            //add to cart button
                            priceModel.DisableBuyButton = product.DisableBuyButton ||
                                                          !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) ||
                                                          !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                            //add to wishlist button
                            priceModel.DisableWishlistButton = product.DisableWishlistButton ||
                                                               !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) ||
                                                               !_permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

                            //rental
                            priceModel.IsRental = product.IsRental;

                            //pre-order
                            if (product.AvailableForPreOrder)
                            {
                                priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                                                  product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                                priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
                            }

                            //prices
                            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                            {
                                if (!product.CustomerEntersPrice)
                                {
                                    if (product.CallForPrice)
                                    {
                                        //call for price
                                        priceModel.OldPrice = null;
                                        priceModel.Price = _localizationService.GetResource("Products.CallForPrice");
                                    }
                                    else
                                    {
                                        //prices

                                        //calculate for the maximum quantity (in case if we have tier prices)
                                        decimal minPossiblePrice = _priceCalculationService.GetFinalPrice(product,
                                            _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);

                                        decimal taxRate;
                                        decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);
                                        decimal finalPriceBase = _taxService.GetProductPrice(product, minPossiblePrice, out taxRate);

                                        decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                                        decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                        //do we have tier prices configured?
                                        var tierPrices = new List<TierPrice>();
                                        if (product.HasTierPrices)
                                        {
                                            tierPrices.AddRange(product.TierPrices
                                                .OrderBy(tp => tp.Quantity)
                                                .ToList()
                                                .FilterByStore(_storeContext.CurrentStore.Id)
                                                .FilterForCustomer(_workContext.CurrentCustomer)
                                                .RemoveDuplicatedQuantities());
                                        }
                                        //When there is just one tier (with  qty 1), 
                                        //there are no actual savings in the list.
                                        bool displayFromMessage = tierPrices.Count > 0 &&
                                                                  !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                                        if (displayFromMessage)
                                        {
                                            priceModel.OldPrice = null;
                                            priceModel.Price = String.Format(_localizationService.GetResource("Products.PriceRangeFrom"), _priceFormatter.FormatPrice(finalPrice));
                                        }
                                        else
                                        {
                                            if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
                                            {
                                                priceModel.OldPrice = _priceFormatter.FormatPrice(oldPrice);
                                                priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
                                            }
                                            else
                                            {
                                                priceModel.OldPrice = null;
                                                priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
                                            }
                                        }
                                        if (product.IsRental)
                                        {
                                            //rental product
                                            priceModel.OldPrice = _priceFormatter.FormatRentalProductPeriod(product, priceModel.OldPrice);
                                            priceModel.Price = _priceFormatter.FormatRentalProductPeriod(product, priceModel.Price);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //hide prices
                                priceModel.OldPrice = null;
                                priceModel.Price = null;
                            }

                            #endregion
                        }
                            break;
                    }

                    model.ProductPrice = priceModel;

                    #endregion
                }

                //picture
                if (preparePictureModel)
                {
                    #region Prepare product picture

                    //If a size has been set in the view, we use it in priority
                    int pictureSize = productThumbPictureSize.HasValue ? productThumbPictureSize.Value : 125;
                    //prepare picture model
                    var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                    model.DefaultPictureModel = new PictureModel
                    {
                        ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture)
                    };
                    //"title" attribute
                    model.DefaultPictureModel.Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)) ?
                        picture.TitleAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), model.Name);
                    //"alt" attribute
                    model.DefaultPictureModel.AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute)) ?
                        picture.AltAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), model.Name);

                    #endregion
                }

                models.Add(model);
            }
            return models;
        }
    }
}
