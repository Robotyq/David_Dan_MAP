using Microsoft.EntityFrameworkCore;

namespace David_Dan_MAP.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext (DbContextOptions<LibraryContext> options)
            : base(options)
        {
        }

        public DbSet<David_Dan_MAP.Models.Book> Book { get; set; } = default!;
        public DbSet<Models.Customer> Customer { get; set; } = default!;
        public DbSet<Models.Genre> Genre { get; set; } = default!;
        public DbSet<Models.Author> Author { get; set; } = default!;
    }
}

