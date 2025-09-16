﻿namespace Tenant.Query.Model.Constant
{
    public static class Constant
    {
        public const string SA_GET_PRODUCTS_FOR_CATEGORY_MAPPING = "SA_GET_PRODUCTS_FOR_CATEGORY_MAPPING";
        public const string XC_GET_PRODUCT_MASTER_LIST_TESTING = "[dbo].[XC_GET_PRODUCT_MASTER_LIST_TESTING]";

        public const string PrductName = "PrductName";

        public const string ASC = "asc";

        //Stock
        public const string Product_In_stock = "In stock";
        public const string Product_Out_of_stock = "Out of stock";

        public class EdiTemplatesConstants
        {
            public const string US_Foods = "05651744-92d3-4f21-968e-206bb7dbb905";
            public const string Sysco = "ce6efb36-7185-4c3e-9d48-67a55df667bf";
            public const string GFS = "d0f4ec9c-8587-4c5f-bfbc-ed858540a7bc";
            public const string Ben_E_Keith = "a7a4f2c3-7ffd-465d-9a4a-35f6b799a7be";
            public const string Cheney_Brothers = "b3f6a3ea-18cd-4760-ac0d-7e6191c64734";
            public const string PFG = "44a4716b-14b7-4016-8446-f11b21b39399";
            public const string ImperialDade = "7cfe278b-5872-4d98-b3d3-ed673f60e9da";

        }

        public static class StoredProcedures
        {
            public const string SA_REALTIMEOCR_ADD_INVOICE = "[dbo].[SA_REALTIMEOCR_ADD_INVOICE_TESTED]";
            public const string SP_ADD_CATEGORY_TESTED = "[dbo].[SP_ADD_CATEGORY_TESTED]";
            public const string SA_REALTIMEOCR_GET_TENANT_VENDOR_DETAIL = "[dbo].[SA_REALTIMEOCR_GET_TENANT_VENDOR_DETAIL]";
            public const string HN_GET_MENU_MASTER = "[dbo].[XC_GET_CATEGORY_TESTING]";
            public const string SA_REALTIMEOCR_VALIDATE_VENDOR = "[dbo].[SA_REALTIMEOCR_VALIDATE_VENDOR]";
            public const string SP_ADD_IMAGES = "[dbo].[XC_ADD_IMAGE_TESTED]";
            public const string SP_REMOVE_CART = "[dbo].[SP_REMOVE_CART]";
            public const string SP_REMOVE_WISHLIST = "[dbo].[SP_REMOVE_WISHLIST]";
            public const string SP_DELETE_IMAGES = "[dbo].[XC_DELETE_IMAGE_TESTED]";
            public const string SP_PRODUCT_CATEGORY_MAPPING = "[dbo].[SP_PRODUCT_CATEGORY_MAPPING]";
            public const string SP_UPSERT_WISHLIST = "[dbo].[SP_UPSERT_WISHLIST]";
            public const string SP_UPSERT_CART = "[dbo].[SP_UPSERT_CART]";
            public const string SP_GET_VALUE_BY_KEY = "xc_get_app_config_by_key";
            public const string SP_SEARCH_PRODUCTS = "[dbo].[SP_SEARCH_PRODUCTS]";
            public const string SP_GET_PRODUCT_BY_ID = "[dbo].[SP_GET_PRODUCT_BY_ID]";
            public const string SP_ADD_PRODUCT = "[dbo].[SP_ADD_PRODUCT]";
            public const string SP_UPDATE_PRODUCT = "[dbo].[SP_UPDATE_PRODUCT]";
            public const string SP_DELETE_PRODUCT = "[dbo].[SP_DELETE_PRODUCT]";
            public const string SP_GET_ALL_CATEGORIES = "[dbo].[SP_GET_ALL_CATEGORIES]";
            public const string SP_ADD_CATEGORY = "[dbo].[SP_ADD_CATEGORY]";
            public const string SP_UPDATE_CATEGORY = "[dbo].[SP_UPDATE_CATEGORY]";
            public const string SP_GET_MENU_MASTER = "[dbo].[SP_GET_MENU_MASTER]";
            public const string SP_USER_LOGIN = "[dbo].[SP_USER_LOGIN]";
            public const string SP_USER_REGISTER = "[dbo].[SP_USER_REGISTER]";
            public const string SP_USER_LOGOUT = "[dbo].[SP_USER_LOGOUT]";
            public const string SP_GET_USER_PROFILE = "[dbo].[SP_GET_USER_PROFILE]";
            public const string SP_UPDATE_USER_PROFILE = "[dbo].[SP_UPDATE_USER_PROFILE]";
            public const string SP_RESET_PASSWORD = "[dbo].[SP_RESET_PASSWORD]";
            public const string SP_GET_USER_CART = "[dbo].[SP_GET_USER_CART]";
            public const string SP_ADD_ITEM_TO_CART = "[dbo].[SP_ADD_ITEM_TO_CART]";
            public const string SP_REMOVE_ITEM_FROM_CART = "[dbo].[SP_REMOVE_ITEM_FROM_CART]";
            public const string SP_CLEAR_CART = "[dbo].[SP_CLEAR_CART]";
            public const string SP_CREATE_ORDER = "[dbo].[SP_CREATE_ORDER]";
            public const string SP_GET_ORDERS = "[dbo].[SP_GET_ORDERS]";
            public const string SP_GET_ORDER_BY_ID = "[dbo].[SP_GET_ORDER_BY_ID]";
            public const string SP_CANCEL_ORDER = "[dbo].[SP_CANCEL_ORDER]";
            public const string SP_UPDATE_ORDER_STATUS = "[dbo].[SP_UPDATE_ORDER_STATUS]";
            public const string SP_ADMIN_GET_ALL_USERS = "[dbo].[SP_ADMIN_GET_ALL_USERS]";
            public const string SP_ADMIN_UPDATE_USER_ROLE = "[dbo].[SP_ADMIN_UPDATE_USER_ROLE]";
            public const string SP_ADMIN_GET_ALL_ORDERS = "[dbo].[SP_ADMIN_GET_ALL_ORDERS]";
            public const string SP_ADD_PRODUCT_IMAGES = "[dbo].[SP_ADD_PRODUCT_IMAGES]";
            public const string SP_UPDATE_PRODUCT_IMAGE = "[dbo].[SP_UPDATE_PRODUCT_IMAGE]";
            public const string SP_DELETE_PRODUCT_IMAGE = "[dbo].[SP_DELETE_PRODUCT_IMAGE]";
        }

        public static class AddressType
        {
            public const string Shipping = "Shipping";
            public const string Billing = "Billing";
            public const string Both = "Both";
        }
    }
}
