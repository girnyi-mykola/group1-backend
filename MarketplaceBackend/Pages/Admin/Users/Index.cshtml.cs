﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketplaceBackend.Data;
using MarketplaceBackend.Models;

namespace MarketplaceBackend.Pages.Admin.Users
{
    public class IndexModel : PageModel
    {
        private readonly MarketplaceBackend.Data.DataContext _context;

        public IndexModel(MarketplaceBackend.Data.DataContext context)
        {
            _context = context;
        }

        public IList<User> UserList { get;set; }

        public async Task OnGetAsync()
        {
            UserList = await _context.Users.ToListAsync();
        }
    }
}