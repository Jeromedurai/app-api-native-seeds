using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Tenant.API.Base.Model.Validation;
using Tenant.API.Base.Repository;
using Tenant.API.Base.Service;
using Tenant.Query.Model.Product;
using Tenant.Query.Model.ProductCart;
using Tenant.Query.Model.Response;
using Tenant.Query.Model.Response.ProductMaster;
using Tenant.Query.Model.WishList;
using UnitsNet;

namespace Tenant.Query.Service.Product
{
    public class ProductService : TnBaseService
    {
        #region Private property

        private Repository.Product.ProductRepository productRepository;
        private readonly ILoggerFactory _loggerFactory;

        #endregion

        public ProductService(Repository.Product.ProductRepository productRepository,
                            IConfiguration configuration,
                            ILoggerFactory loggerFactory,
                            TnAudit xcAudit,
                            TnValidation xcValidation) : base(xcAudit, xcValidation)
        {
            this.productRepository = productRepository;
            this._loggerFactory = loggerFactory;
            this.productRepository.Logger = loggerFactory.CreateLogger<Repository.Product.ProductRepository>();
        }


        #region Get End Point
        public List<Model.Response.Category> GetMenuMaster(string tenantId)
        {
            try
            {
                string spName = Model.Constant.Constant.StoredProcedures.HN_GET_MENU_MASTER;

                var productCategories = this.productRepository.GetMenuMaster(tenantId, spName) ?? new List<Model.Product.ProductCategory>();

                return productCategories
                    .Where(x => x?.SubCategory == 0)
                    .OrderBy(x => x.OrderBy)
                    .Select(category => new Model.Response.Category
                    {
                        CategoryId = category.CategoryId,
                        Name = category.Category,
                        Order = category.OrderBy,
                        subMenu = category.SubMenu,
                        link = category.Link,
                        subCategories = productCategories
                            .Where(sub => sub?.SubCategory == category.CategoryId)
                            .Select(sub => new Model.Response.SubCategory
                            {
                                Id = sub.CategoryId,
                                Name = sub.Category,
                                Order = sub.OrderBy
                            })
                            .ToList()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Category> GetCategories(string tenantId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public async Task<byte[]> CreateThumbnailAsync(byte[] imageData, int width, int height)
        {
            using var imageStream = new MemoryStream(imageData);
            using var image = await Image.LoadAsync(imageStream);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max
            }));

            using var resultStream = new MemoryStream();
            await image.SaveAsync(resultStream, new JpegEncoder
            {
                Quality = 80
            });
            return resultStream.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public bool IsImageValid(byte[] imageData)
        {
            try
            {
                using var imageStream = new MemoryStream(imageData);
                var imageInfo = Image.Identify(imageStream);
                return imageInfo != null;
            }
            catch
            {
                return false;
            }
        }

        public string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Model.Product.ProductImages> GetImageAsync(long Id)
        {
            var productImage = new Model.Product.ProductImages();
            DataTable productImageDt = await Task.Run(() => productRepository.GetImageAsync(Id));
            if (productImageDt?.Rows.Count > 0)
            {
                MapProductImage(productImage, productImageDt.Rows[0]);
            }

            return productImage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productImage"></param>
        /// <param name="row"></param>
        private void MapProductImage(Model.Product.ProductImages productImage, DataRow row)
        {
            productImage.ImageData = GetColumnValue<byte[]>(row, "ImageData", null);
            productImage.ContentType = GetColumnValue<string>(row, "ContentType", string.Empty);
            productImage.ImageName = GetColumnValue<string>(row, "ImageName", string.Empty);
            productImage.ThumbnailData = GetColumnValue<byte[]>(row, "ThumbnailData", null);
            productImage.ProductId = GetColumnValue<long>(row, "ProductId", 0);
            productImage.Active = GetColumnValue<bool>(row, "", false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        public long AddImages(ProductNewImage images)
        {
            try
            {
                return productRepository.AddImages(images.ProductId, images.ImageName, images.ContentType, images.ImageData, images.ThumbnailData, images.FileSize);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// DeleteImages
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public long DeleteImage(long id)
        {
            try
            {
                return productRepository.DeleteImages(id);
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                throw new Exception("An error occurred while deleting the image.", ex);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        public async Task<long> AddProduct(long tenantId, Model.Product.Product product)
        {
            try
            {
                long productId = await this.productRepository.AddProduct(tenantId, product);
                return productId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="productCategory"></param>
        /// <returns></returns>
        public long AddProductCategory(long tenantId, Model.Product.ProductCategoryPayload productCategory)
        {
            try
            {
                long productCategoryId = this.productRepository.AddProductCategory(tenantId, productCategory);
                return productCategoryId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="productCategory"></param>
        /// <returns></returns>
        public long AddCategory(long tenantId, Model.Response.CatrtegoryPayload catrtegoryPayload)
        {
            try
            {
                long categoryId = this.productRepository.AddCategory(tenantId, catrtegoryPayload);
                return categoryId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<ProductCartResponse> UpsertCart(long tenantId, CartPayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload), "Payload cannot be null.");

            if (tenantId <= 0)
                throw new ArgumentException("Invalid tenant ID.", nameof(tenantId));

            try
            {
                var productCartResponse = new ProductCartResponse();
                var dtProductCartResponse = await Task.Run(() => productRepository.UpsertCart(payload));

                if (dtProductCartResponse == null || dtProductCartResponse.Rows.Count == 0)
                    return new ProductCartResponse();

                var productPayload = CreateProductPayload(payload.ProductId);
                var spName = Model.Constant.Constant.XC_GET_PRODUCT_MASTER_LIST_TESTING;

                var (orderBy, order) = ParseOrderBy(productPayload.OrderBy);
                var response = GetProductList(spName, tenantId.ToString(), productPayload, orderBy, order);

                if (response == null || !response.Any())
                    return new ProductCartResponse();

                return MapProductCartResponse(payload, dtProductCartResponse, response.First());
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException("Invalid argument provided.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while upserting the cart.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<ProductWishListResponse> UpsertWishList(long tenantId, WishListPayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload), "Payload cannot be null.");

            if (tenantId <= 0)
                throw new ArgumentException("Invalid tenant ID.", nameof(tenantId));

            try
            {
                var productWishListResponse = new ProductCartResponse();
                var dtProductWishList = await Task.Run(() => productRepository.UpsertWishList(payload));

                if (dtProductWishList == null || dtProductWishList.Rows.Count == 0)
                    return new ProductWishListResponse();

                var productPayload = CreateProductPayload(payload.ProductId);
                var spName = Model.Constant.Constant.XC_GET_PRODUCT_MASTER_LIST_TESTING;

                var (orderBy, order) = ParseOrderBy(productPayload.OrderBy);
                var response = GetProductList(spName, tenantId.ToString(), productPayload, orderBy, order);

                if (response == null || !response.Any())
                    return new ProductWishListResponse();

                return MapProductWishList(payload, dtProductWishList, response.First());
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException("Invalid argument provided.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while upserting the cart.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public List<ProductCartResponse> GetUserCart(long userId)
        {
            if (userId == 0)
                throw new ArgumentNullException(nameof(userId), "Payload cannot be null.");

            try
            {
                var productWishListResponse = new List<ProductCartResponse>();
                List<Model.ProductCart.SpProductCart> spProductCarts = productRepository.GetUserCart("SP_CUSTOMER_CART", userId);

                if (spProductCarts == null || spProductCarts.Count == 0)
                    return new List<ProductCartResponse>();

                var productPayload = CreateProductListPayload(spProductCarts.Select(x => x.ProductId).ToList());
                var spName = Model.Constant.Constant.XC_GET_PRODUCT_MASTER_LIST_TESTING;

                var (orderBy, order) = ParseOrderBy(productPayload.OrderBy);
                var response = GetProductList(spName, "10", productPayload, orderBy, order);

                if (response == null || !response.Any())
                    return new List<ProductCartResponse>();

                // Map the response to a list of ProductWishListResponse
                foreach (var product in response)   
                {
                    var wishListResponse = new ProductCartResponse
                    {

                        UserId = spProductCarts.FirstOrDefault()?.UserId ?? 0,
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        Price = product.Price,
                        Quantity = spProductCarts.FirstOrDefault(x => x.ProductId == product.ProductId)?.Quantity ?? 0,
                        images = product.images ?? new List<Model.Product.ImageResponseDto>()
                    };
                    productWishListResponse.Add(wishListResponse);
                }

                return productWishListResponse;
            }   
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException("Invalid argument provided.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the user cart.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        private Model.Product.ProductPayload CreateProductPayload(long productId)
        {
            return new Model.Product.ProductPayload
            {
                Page = 1,
                PageSize = 10,
                Category = 1,
                OrderBy = "ProductName,asc",
                ProductId = new List<long> { productId },
                RoleId = "1",
                Search = ""
            };
        }

        private Model.Product.ProductPayload CreateProductListPayload(List<long> productIds)
        {
            return new Model.Product.ProductPayload
            {
                Page = 1,
                PageSize = 10,
                Category = 0,
                OrderBy = "ProductName,asc",
                ProductId = productIds ,
                RoleId = "1",
                Search = ""
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderByClause"></param>
        /// <returns></returns>
        private (string orderBy, string order) ParseOrderBy(string orderByClause)
        {
            var filters = orderByClause?.Split(',');
            var orderBy = string.IsNullOrEmpty(filters?.ElementAtOrDefault(0)) ? Model.Constant.Constant.PrductName : filters[0];
            var order = string.IsNullOrEmpty(filters?.ElementAtOrDefault(1)) ? Model.Constant.Constant.ASC : filters[1];
            return (orderBy, order);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="dtProductCartResponse"></param>
        /// <param name="firstProduct"></param>
        /// <returns></returns>
        private ProductCartResponse MapProductCartResponse(CartPayload payload, DataTable dtProductCartResponse, Model.Product.ProductItemList firstProduct)
        {
            return new ProductCartResponse
            {
                UserId = payload.UserId,
                ProductId = payload.ProductId,
                Quantity = Convert.ToInt64(dtProductCartResponse.Rows[0]["QUANTITY"]),
                Price = firstProduct.Price,
                Total = firstProduct.Price * Convert.ToInt64(dtProductCartResponse.Rows[0]["QUANTITY"]),
                ProductName = firstProduct.ProductName,
                images = firstProduct.images ?? new List<Model.Product.ImageResponseDto>()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="dtProductCartResponse"></param>
        /// <param name="firstProduct"></param>
        /// <returns></returns>
       private ProductWishListResponse MapProductWishList(WishListPayload payload, DataTable dtProductWishList, Model.Product.ProductItemList firstProduct)
        {
            return new ProductWishListResponse
            {
                UserId = payload.UserId,
                ProductId = payload.ProductId,
                Quantity = Convert.ToInt64(dtProductWishList.Rows[0]["QUANTITY"]),
                Price = firstProduct.Price,
                Total = firstProduct.Price * Convert.ToInt64(dtProductWishList.Rows[0]["QUANTITY"]),
                ProductName = firstProduct.ProductName,
                images = firstProduct.images ?? new List<Model.Product.ImageResponseDto>()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static T GetColumnValue<T>(DataRow row, string columnName, T defaultValue = default)
        {
            if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
            {
                return (T)Convert.ChangeType(row[columnName], typeof(T));
            }
            return defaultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public List<Model.Response.Category> GetCategory(string tenantId)
        {
            try
            {
                List<Model.Product.ProductCategory> productCategories = this.productRepository.GetCategory(tenantId);

                List<Model.Response.Category> categories = new List<Model.Response.Category>();

                productCategories.Where(x => x.SubCategory == 0).OrderBy(x => x.CategoryId).ToList().ForEach(category =>
                {
                    Model.Response.Category cate = new Model.Response.Category();
                    List<Model.Response.SubCategory> categoryList = new List<Model.Response.SubCategory>();

                    List<Model.Product.ProductCategory> subCategories = productCategories.Where(x => x.SubCategory == category.CategoryId).ToList();

                    cate.CategoryId = category.CategoryId;
                    cate.Name = category.Category;

                    subCategories.ForEach(sub =>
                    {
                        Model.Response.SubCategory subCategory = new Model.Response.SubCategory();
                        subCategory.Id = sub.CategoryId;
                        subCategory.Name = sub.Category;
                        subCategory.Order = sub.CategoryId;
                        categoryList.Add(subCategory);
                    });
                    cate.subCategories = categoryList;

                    categories.Add(cate);
                });

                //string psw = EnDecrypt("I0FEO09BK0VEEN9DGJ9FGU8BMQ8DKTSP", false);

                return categories;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public List<Model.Product.ProductItemList> GetProductList(string tenantId, Model.Product.ProductPayload payload)
        {
            try
            {
                string spName = Model.Constant.Constant.XC_GET_PRODUCT_MASTER_LIST_TESTING;
                string[] filters = payload.OrderBy?.Split(',');
                string orderBy = string.IsNullOrEmpty(filters?.ElementAtOrDefault(0)) ? Model.Constant.Constant.PrductName : filters[0];
                string order = string.IsNullOrEmpty(filters?.ElementAtOrDefault(1)) ? Model.Constant.Constant.ASC : filters[1];


                List<Model.Product.ProductItemList> response = this.GetProductList(spName, tenantId, payload, orderBy, order);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        

        /// <summary>
        /// GetProductList
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="tenantId"></param>
        /// <param name="payload"></param>
        /// <param name="orderBy"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private List<ProductItemList> GetProductList(string spName, string tenantId, Model.Product.ProductPayload payload, string orderBy, string order)
        {
            List<Model.Product.SpProductMasterList> SpProductMasterList = this.productRepository.GetProductList(spName,
                    tenantId,
                    payload,
                    orderBy,
                    order);

            List<Model.Product.ProductItemList> productItemList = new List<Model.Product.ProductItemList>();
            if (SpProductMasterList == null || SpProductMasterList.Count < 1)
                return productItemList;

            Model.Product.ProductItemList mapitem;
            foreach (var productItems in SpProductMasterList.GroupBy(p => p.ProductId).Select(g => g.First()))
            {
                mapitem = new Model.Product.ProductItemList()
                {
                    ProductId = productItems.ProductId,
                    ProductName = productItems.ProductName,
                    Description = productItems.Description,
                    Price = productItems.Price,
                    Rating = productItems.Rating,
                    Stock = productItems.Stock,
                    BestSeller = productItems.BestSeller,
                    TenantId = productItems.TenantId,
                    Quantity = productItems.Quantity,
                    Numofreviews = productItems.Numofreviews,
                    Displayname = productItems.Displayname,
                    Guid = productItems.Guid,
                    Created = productItems.Created,
                    LastModified = productItems.LastModified,
                    LastModifiedBy = productItems.LastModifiedBy,
                    images = SpProductMasterList.Where(p => p.ProductId == productItems.ProductId)
                    .Select(image => new Model.Product.ImageResponseDto
                    {
                        Id = image.Id,
                        ImageUrl = $"/Products/GetImage/{image.Id}",
                        ThumbnailUrl = $"/Products/{image.Id}/GetThumbnail",
                        ContentType = image.ContentType,
                        ImageName = image.ImageName,
                    })
                    .ToList()
                };
                productItemList.Add(mapitem);
            }

            return productItemList;
        }

        //public void InsertCategory(string tenantId, Model.Response.Category category)
        //{
        //    // Logic to insert category into the database
        //    // This could involve calling a repository method to save the category
        //    try
        //    {
        //        // Assuming _productRepository is an instance of ProductRepository
        //        this.productRepository.InsertCategory(tenantId, category);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle exceptions
        //        throw new Exception("An error occurred while inserting the category", ex);
        //    }
        //}

        ///// <summary>
        ///// Get Product details by filter in sp route
        ///// </summary>
        ///// <param name="tenantId"></param>
        ///// <param name="locationId"></param>
        ///// <returns></returns>
        //internal List<Model.Product.ProductMaster> GetProducts(string tenantId, Model.Product.Search search)
        //{
        //    try
        //    {
        //        this.Logger.LogInformation($"getting the the list of prduct detils by filter in sp route for ( tenant :{tenantId} )");

        //        string spName = String.Empty;
        //        List<Model.Product.ProductMaster> products = new List<Model.Product.ProductMaster>();

        //        if (search.SubCategory == null)
        //            search.SubCategory = new List<string>();
        //        if (search.Category == null)
        //            search.Category = new List<string>();

        //        //product catalog sp name
        //        spName = "XC_GET_PRODUCT_DETAILS";
        //        //calling the product repository to get the list of prduct detils
        //        products = this.productRepository.GetProductDtailsUsingSp(tenantId, spName, search);

        //        return products;

        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        throw ex;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="RestaurantGuid"></param>
        ///// <param name="LocationGuid"></param>
        ///// <returns></returns>
        //public Model.Product.ProductMaster GetPartnerEventDetails(string RestaurantGuid, string LocationGuid)
        //{
        //    try
        //    {
        //        string spName = this.Configuration["StoredProcedure:GetPartnerEventDetail"];
        //        var detail = this.productRepository.GetPartnerEventDetails(RestaurantGuid: RestaurantGuid, LocationGuid: LocationGuid, storeProcedureName: spName).Result;

        //        if (detail == null)
        //            this.Logger.LogWarning($"Called service : Partner detail not exists for Restaurant Guid {RestaurantGuid} and Location Guid {LocationGuid} ");
        //        else
        //            this.Logger.LogWarning($"Called service: Received partner detail for Restaurant Guid {RestaurantGuid} and Location Guid {LocationGuid} ");

        //        return detail;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public List<Model.Product.ProductMaster> GetProductMaster(string tenantId, Model.Product.ProductPayload payload)
        //{
        //    try
        //    {

        //        string spName = Model.Constant.Constant.SA_GET_PRODUCTS_FOR_CATEGORY_MAPPING;

        //        string[] filters = payload.OrderBy?.Split(',');
        //        //string orderBy = string.IsNullOrEmpty(filters?.ElementAtOrDefault(0)) ? Constant.VendorName : filters[0];
        //        //string order = string.IsNullOrEmpty(filters?.ElementAtOrDefault(1)) ? Constant.ASC : filters[1];

        //        List<Model.Product.ProductMaster> response = this.productRepository.GetProductMaster(tenantId, payload, spName);

        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}


        #endregion


        #region Examples
        //private static void MapInvoiceTenantVendor(ProductMaster productMaster, DataRow row)
        //{
        //    productMaster.Id = GetColumnValue<long>(row, "tenant_vendor_id", 0);
        //    productMaster.ProductName = GetColumnValue<string>(row, "tenant_vendor_name", string.Empty);
        //    productMaster.Active = GetColumnValue<bool>(row, "default_invoice_id_as_invoice_date", false);
        //}



        //internal List<Model.Product.ProductMaster> GetProductMaster(string tenantId, PorductFilter filter)
        //{
        //    try
        //    {
        //        //Local variable
        //        List<Model.Product.ProductMaster> productMaster = new List<ProductMaster>();

        //        //Get Invoice aggregate
        //        productMaster = this.productRepository.GetProductMaster(tenantId, filter).Result;

        //        //Return
        //        return productMaster;
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        throw ex;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public List<Model.Response.MenuMaster.MenuMaster> GetMenuMaster()
        //{
        //    List<Model.Response.MenuMaster.MenuMaster> menu = (List<Model.Response.MenuMaster.MenuMaster>)Newtonsoft.Json.JsonConvert.DeserializeObject("[ { \"menuId\": 1, \"menuName\": \"Home\", \"orderBy\": 1, \"active\": true, \"image\": \"\", \"subMenu\": false, \"category\": [], \"link\": \"/\" }, { \"menuId\": 2, \"menuName\": \"Seed\", \"orderBy\": 2, \"active\": true, \"image\": \"\", \"subMenu\": true, \"category\": [ { \"categoryId\": 1, \"category\": \"All Seed\", \"active\": true }, { \"categoryId\": 2, \"category\": \"Vegetable\", \"active\": true }, { \"categoryId\": 3, \"category\": \"Herbal\", \"active\": true }, { \"categoryId\": 4, \"category\": \"Fruits\", \"active\": true }, { \"categoryId\": 5, \"category\": \"Greens\", \"active\": true } ] }, { \"menuId\": 3, \"menuName\": \"Plants\", \"orderBy\": 3, \"active\": true, \"image\": \"\", \"subMenu\": true, \"category\": [ { \"categoryId\": 1, \"category\": \"All Plants\", \"active\": true }, { \"categoryId\": 2, \"category\": \"Indoor\", \"active\": true }, { \"categoryId\": 3, \"category\": \"Outdoor\", \"active\": true }, { \"categoryId\": 4, \"category\": \"New Arrivals\", \"active\": true }, { \"categoryId\": 5, \"category\": \"Air Furify\", \"active\": true } ] }, { \"menuId\": 4, \"menuName\": \"Contact Us\", \"orderBy\": 4, \"active\": true, \"link\": \"/contactus\", \"image\": \"\", \"subMenu\": false } ]", typeof(List<Model.Response.MenuMaster.MenuMaster>));

        //    return menu.OrderBy(x => x.orderBy).ToList();
        //}

        //public List<Model.Response.ProductMaster.ProductMaster> GetProductMaster()
        //{
        //    List<Model.Response.ProductMaster.ProductMaster> menu = (List<Model.Response.ProductMaster.ProductMaster>)Newtonsoft.Json.JsonConvert.DeserializeObject("[\r\n        {\r\n            \"productId\": 1001,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Apple\",\r\n            \"productDescription\": \"Apple\",\r\n            \"productCode\": \"SD101\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 10,\r\n            \"total\": 100,\r\n            \"price\": 200,\r\n            \"category\": 1,\r\n            \"rating\": 2,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 2,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"50%\",\r\n            \"orderBy\": 15,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1633356122544-f134324a6cee?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1002,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Orange\",\r\n            \"productDescription\": \"Orange\",\r\n            \"productCode\": \"OR1O1\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 100,\r\n            \"total\": 100,\r\n            \"price\": 200,\r\n            \"category\": 2,\r\n            \"rating\": 3,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 1,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"50%\",\r\n            \"orderBy\": 1,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1580894894513-541e068a3e2b?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"orderBy\": 1,\r\n                    \"main\" : true,\r\n                    \"active\":true\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1003,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Graps\",\r\n            \"productDescription\": \"Graps\",\r\n            \"productCode\": \"DR101\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 200,\r\n            \"total\": 200,\r\n            \"price\": 200,\r\n            \"category\": 2,\r\n            \"rating\": 1,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 2,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"50%\",\r\n            \"orderBy\": 2,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1523726491678-bf852e717f6a?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1004,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Givi\",\r\n            \"productDescription\": \"Givi\",\r\n            \"productCode\": \"app001\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 400,\r\n            \"total\": 400,\r\n            \"price\": 400,\r\n            \"category\": 3,\r\n            \"rating\": 5,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 1,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"10%\",\r\n            \"orderBy\": 3,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1523726491678-bf852e717f6a?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1005,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Gova\",\r\n            \"productDescription\": \"Gova\",\r\n            \"productCode\": \"app001\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 50,\r\n            \"total\": 50,\r\n            \"price\": 50,\r\n            \"category\": 1,\r\n            \"rating\": 3,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 1,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": false,\r\n            \"best_seller\": true,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"70%\",\r\n            \"orderBy\": 4,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1595617795501-9661aafda72a?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1006,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Mango\",\r\n            \"productDescription\": \"Mango\",\r\n            \"productCode\": \"PRT100\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 60,\r\n            \"total\": 60,\r\n            \"price\": 60,\r\n            \"category\": 1,\r\n            \"rating\": 4,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 2,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": false,\r\n            \"best_seller\": true,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"80%\",\r\n            \"orderBy\": 5,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1639322537228-f710d846310a?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1007,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"JackFruit\",\r\n            \"productDescription\": \"JackFruit\",\r\n            \"productCode\": \"app001\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 70,\r\n            \"total\": 70,\r\n            \"price\": 70,\r\n            \"category\": 1,\r\n            \"rating\": 1,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 2,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"20%\",\r\n            \"orderBy\": 6,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1522542550221-31fd19575a2d?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1008,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Banana\",\r\n            \"productDescription\": \"Banana\",\r\n            \"productCode\": \"app001\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 80,\r\n            \"total\": 80,\r\n            \"price\": 80,\r\n            \"category\": 1,\r\n            \"rating\": 4,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 2,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"40%\",\r\n            \"orderBy\": 7,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1621839673705-6617adf9e890?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1009,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Cherry\",\r\n            \"productDescription\": \"Cherry\",\r\n            \"productCode\": \"app001\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 90,\r\n            \"total\": 90,\r\n            \"price\": 90,\r\n            \"category\": 1,\r\n            \"rating\": 5,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 1,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"20%\",\r\n            \"orderBy\": 8,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1613490900233-141c5560d75d?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1010,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Water Apple\",\r\n            \"productDescription\": \"Water Apple\",\r\n            \"productCode\": \"app001\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 10,\r\n            \"total\": 10,\r\n            \"price\": 20,\r\n            \"category\": 1,\r\n            \"rating\": 1,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 2,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"50%\",\r\n            \"orderBy\": 9,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1624953587687-daf255b6b80a?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1011,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Pappaya\",\r\n            \"productDescription\": \"Pappaya\",\r\n            \"productCode\": \"app001\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 110,\r\n            \"total\": 110,\r\n            \"price\": 210,\r\n            \"category\": 1,\r\n            \"rating\": 3,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 2,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"50%\",\r\n            \"orderBy\": 10,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1623479322729-28b25c16b011?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1012,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Sapotta\",\r\n            \"productDescription\": \"Sapotta\",\r\n            \"productCode\": \"app001\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 120,\r\n            \"total\": 120,\r\n            \"price\": 220,\r\n            \"category\": 1,\r\n            \"rating\": 4,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 2,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"50%\",\r\n            \"orderBy\": 11,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1507721999472-8ed4421c4af2?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1013,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Pomagrante\",\r\n            \"productDescription\": \"Pomagrante\",\r\n            \"productCode\": \"app001\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 130,\r\n            \"total\": 130,\r\n            \"price\": 230,\r\n            \"category\": 1,\r\n            \"rating\": 1,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 2,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"50%\",\r\n            \"orderBy\": 12,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1633356122102-3fe601e05bd2?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1014,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Apple\",\r\n            \"productDescription\": \"Ooty Apple\",\r\n            \"productCode\": \"app001\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 140,\r\n            \"total\": 140,\r\n            \"price\": 140,\r\n            \"category\": 1,\r\n            \"rating\": 5,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 2,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"50%\",\r\n            \"orderBy\": 13,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1515879218367-8466d910aaa4?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"productId\": 1015,\r\n            \"tenantId\": 10,\r\n            \"productName\": \"Apple\",\r\n            \"productDescription\": \"Ooty Apple\",\r\n            \"productCode\": \"app001\",\r\n            \"fullDescription\": \"\",\r\n            \"specification\": \"\",\r\n            \"story\": \"\",\r\n            \"packQuantity\": 10,\r\n            \"quantity\": 100,\r\n            \"total\": 100,\r\n            \"price\": 200,\r\n            \"category\": 1,\r\n            \"rating\": 1,\r\n            \"active\": true,\r\n            \"trending\": 1,\r\n            \"userBuyCount\": 50,\r\n            \"return\": 1,\r\n            \"created\": \"date\",\r\n            \"modified\": \"date\",\r\n            \"in_stock\": true,\r\n            \"best_seller\": false,\r\n            \"deleveryDate\": 5,\r\n            \"offer\": \"50%\",\r\n            \"orderBy\": 14,\r\n            \"userId\": 1,\r\n            \"overview\": \"Lorem ipsum dolor sit amet consectetur adipisicing elit. Error unde quisquam magni vel eligendi nam.\",\r\n            \"long_description\": \"Lorem ipsum dolor sit amet consectetur, adipisicing elit. Soluta aut, vel ipsum maxime quam quia, quaerat tempore minus odio exercitationem illum et eos, quas ipsa aperiam magnam officiis libero expedita quo voluptas deleniti sit dolore? Praesentium tempora cumque facere consectetur quia, molestiae quam, accusamus eius corrupti laudantium aliquid! Tempore laudantium unde labore voluptates repellat, dignissimos aperiam ad ipsum laborum recusandae voluptatem non dolore. Reiciendis cum quo illum. Dolorem, molestiae corporis.\",\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1587440871875-191322ee64b0?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"main\" : true,\r\n                    \"active\":true,\r\n                    \"orderBy\": 1\r\n                }\r\n            ]\r\n        }\r\n    ]", typeof(List<Model.Response.ProductMaster.ProductMaster>));

        //    return menu.OrderBy(x => x.OrderBy).ToList();
        //}

        //public Model.Response.ProductMaster.ProductMaster GetProductDetails(string productId)
        //{
        //    List<Model.Response.ProductMaster.ProductMaster> productMaster = GetProductMaster();

        //    return productMaster.Where(x => x.ProductId == Convert.ToInt64(productId)).FirstOrDefault();
        //}

        //public List<Model.Response.CartList> GetCartProduct()
        //{
        //    List<Model.Response.CartList> cartLists = (List<Model.Response.CartList>)Newtonsoft.Json.JsonConvert.DeserializeObject("[\r\n        {\r\n            \"productId\": 1001,\r\n            \"productName\": \"Apple\",\r\n            \"tenantId\": 10,\r\n            \"quantity\": 9,\r\n            \"orderBy\": 15,\r\n            \"userId\": 1,\r\n            \"price\": 200,\r\n            \"images\": [\r\n                {\r\n                    \"imageId\": 1,\r\n                    \"poster\": \"https://images.unsplash.com/photo-1580894894513-541e068a3e2b?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=650&q=40\",\r\n                    \"orderBy\": 1,\r\n                    \"main\": true,\r\n                    \"active\": true\r\n                }\r\n            ]\r\n        }\r\n    ]", typeof(List<Model.Response.CartList>));

        //    return cartLists.OrderBy(x => x.OrderBy).ToList();
        //}        


        public static string EnDecrypt(string input, bool decrypt = false)
        {
            string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ984023";

            if (decrypt)
            {
                Dictionary<string, uint> _index = null;
                Dictionary<string, Dictionary<string, uint>> _indexes =
                    new Dictionary<string, Dictionary<string, uint>>(2, StringComparer.InvariantCulture);

                if (_index == null)
                {
                    Dictionary<string, uint> cidx;

                    string indexKey = "I" + _alphabet;

                    if (!_indexes.TryGetValue(indexKey, out cidx))
                    {
                        lock (_indexes)
                        {
                            if (!_indexes.TryGetValue(indexKey, out cidx))
                            {
                                cidx = new Dictionary<string, uint>(_alphabet.Length, StringComparer.InvariantCulture);
                                for (int i = 0; i < _alphabet.Length; i++)
                                {
                                    cidx[_alphabet.Substring(i, 1)] = (uint)i;
                                }

                                _indexes.Add(indexKey, cidx);
                            }
                        }
                    }

                    _index = cidx;
                }

                MemoryStream ms = new MemoryStream(Math.Max((int)Math.Ceiling(input.Length * 5 / 8.0), 1));

                for (int i = 0; i < input.Length; i += 8)
                {
                    int chars = Math.Min(input.Length - i, 8);

                    ulong val = 0;

                    int bytes = (int)Math.Floor(chars * (5 / 8.0));

                    for (int charOffset = 0; charOffset < chars; charOffset++)
                    {
                        uint cbyte;
                        if (!_index.TryGetValue(input.Substring(i + charOffset, 1), out cbyte))
                        {
                            throw new ArgumentException(string.Format("Invalid character {0} valid characters are: {1}",
                                input.Substring(i + charOffset, 1), _alphabet));
                        }

                        val |= (((ulong)cbyte) << ((((bytes + 1) * 8) - (charOffset * 5)) - 5));
                    }

                    byte[] buff = BitConverter.GetBytes(val);
                    Array.Reverse(buff);
                    ms.Write(buff, buff.Length - (bytes + 1), bytes);
                }

                return System.Text.ASCIIEncoding.ASCII.GetString(ms.ToArray());
            }
            else
            {
                byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(input);

                StringBuilder result = new StringBuilder(Math.Max((int)Math.Ceiling(data.Length * 8 / 5.0), 1));

                byte[] emptyBuff = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                byte[] buff = new byte[8];

                for (int i = 0; i < data.Length; i += 5)
                {
                    int bytes = Math.Min(data.Length - i, 5);

                    Array.Copy(emptyBuff, buff, emptyBuff.Length);
                    Array.Copy(data, i, buff, buff.Length - (bytes + 1), bytes);
                    Array.Reverse(buff);
                    ulong val = BitConverter.ToUInt64(buff, 0);

                    for (int bitOffset = ((bytes + 1) * 8) - 5; bitOffset > 3; bitOffset -= 5)
                    {
                        result.Append(_alphabet[(int)((val >> bitOffset) & 0x1f)]);
                    }
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Removes a product from the cart.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="payload">The payload containing product and user details.</param>
        /// <returns>The result of the removal operation.</returns>
        public long RemoveProductCart(string tenantId, RemoveCartPayLoad payload)
        {
            try
            {
                return productRepository.RemoveProductCart(tenantId, payload);
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                throw new Exception("An error occurred while removing the product from the cart.", ex);
            }
        }

        /// <summary>
        /// Removes a product from the wishlist.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="payload">The payload containing product and user details.</param>
        /// <returns>The result of the removal operation.</returns>
        public long RemoveProductWishList(string tenantId, RemoveWhishListPayload payload)
        {
            try
            {
                return productRepository.RemoveProductWishList(tenantId, payload);
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                throw new Exception("An error occurred while removing the product from the wishlist.", ex);
            }
        }
        #endregion
    }
}
