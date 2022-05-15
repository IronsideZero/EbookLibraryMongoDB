using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using EbookLibraryMongoDB.Models;

namespace EbookLibraryMongoDB.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<EbookLibraryMongoDB.Models.User> User { get; set; }
        public DbSet<EbookLibraryMongoDB.Models.Book> Book { get; set; }
    }
}
