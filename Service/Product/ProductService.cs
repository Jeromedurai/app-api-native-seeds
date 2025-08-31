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
                ProductId = productIds,
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

        public string GetValueByKey(string key)
        {
            try
            {
                return productRepository.GetConfigValueByKey(key);
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                throw new Exception("An error occurred while retrieving the value by key.", ex);
            }  
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="productId">Product ID</param>
        /// <returns>Task</returns>
        public async Task DeleteProduct(long tenantId, long productId)
        {
            try
            {
                if (tenantId <= 0)
                    throw new ArgumentException("Invalid tenant ID", nameof(tenantId));

                if (productId <= 0)
                    throw new ArgumentException("Invalid product ID", nameof(productId));

                // Get current user ID from context or pass it as parameter
                long userId = 1; // TODO: Get from context

                // Call repository to delete product
                await productRepository.DeleteProduct(tenantId, productId, userId);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the product.", ex);
            }
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <param name="tenantId">Optional tenant ID filter</param>
        /// <returns>List of categories</returns>
        public List<CategoryListItem> GetAllCategories(long? tenantId = null)
        {
            try
            {
                return productRepository.GetAllCategories(tenantId);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving categories.", ex);
            }
        }

        /// <summary>
        /// Add a new category
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Category details</param>
        /// <returns>Newly created category</returns>
        public async Task<AddCategoryResponse> AddCategory(long tenantId, AddCategoryRequest request)
        {
            try
            {
                if (tenantId <= 0)
                    throw new ArgumentException("Invalid tenant ID", nameof(tenantId));

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (string.IsNullOrWhiteSpace(request.CategoryName))
                    throw new ArgumentException("Category name is required");

                // Get current user ID from context or pass it as parameter
                long userId = 1; // TODO: Get from context

                // Call repository to add category
                var categoryId = await productRepository.AddCategory(tenantId, request, userId);

                // Return the newly created category
                return new AddCategoryResponse
                {
                    CategoryId = categoryId,
                    Category = request.CategoryName,
                    Active = request.Active,
                    ParentId = request.ParentCategoryId,
                    Description = request.Description,
                    OrderBy = request.OrderBy,
                    Icon = request.Icon,
                    SubMenu = request.HasSubMenu,
                    Link = request.Link,
                    Created = DateTime.UtcNow,
                    TenantId = tenantId
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the category.", ex);
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Category details</param>
        /// <returns>Task</returns>
        public async Task UpdateCategory(long tenantId, UpdateCategoryRequest request)
        {
            try
            {
                if (tenantId <= 0)
                    throw new ArgumentException("Invalid tenant ID", nameof(tenantId));

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.CategoryId <= 0)
                    throw new ArgumentException("Invalid category ID", nameof(request.CategoryId));

                if (string.IsNullOrWhiteSpace(request.CategoryName))
                    throw new ArgumentException("Category name is required");

                // Get current user ID from context or pass it as parameter
                long userId = 1; // TODO: Get from context

                // Call repository to update category
                await productRepository.UpdateCategory(tenantId, request, userId);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the category.", ex);
            }
        }

        /// <summary>
        /// Get menu master with categories
        /// </summary>
        /// <param name="tenantId">Optional tenant ID filter</param>
        /// <returns>Menu master with associated categories</returns>
        public MenuMasterResponse GetMenuMaster(long? tenantId = null)
        {
            try
            {
                return productRepository.GetMenuMaster(tenantId);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving menu master.", ex);
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Product details</param>
        /// <returns>Product ID</returns>
        public async Task<long> UpdateProduct(long tenantId, UpdateProductRequest request)
        {
            try
            {
                if (tenantId <= 0)
                    throw new ArgumentException("Invalid tenant ID", nameof(tenantId));

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.ProductId <= 0)
                    throw new ArgumentException("Invalid product ID", nameof(request.ProductId));

                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.ProductName))
                    throw new ArgumentException("Product name is required");

                if (string.IsNullOrWhiteSpace(request.ProductCode))
                    throw new ArgumentException("Product code is required");

                if (request.Price <= 0)
                    throw new ArgumentException("Price must be greater than zero");

                if (request.Category <= 0)
                    throw new ArgumentException("Category is required");

                // Call repository to update product
                return await productRepository.UpdateProduct(tenantId, request);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the product.", ex);
            }
        }

        /// <summary>
        /// Add a new product
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Product details</param>
        /// <returns>Product ID</returns>
        public async Task<long> AddProduct(long tenantId, AddProductRequest request)
        {
            try
            {
                if (tenantId <= 0)
                    throw new ArgumentException("Invalid tenant ID", nameof(tenantId));

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.ProductName))
                    throw new ArgumentException("Product name is required");

                if (string.IsNullOrWhiteSpace(request.ProductCode))
                    throw new ArgumentException("Product code is required");

                if (request.Price <= 0)
                    throw new ArgumentException("Price must be greater than zero");

                if (request.Category <= 0)
                    throw new ArgumentException("Category is required");

                // Call repository to add product
                return await productRepository.AddProduct(tenantId, request);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while adding the product.", ex);
            }
        }

        /// <summary>
        /// Get product details by ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Product details with images</returns>
        public async Task<ProductDetailItem> GetProductById(long productId)
        {
            try
            {
                if (productId <= 0)
                    throw new ArgumentException("Invalid product ID", nameof(productId));

                var result = await productRepository.GetProductById(productId);

                if (result == null || result.Tables.Count == 0 || result.Tables[0].Rows.Count == 0)
                    return null;

                // Map product details
                var product = MapProductSearchResults(result.Tables[0]).FirstOrDefault();

                if (product != null && result.Tables.Count > 1)
                {
                    // Map images
                    product.Images = MapProductImages(result.Tables[1]);
                }

                return product;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the product.", ex);
            }
        }

        /// <summary>
        /// Maps DataTable to list of product images
        /// </summary>
        private List<ProductSearchImageInfo> MapProductImages(DataTable dataTable)
        {
            var images = new List<ProductSearchImageInfo>();

            foreach (DataRow row in dataTable.Rows)
            {
                images.Add(new ProductSearchImageInfo
                {
                    ImageId = GetColumnValue<long>(row, "ImageId"),
                    Poster = GetColumnValue<string>(row, "Poster", string.Empty),
                    Main = GetColumnValue<bool>(row, "Main"),
                    Active = GetColumnValue<bool>(row, "Active"),
                    OrderBy = GetColumnValue<int>(row, "OrderBy")
                });
            }

            return images;
        }

        /// <summary>
        /// Search products with advanced filtering and pagination
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="payload">Search parameters</param>
        /// <returns>Product search results with pagination</returns>
        public ProductSearchResponse SearchProducts(string tenantId, ProductSearchPayload payload)
        {
            try
            {
                if (payload == null)
                    throw new ArgumentNullException(nameof(payload), "Search payload cannot be null.");

                // Calculate offset for pagination
                int offset = (payload.Page - 1) * payload.Limit;

                // Call repository method to get search results
                var searchResults = productRepository.SearchProducts(tenantId, payload, offset);

                // Map the results to response model
                var response = new ProductSearchResponse();

                if (searchResults != null && searchResults.Tables.Count >= 2)
                {
                    // First table contains product data
                    var productTable = searchResults.Tables[0];
                    // Second table contains total count
                    var countTable = searchResults.Tables[1];

                    // Map products
                    response.Products = MapProductSearchResults(productTable);

                    // Map pagination info
                    int totalCount = countTable.Rows.Count > 0 ? Convert.ToInt32(countTable.Rows[0]["TotalCount"]) : 0;
                    response.Pagination = new PaginationInfo
                    {
                        Page = payload.Page,
                        Limit = payload.Limit,
                        Total = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / payload.Limit),
                        HasNext = payload.Page * payload.Limit < totalCount,
                        HasPrevious = payload.Page > 1
                    };
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while searching products.", ex);
            }
        }

        /// <summary>
        /// Maps DataTable results to ProductDetailItem list
        /// </summary>
        /// <param name="dataTable">DataTable containing product data</param>
        /// <returns>List of ProductDetailItem</returns>
        private List<ProductDetailItem> MapProductSearchResults(DataTable dataTable)
        {
            var products = new List<ProductDetailItem>();

            foreach (DataRow row in dataTable.Rows)
            {
                var product = new ProductDetailItem
                {
                    ProductId = GetColumnValue<long>(row, "ProductId"),
                    TenantId = GetColumnValue<long>(row, "TenantId"),
                    ProductName = GetColumnValue<string>(row, "ProductName", string.Empty),
                    ProductDescription = GetColumnValue<string>(row, "ProductDescription", string.Empty),
                    ProductCode = GetColumnValue<string>(row, "ProductCode", string.Empty),
                    FullDescription = GetColumnValue<string>(row, "FullDescription", string.Empty),
                    Specification = GetColumnValue<string>(row, "Specification", string.Empty),
                    Story = GetColumnValue<string>(row, "Story", string.Empty),
                    PackQuantity = GetColumnValue<int>(row, "PackQuantity"),
                    Quantity = GetColumnValue<int>(row, "Quantity"),
                    Total = GetColumnValue<int>(row, "Total"),
                    Price = GetColumnValue<decimal>(row, "Price"),
                    Category = GetColumnValue<int>(row, "Category"),
                    Rating = GetColumnValue<int>(row, "Rating"),
                    Active = GetColumnValue<bool>(row, "Active"),
                    Trending = GetColumnValue<int>(row, "Trending"),
                    UserBuyCount = GetColumnValue<int>(row, "UserBuyCount"),
                    Return = GetColumnValue<int>(row, "Return"),
                    Created = GetColumnValue<DateTime>(row, "Created"),
                    Modified = GetColumnValue<DateTime>(row, "Modified"),
                    InStock = GetColumnValue<bool>(row, "InStock"),
                    BestSeller = GetColumnValue<bool>(row, "BestSeller"),
                    DeliveryDate = GetColumnValue<int>(row, "DeliveryDate"),
                    Offer = GetColumnValue<string>(row, "Offer", string.Empty),
                    OrderBy = GetColumnValue<int>(row, "OrderBy"),
                    UserId = GetColumnValue<long>(row, "UserId"),
                    Overview = GetColumnValue<string>(row, "Overview", string.Empty),
                    LongDescription = GetColumnValue<string>(row, "LongDescription", string.Empty),
                    Images = new List<ProductSearchImageInfo>() // Images will be populated separately if needed
                };

                products.Add(product);
            }

            return products;
        }

        /// <summary>x
        /// Get user's shopping cart with full product details
        /// </summary>
        /// <param name="request">Cart request with user details</param>
        /// <returns>Complete cart information with products</returns>
        public async Task<Model.User.CartResponse> GetUserCart(Model.ProductCart.GetCartRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Get cart attempt for user: {request.UserId}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.UserId <= 0)
                    throw new ArgumentException("Valid User ID is required");

                var cartData = await this.productRepository.GetUserCart(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Cart retrieval successful for user: {request.UserId} - {cartData.Items.Count} items found");

                return cartData;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Cart retrieval failed - user not found: {request?.UserId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Cart retrieval error for user {request?.UserId}: {ex.Message}");
                throw new Exception("An error occurred while retrieving the cart.", ex);
            }
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        /// <param name="request">Add to cart request</param>
        /// <returns>Cart item details and summary</returns>
        public async Task<Model.ProductCart.AddToCartResponse> AddItemToCart(Model.ProductCart.AddToCartRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Add to cart attempt for user: {request.UserId}, product: {request.ProductId}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.UserId <= 0)
                    throw new ArgumentException("Valid User ID is required");

                if (request.ProductId <= 0)
                    throw new ArgumentException("Valid Product ID is required");

                if (request.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than 0");

                var cartResponse = await this.productRepository.AddItemToCart(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Add to cart successful for user: {request.UserId}, product: {request.ProductId}");

                return cartResponse;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Add to cart failed - product/user not found: {request?.ProductId}/{request?.UserId}");
                throw;
            }
            catch (InvalidOperationException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Add to cart failed - business rule violation for user: {request?.UserId}, product: {request?.ProductId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Add to cart error for user {request?.UserId}, product {request?.ProductId}: {ex.Message}");
                throw new Exception("An error occurred while adding item to cart.", ex);
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        /// <param name="request">Remove from cart request</param>
        /// <returns>Removal confirmation and updated cart summary</returns>
        public async Task<Model.ProductCart.RemoveFromCartResponse> RemoveItemFromCart(Model.ProductCart.RemoveFromCartRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Remove from cart attempt for user: {request.UserId}, product: {request.ProductId}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.UserId <= 0)
                    throw new ArgumentException("Valid User ID is required");

                if (request.ProductId <= 0)
                    throw new ArgumentException("Valid Product ID is required");

                var removeResponse = await this.productRepository.RemoveItemFromCart(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Remove from cart successful for user: {request.UserId}, product: {request.ProductId}");

                return removeResponse;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Remove from cart failed - product not found in cart: {request?.ProductId} for user: {request?.UserId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Remove from cart error for user {request?.UserId}, product {request?.ProductId}: {ex.Message}");
                throw new Exception("An error occurred while removing item from cart.", ex);
            }
        }

        /// <summary>
        /// Clear entire cart
        /// </summary>
        /// <param name="request">Clear cart request</param>
        /// <returns>Cart clearing confirmation and statistics</returns>
        public async Task<Model.ProductCart.ClearCartResponse> ClearCart(Model.ProductCart.ClearCartRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Clear cart attempt for user: {request.UserId}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.UserId <= 0)
                    throw new ArgumentException("Valid User ID is required");

                var clearResponse = await this.productRepository.ClearCart(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Clear cart successful for user: {request.UserId} - {clearResponse.ClearedItemCount} items cleared");

                return clearResponse;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Clear cart failed - user not found or cart already empty: {request?.UserId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Clear cart error for user {request?.UserId}: {ex.Message}");
                throw new Exception("An error occurred while clearing the cart.", ex);
            }
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        /// <param name="request">Create order request</param>
        /// <returns>Order creation confirmation and details</returns>
        public async Task<Model.Order.CreateOrderResponse> CreateOrder(Model.Order.CreateOrderRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Create order attempt for user: {request.UserId} with {request.Items.Count} items");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.UserId <= 0)
                    throw new ArgumentException("Valid User ID is required");

                if (request.Items == null || !request.Items.Any())
                    throw new ArgumentException("Order must contain at least one item");

                if (request.ShippingAddress == null)
                    throw new ArgumentException("Shipping address is required");

                if (request.BillingAddress == null)
                    throw new ArgumentException("Billing address is required");

                if (request.PaymentMethod == null)
                    throw new ArgumentException("Payment method is required");

                if (request.ShippingMethod == null)
                    throw new ArgumentException("Shipping method is required");

                if (request.Totals == null)
                    throw new ArgumentException("Order totals are required");

                // Validate each item has valid values
                foreach (var item in request.Items)
                {
                    if (item.ProductId <= 0)
                        throw new ArgumentException($"Invalid product ID: {item.ProductId}");

                    if (item.Quantity <= 0)
                        throw new ArgumentException($"Invalid quantity for product {item.ProductId}: {item.Quantity}");

                    if (item.Price <= 0)
                        throw new ArgumentException($"Invalid price for product {item.ProductId}: {item.Price}");

                    if (Math.Abs(item.Total - (item.Price * item.Quantity)) > 0.01m)
                        throw new ArgumentException($"Price calculation mismatch for product {item.ProductId}");
                }

                var orderResponse = await this.productRepository.CreateOrder(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Create order successful for user: {request.UserId}, Order Number: {orderResponse.OrderNumber}");

                return orderResponse;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Create order failed - user not found: {request?.UserId}");
                throw;
            }
            catch (InvalidOperationException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Create order failed - business rule violation for user: {request?.UserId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Create order error for user {request?.UserId}: {ex.Message}");
                throw new Exception("An error occurred while creating the order.", ex);
            }
        }

        /// <summary>
        /// Get user orders with pagination and filtering
        /// </summary>
        /// <param name="request">Get orders request</param>
        /// <returns>List of orders with pagination information</returns>
        public async Task<Model.Order.GetOrdersResponse> GetOrders(Model.Order.GetOrdersRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Get orders attempt for user: {request.UserId}, page: {request.Page}, limit: {request.Limit}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.UserId <= 0)
                    throw new ArgumentException("Valid User ID is required");

                if (request.Page < 1)
                    request.Page = 1;

                if (request.Limit < 1 || request.Limit > 100)
                    request.Limit = 10;

                var ordersResponse = await this.productRepository.GetOrders(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Get orders successful for user: {request.UserId}, found {ordersResponse.Orders.Count} orders");

                return ordersResponse;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Get orders failed - user not found: {request?.UserId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Get orders error for user {request?.UserId}: {ex.Message}");
                throw new Exception("An error occurred while retrieving orders.", ex);
            }
        }

        /// <summary>
        /// Get order details by order ID
        /// </summary>
        /// <param name="request">Get order by ID request</param>
        /// <returns>Detailed order information</returns>
        public async Task<Model.Order.GetOrderByIdResponse> GetOrderById(Model.Order.GetOrderByIdRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Get order by ID attempt for user: {request.UserId}, order: {request.OrderId}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.UserId <= 0)
                    throw new ArgumentException("Valid User ID is required");

                if (request.OrderId <= 0)
                    throw new ArgumentException("Valid Order ID is required");

                var orderResponse = await this.productRepository.GetOrderById(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Get order by ID successful for user: {request.UserId}, order: {request.OrderId}");

                return orderResponse;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Get order by ID failed - order not found or doesn't belong to user: {request?.OrderId} for user: {request?.UserId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Get order by ID error for user {request?.UserId}, order {request?.OrderId}: {ex.Message}");
                throw new Exception("An error occurred while retrieving the order.", ex);
            }
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        /// <param name="request">Cancel order request</param>
        /// <returns>Order cancellation confirmation</returns>
        public async Task<Model.Order.CancelOrderResponse> CancelOrder(Model.Order.CancelOrderRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Cancel order attempt for user: {request.UserId}, order: {request.OrderId}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.UserId <= 0)
                    throw new ArgumentException("Valid User ID is required");

                if (request.OrderId <= 0)
                    throw new ArgumentException("Valid Order ID is required");

                var cancelResponse = await this.productRepository.CancelOrder(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Cancel order successful for user: {request.UserId}, order: {request.OrderId}");

                return cancelResponse;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Cancel order failed - order not found or doesn't belong to user: {request?.OrderId} for user: {request?.UserId}");
                throw;
            }
            catch (InvalidOperationException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Cancel order failed - order cannot be cancelled: {request?.OrderId} for user: {request?.UserId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Cancel order error for user {request?.UserId}, order {request?.OrderId}: {ex.Message}");
                throw new Exception("An error occurred while cancelling the order.", ex);
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        /// <param name="request">Update order status request</param>
        /// <returns>Order status update confirmation</returns>
        public async Task<Model.Order.UpdateOrderStatusResponse> UpdateOrderStatus(Model.Order.UpdateOrderStatusRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Update order status attempt for order: {request.OrderId} to status: {request.Status}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.OrderId <= 0)
                    throw new ArgumentException("Valid Order ID is required");

                if (string.IsNullOrEmpty(request.Status))
                    throw new ArgumentException("Status is required");

                // Validate status value (optional - could also be done in stored procedure)
                var validStatuses = new[] { "Pending", "Confirmed", "Processing", "Shipped", "Delivered", "Cancelled", "Returned", "Refunded" };
                if (!validStatuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Invalid status: {request.Status}. Valid statuses are: {string.Join(", ", validStatuses)}");
                }

                var statusResponse = await this.productRepository.UpdateOrderStatus(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Update order status successful for order: {request.OrderId} to status: {request.Status}");

                return statusResponse;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Update order status failed - order not found: {request?.OrderId}");
                throw;
            }
            catch (InvalidOperationException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Update order status failed - invalid status transition: {request?.OrderId} to {request?.Status}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Update order status error for order {request?.OrderId}: {ex.Message}");
                throw new Exception("An error occurred while updating the order status.", ex);
            }
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        /// <param name="request">Get all users request</param>
        /// <returns>Users list with pagination</returns>
        public async Task<Model.Admin.GetAllUsersResponse> GetAllUsers(Model.Admin.GetAllUsersRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Admin get all users attempt by admin: {request.AdminUserId}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.AdminUserId <= 0)
                    throw new ArgumentException("Valid Admin User ID is required");

                if (request.Page < 1)
                    request.Page = 1;

                if (request.Limit < 1 || request.Limit > 100)
                    request.Limit = 10;

                var usersResponse = await this.productRepository.GetAllUsers(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Admin get all users successful by admin: {request.AdminUserId}, found {usersResponse.Users.Count} users");

                return usersResponse;
            }
            catch (UnauthorizedAccessException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Admin get all users failed - insufficient privileges: {request?.AdminUserId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Admin get all users error for admin {request?.AdminUserId}: {ex.Message}");
                throw new Exception("An error occurred while retrieving users.", ex);
            }
        }

        /// <summary>
        /// Update user role (Admin only)
        /// </summary>
        /// <param name="request">Update user role request</param>
        /// <returns>Role update confirmation</returns>
        public async Task<Model.Admin.UpdateUserRoleResponse> UpdateUserRole(Model.Admin.UpdateUserRoleRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Admin update user role attempt by admin: {request.AdminUserId} for user: {request.UserId} to role: {request.Role}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.AdminUserId <= 0)
                    throw new ArgumentException("Valid Admin User ID is required");

                if (request.UserId <= 0)
                    throw new ArgumentException("Valid User ID is required");

                if (string.IsNullOrEmpty(request.Role))
                    throw new ArgumentException("Role is required");

                // Validate role value (optional - could also be done in stored procedure)
                var validRoles = new[] { "Customer", "Executive", "Admin", "SuperAdmin", "Manager", "Support" };
                if (!validRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Invalid role: {request.Role}. Valid roles are: {string.Join(", ", validRoles)}");
                }

                // Validate permissions if provided
                if (request.Permissions != null && request.Permissions.Any())
                {
                    var validPermissions = new[] { 
                        "view_products", "manage_products", "view_orders", "manage_orders", 
                        "view_users", "manage_users", "view_reports", "manage_settings",
                        "view_inventory", "manage_inventory", "view_analytics", "manage_roles"
                    };
                    
                    var invalidPermissions = request.Permissions.Where(p => !validPermissions.Contains(p, StringComparer.OrdinalIgnoreCase)).ToList();
                    if (invalidPermissions.Any())
                    {
                        throw new ArgumentException($"Invalid permissions: {string.Join(", ", invalidPermissions)}");
                    }
                }

                var roleResponse = await this.productRepository.UpdateUserRole(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Admin update user role successful by admin: {request.AdminUserId} for user: {request.UserId} to role: {request.Role}");

                return roleResponse;
            }
            catch (UnauthorizedAccessException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Admin update user role failed - insufficient privileges: {request?.AdminUserId}");
                throw;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Admin update user role failed - user not found: {request?.UserId} by admin: {request?.AdminUserId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Admin update user role error by admin {request?.AdminUserId} for user {request?.UserId}: {ex.Message}");
                throw new Exception("An error occurred while updating user role.", ex);
            }
        }

        /// <summary>
        /// Get all orders (Admin only)
        /// </summary>
        /// <param name="request">Get all orders request</param>
        /// <returns>Orders list with pagination and statistics</returns>
        public async Task<Model.Admin.GetAllOrdersResponse> GetAllOrders(Model.Admin.GetAllOrdersRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Admin get all orders attempt by admin: {request.AdminUserId}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.AdminUserId <= 0)
                    throw new ArgumentException("Valid Admin User ID is required");

                if (request.Page < 1)
                    request.Page = 1;

                if (request.Limit < 1 || request.Limit > 100)
                    request.Limit = 10;

                // Validate date range
                if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
                    throw new ArgumentException("Start date cannot be later than end date");

                // Validate status if provided
                if (!string.IsNullOrEmpty(request.Status))
                {
                    var validStatuses = new[] { "Pending", "Confirmed", "Processing", "Shipped", "Delivered", "Cancelled", "Returned", "Refunded" };
                    if (!validStatuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException($"Invalid status: {request.Status}. Valid statuses are: {string.Join(", ", validStatuses)}");
                    }
                }

                var ordersResponse = await this.productRepository.GetAllOrders(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Admin get all orders successful by admin: {request.AdminUserId}, found {ordersResponse.Orders.Count} orders");

                return ordersResponse;
            }
            catch (UnauthorizedAccessException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Admin get all orders failed - insufficient privileges: {request?.AdminUserId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Admin get all orders error for admin {request?.AdminUserId}: {ex.Message}");
                throw new Exception("An error occurred while retrieving orders.", ex);
            }
        }

        /// <summary>
        /// Add multiple images to a product
        /// </summary>
        /// <param name="request">Add product images request</param>
        /// <returns>List of added images</returns>
        public async Task<Model.Product.AddProductImagesResponse> AddProductImages(Model.Product.AddProductImagesRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Add product images attempt for product: {request.ProductId}, count: {request.Images?.Count ?? 0}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.ProductId <= 0)
                    throw new ArgumentException("Valid Product ID is required");

                if (request.Images == null || !request.Images.Any())
                    throw new ArgumentException("At least one image file is required");

                // Validate and process each image
                var imageDataList = new List<Model.Product.ImageUploadData>();
                int orderCounter = request.OrderBy;

                foreach (var file in request.Images)
                {
                    // Validate file
                    if (file == null || file.Length == 0)
                        throw new ArgumentException($"Invalid file uploaded");

                    // Validate file size
                    if (file.Length > 10 * 1024 * 1024) // 10MB
                        throw new ArgumentException($"File {file.FileName} exceeds size limit");

                    // Validate file type
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(file.ContentType.ToLower()))
                        throw new ArgumentException($"File {file.FileName} has invalid type. Allowed: JPEG, PNG, GIF, WebP");

                    // Read file data
                    byte[] imageData;
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        imageData = memoryStream.ToArray();
                    }

                    // Create thumbnail if image is large enough
                    byte[] thumbnailData = null;
                    try
                    {
                        thumbnailData = await CreateThumbnailAsync(imageData, 200, 200);
                    }
                    catch (Exception ex)
                    {
                        this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Failed to create thumbnail for {file.FileName}: {ex.Message}");
                    }

                    // Add to upload list
                    imageDataList.Add(new Model.Product.ImageUploadData
                    {
                        ImageName = file.FileName,
                        ContentType = file.ContentType,
                        FileSize = file.Length,
                        ImageData = Convert.ToBase64String(imageData),
                        ThumbnailData = thumbnailData != null ? Convert.ToBase64String(thumbnailData) : null,
                        IsMain = request.Main && imageDataList.Count == 0, // Only first image can be main if requested
                        OrderBy = orderCounter++
                    });
                }

                var imagesResponse = await this.productRepository.AddProductImages(request, imageDataList);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Add product images successful for product: {request.ProductId}, added: {imagesResponse.TotalAdded}");

                return imagesResponse;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Add product images error for product {request?.ProductId}: {ex.Message}");
                throw new Exception("An error occurred while adding product images.", ex);
            }
        }

        /// <summary>
        /// Update product image properties
        /// </summary>
        /// <param name="request">Update product image request</param>
        /// <returns>Updated image information</returns>
        public async Task<Model.Product.UpdateProductImageResponse> UpdateProductImage(Model.Product.UpdateProductImageRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Update product image attempt for product: {request.ProductId}, image: {request.ImageId}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.ProductId <= 0)
                    throw new ArgumentException("Valid Product ID is required");

                if (request.ImageId <= 0)
                    throw new ArgumentException("Valid Image ID is required");

                // Validate that at least one property is being updated
                if (!request.Main.HasValue && !request.Active.HasValue && !request.OrderBy.HasValue)
                    throw new ArgumentException("At least one property (main, active, or orderBy) must be specified for update");

                // Validate OrderBy if provided
                if (request.OrderBy.HasValue && request.OrderBy.Value < 0)
                    throw new ArgumentException("OrderBy must be a non-negative number");

                var imageResponse = await this.productRepository.UpdateProductImage(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Update product image successful for product: {request.ProductId}, image: {request.ImageId}");

                return imageResponse;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Update product image failed - image not found: {request?.ImageId} for product: {request?.ProductId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Update product image error for product {request?.ProductId}, image {request?.ImageId}: {ex.Message}");
                throw new Exception("An error occurred while updating the product image.", ex);
            }
        }

        /// <summary>
        /// Delete product image
        /// </summary>
        /// <param name="request">Delete product image request</param>
        /// <returns>Deletion confirmation and remaining images</returns>
        public async Task<Model.Product.DeleteProductImageResponse> DeleteProductImage(Model.Product.DeleteProductImageRequest request)
        {
            try
            {
                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Delete product image attempt for product: {request.ProductId}, image: {request.ImageId}, hard: {request.HardDelete}");

                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.ProductId <= 0)
                    throw new ArgumentException("Valid Product ID is required");

                if (request.ImageId <= 0)
                    throw new ArgumentException("Valid Image ID is required");

                var deleteResponse = await this.productRepository.DeleteProductImage(request);

                this._loggerFactory.CreateLogger<ProductService>().LogInformation($"Delete product image successful for product: {request.ProductId}, image: {request.ImageId}");

                return deleteResponse;
            }
            catch (KeyNotFoundException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Delete product image failed - image not found: {request?.ImageId} for product: {request?.ProductId}");
                throw;
            }
            catch (InvalidOperationException)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogWarning($"Delete product image failed - cannot delete last main image: {request?.ImageId} for product: {request?.ProductId}");
                throw;
            }
            catch (Exception ex)
            {
                this._loggerFactory.CreateLogger<ProductService>().LogError($"Delete product image error for product {request?.ProductId}, image {request?.ImageId}: {ex.Message}");
                throw new Exception("An error occurred while deleting the product image.", ex);
            }
        }

        #endregion
    }
}
