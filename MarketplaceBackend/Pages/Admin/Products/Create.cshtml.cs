﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketplaceBackend.Data;
using MarketplaceBackend.Models;
using MarketplaceBackend.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace MarketplaceBackend.Pages.Admin.Products
{
    public class CreateModel : PageModel
    {
        private readonly MarketplaceBackend.Data.DataContext _context;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;

        public CreateModel(MarketplaceBackend.Data.DataContext context, IFileService fileService, IConfiguration configuration)
        {
            _context = context;
            _fileService = fileService;
            _configuration = configuration;
        }

        public IActionResult OnGet()
        {
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public Product Product { get; set; }

        [BindProperty]
        public IFormFile ProductImage { get; set; }

        [BindProperty]
        public IFormFile DetailsPicturePrimary { get; set; }

        [BindProperty]
        public IFormFile DetailsPictureSecondary1 { get; set; }

        [BindProperty]
        public IFormFile DetailsPictureSecondary2 { get; set; }

        [BindProperty]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(14, 2)")]
        public string PriceString { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Product.Price = decimal.Parse(PriceString.Replace(",", "."), CultureInfo.InvariantCulture);


            Product.ImageURL = $"https://{_configuration.GetValue<string>("AWS:BucketName")}.s3.amazonaws.com/product_{Product.Id}";
            if (ProductImage is not null)
                await _fileService.UploadFileAsync(ProductImage, $"product_{Product.Id}");

            Product.DetailsPictureURLPrimary = Product.ImageURL + "_primary";
            if (ProductImage is not null)
                await _fileService.UploadFileAsync(DetailsPicturePrimary, $"product_{Product.Id}_primary");

            Product.DetailsPictureURLSecondary = new[] {
                Product.ImageURL + "_secondary1",
                Product.ImageURL + "_secondary2"
            };
            if (DetailsPictureSecondary1 is not null)
                await _fileService.UploadFileAsync(DetailsPictureSecondary1, $"product_{Product.Id}_secondary1");
            if (DetailsPictureSecondary2 is not null)
                await _fileService.UploadFileAsync(DetailsPictureSecondary2, $"product_{Product.Id}_secondary2");

            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
