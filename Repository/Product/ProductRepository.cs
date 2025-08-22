using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tenant.API.Base.Repository;
using Tenant.Query.Context.Product;
using Tenant.Query.Model.Product;
using Microsoft.Extensions.Configuration;
using Sa.Common.ADO.DataAccess;
using Microsoft.CodeAnalysis;
using Tenant.Query.Uitility;
using SixLabors.ImageSharp;
using Tenant.Query.Model.WishList;
using Tenant.Query.Model.ProductCart;

namespace Tenant.Query.Repository.Product
{
    public class ProductRepository : TnBaseQueryRepository<Model.Product.ProductMaster, Context.Product.ProductContext>
    {

        #region Variable
        DataAccess _dataAccess;
        private readonly Sa.Common.ADO.DataAccess.DataAccess _dataAccessCmd;
        private string dbConnectionString = string.Empty;
        #endregion

        public ProductRepository(ProductContext dbContext,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            DataAccess dataAccess) : base(dbContext, loggerFactory)
        {
            dbConnectionString = this.DbContext.Database.GetDbConnection().ConnectionString;
            _dataAccess = dataAccess;
            _dataAccessCmd = dataAccess;
        }

        #region Get End Point
        /// <summary>
        /// s               
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="spName"></param>
        /// <returns></returns>
        public List<Model.Product.ProductCategory> GetMenuMaster(string tenantId, string spName)
        {
            try
            {
                //Executing query
                List<Model.Product.ProductCategory> productCategories = _dataAccess.ExecuteGenericList<Model.Product.ProductCategory>(spName, Convert.ToInt64(tenantId));

                return productCategories;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        
        #endregion 

        #region Example methods
        public override Task<ProductMaster> GetById(string tenantId, string id)
        {
            throw new NotImplementedException();
        }

        public DataTable VendorLookupById(Int64 tenantVendorId, Int64 tenantID)
        {
            try
            {
                return _dataAccess.ExecuteDataTable(Model.Constant.Constant.StoredProcedures.SA_REALTIMEOCR_GET_TENANT_VENDOR_DETAIL, tenantVendorId, tenantID);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Remove cart
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="orgImageName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public long RemoveProductCart(string tenantId,RemoveCartPayLoad removeCartPayLoad)
        {
            try
            {
                var cmd = _dataAccess.ExecuteNonQueryCMD(Model.Constant.Constant.StoredProcedures.SP_REMOVE_CART,
                    removeCartPayLoad.UserId,
                    removeCartPayLoad.ProductId);
                return Convert.ToInt64(cmd.Parameters["@RETURN_VALUE"].Value.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Remove cart
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="orgImageName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public long RemoveProductWishList(string tenantId, RemoveWhishListPayload removeWhishListPayload)
        {
            try
            {
                var cmd = _dataAccess.ExecuteNonQueryCMD(Model.Constant.Constant.StoredProcedures.SP_REMOVE_WISHLIST,
                    removeWhishListPayload.UserId,
                    removeWhishListPayload.ProductId);
                return Convert.ToInt64(cmd.Parameters["@RETURN_VALUE"].Value.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public long InsertImage(string imageName, string orgImageName, long userId)
        {
            try
            {
                var cmd = _dataAccess.ExecuteNonQueryCMD(Model.Constant.Constant.StoredProcedures.SP_ADD_IMAGES, imageName, orgImageName, userId);
                return Convert.ToInt64(cmd.Parameters["@RETURN_VALUE"].Value.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Add Product
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        public async Task<long> AddProduct(long tenantId, Model.Product.Product product)
        {
            try
            {
                var productId = await Task.Run(() =>
                    _dataAccess.ExecuteDataTable(Model.Constant.Constant.StoredProcedures.SA_REALTIMEOCR_ADD_INVOICE,
                    tenantId, 
                    product.ProductName,
                    product.Displayname,
                    product.Rating,
                    product.Total,
                    product.Price,
                    product.Tax,
                    product.Stock,
                    product.Description,
                    product.Quantity,
                    product.MinimunQuantity,
                    product.Numofreviews,
                    product.Active,
                    1,
                    1,
                    product.BestSeller,
                    product.VendorId));

                if (productId.Rows.Count > 0)
                {
                    return Convert.ToInt64(productId.Rows[0]["PRODUCT_ID"]);
                }

                return 0;
            }
            catch (Exception ex)
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
        public long AddProductCategory(long tenantId,Model.Product.ProductCategoryPayload productCategory)
        {
            try
            {
                DataTable dtProductCategory = new DataTable();
                dtProductCategory.Columns.Add("PRODUCTID", typeof(long));
                dtProductCategory.Columns.Add("CATEGORYID", typeof(long));
                foreach (var CategoryId in productCategory.CategoryId)
                {
                    DataRow drdtProductCategoryRow = dtProductCategory.NewRow();
                    drdtProductCategoryRow["PRODUCTID"] = productCategory.ProductId;
                    drdtProductCategoryRow["CATEGORYID"] = CategoryId;
                    dtProductCategory.Rows.Add(drdtProductCategoryRow);
                }

                //insert the invoice details
                var ProductCategoryId = _dataAccess.ExecuteScalar(Model.Constant.Constant.StoredProcedures.SP_PRODUCT_CATEGORY_MAPPING,
                dtProductCategory);

                return Convert.ToInt64(ProductCategoryId);
            }
                catch (Exception ex)
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
                var categoryId = _dataAccess.ExecuteDataTable(Model.Constant.Constant.StoredProcedures.SP_ADD_CATEGORY_TESTED,
                                    tenantId,
                                    catrtegoryPayload.Category,
                                    catrtegoryPayload.SubCategory,
                                    catrtegoryPayload.SubMenu,
                                    catrtegoryPayload.Link,
                                    catrtegoryPayload.SubMenu,
                                    catrtegoryPayload.OrderBy,
                                    catrtegoryPayload.Active);

                if (categoryId.Rows.Count > 0)
                {
                    return Convert.ToInt64(categoryId.Rows[0]["CATEGORY_ID"]);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        /// <summary>
        /// UpsertCart
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public DataTable UpsertCart(CartPayload payload)
        {
            try
            {
                return _dataAccess.ExecuteDataTable(Model.Constant.Constant.StoredProcedures.SP_UPSERT_CART,
                    payload.UserId,
                    payload.ProductId,
                    payload.Quantity);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// UpsertWishList
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public DataTable UpsertWishList(WishListPayload payload)
        {
            try
            {
                return _dataAccess.ExecuteDataTable(Model.Constant.Constant.StoredProcedures.SP_UPSERT_WISHLIST,
                    payload.UserId,
                    payload.ProductId,
                    payload.Quantity);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Model.ProductCart.SpProductCart> GetUserCart(string spName, long userId)
        {
            try
            {
                //Executing query
                List<Model.ProductCart.SpProductCart> spUserMasterLists = _dataAccess.ExecuteGenericList<Model.ProductCart.SpProductCart>(spName,
                    userId);

                return spUserMasterLists;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //public DataTable ValidateVendor(long tenantId, long locationId)
        //{
        //    try
        //    {
        //        return _dataAccess.ExecuteDataTable(Model.Constant.Constant.StoredProcedures.SA_REALTIMEOCR_VALIDATE_VENDOR, tenantId, locationId);

        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        //internal async Task<List<ProductMaster>> GetProductMaster(string tenantId, PorductFilter filter)
        //{
        //    try
        //    {
        //        //Local variable
        //        List<Model.Product.ProductMaster> productMasters = new List<Model.Product.ProductMaster>();

        //        #region Query Builder

        //        if (filter.IncludeReviews && filter.IncludeImages)
        //        {
        //            productMasters = await this.DbContext.productMasters.Include("ProductImages").
        //                                                                    Include("ProductReviews")
        //                                                                    .Where(x => (!filter.IncludeInactive ? x.Active.Equals(!filter.IncludeInactive) : filter.IncludeInactive))
        //                                                                    .AsNoTracking().ToListAsync();
        //        }
        //        else if (filter.IncludeImages)
        //        {
        //            productMasters = await this.DbContext.productMasters.Include("ProductImages").Where(x => (!filter.IncludeInactive ? x.Active.Equals(!filter.IncludeInactive) : filter.IncludeInactive))
        //                                                                    .AsNoTracking().ToListAsync();
        //        }
        //        else if (filter.IncludeReviews)
        //        {
        //            productMasters = await this.DbContext.productMasters.Include("ProductReviews").Where(x => (!filter.IncludeInactive ? x.Active.Equals(!filter.IncludeInactive) : filter.IncludeInactive))
        //                                                                    .AsNoTracking().ToListAsync();
        //        }
        //        else
        //        {
        //            productMasters = await this.DbContext.productMasters.Where(x => (!filter.IncludeInactive ? x.Active.Equals(!filter.IncludeInactive) : filter.IncludeInactive))
        //                                                .AsNoTracking().ToListAsync();
        //        }

        //        // Null check
        //        if (productMasters == null || productMasters.Count == 0)
        //        {
        //            this.Logger.LogWarning($"product not available for tenantId : {tenantId}");
        //            throw new KeyNotFoundException($"product not available f");
        //        }

        //        return productMasters;
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        throw ex;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //    #endregion
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        internal List<ProductCategory> GetCategory(string tenantId)
        {
            try
            {
                // method 1
                List<Model.Product.ProductCategory> productCategories = this.DbContext.productCategories
                .Where(x => x.TenantId.Equals(tenantId)).ToList();


                SqlParameter Tenant = new SqlParameter
                {
                    ParameterName = "@TenantId",
                    SqlDbType = SqlDbType.BigInt,
                    Value = Convert.ToInt64(tenantId)
                };

                // method 2
                string spName = "XC_GET_CATEGORY_TEST";
                List<Model.Product.ProductCategory> productCategories1 = this.DbContext.productCategories.FromSqlRaw($"exec {spName} @TenantId", Tenant).AsNoTracking().ToList();

                return productCategories1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetProductVerificationDataToEdit(string tenantId, string productId, string spname, string connectionstring)
        {
            try
            {
                //Logger 
                this.Logger.LogInformation($"Calling the GetProductVerificationDataToEdit for tenant : {tenantId}, product : {productId}");

                this.Logger.LogInformation($"Executing {spname} to fetch product verification item details for tenant : {tenantId}, product : {productId}");

                DataSet ds = new DataSet();
                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    SqlCommand sqlComm = new SqlCommand(spname, conn);
                    sqlComm.Parameters.AddWithValue("@PRODUCTID", Convert.ToInt64(productId));
                    sqlComm.Parameters.AddWithValue("@TENANT_ID", Convert.ToInt64(tenantId));

                    sqlComm.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;

                    da.Fill(ds);
                }
                //return reacord
                return ds;
            }
            catch (Exception ex)
            {
                this.Logger.LogError($"error while calling the GetProductVerificationDataToEdit :{ex.Message}");
                this.Logger.LogError($"Inner exception: {ex.InnerException}");
                this.Logger.LogError($"Stack trace{ex.StackTrace}");
                throw ex;
            }
        }

        /// <summary>
        /// // method 3
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="payload"></param>
        /// <param name="orderBy"></param>
        /// <param name="order"></param>
        /// <param name="spName"></param>
        /// <returns></returns>
        public List<Model.Product.SpProductMasterList> GetProductList(string spName, string tenantId, Model.Product.ProductPayload payload, string orderBy, string order)
        {
            try
            {
                DataTable productIdList = new DataTable();
                productIdList.Columns.Add("VALUE", typeof(Int64));

                DataRow dr;
                payload.ProductId.ForEach(x =>
                {
                    dr = productIdList.NewRow();
                    dr["VALUE"] = x;
                    productIdList.Rows.Add(dr);
                });

                //Executing query
                List<Model.Product.SpProductMasterList> spProductMasterLists = _dataAccess.ExecuteGenericList<Model.Product.SpProductMasterList>(spName,
                    Convert.ToInt64(tenantId),
                    Convert.ToInt64(payload.RoleId),
                    Convert.ToInt32(payload.Page),
                    Convert.ToInt32(payload.PageSize),
                    orderBy, order, payload.Search ?? string.Empty,
                    payload.Category,
                    productIdList
                    );


                return spProductMasterLists;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

                

        //public Models.Group GetProductGroupById(string tenantId, string groupId, string SpName)
        //{
        //    try
        //    {
        //        Models.Group group = _dataAccess.ExecuteGenericList<Models.Group>(SpName, tenantId, groupId).AsEnumerable().FirstOrDefault();
        //        SetProductGroupAggregate(new List<Models.Group> { group });

        //        return group;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="spName"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        internal List<Model.Product.ProductMaster> GetProductDtailsUsingSp(string tenantId, string spName, Model.Product.Search search)
        {
            try
            {
                #region Vendor
                //vendor
                DataTable subcategoryCollection = new DataTable();
                subcategoryCollection.Columns.Add("VALUE", typeof(Int64));

                if (search.SubCategory != null && search.SubCategory.Count() > 0)
                {
                    DataRow dr;
                    search.SubCategory.ForEach(x =>
                    {
                        dr = subcategoryCollection.NewRow();
                        dr["VALUE"] = Convert.ToInt64(x);
                        subcategoryCollection.Rows.Add(dr);
                    });
                }
                #endregion

                #region Category
                //Category
                DataTable categoryCollection = new DataTable();
                categoryCollection.Columns.Add("VALUE", typeof(Int64));

                if (search.Category != null && search.Category.Count() > 0)
                {
                    DataRow dr;
                    search.Category.ForEach(x =>
                    {
                        dr = categoryCollection.NewRow();
                        dr["VALUE"] = Convert.ToInt64(x);
                        categoryCollection.Rows.Add(dr);
                    });
                }
                #endregion

                //Executing query
                List<Model.Product.ProductMaster> productDetails = _dataAccess.ExecuteGenericList<Model.Product.ProductMaster>(spName, tenantId, search.isActive, categoryCollection, subcategoryCollection);

                //return product details
                return productDetails;

            }
            catch (Exception ex)
            {
                this.Logger.LogError($"Inner exception : {ex.InnerException}");
                this.Logger.LogError($"Stack trace : {ex.StackTrace}");
                throw ex;
            }
        }

        public async Task<Model.Product.ProductMaster> GetPartnerEventDetails(string RestaurantGuid, string LocationGuid, string storeProcedureName)
        {
            SqlParameter RestaurantGuidParam = Utility.PrepareParametersForStoreProcedure("@RESTAURANTGUID", SqlDbType.VarChar, string.Empty, RestaurantGuid);
            SqlParameter LocationGuidParam = Utility.PrepareParametersForStoreProcedure("@LOCATIONGUID", SqlDbType.VarChar, string.Empty, LocationGuid);
            Model.Product.ProductMaster partnerEventDetails = this.DbContext.productMasters.FromSqlRaw($"exec {storeProcedureName} @RESTAURANTGUID, @LOCATIONGUID", RestaurantGuidParam, LocationGuidParam).AsNoTracking().AsEnumerable().FirstOrDefault();

            return partnerEventDetails;
        }

        //public List<Model.Product.SpProductMasterList> GetProductMaster(string tenantId, 
        //    Model.Product.ProductPayload payload, string spName)
        //{
        //    try
        //    {
        //        DataTable category = new DataTable();
        //        category.Columns.Add("VALUE", typeof(Int64));

        //        DataRow dr;
        //        payload.Category.ForEach(x =>
        //        {
        //            dr = category.NewRow();
        //            dr["VALUE"] = x;
        //            category.Rows.Add(dr);
        //        });

        //        //Executing query
        //        List<Model.Product.ProductMaster> productMasters = _dataAccess.ExecuteGenericList<Model.Product.ProductMaster>(spName, 
        //            Convert.ToInt64(tenantId), 
        //            category);

        //        return productMasters;

        //    }
        //    catch (Exception ex)
        //    {
        //        //logger
        //        throw;
        //    }
        //}


        // only sample
        public async Task<List<Model.Product.ProductMaster>> GetStandardUoms(string spname)
        {
            try
            {
                //get standarduom list
                List<Model.Product.ProductMaster> uomstandard = _dataAccess.ExecuteGenericList<Model.Product.ProductMaster>(spname);

                return uomstandard;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Asynchronously retrieves an image by its ID.
        /// </summary>
        /// <param name="Id">The ID of the image to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, containing a DataTable with the image data.</returns>
        public async Task<DataTable> GetImageAsync(long Id)
        {
            try
            {
                return await Task.Run(() => _dataAccess.ExecuteDataTable("GOT_PRODUCT_LIST_TESTING", Id));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ProductId"></param>
        /// <param name="ProductImageName"></param>
        /// <param name="ContentType"></param>
        /// <param name="ImageData"></param>
        /// <param name="ThumbnailData"></param>
        /// <param name="FileSize"></param>
        /// <returns></returns>
        public long AddImages(long ProductId, string ProductImageName, string ContentType, byte[]  ImageData, byte[] ThumbnailData, int FileSize)
        {
            try
            {
                var cmd = _dataAccessCmd.ExecuteNonQueryCMD(Model.Constant.Constant.StoredProcedures.SP_ADD_IMAGES, ProductId,ProductImageName, ContentType, ImageData, ThumbnailData, FileSize);
                return Convert.ToInt64(cmd.Parameters["@RETURN_VALUE"].Value.ToString());
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
        public long DeleteImages(long id)
        {
            try
            {
                var cmd = _dataAccessCmd.ExecuteNonQueryCMD(Model.Constant.Constant.StoredProcedures.SP_DELETE_IMAGES, id);
                return Convert.ToInt64(cmd.Parameters["@RETURN_VALUE"].Value.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public void InsertCategory(string tenantId, Model.Response.Category category)
        //{
        //    try
        //    {
        //        using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
        //        {
        //            using (var command = new SqlCommand("sp_InsertCategory", connection))
        //            {
        //                command.CommandType = CommandType.StoredProcedure;
        //                command.Parameters.AddWithValue("@TenantId", tenantId);
        //                command.Parameters.AddWithValue("@CategoryId", category.CategoryId);
        //                command.Parameters.AddWithValue("@Name", category.Name);
        //                command.Parameters.AddWithValue("@Link", category.link);

        //                connection.Open();
        //                command.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("An error occurred while inserting the category", ex);
        //    }
        //}
        #endregion
    }
}
