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

        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Route("tenants/{tenantId}")]
        public IActionResult GetValueByKey([FromRoute] string tenantId)
        {
            try
            {
                string key = "MAX_ITEMS_IN_CART"; // Example key, this could be a parameter
                string productItemList = this.Service.GetValueByKey(key);

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
        [AllowAnonymous]
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



        /// <summary>
        /// Search products with advanced filtering and pagination
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [AllowAnonymous]
        [HttpPost]
        [Route("tenants/{tenantId}/search-products")]
        public IActionResult SearchProducts([FromRoute] string tenantId, [FromBody] ProductSearchPayload payload)
        {
            try
            {
                if (payload == null)
                {
                    return BadRequest(new ApiResult { Exception = "Payload cannot be null" });
                }

                // Validate pagination parameters
                if (payload.Page < 1)
                {
                    payload.Page = 1;
                }

                if (payload.Limit < 1 || payload.Limit > 100)
                {
                    payload.Limit = 10;
                }

                // Validate sort parameters
                var validSortFields = new[] { "productName", "price", "rating", "userBuyCount", "created" };
                if (!validSortFields.Contains(payload.SortBy.ToLower()))
                {
                    payload.SortBy = "created";
                }

                var validSortOrders = new[] { "asc", "desc" };
                if (!validSortOrders.Contains(payload.SortOrder.ToLower()))
                {
                    payload.SortOrder = "desc";
                }

                var result = this.Service.SearchProducts(tenantId, payload);

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
        /// Get product details by ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Product details with images</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Product not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [AllowAnonymous]
        [HttpGet]
        [Route("{productId:long}")]
        public async Task<IActionResult> GetProductById([FromRoute] long productId)
        {
            try
            {
                var result = await this.Service.GetProductById(productId);

                if (result == null)
                {
                    return NotFound(new ApiResult { Exception = "Product not found" });
                }

                // Map image URLs
                if (result.Images != null && result.Images.Any())
                {
                    // result.Images = result.Images.Select(GetImages).ToList(); 
                }

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
        /// Add a new product
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Product details</param>
        /// <returns>Product ID</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("tenants/{tenantId:long}/add-product")]
        public async Task<IActionResult> AddProduct([FromRoute] long tenantId, [FromBody] AddProductRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResult { Exception = "Invalid request model" });
                }

                var productId = await this.Service.AddProduct(tenantId, request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = "Product added successfully" });
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
        /// Update an existing product
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Product details</param>
        /// <returns>Success message</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [AllowAnonymous]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Product not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("tenants/{tenantId:long}/update-product")]
        public async Task<IActionResult> UpdateProduct([FromRoute] long tenantId, [FromBody] UpdateProductRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResult { Exception = "Invalid request model" });
                }

                var productId = await this.Service.UpdateProduct(tenantId, request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = "Product updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResult { Exception = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="productId">Product ID</param>
        /// <returns>Success message</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Product not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpDelete]
        [Route("tenants/{tenantId:long}/{productId:long}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] long tenantId, [FromRoute] long productId)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new ApiResult { Exception = "Invalid product ID" });
                }

                await this.Service.DeleteProduct(tenantId, productId);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = "Product deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <param name="tenantId">Optional tenant ID filter</param>
        /// <returns>List of categories</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpGet]
        [Route("categories")]
        public IActionResult GetAllCategories([FromQuery] long? tenantId = null)
        {
            try
            {
                var categories = this.Service.GetAllCategories(tenantId);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = categories });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
        }

        /// <summary>
        /// Add a new category
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Category details</param>
        /// <returns>Newly created category</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("tenantId/{tenantId:long}/add-category")]
        public async Task<IActionResult> AddCategory([FromRoute] long tenantId, [FromBody] AddCategoryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResult { Exception = "Invalid request model" });
                }

                var result = await this.Service.AddCategory(tenantId, request);
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
        /// Update an existing category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Category details</param>
        /// <returns>Success message</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Category not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPut]
        [Route("tenantId/{tenantId:long}/update-category/{categoryId:long}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] long categoryId, [FromRoute] long tenantId,
            [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (categoryId != request.CategoryId)
                {
                    return BadRequest(new ApiResult { Exception = "Category ID in route does not match request body" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResult { Exception = "Invalid request model" });
                }

                await this.Service.UpdateCategory(tenantId, request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = "Category updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResult { Exception = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
        }

        /// <summary>
        /// Get menu master with categories
        /// </summary>
        /// <param name="tenantId">Optional tenant ID filter</param>
        /// <returns>Menu master with associated categories</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [AllowAnonymous]
        [HttpGet]
        [Route("menu/master")]
        public IActionResult GetMenuMaster([FromQuery] long? tenantId = null)
        {
            try
            {
                var menuMaster = this.Service.GetMenuMaster(tenantId);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = menuMaster });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResult { Exception = ex.Message });
            }
        }

        #endregion

        #region Examples

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
                if (item.images != null)
                {
                    item.images = item.images.Select(GetImages).ToList();
                }
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

        /// <summary>
        /// Get user's shopping cart with full product details
        /// </summary>
        /// <param name="request">Cart request with user details</param>
        /// <returns>Cart items with complete product information</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [AllowAnonymous]
        [Route("cart")]
        public async Task<IActionResult> GetCart([FromBody] Model.ProductCart.GetCartRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                var cartResponse = await this.Service.GetUserCart(request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = cartResponse });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
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
        /// Add item to cart
        /// </summary>
        /// <param name="request">Add to cart request details</param>
        /// <returns>Cart item details and summary</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]        
        [SwaggerResponse(StatusCodes.Status404NotFound, "Product or user not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Insufficient stock", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("tenantId/{tenantId:long}/add-cart")]
        public async Task<IActionResult> AddItemToCart([FromRoute] long tenantId, [FromBody] Model.ProductCart.AddToCartRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Extract IP address and User-Agent from request headers if not provided
                if (string.IsNullOrEmpty(request.IpAddress))
                {
                    request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (string.IsNullOrEmpty(request.UserAgent))
                {
                    request.UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                }

                var result = await this.Service.AddItemToCart(tenantId,request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Insufficient stock or other business rule violations
                return StatusCode(StatusCodes.Status409Conflict, new ApiResult { Exception = ex.Message });
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
        /// Remove item from cart
        /// </summary>
        /// <param name="request">Remove from cart request details</param>
        /// <returns>Removal confirmation and updated cart summary</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Product not found in cart", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpDelete]
        [Route("tenantId/{tenantId:long}/cart/remove-item")]
        public async Task<IActionResult> RemoveItemFromCart([FromRoute] long tenantId, [FromBody] Model.ProductCart.RemoveFromCartRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Extract IP address and User-Agent from request headers if not provided
                if (string.IsNullOrEmpty(request.IpAddress))
                {
                    request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (string.IsNullOrEmpty(request.UserAgent))
                {
                    request.UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                }

                var result = await this.Service.RemoveItemFromCart(tenantId, request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
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
        /// Clear entire cart
        /// </summary>
        /// <param name="request">Clear cart request details</param>
        /// <returns>Cart clearing confirmation and statistics</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found or cart is empty", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpDelete]
        [Route("tenantId/{tenantId:long}/cart/clear")]
        public async Task<IActionResult> ClearCart([FromRoute] long tenantId, [FromBody] Model.ProductCart.ClearCartRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Extract IP address and User-Agent from request headers if not provided
                if (string.IsNullOrEmpty(request.IpAddress))
                {
                    request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (string.IsNullOrEmpty(request.UserAgent))
                {
                    request.UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                }

                var result = await this.Service.ClearCart(tenantId, request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
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
        /// Create a new order from cart items or direct order
        /// </summary>
        /// <param name="request">Create order request details</param>
        /// <returns>Order creation confirmation and details</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Insufficient stock", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("create-orders")]
        public async Task<IActionResult> CreateOrder([FromBody] Model.Order.CreateOrderRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Validate order items exist
                if (request.Items == null || !request.Items.Any())
                {
                    return BadRequest(new ApiResult { Exception = "Order must contain at least one item" });
                }

                // Validate totals calculation
                var calculatedSubtotal = request.Items.Sum(item => item.Total);
                if (Math.Abs(calculatedSubtotal - request.Totals.Subtotal) > 0.01m)
                {
                    return BadRequest(new ApiResult { Exception = "Subtotal calculation mismatch" });
                }

                var calculatedTotal = request.Totals.Subtotal + request.Totals.Shipping + request.Totals.Tax - request.Totals.Discount;
                if (Math.Abs(calculatedTotal - request.Totals.Total) > 0.01m)
                {
                    return BadRequest(new ApiResult { Exception = "Total calculation mismatch" });
                }

                // Extract IP address and User-Agent from request headers if not provided
                if (string.IsNullOrEmpty(request.IpAddress))
                {
                    request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (string.IsNullOrEmpty(request.UserAgent))
                {
                    request.UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                }

                var result = await this.Service.CreateOrder(request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Insufficient stock or other business rule violations
                return StatusCode(StatusCodes.Status409Conflict, new ApiResult { Exception = ex.Message });
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
        /// Get user orders with pagination and filtering
        /// </summary>
        /// <param name="request">Get orders request with pagination and filters</param>
        /// <returns>List of orders with pagination information</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("get-orders")]
        public async Task<IActionResult> GetOrders([FromBody] Model.Order.GetOrdersRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Validate pagination parameters
                if (request.Page < 1)
                {
                    request.Page = 1;
                }

                if (request.Limit < 1 || request.Limit > 100)
                {
                    request.Limit = 10;
                }

                var result = await this.Service.GetOrders(request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
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
        /// Get order details by order ID
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="tenantId">Tenant ID (optional)</param>
        /// <returns>Detailed order information</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Order not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpGet]
        [Route("orders/{orderId:long}")]
        public async Task<IActionResult> GetOrderById([FromRoute] long orderId, [FromQuery] long userId, [FromQuery] long? tenantId = null)
        {
            try
            {
                if (orderId <= 0)
                {
                    return BadRequest(new ApiResult { Exception = "Valid Order ID is required" });
                }

                if (userId <= 0)
                {
                    return BadRequest(new ApiResult { Exception = "Valid User ID is required" });
                }

                var request = new Model.Order.GetOrderByIdRequest
                {
                    OrderId = orderId,
                    UserId = userId,
                    TenantId = tenantId
                };

                var result = await this.Service.GetOrderById(request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
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
        /// Cancel an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="request">Cancel order request details</param>
        /// <returns>Order cancellation confirmation</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Order not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Order cannot be cancelled", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPut]
        [Route("orders/{orderId:long}/cancel")]
        public async Task<IActionResult> CancelOrder([FromRoute] long orderId, [FromBody] Model.Order.CancelOrderRequest request = null)
        {
            try
            {
                if (orderId <= 0)
                {
                    return BadRequest(new ApiResult { Exception = "Valid Order ID is required" });
                }

                // If request is null, create a minimal request with just the order ID
                if (request == null)
                {
                    request = new Model.Order.CancelOrderRequest();
                }

                request.OrderId = orderId; // Ensure order ID matches route parameter

                // UserId is required - if not provided in body, it should be extracted from auth context
                // For this example, we'll require it in the request body or query parameter
                if (request.UserId <= 0)
                {
                    // Try to get from query parameter as fallback
                    if (HttpContext.Request.Query.TryGetValue("userId", out var userIdValue) && 
                        long.TryParse(userIdValue, out var userId))
                    {
                        request.UserId = userId;
                    }
                    else
                    {
                        return BadRequest(new ApiResult { Exception = "Valid User ID is required" });
                    }
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Extract IP address and User-Agent from request headers if not provided
                if (string.IsNullOrEmpty(request.IpAddress))
                {
                    request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (string.IsNullOrEmpty(request.UserAgent))
                {
                    request.UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                }

                var result = await this.Service.CancelOrder(request);
                
                // Return simple response format as requested
                var simpleResponse = new Model.Order.SimpleOperationResponse
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result
                };

                return StatusCode(StatusCodes.Status200OK, simpleResponse);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Order cannot be cancelled in current status
                return StatusCode(StatusCodes.Status409Conflict, new ApiResult { Exception = ex.Message });
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
        /// Update order status
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="request">Update order status request details</param>
        /// <returns>Order status update confirmation</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Order not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Invalid status transition", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPut]
        [Route("orders/{orderId:long}/status")]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] long orderId, [FromBody] Model.Order.UpdateOrderStatusRequest request)
        {
            try
            {
                if (orderId <= 0)
                {
                    return BadRequest(new ApiResult { Exception = "Valid Order ID is required" });
                }

                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                request.OrderId = orderId; // Ensure order ID matches route parameter

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Extract IP address and User-Agent from request headers if not provided
                if (string.IsNullOrEmpty(request.IpAddress))
                {
                    request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (string.IsNullOrEmpty(request.UserAgent))
                {
                    request.UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                }

                var result = await this.Service.UpdateOrderStatus(request);
                
                // Return simple response format as requested
                var simpleResponse = new Model.Order.SimpleOperationResponse
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result
                };

                return StatusCode(StatusCodes.Status200OK, simpleResponse);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Invalid status transition
                return StatusCode(StatusCodes.Status409Conflict, new ApiResult { Exception = ex.Message });
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
        /// Get all users (Admin only)
        /// </summary>
        /// <param name="request">Get all users request with pagination and filters</param>
        /// <returns>List of users with detailed information</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Insufficient privileges", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("admin/users")]
        public async Task<IActionResult> GetAllUsers([FromBody] Model.Admin.GetAllUsersRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Validate pagination parameters
                if (request.Page < 1)
                {
                    request.Page = 1;
                }

                if (request.Limit < 1 || request.Limit > 100)
                {
                    request.Limit = 10;
                }

                var result = await this.Service.GetAllUsers(request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResult { Exception = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
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
        /// Update user role (Admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="request">Update user role request details</param>
        /// <returns>Role update confirmation</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Insufficient privileges", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("admin/users/{userId:long}/role")]
        public async Task<IActionResult> UpdateUserRole([FromRoute] long userId, [FromBody] Model.Admin.UpdateUserRoleRequest request)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new ApiResult { Exception = "Valid User ID is required" });
                }

                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                request.UserId = userId; // Ensure user ID matches route parameter

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Extract IP address and User-Agent from request headers if not provided
                if (string.IsNullOrEmpty(request.IpAddress))
                {
                    request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (string.IsNullOrEmpty(request.UserAgent))
                {
                    request.UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                }

                var result = await this.Service.UpdateUserRole(request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResult { Exception = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
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
        /// Get all orders (Admin only)
        /// </summary>
        /// <param name="request">Get all orders request with pagination and filters</param>
        /// <returns>List of orders with detailed information and statistics</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Insufficient privileges", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("admin/orders")]
        public async Task<IActionResult> GetAllOrders([FromBody] Model.Admin.GetAllOrdersRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Validate pagination parameters
                if (request.Page < 1)
                {
                    request.Page = 1;
                }

                if (request.Limit < 1 || request.Limit > 100)
                {
                    request.Limit = 10;
                }

                // Validate date range
                if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
                {
                    return BadRequest(new ApiResult { Exception = "Start date cannot be later than end date" });
                }

                var result = await this.Service.GetAllOrders(request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResult { Exception = ex.Message });
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
        /// Add multiple images to a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="request">Add product images request with multipart form data</param>
        /// <returns>List of added images</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Product not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status413PayloadTooLarge, "File too large", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPost]
        [Route("products/{productId:long}/images")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize * 10)] // Allow multiple files
        public async Task<IActionResult> AddProductImages([FromRoute] long productId, 
            [FromForm] Model.Product.AddProductImagesRequest request)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new ApiResult { Exception = "Valid Product ID is required" });
                }

                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                request.ProductId = productId; // Ensure product ID matches route parameter

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Validate files
                if (request.Images == null || !request.Images.Any())
                {
                    return BadRequest(new ApiResult { Exception = "At least one image file is required" });
                }

                // Validate each file
                foreach (var file in request.Images)
                {
                    if (file == null || file.Length == 0)
                    {
                        return BadRequest(new ApiResult { Exception = "Invalid file uploaded" });
                    }

                    if (file.Length > MaxFileSize)
                    {
                        return BadRequest(new ApiResult { Exception = $"File {file.FileName} exceeds {MaxFileSize / (1024 * 1024)}MB limit" });
                    }

                    // Validate file type
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(file.ContentType.ToLower()))
                    {
                        return BadRequest(new ApiResult { Exception = $"File {file.FileName} has invalid type. Allowed: JPEG, PNG, GIF, WebP" });
                    }
                }

                // Extract IP address and User-Agent from request headers if not provided
                if (string.IsNullOrEmpty(request.IpAddress))
                {
                    request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (string.IsNullOrEmpty(request.UserAgent))
                {
                    request.UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                }

                var result = await this.Service.AddProductImages(request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
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
        /// Update product image properties
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="imageId">Image ID</param>
        /// <param name="request">Update product image request details</param>
        /// <returns>Updated image information</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Product or image not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpPut]
        [Route("products/{productId:long}/images/{imageId:long}")]
        public async Task<IActionResult> UpdateProductImage([FromRoute] long productId, [FromRoute] long imageId, [FromBody] Model.Product.UpdateProductImageRequest request)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new ApiResult { Exception = "Valid Product ID is required" });
                }

                if (imageId <= 0)
                {
                    return BadRequest(new ApiResult { Exception = "Valid Image ID is required" });
                }

                if (request == null)
                {
                    return BadRequest(new ApiResult { Exception = "Request cannot be null" });
                }

                request.ProductId = productId; // Ensure product ID matches route parameter
                request.ImageId = imageId; // Ensure image ID matches route parameter

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResult { Exception = string.Join("; ", errors) });
                }

                // Validate that at least one property is being updated
                if (!request.Main.HasValue && !request.Active.HasValue && !request.OrderBy.HasValue)
                {
                    return BadRequest(new ApiResult { Exception = "At least one property (main, active, or orderBy) must be specified for update" });
                }

                // Extract IP address and User-Agent from request headers if not provided
                if (string.IsNullOrEmpty(request.IpAddress))
                {
                    request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (string.IsNullOrEmpty(request.UserAgent))
                {
                    request.UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                }

                var result = await this.Service.UpdateProductImage(request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
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
        /// Delete product image
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="imageId">Image ID</param>
        /// <param name="hardDelete">Whether to perform hard delete (permanent) or soft delete</param>
        /// <param name="userId">User ID for activity logging</param>
        /// <returns>Deletion confirmation and remaining images</returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Success", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad Request", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Product or image not found", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Cannot delete last main image", typeof(ApiResult))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", typeof(ApiResult))]
        [HttpDelete]
        [Route("products/{productId:long}/images/{imageId:long}")]
        public async Task<IActionResult> DeleteProductImage([FromRoute] long productId, [FromRoute] long imageId, [FromQuery] bool hardDelete = false, [FromQuery] long? userId = null)
        {
            try
            {
                if (productId <= 0)
                {
                    return BadRequest(new ApiResult { Exception = "Valid Product ID is required" });
                }

                if (imageId <= 0)
                {
                    return BadRequest(new ApiResult { Exception = "Valid Image ID is required" });
                }

                var request = new Model.Product.DeleteProductImageRequest
                {
                    ProductId = productId,
                    ImageId = imageId,
                    UserId = userId,
                    HardDelete = hardDelete
                };

                // Extract IP address and User-Agent from request headers
                request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                request.UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();

                var result = await this.Service.DeleteProductImage(request);
                return StatusCode(StatusCodes.Status200OK, new ApiResult { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResult { Exception = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Cannot delete last main image
                return StatusCode(StatusCodes.Status409Conflict, new ApiResult { Exception = ex.Message });
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
        #endregion
    }
}
