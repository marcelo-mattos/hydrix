using System;
using System.Text.Json.Serialization;

namespace Hydrix.Tests.Database.Dto
{
    /// <summary>
    /// Represents a data transfer object (DTO) for product information, including identifiers, customer association,
    /// product details, and authorization token.
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the customer associated with this entity.
        /// </summary>
        [JsonPropertyName("customer_id")]
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer associated with this entity.
        /// </summary>
        [JsonPropertyName("customer")]
        public CustomerDto Customer { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        [JsonPropertyName("name")]
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the International Article Number (EAN) associated with the item.
        /// </summary>
        [JsonPropertyName("ean")]
        public String Ean { get; set; }

        /// <summary>
        /// Gets or sets the quantity associated with the current entity.
        /// </summary>
        [JsonPropertyName("quantity")]
        public Decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the price value associated with the item.
        /// </summary>
        [JsonPropertyName("price")]
        public Decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the type of the object represented by this instance.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the authentication token used to authorize requests.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
