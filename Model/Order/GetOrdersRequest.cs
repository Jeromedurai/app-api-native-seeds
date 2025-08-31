using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tenant.Query.Model.Order
{
    /// <summary>
    /// Model for getting orders request
    /// </summary>
    public class GetOrdersRequest
    {
        /// <summary>
        /// User ID (required)
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public long UserId { get; set; }

        /// <summary>
        /// Tenant ID (optional for filtering)
        /// </summary>
        public long? TenantId { get; set; }

        /// <summary>
        /// Page number (default: 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of items per page (default: 10)
        /// </summary>
        [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")]
        public int Limit { get; set; } = 10;

        /// <summary>
        /// Filter by order status (optional)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Search by order number (optional)
        /// </summary>
        public string Search { get; set; }
    }

    /// <summary>
    /// Model for get orders response
    /// </summary>
    public class GetOrdersResponse
    {
        /// <summary>
        /// List of orders
        /// </summary>
        public List<OrderListItem> Orders { get; set; } = new List<OrderListItem>();

        /// <summary>
        /// Pagination information
        /// </summary>
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    }

    /// <summary>
    /// Model for order list item
    /// </summary>
    public class OrderListItem
    {
        /// <summary>
        /// Order ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Order number
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// Order status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Payment status
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Total order amount
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Subtotal amount
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Shipping amount
        /// </summary>
        public decimal ShippingAmount { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Order notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Order creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Order last update date
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Total number of unique items
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total quantity of all items
        /// </summary>
        public int TotalQuantity { get; set; }

        /// <summary>
        /// List of order items
        /// </summary>
        public List<OrderListItemInfo> Items { get; set; } = new List<OrderListItemInfo>();
    }

    /// <summary>
    /// Model for order item information in list
    /// </summary>
    public class OrderListItemInfo
    {
        /// <summary>
        /// Order item ID
        /// </summary>
        public long OrderItemId { get; set; }

        /// <summary>
        /// Product ID
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Product image URL
        /// </summary>
        public string ProductImage { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Quantity ordered
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Total price for this item
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Product code
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// Product category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Product rating
        /// </summary>
        public decimal Rating { get; set; }

        /// <summary>
        /// Product offer/discount information
        /// </summary>
        public string Offer { get; set; }
    }

    /// <summary>
    /// Model for pagination information
    /// </summary>
    public class PaginationInfo
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Total number of items
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNext { get; set; }

        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPrevious { get; set; }
    }

    /// <summary>
    /// Model for get order by ID request
    /// </summary>
    public class GetOrderByIdRequest
    {
        /// <summary>
        /// Order ID (required)
        /// </summary>
        [Required(ErrorMessage = "Order ID is required")]
        public long OrderId { get; set; }

        /// <summary>
        /// User ID (required)
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public long UserId { get; set; }

        /// <summary>
        /// Tenant ID (optional for filtering)
        /// </summary>
        public long? TenantId { get; set; }
    }

    /// <summary>
    /// Model for get order by ID response
    /// </summary>
    public class GetOrderByIdResponse
    {
        /// <summary>
        /// Order ID
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Order number
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// Order status
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// Payment status
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Total order amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Subtotal amount
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Shipping amount
        /// </summary>
        public decimal ShippingAmount { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Order notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Shipping address (JSON)
        /// </summary>
        public string ShippingAddress { get; set; }

        /// <summary>
        /// Billing address (JSON)
        /// </summary>
        public string BillingAddress { get; set; }

        /// <summary>
        /// Payment method (JSON)
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Shipping method (JSON)
        /// </summary>
        public string ShippingMethod { get; set; }

        /// <summary>
        /// Order creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Order last update date
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Customer name
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Customer email
        /// </summary>
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Customer phone
        /// </summary>
        public string CustomerPhone { get; set; }

        /// <summary>
        /// Total number of unique items
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total quantity of all items
        /// </summary>
        public int TotalQuantity { get; set; }

        /// <summary>
        /// List of order items
        /// </summary>
        public List<OrderDetailItemInfo> Items { get; set; } = new List<OrderDetailItemInfo>();

        /// <summary>
        /// Order status history
        /// </summary>
        public List<OrderStatusHistoryInfo> StatusHistory { get; set; } = new List<OrderStatusHistoryInfo>();

        /// <summary>
        /// Order tracking information
        /// </summary>
        public List<OrderTrackingInfo> TrackingInfo { get; set; } = new List<OrderTrackingInfo>();
    }

    /// <summary>
    /// Model for detailed order item information
    /// </summary>
    public class OrderDetailItemInfo
    {
        /// <summary>
        /// Order item ID
        /// </summary>
        public long OrderItemId { get; set; }

        /// <summary>
        /// Product ID
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Product image URL
        /// </summary>
        public string ProductImage { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Quantity ordered
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Total price for this item
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Item creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Product code
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// Product description
        /// </summary>
        public string ProductDescription { get; set; }

        /// <summary>
        /// Product category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Product rating
        /// </summary>
        public decimal Rating { get; set; }

        /// <summary>
        /// Product offer/discount information
        /// </summary>
        public string Offer { get; set; }

        /// <summary>
        /// Whether product is in stock
        /// </summary>
        public bool InStock { get; set; }

        /// <summary>
        /// Whether product is a best seller
        /// </summary>
        public bool BestSeller { get; set; }
    }

    /// <summary>
    /// Model for order status history information
    /// </summary>
    public class OrderStatusHistoryInfo
    {
        /// <summary>
        /// Status history ID
        /// </summary>
        public long StatusHistoryId { get; set; }

        /// <summary>
        /// Previous status
        /// </summary>
        public string PreviousStatus { get; set; }

        /// <summary>
        /// New status
        /// </summary>
        public string NewStatus { get; set; }

        /// <summary>
        /// Status change note
        /// </summary>
        public string StatusNote { get; set; }

        /// <summary>
        /// User ID who changed the status
        /// </summary>
        public long ChangedBy { get; set; }

        /// <summary>
        /// Name of user who changed the status
        /// </summary>
        public string ChangedByName { get; set; }

        /// <summary>
        /// Date and time of status change
        /// </summary>
        public DateTime ChangedAt { get; set; }
    }

    /// <summary>
    /// Model for order tracking information
    /// </summary>
    public class OrderTrackingInfo
    {
        /// <summary>
        /// Tracking ID
        /// </summary>
        public long TrackingId { get; set; }

        /// <summary>
        /// Tracking number
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Shipping carrier
        /// </summary>
        public string Carrier { get; set; }

        /// <summary>
        /// Tracking status
        /// </summary>
        public string TrackingStatus { get; set; }

        /// <summary>
        /// Estimated delivery date
        /// </summary>
        public DateTime? EstimatedDelivery { get; set; }

        /// <summary>
        /// Actual delivery date
        /// </summary>
        public DateTime? ActualDelivery { get; set; }

        /// <summary>
        /// Tracking URL
        /// </summary>
        public string TrackingUrl { get; set; }

        /// <summary>
        /// Tracking creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Tracking last update date
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
