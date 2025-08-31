using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tenant.Query.Model.Order
{
    /// <summary>
    /// Model for creating order request
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// User ID placing the order (required)
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public long UserId { get; set; }

        /// <summary>
        /// Tenant ID (optional for filtering)
        /// </summary>
        public long? TenantId { get; set; }

        /// <summary>
        /// List of items in the order (required)
        /// </summary>
        [Required(ErrorMessage = "Order items are required")]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();

        /// <summary>
        /// Shipping address (required)
        /// </summary>
        [Required(ErrorMessage = "Shipping address is required")]
        public AddressRequest ShippingAddress { get; set; }

        /// <summary>
        /// Billing address (required)
        /// </summary>
        [Required(ErrorMessage = "Billing address is required")]
        public AddressRequest BillingAddress { get; set; }

        /// <summary>
        /// Payment method (required)
        /// </summary>
        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethodRequest PaymentMethod { get; set; }

        /// <summary>
        /// Shipping method (required)
        /// </summary>
        [Required(ErrorMessage = "Shipping method is required")]
        public ShippingMethodRequest ShippingMethod { get; set; }

        /// <summary>
        /// Order totals (required)
        /// </summary>
        [Required(ErrorMessage = "Order totals are required")]
        public OrderTotalsRequest Totals { get; set; }

        /// <summary>
        /// Additional notes or instructions
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// IP address of the client (optional, will be extracted from request if not provided)
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User agent of the client (optional, will be extracted from request if not provided)
        /// </summary>
        public string UserAgent { get; set; }
    }

    /// <summary>
    /// Model for order item request
    /// </summary>
    public class OrderItemRequest
    {
        /// <summary>
        /// Product ID
        /// </summary>
        [Required(ErrorMessage = "Product ID is required")]
        public long ProductId { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        [Required(ErrorMessage = "Product name is required")]
        public string ProductName { get; set; }

        /// <summary>
        /// Product image URL
        /// </summary>
        public string ProductImage { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        /// <summary>
        /// Quantity ordered
        /// </summary>
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        /// <summary>
        /// Total price for this item (price * quantity)
        /// </summary>
        [Required(ErrorMessage = "Total is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total must be greater than 0")]
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Model for address request
    /// </summary>
    public class AddressRequest
    {
        /// <summary>
        /// First name
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [Required(ErrorMessage = "Phone number is required")]
        public string Phone { get; set; }

        /// <summary>
        /// Address line 1
        /// </summary>
        [Required(ErrorMessage = "Address is required")]
        public string Address1 { get; set; }

        /// <summary>
        /// Address line 2 (optional)
        /// </summary>
        public string Address2 { get; set; }

        /// <summary>
        /// City
        /// </summary>
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        /// <summary>
        /// State or province
        /// </summary>
        [Required(ErrorMessage = "State is required")]
        public string State { get; set; }

        /// <summary>
        /// ZIP or postal code
        /// </summary>
        [Required(ErrorMessage = "ZIP code is required")]
        public string ZipCode { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        [Required(ErrorMessage = "Country is required")]
        public string Country { get; set; }
    }

    /// <summary>
    /// Model for payment method request
    /// </summary>
    public class PaymentMethodRequest
    {
        /// <summary>
        /// Payment method ID
        /// </summary>
        [Required(ErrorMessage = "Payment method ID is required")]
        public string Id { get; set; }

        /// <summary>
        /// Payment method type
        /// </summary>
        [Required(ErrorMessage = "Payment method type is required")]
        public string Type { get; set; }

        /// <summary>
        /// Payment method display name
        /// </summary>
        [Required(ErrorMessage = "Payment method name is required")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Model for shipping method request
    /// </summary>
    public class ShippingMethodRequest
    {
        /// <summary>
        /// Shipping method ID
        /// </summary>
        [Required(ErrorMessage = "Shipping method ID is required")]
        public string Id { get; set; }

        /// <summary>
        /// Shipping method name
        /// </summary>
        [Required(ErrorMessage = "Shipping method name is required")]
        public string Name { get; set; }

        /// <summary>
        /// Shipping method description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Shipping price
        /// </summary>
        [Required(ErrorMessage = "Shipping price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Shipping price must be non-negative")]
        public decimal Price { get; set; }

        /// <summary>
        /// Estimated delivery time
        /// </summary>
        public string EstimatedDays { get; set; }
    }

    /// <summary>
    /// Model for order totals request
    /// </summary>
    public class OrderTotalsRequest
    {
        /// <summary>
        /// Subtotal (before shipping, tax, discount)
        /// </summary>
        [Required(ErrorMessage = "Subtotal is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Subtotal must be greater than 0")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Shipping cost
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Shipping cost must be non-negative")]
        public decimal Shipping { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Tax amount must be non-negative")]
        public decimal Tax { get; set; }

        /// <summary>
        /// Discount amount
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be non-negative")]
        public decimal Discount { get; set; }

        /// <summary>
        /// Final total amount
        /// </summary>
        [Required(ErrorMessage = "Total is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total must be greater than 0")]
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Model for create order response
    /// </summary>
    public class CreateOrderResponse
    {
        /// <summary>
        /// Created order ID
        /// </summary>
        public long OrderId { get; set; }

        /// <summary>
        /// Generated order number
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// User ID who placed the order
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Number of items in the order
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Total order amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Order status
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// Payment status
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Success message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Date and time when the order was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Order summary information
        /// </summary>
        public OrderSummaryInfo Summary { get; set; } = new OrderSummaryInfo();

        /// <summary>
        /// List of ordered items
        /// </summary>
        public List<OrderItemInfo> Items { get; set; } = new List<OrderItemInfo>();
    }

    /// <summary>
    /// Model for order summary information
    /// </summary>
    public class OrderSummaryInfo
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
        /// Total order amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Order status
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// Payment status
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Order creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Total number of unique items
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total quantity of all items
        /// </summary>
        public int TotalQuantity { get; set; }
    }

    /// <summary>
    /// Model for order item information
    /// </summary>
    public class OrderItemInfo
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
    }
}
