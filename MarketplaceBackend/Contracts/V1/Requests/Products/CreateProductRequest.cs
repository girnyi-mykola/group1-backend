﻿namespace MarketplaceBackend.Contracts.V1.Requests.Products
{
    public class CreateProductRequest
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        public string ExtraInformation { get; set; }

        public int CategoryId { get; set; }

        // public string ImageUrl { get; set; }
    }
}