﻿using api_auth_service.Services;
using api_flms_service.Entity;
using api_flms_service.Model;
using api_flms_service.ServiceInterface;
using Microsoft.AspNetCore.Mvc;

namespace api_flms_service.Controllers
{
    [ApiController]
    [Route("api/v0/books")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IUserService _userService;
        private readonly AuthService _authService;

        public BookController(IBookService bookService, IUserService userService, AuthService authService)
        {
            _bookService = bookService;
            _userService = userService;
            _authService = authService;
        }


        /// <summary>
        /// Get all books
        /// </summary>
        /// <returns>List of books with their authors and categories</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                var books = await _bookService.GetAllBooksAsync();
                var bookDtos = books.Select(b => new BookDto
                {
                    BookId = b.BookId,
                    BookName = b.Title,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author.Name,
                    Category = b.Categories.ToList(),
                    BookNo = b.ISBN,
                    BookPrice = b.PublicationYear
                }).ToList();

                return Ok(bookDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving books.", details = ex.Message });
            }
        }


        /// <summary>
        /// Get book by ID
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns>Book details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    return NotFound(new { message = "Book not found" });
                }

                var bookDto = new BookDto
                {
                    BookId = book.BookId,
                    BookName = book.Title,
                    AuthorId = book.AuthorId,
                    AuthorName = book.Author.Name,
                    Category = book.Categories.ToList(),
                    BookNo = book.ISBN,
                    BookPrice = book.PublicationYear
                };

                return Ok(bookDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the book.", details = ex.Message });
            }
        }

        /// <summary>
        /// Add a new book
        /// </summary>
        /// <param name="bookDto">Book DTO</param>
        /// <returns>Created book</returns>
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] BookDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var book = new Book
                {
                    Title = bookDto.BookName,
                    AuthorId = bookDto.AuthorId,
                    ISBN = bookDto.BookNo,
                    AvailableCopies = bookDto.BookPrice
                };

                var createdBook = await _bookService.CreateBookAsync(book);

                var createdBookDto = new BookDto
                {
                    BookId = createdBook.BookId,
                    BookName = createdBook.Title,
                    AuthorId = createdBook.AuthorId,
                    AuthorName = createdBook.Author.Name,
                    BookNo = createdBook.ISBN,
                    BookPrice = 0
                };

                return CreatedAtAction(nameof(GetBookById), new { id = createdBook.BookId }, createdBookDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the book.", details = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing book
        /// </summary>
        /// <param name="bookDto">Updated book DTO</param>
        /// <returns>Updated book</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateBook([FromBody] BookDto bookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingBook = await _bookService.GetBookByIdAsync(bookDto.BookId);
                if (existingBook == null)
                {
                    return NotFound(new { message = "Book not found" });
                }

                existingBook.Title = bookDto.BookName;
                existingBook.AuthorId = bookDto.AuthorId;
                existingBook.ISBN = bookDto.BookNo;

                var updatedBook = await _bookService.UpdateBookAsync(existingBook);

                var updatedBookDto = new BookDto
                {
                    BookId = updatedBook.BookId,
                    BookName = updatedBook.Title,
                    AuthorId = updatedBook.AuthorId,
                    AuthorName = updatedBook.Author.Name,
                    BookNo = updatedBook.ISBN
                };

                return Ok(updatedBookDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the book.", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a book by ID
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null)
                {
                    return NotFound(new { message = "Book not found" });
                }

                await _bookService.DeleteBookAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the book.", details = ex.Message });
            }
        }

        // API để lấy danh sách sách mượn của người dùng
        [HttpGet("borrowed")]
        public async Task<IActionResult> GetBorrowedBooks()
        {
            var userOfGG = await _authService.GetCurrentUserAsync();

            var user = await _userService.GetUserByEmail(userOfGG.Email); // Lấy UserId từ JWT hoặc Session
            if (user.UserId <= 0)
            {
                return Unauthorized("User is not logged in");
            }

            var books = await _bookService.GetBorrowedBooksAsync(user.UserId);
            return Ok(books);
        }

        // API gia hạn sách
        [HttpPost("renew")]
        public async Task<IActionResult> RenewBook([FromBody] RenewRequest request)
        {
            var userOfGG = await _authService.GetCurrentUserAsync();

            var user = await _userService.GetUserByEmail(userOfGG.Email); // Lấy UserId từ JWT hoặc Session
            if (user.UserId <= 0)
            {
                return Unauthorized("User is not logged in");
            }

            if (request.BookId <= 0)
            {
                return BadRequest("Invalid ID");
            }

            var result = await _bookService.RenewBookAsync(user.UserId, request.BookId);
            return result;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks([FromQuery] string? bookName, [FromQuery] string? authorName,
                                                  [FromQuery] int? categoryId, [FromQuery] int? minPrice,
                                                  [FromQuery] int? maxPrice)
        {
            var books = await _bookService.SearchBooksAsync(bookName, authorName, categoryId, minPrice, maxPrice);
            return Ok(books);
        }
    }
}
