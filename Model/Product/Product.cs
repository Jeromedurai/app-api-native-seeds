using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Tenant.Query.Model.Product
{
    public class Product
    {
        [JsonProperty("name")]
        public string ProductName { get; set; }

        [JsonProperty("displayname")]
        public string Displayname { get; set; }

        [JsonProperty("rating"),]
        public long Rating { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("tax")]
        public decimal Tax { get; set; }

        [JsonProperty("stock")]
        public bool Stock { get; set; }

        [JsonProperty("description"), Column("Description")]
        public string Description { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("minimunQuantity")]
        public long MinimunQuantity { get; set; }

        [JsonProperty("numofReviews")]
        public long? Numofreviews { get; set; }

        [JsonProperty("bestSeller")]
        public bool BestSeller { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("lastModified")]
        public DateTime LastModified { get; set; }

        [JsonProperty("lastModifiedBy")]
        public string LastModifiedBy { get; set; }

        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("vendorId")]
        public long? VendorId { get; set; }
    }
}
