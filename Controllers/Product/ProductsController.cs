using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tenant.API.Base.Controller;
using Tenant.API.Base.Model;
using Tenant.Query.Model.Product;
using Tenant.Query.Model.ProductCart;
using Tenant.Query.Model.WishList;
using Tenant.Query.Service.Product;
using Exception = System.Exception;

namespace Tenant.Query.Controllers.Product
{
    [Route("api/1.0/products")]
    public class ProductsController : TnBaseController<Service.Product.ProductService>
    {
        #region Initialize the value
        Service.Product.ProductService productService;
        private const int MaxFileSize = 10 * 1024 * 1024; // 10MB
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configuration"></param>
        /// <param name="loggerFactory"></param>
        public ProductsController(ProductService service, IConfiguration configuration, ILoggerFactory loggerFactory) : base(service, configuration, loggerFactory)
        {
            this.productService = service;
        }

        #region New Endpoint 

        /// <summary>
        /// Upload image by productid
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("tenants/{tenantId}/upload")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
        public async Task<ActionResult<ImageResponseDto>> UploadImage([FromRoute] string tenantId, [FromForm] ImageUploadDto dto)
        {
            try
            {
                // Validation
                if (dto.File == null || dto.File.Length == 0)
                    return BadRequest("No file uploaded");

                if (dto.File.Length > MaxFileSize)
                    return BadRequest($"File size exceeds {MaxFileSize / (1024 * 1024)}MB limit");

                // Process image
                using var memoryStream = new MemoryStream();
                await dto.File.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                if (!productService.IsImageValid(imageData))
                    return BadRequest("Invalid image file");

                // Create thumbnail if requested
                byte[] thumbnailData = null;
                if (dto.GenerateThumbnail)
                {
                    try
                    {
                        thumbnailData = await productService.CreateThumbnailAsync(
                            imageData,
                            dto.ThumbnailWidth ?? 200,
                            dto.ThumbnailHeight ?? 200);
                    }
                    catch (Exception ex)
                    {

                    }
                }

                // Save to database
                var image = new ProductNewImage
                {
                    ProductId = dto.ProductId,
                    ImageName = Path.GetFileName(dto.File.FileName),
                    ContentType = productService.GetContentType(dto.File.FileName),
                    ImageData = imageData,
                    ThumbnailData = thumbnailData,
                    FileSize = (int)dto.File.Length,
                    CreatedAt = DateTime.UtcNow
                };

                long imageId = this.Service.AddImages(image);

                // Return response
                return Ok(new ImageResponseDto
                {
                    Id = imageId,
                    ImageName = image.ImageName,
                    ContentType = image.ContentType,
                    FileSize = image.FileSize,
                    CreatedAt = image.CreatedAt,
                    ImageUrl = Url.ActionLink("GetImage", "Products", new { id = imageId }),
                    ThumbnailUrl = thumbnailData != null
                        ? Url.ActionLink("GetThumbnail", "Products", new { id = imageId })
                        : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get image by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ResponseCache(Duration = 86400)] // Cache for 1 day
        public async Task<IActionResult> GetImage(long id)
        {
            try
            {
                var image = await productService.GetImageAsync(id);

                if (image == null)
                    return NotFound();

                return File(image.ImageData, image.ContentType, image.ImageName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get product thumline image
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}/thumbnail")]
        [ResponseCache(Duration = 86400)]
        public async Task<IActionResult> GetThumbnail(long id)
        {
            try
            {
                var image = await productService.GetImageAsync(id); // Ensure GetImage is async

                if (image == null)
                    return NotFound();

                // Fallback to full image if thumbnail doesn't exist
                var data = image.ThumbnailData ?? image.ImageData;
                return File(data, image.ContentType, $"thumb_{image.ImageName}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get product list
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        [HttpPost]
        [Route("tenants/{tenantId}/get-product-list")]
        public IActionResult GetProductList([FromRoute] string tenantId, [FromBody] Model.Product.ProductPayload payload)
        {
            try
            {
                List<Model.Product.ProductItemList> productItemList = this.Service.GetProductList(tenantId, payload);


                    // Updated usage in the existing code
                    productItemList = productItemList ?? new List<ProductItemList>();
                    MapProductImages(productItemList);

                return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = productItemList });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = ex.Message });
            }
        }

        /// <summary>
        /// Generate image url
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public ImageResponseDto GetImages(ImageResponseDto image)
        {
            return new ImageResponseDto
            {
                Id = image.Id,
                ImageName = image.ImageName,
                ContentType = image.ContentType,
                FileSize = image.FileSize,
                CreatedAt = image.CreatedAt,
                ImageUrl = Url.ActionLink("GetImage", "Products", new { id = image.Id }),
                ThumbnailUrl = Url.ActionLink("GetThumbnail", "Products", new { id = image.Id })
            };
        }

        //// DELETE api/images/{id}
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteImage(long id)
        //{
        //    try
        //    {
        //        var image = await productService.DeleteImage(id);
        //        if (image == null)
        //            return NotFound();

        //        return NoContent();
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        #endregion

        #region Crude endpoint
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Not Found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("tenants/{tenantId:long}/add-product")]
        public async Task<IActionResult> AddProduct([FromRoute] long tenantId, [FromBody] Model.Product.Product product)
        {
            try
            {
                var result = await this.Service.AddProduct(tenantId, product);
                return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResult { Exception = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
            finally
            {
            }
        }

        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Not Found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("tenants/{tenantId:long}/add-product_category")]
        public IActionResult AddProductCategory([FromRoute] long tenantId, [FromBody] Model.Product.ProductCategoryPayload payload)
        {
            try
            {
                var result = this.Service.AddProductCategory(tenantId, payload);
                return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResult { Exception = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
            finally
            {
            }
        }

        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Not Found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("tenants/{tenantId:long}/add-to-cart")]
        public async Task<IActionResult> UpsertCart(long tenantId, [FromBody] CartPayload payload)
        {
            try
            {
                ProductCartResponse result = await productService.UpsertCart(tenantId, payload);

                result.images = result.images.Select(GetImages).ToList();

                return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResult { Exception = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
            finally
            {
            }
        }

        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Not Found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpGet]
        [Route("tenants/{userId:long}/get-user-cart")]
        public IActionResult GeUserCart(long userId)
        {
            try
            {
                List<ProductCartResponse> productCartList = productService.GetUserCart(userId);

                productCartList = productCartList ?? new List<ProductCartResponse>();
                MapProductCartImages(productCartList);

                return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = productCartList });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResult { Exception = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
            finally
            {
            }
        }

        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Not Found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("tenants/{tenantId:long}/add-to-wishlist")]
        public async Task<IActionResult> UpsertWishList(long tenantId, [FromBody] WishListPayload payload)
        {
            try
            {
                ProductWishListResponse result = await productService.UpsertWishList(tenantId, payload);

                result.images = result.images.Select(GetImages).ToList();

                return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResult { Exception = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
            finally
            {
            }
        }



        #endregion

        #region Examples

        ///// <summary>
        ///// Get list of product for the tenant and vendor
        ///// </summary>
        ///// <param name="tenantId"></param>
        ///// <param name="vendorId"></param>
        ///// <returns></returns>
        //[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(List<Model.Product.ProductMaster>))]
        //[SwaggerResponse(StatusCodes.Status500InternalServerError)]
        //[Route("tenants/{tenantId}")]
        //[HttpGet]
        //public IActionResult GetProducts([FromRoute] string tenantId, [FromBody] Search search)
        //{
        //    try
        //    {
        //        //get list of products
        //        List<Model.Product.ProductMaster> products = this.Service.GetProducts(tenantId, search);

        //        //return data
        //        return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = products });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return StatusCode(StatusCodes.Status404NotFound, new ApiResult() { Exception = ex.Message });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        API.Base.Model.Exception modelException = new API.Base.Model.Exception(ex.Message);

        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = modelException });
        //    }
        //}

        //[HttpGet]
        //[Route("tenants/{tenantId}/products")]
        //[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Model.Product.ProductMaster))]
        //[SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(ApiResult))]
        //[SwaggerResponse(StatusCodes.Status404NotFound, "Resource not found", typeof(ApiResult))]
        //[SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error", typeof(ApiResult))]
        //public IActionResult GetProducts([FromRoute] string tenantId,
        //                                 [FromQuery(Name = "include-inactive")] bool includeInactive,
        //                                 [FromQuery(Name = "include-category")] bool includeCategory,
        //                                 [FromQuery(Name = "include-images")] bool includeImages,
        //                                 [FromQuery(Name = "include-reviews")] bool includeReviews)
        //{
        //    try
        //    {
        //        //TODO set Query parameter to the filter
        //        Model.Product.PorductFilter filter = new Model.Product.PorductFilter()
        //        {
        //            IncludeInactive = includeInactive,
        //            IncludeCategory = includeCategory,
        //            IncludeImages = includeImages,
        //            IncludeReviews = includeReviews
        //        };

        //        //Local variable
        //        List<Model.Product.ProductMaster> productMaster = new List<Model.Product.ProductMaster>();

        //        //calling service
        //        productMaster = this.Service.GetProductMaster(tenantId, filter);

        //        // Return productMaster
        //        return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = productMaster });
        //    }
        //    // key not found exeception
        //    catch (KeyNotFoundException ex)
        //    {
        //        return StatusCode(StatusCodes.Status404NotFound, new ApiResult() { Exception = ex.Message });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = ex.Message });
        //    }
        //}

        [HttpGet]
        [Route("tenants/{tenantId}/category")]
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Model.Product.ProductCategory))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Resource not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error", typeof(ApiResult))]
        public IActionResult GetCategory([FromRoute] string tenantId)
        {
            try
            {
                //Local variable
                List<Model.Response.Category> productCategories = new List<Model.Response.Category>();

                //calling service
                productCategories = this.Service.GetCategory(tenantId);

                // Return productMaster
                return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = productCategories });
            }
            // key not found exeception
            catch (KeyNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ApiResult() { Exception = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = ex.Message });
            }
        }

        [HttpPost]
        [Route("tenants/{tenantId}/add-category")]
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Model.Product.ProductCategory))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Resource not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error", typeof(ApiResult))]
        public IActionResult AddCategory([FromRoute] string tenantId, [FromBody] Model.Response.CatrtegoryPayload catrtegoryPayload)
        {
            try
            {
                //calling service
                long categoryId = this.Service.AddCategory(Convert.ToInt64(tenantId), catrtegoryPayload);

                // Return productMaster
                return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = categoryId });
            }
            // key not found exeception
            catch (KeyNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ApiResult() { Exception = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = ex.Message });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productItemList"></param>
        private void MapProductImages(List<ProductItemList> productItemList)
        {
            foreach (var item in productItemList)
            {
                item.images = item.images.Select(GetImages).ToList();
            }
        }

        private void MapProductCartImages(List<ProductCartResponse> productItemList)
        {
            foreach (var item in productItemList)
            {
                item.images = item.images.Select(GetImages).ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("tenants/{tenantId}/menu-master")]
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Model.Product.ProductCategory))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Resource not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error", typeof(ApiResult))]
        public IActionResult GetMenuMaster([FromRoute] string tenantId)
        {
            try
            {
                //calling service
                List < Model.Response.Category > menuMasters = this.Service.GetMenuMaster(tenantId);

                // Return productMaster
                return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = menuMasters });
            }
            // key not found exeception
            catch (KeyNotFoundException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ApiResult() { Exception = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = ex.Message });
            }
        }

        /// <summary>
        /// Remove product from cart
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Not Found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpDelete]
        [Route("tenants/{tenantId:long}/remove-from-cart")]
        public IActionResult RemoveProductFromCart([FromRoute] long tenantId, [FromBody] RemoveCartPayLoad payload)
        {
            try
            {
                var result = productService.RemoveProductCart(tenantId.ToString(), payload);

                if (result <= 0)
                    return NotFound(new ApiResult { Exception = "Product not found in cart" });

                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResult { Exception = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
        }

        /// <summary>
        /// Remove product from wishlist
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Not Found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpDelete]
        [Route("tenants/{tenantId:long}/remove-from-wishlist")]
        public IActionResult RemoveProductFromWishList([FromRoute] long tenantId, [FromBody] RemoveWhishListPayload payload)
        {
            try
            {
                var result = productService.RemoveProductWishList(tenantId.ToString(), payload);

                if (result <= 0)
                    return NotFound(new ApiResult { Exception = "Product not found in wishlist" });

                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResult { Exception = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
        }




        //[HttpGet]
        //[Route("tenants/{tenantId}/product-master")]
        //[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Model.Product.ProductMaster))]
        //[SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(ApiResult))]
        //[SwaggerResponse(StatusCodes.Status404NotFound, "Resource not found", typeof(ApiResult))]
        //[SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error", typeof(ApiResult))]
        //public IActionResult GetProductMaster([FromRoute] string tenantId)
        //{
        //    try
        //    {
        //        //Local variable
        //        List<Model.Response.ProductMaster.ProductMaster> productMasters = new List<Model.Response.ProductMaster.ProductMaster>();

        //        //calling service
        //        productMasters = this.Service.GetProductMaster();

        //        // Return productMaster
        //        return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = productMasters });
        //    }
        //    // key not found exeception
        //    catch (KeyNotFoundException ex)
        //    {
        //        return StatusCode(StatusCodes.Status404NotFound, new ApiResult() { Exception = ex.Message });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = ex.Message });
        //    }
        //}

        //[HttpGet]
        //[Route("{productId}/product-details")]
        //[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Model.Product.ProductCategory))]
        //[SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(ApiResult))]
        //[SwaggerResponse(StatusCodes.Status404NotFound, "Resource not found", typeof(ApiResult))]
        //[SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error", typeof(ApiResult))]
        //public IActionResult GetProductDetails([FromRoute] string productId)
        //{
        //    try
        //    {
        //        //Local variable
        //        Model.Response.ProductMaster.ProductMaster productDetails = new Model.Response.ProductMaster.ProductMaster();

        //        //calling service
        //        productDetails = this.Service.GetProductDetails(productId);

        //        // Return productMaster
        //        return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = productDetails });
        //    }
        //    // key not found exeception
        //    catch (KeyNotFoundException ex)
        //    {
        //        return StatusCode(StatusCodes.Status404NotFound, new ApiResult() { Exception = ex.Message });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = ex.Message });
        //    }
        //}

        //[HttpGet]
        //[Route("tenants/{tenantId}/cart-list")]
        //[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(Model.Product.ProductCategory))]
        //[SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(ApiResult))]
        //[SwaggerResponse(StatusCodes.Status404NotFound, "Resource not found", typeof(ApiResult))]
        //[SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal server error", typeof(ApiResult))]
        //public IActionResult GetCartProduct([FromRoute] string tenantId)
        //{
        //    try
        //    {
        //        //Local variable
        //        List<Model.Response.CartList> productMasters = new List<Model.Response.CartList>();

        //        //calling service
        //        productMasters = this.Service.GetCartProduct();

        //        // Return productMaster
        //        return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = productMasters });
        //    }
        //    // key not found exeception
        //    catch (KeyNotFoundException ex)
        //    {
        //        return StatusCode(StatusCodes.Status404NotFound, new ApiResult() { Exception = ex.Message });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = ex.Message });
        //    }
        //}

        //[SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        //[SwaggerResponse(StatusCodes.Status404NotFound, "Success", typeof(ApiResult))]
        //[SwaggerResponse(StatusCodes.Status500InternalServerError)]
        //[HttpPost]
        //[Route("tenants/{tenantId}/get-invoice-items")]
        //public IActionResult GetInvoiceItemsForMapping([FromRoute] string tenantId, [FromBody] Model.Product.InvoiceItemMappingPayload payload)
        //{
        //    try
        //    {
        //        //this.Logger.LogInformation($"Get invoice item(s) of tenantId : {tenantId} for mapping ");

        //        Model.Product.Response responses = this.Service.GetInvoiceItemsForMapping(tenantId, payload);

        //        return StatusCode(StatusCodes.Status200OK, new ApiResult() { Data = responses });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult() { Exception = ex.Message });
        //    }
        //}
        #endregion
    }
}
