using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using David_Dan_MAP.Data;
using David_Dan_MAP.Models;
using Microsoft.Data.SqlClient;

namespace David_Dan_MAP.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder; 
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["CurrentFilter"] = searchString;
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            var bookViewModels = from b in _context.Book
                                 join a in _context.Author on b.AuthorID equals a.ID
                                 select new BookViewModel
                                 {
                                     ID = b.ID,
                                     Title = b.Title,
                                     Price = b.Price,
                                     FullName = a.FirstName + " " + a.LastName,
                                 };
            if (!String.IsNullOrEmpty(searchString))
            {
                bookViewModels = bookViewModels.Where(s => s.Title.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "title_desc":
                    bookViewModels = bookViewModels.OrderByDescending(b => b.Title);
                    break;
                case "Price":
                    bookViewModels = bookViewModels.OrderBy(b => b.Price);
                    break;
                case "price_desc":
                    bookViewModels = bookViewModels.OrderByDescending(b => b.Price);
                    break;
                default:
                    bookViewModels = bookViewModels.OrderBy(b => b.Title);
                    break;
            }
            // Map BookViewModel to Book
            var books = await bookViewModels
                .AsNoTracking()
                .ToListAsync();

            var mappedBooks = books.Select(bvm => new Book
            {
                ID = bvm.ID,
                Title = bvm.Title,
                Price = bvm.Price,
                Author = new Author
                {
                    FirstName = bvm.FullName.Split(' ')[0], // Split FullName for Author.FirstName
                    LastName = bvm.FullName.Split(' ').Length > 1 ? bvm.FullName.Split(' ')[1] : ""
                }
            }).ToList();
            int pageSize = 2;
            return View(await PaginatedList<BookViewModel>.CreateAsync(books.AsNoTracking(), pageNumber ?? 1, pageSize)); 
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
             .Include(s => s.Orders)
             .ThenInclude(e => e.Customer)
             .AsNoTracking()
             .FirstOrDefaultAsync(m => m.ID == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["AuthorID"] = new SelectList(_context.Author
                        .Select(a => new
                        {
                            ID = a.ID,
                            FullName = a.FirstName + " " + a.LastName
                        }), "ID", "FullName");
            ViewData["GenreID"] = new SelectList(_context.Genre, "ID", "Name");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,AuthorID,Price,GenreID")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AuthorID"] = new SelectList(_context.Author
                .Select(a => new
                {
                    ID = a.ID,
                    FullName = a.FirstName + " " + a.LastName
                }), "ID", "FullName", book.AuthorID);

            ViewData["GenreID"] = new SelectList(_context.Genre, "ID", "Name", book.GenreID);

            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["AuthorID"] = new SelectList(_context.Author
                        .Select(a => new
                        {
                            ID = a.ID,
                            FullName = a.FirstName + " " + a.LastName
                        }), "ID", "FullName");
            ViewData["GenreID"] = new SelectList(_context.Genre, "ID", "Name");
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,AuthorID,Price,GenreID")] Book book)
        {
            if (id != book.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorID"] = new SelectList(_context.Author
                        .Select(a => new
                        {
                            ID = a.ID,
                            FullName = a.FirstName + " " + a.LastName
                        }), "ID", "FullName");
            ViewData["GenreID"] = new SelectList(_context.Genre, "ID", "Name");
            return View(book);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var bookToUpdate = await _context.Book.FirstOrDefaultAsync(s => s.ID == id);
            if (await TryUpdateModelAsync<Book>(
            bookToUpdate,
            "",
            s => s.AuthorID, s => s.Title, s => s.Price))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists");
                }
            }
            ViewData["AuthorID"] = new SelectList(_context.Author, "ID", "FullName",
           bookToUpdate.AuthorID);
            return View(bookToUpdate);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)

        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (book == null)
            {
                return NotFound();
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "Delete failed. Try again";
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.FindAsync(id);

            if (book == null)
            {
                return RedirectToAction(nameof(Index));
            }
            try
            {
                _context.Book.Remove(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        
     }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.ID == id);
        }
    }
}
