﻿using System.Net;
using api_auth_service.Services;
using api_flms_service.Entity;
using api_flms_service.Model;
using api_flms_service.Service;
using api_flms_service.ServiceInterface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace api_flms_service.Controllers
{
    [ApiController]
    [Route("api/v0/books")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IUserService _userService;
        private readonly AuthService _authService;
        private readonly ICloudinaryService _cloudinaryService;

        public BookController(IBookService bookService, IUserService userService, AuthService authService, ICloudinaryService cloudinaryService)
        {
            _bookService = bookService;
            _userService = userService;
            _authService = authService;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                var books = await _bookService.GetAllBooksAsync();
                var bookDtos = books.Select(b => new BookDto
                {
                    BookId = b.BookId,
                    BookName = b.Title ?? "No Title",
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author?.Name ?? "No Author",
                    Category = b.BookCategories?.Select(bc => bc.Category).ToList() ?? new List<Category>(),
                    BookNo = b.ISBN ?? "No ISBN",
                    BookPrice = b.PublicationYear,
                    AvailableCopies = b.AvailableCopies,
                    BookDescription = b.BookDescription,
                    CloudinaryImageId = b.CloudinaryImageId,
                    ImageUrls = b.ImageUrls
                }).ToList();

                return Ok(bookDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllBooks: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred while retrieving books.", details = ex.Message });
            }
        }

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
                    AuthorName = book.Author?.Name ?? "No Author",
                    Category = book.BookCategories?.Select(bc => bc.Category).ToList() ?? new List<Category>(),
                    BookNo = book.ISBN,
                    BookPrice = book.PublicationYear,
                    AvailableCopies = book.AvailableCopies,
                    BookDescription = book.BookDescription,
                    CloudinaryImageId = book.CloudinaryImageId,
                    ImageUrls = book.ImageUrls
                };

                return Ok(bookDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the book.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] BookRequestDto bookDto)
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
                    PublicationYear = (int)bookDto.BookPrice,
                    AvailableCopies = bookDto.AvailableCopies,
                    BookDescription = bookDto.BookDescription,
                    BookFileUrl = bookDto.BookFileUrl,
                    ImageUrls = bookDto.ImageUrls != null ? string.Join(", ", bookDto.ImageUrls) : null,
                    BookCategories = bookDto.CategoryIds?.Select(id => new BookCategory { CategoryId = id }).ToList()
                };

                var createdBook = await _bookService.CreateBookAsync(book);

                var createdBookDto = new BookRequestDto
                {
                    BookId = createdBook.BookId,
                    BookName = createdBook.Title,
                    AuthorId = createdBook.AuthorId,
                    AuthorName = createdBook.Author?.Name ?? "No Author",
                    CategoryIds = createdBook.BookCategories?.Select(bc => bc.CategoryId).ToList(),
                    BookNo = createdBook.ISBN,
                    BookPrice = createdBook.PublicationYear,
                    AvailableCopies = createdBook.AvailableCopies,
                    BookDescription = createdBook.BookDescription,
                    BookFileUrl = createdBook.BookFileUrl,
                    ImageUrls = createdBook.ImageUrls?.Split(", ").ToList()
                };

                return CreatedAtAction(nameof(GetBookById), new { id = createdBook.BookId }, createdBookDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the book.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] BookRequestDto bookDto, IFormFile? bookFile)
        {
            Console.WriteLine($"UpdateBook called with id: {id}, bookDto.BookId: {bookDto.BookId}");
            if (id != bookDto.BookId)
            {
                return BadRequest(new { message = "Book ID in URL does not match Book ID in form data" });
            }

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
                existingBook.PublicationYear = (int)bookDto.BookPrice;
                existingBook.AvailableCopies = bookDto.AvailableCopies;
                existingBook.BookDescription = bookDto.BookDescription ?? existingBook.BookDescription;

                if (bookFile != null)
                {
                    try
                    {
                        existingBook.BookFileUrl = await _cloudinaryService.UploadFileAsync(bookFile);
                    }
                    catch (Exception uploadEx)
                    {
                        return StatusCode(500, new { message = "Error uploading file to Cloudinary", details = uploadEx.Message });
                    }
                }
                else if (!string.IsNullOrEmpty(bookDto.BookFileUrl))
                {
                    existingBook.BookFileUrl = bookDto.BookFileUrl;
                }

                if (bookDto.ImageUrls != null && bookDto.ImageUrls.Any())
                {
                    existingBook.ImageUrls = string.Join(", ", bookDto.ImageUrls);
                }

                if (bookDto.CategoryIds != null && bookDto.CategoryIds.Any())
                {
                    var existingCategories = existingBook.BookCategories.Select(c => c.CategoryId).ToHashSet();
                    var newCategories = bookDto.CategoryIds.ToHashSet();

                    var categoriesToRemove = existingCategories.Except(newCategories).ToList();
                    var categoriesToAdd = newCategories.Except(existingCategories).ToList();

                    var itemsToRemove = existingBook.BookCategories
                        .Where(c => categoriesToRemove.Contains(c.CategoryId))
                        .ToList();

                    foreach (var item in itemsToRemove)
                    {
                        existingBook.BookCategories.Remove(item);
                    }

                    foreach (var categoryId in categoriesToAdd)
                    {
                        existingBook.BookCategories.Add(new BookCategory
                        {
                            BookId = bookDto.BookId,
                            CategoryId = categoryId
                        });
                    }
                }

                await _bookService.UpdateBookAsync(existingBook);
                return Ok(new { message = "Book updated successfully", bookId = existingBook.BookId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateBook: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred while updating the book.", details = ex.Message });
            }
        }

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

        [HttpGet("borrowed")]
        public async Task<IActionResult> GetBorrowedBooks()
        {
            var userOfGG = await _authService.GetCurrentUserAsync();
            if (userOfGG == null)
            {
                return Unauthorized("User is not logged in");
            }

            var user = await _userService.GetUserByEmail(userOfGG.Email);
            if (user == null || user.UserId <= 0)
            {
                return RedirectToPage("/AccessDenied");
            }

            if (user.Role != "User" && user.Role != "Admin")
            {
                return RedirectToPage("/AccessDenied");
            }

            var books = await _bookService.GetBorrowedBooksAsync(user.UserId);
            return Ok(books);
        }

        [HttpPost("renew")]
        public async Task<IActionResult> RenewBook([FromBody] RenewRequest request)
        {
            var userOfGG = await _authService.GetCurrentUserAsync();
            var user = await _userService.GetUserByEmail(userOfGG?.Email);
            if (user == null || user.UserId <= 0)
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
        public async Task<IActionResult> Search(string searchTerm, string categoryName)
        {
            var books = await _bookService.SearchBooksAsync(searchTerm, categoryName);
            return Ok(books);
        }

        [HttpPost("reserve/{bookId}")]
        public async Task<IActionResult> ReserveBook(int bookId)
        {
            var userOfGG = await _authService.GetCurrentUserAsync();
            if (userOfGG == null) return Unauthorized("User is not logged in");

            var user = await _userService.GetUserByEmail(userOfGG.Email);
            if (user == null || user.Role != "User") return Unauthorized("Only users with role 'User' can reserve books");

            var result = await _bookService.ReserveBookAsync(bookId, user.UserId);
            if (result.Contains("successfully"))
            {
                return Ok(new { message = result });
            }
            return BadRequest(new { message = result });
        }

        [HttpGet("reserved")]
        public async Task<IActionResult> GetReservedBooks()
        {
            var userOfGG = await _authService.GetCurrentUserAsync();
            if (userOfGG == null) return Unauthorized("User is not logged in");

            var user = await _userService.GetUserByEmail(userOfGG.Email);
            if (user == null || user.UserId <= 0) return Unauthorized("Invalid user");

            var books = await _bookService.GetReservedBooksAsync(user.UserId);
            return Ok(books);
        }

        private readonly List<string> _allowedExtensions = new() { ".pdf", ".epub", ".mobi", ".png", ".jpg" };

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file format. Allowed formats: PDF, EPUB, MOBI, PNG, JPG." });
            }

            var uploadResult = await _cloudinaryService.UploadFileAsync(file);
            return Ok(new { fileUrl = uploadResult });
        }

        [HttpGet("get/{publicId}")]
        public async Task<IActionResult> GetFile(string publicId)
        {
            var file = await _cloudinaryService.GetFileAsync(publicId);
            if (file == null)
            {
                return NotFound("File not found.");
            }
            return Ok(file);
        }

        [HttpDelete("delete/{publicId}")]
        public async Task<IActionResult> DeleteFile(string publicId)
        {
            var result = await _cloudinaryService.DeleteFileAsync(publicId);
            if (result == null || result.Result != "ok")
            {
                return NotFound("File not found or could not be deleted.");
            }
            return Ok("File deleted successfully.");
        }
    }
}