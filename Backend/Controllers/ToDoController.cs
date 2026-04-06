using Backend.DTO;
using Backend.Models;
using Backend.Models.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.CompilerServices;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/todos")]
    public class ToDoController : ControllerBase
    {
        private readonly DbModel db;

        public ToDoController(DbModel db)
        {
            this.db = db;
        }

        [HttpGet("public")]
        public IActionResult GetPublicToDos([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
                                            [FromQuery] string status = "all", [FromQuery] string? priority = null,
                                            [FromQuery] string? search = null, [FromQuery] string sortDir = "desc",
                                            [FromQuery] string? sortBy = "createdAt", [FromQuery] string? dueFrom = null,
                                            [FromQuery] string? dueTo = null)
        { return GetToDos(page, pageSize, status, priority, search, sortDir, sortBy, dueFrom, dueTo, true); }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetToDos([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
                                      [FromQuery] string status = "all", [FromQuery] string? priority = null,
                                      [FromQuery] string? search = null, [FromQuery] string sortDir = "desc",
                                      [FromQuery] string? sortBy = "createdAt", [FromQuery] string? dueFrom = null,
                                      [FromQuery] string? dueTo = null)
        { return GetToDos(page, pageSize, status, priority, search, sortDir, sortBy, dueFrom, dueTo, false); }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CreateToDo(CreateToDoDTO createToDoDTO)
        {
            if (String.IsNullOrEmpty(createToDoDTO.Title))
                return BadRequest(new ErrorModel {
                  Type = "https://httpstatuses.com/400",
                  Tittle = "Validation failed",
                  StatusCode = 400,
                  Errors = new ErrorDetails { Title = new string[] { "Title is required" } }
                });

            if (createToDoDTO.Title.Length < 3 || createToDoDTO.Title.Length > 100)
                return BadRequest(new ErrorModel {
                  Type = "https://httpstatuses.com/400",
                  Tittle = "Validation failed",
                  StatusCode = 400,
                  Errors = new ErrorDetails { Title = new string[] { "Title must be between 3 and 100 characters." } }
                });

            var userId = User.FindFirst("uid")?.Value;

            var todo = new ToDoModel
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = createToDoDTO.Title,
                Details = createToDoDTO.Details ?? "",
                Priority = createToDoDTO.Priority ?? "medium",
                IsCompleted = false,
                IsPublic = createToDoDTO.IsPublic,
                DueDate = createToDoDTO.DueDate.HasValue
                    ? DateTime.SpecifyKind(createToDoDTO.DueDate.Value, DateTimeKind.Utc)
                    : (DateTime?)null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.ToDos.Add(todo);
            db.SaveChanges();

            return StatusCode(201, new {
                todo.Title,
                todo.Details,
                todo.Priority,
                todo.DueDate,
                todo.IsPublic
            });
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetToDo(string id)
        {
            var todo = db.ToDos.FirstOrDefault(t => t.Id == id);

            if (todo == null)
                return NotFound(new ErrorModel {
                  Type = "https://httpstatuses.com/404",
                  Tittle = "Not found",
                  StatusCode = 404,
                  Errors = new ErrorDetails { Title = new string[] { "ToDo not found" } }
                });

            var userId = User.FindFirst("uid")?.Value;
            if (todo.UserId != userId && !todo.IsPublic)
                return StatusCode(403, new ErrorModel {
                  Type = "https://httpstatuses.com/403",
                  Tittle = "Forbidden",
                  StatusCode = 403,
                  Errors = new ErrorDetails { Title = new string[] { "You do not have access to this ToDo" } }
                });

            return Ok(todo);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateToDo([FromRoute] string id, [FromBody] UpdateToDoDTO updateToDoDTO)
        {
            var todo = db.ToDos.FirstOrDefault(t => t.Id == id);

            if (todo == null)
                return NotFound(new ErrorModel {
                  Type = "https://httpstatuses.com/404",
                  Tittle = "Not found",
                  StatusCode = 404,
                  Errors = new ErrorDetails { Title = new string[] { "ToDo not found" } }
                });

            if (updateToDoDTO.Title != null && (updateToDoDTO.Title.Length < 3 || updateToDoDTO.Title.Length > 100))
                return BadRequest(new ErrorModel {
                  Type = "https://httpstatuses.com/400",
                  Tittle = "Bad request",
                  StatusCode = 400,
                  Errors = new ErrorDetails { Title = new string[] { "Title must be between 3 and 100 characters." } }
                });

            var userId = User.FindFirst("uid")?.Value;
            if (todo.UserId != userId)
                return StatusCode(403, new ErrorModel {
                  Type = "https://httpstatuses.com/403",
                  Tittle = "Forbidden",
                  StatusCode = 403,
                  Errors = new ErrorDetails { Title = new string[] { "You do not have access to this ToDo" } }
                });

            todo.Title = updateToDoDTO.Title ?? todo.Title;
            todo.Details = updateToDoDTO.Details ?? todo.Details;
            todo.Priority = updateToDoDTO.Priority ?? todo.Priority;
            todo.DueDate = updateToDoDTO.DueDate != default(DateTime) ? updateToDoDTO.DueDate : todo.DueDate;
            todo.IsPublic = updateToDoDTO.IsPublic ?? todo.IsPublic;
            todo.IsCompleted = updateToDoDTO.IsCompleted ?? todo.IsCompleted;
            todo.UpdatedAt = DateTime.UtcNow;

            db.ToDos.Update(todo);
            db.SaveChanges();

            return Ok(todo);
        }

        [HttpPatch("{id}/completion")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult SetCompletionToDo([FromRoute] string id, [FromBody] SetCompletedToDoDTO setCompletedToDoDTO)
        {
            var todo = db.ToDos.FirstOrDefault(t => t.Id == id);

            if (todo == null)
                return NotFound(new ErrorModel {
                  Type = "https://httpstatuses.com/404",
                  Tittle = "Not found",
                  StatusCode = 404,
                  Errors = new ErrorDetails { Title = new string[] { "ToDo not found" } }
                });

            var userId = User.FindFirst("uid")?.Value;
            if (todo.UserId != userId)
                return StatusCode(403, new ErrorModel {
                  Type = "https://httpstatuses.com/403",
                  Tittle = "Forbidden",
                  StatusCode = 403,
                  Errors = new ErrorDetails { Title = new string[] { "You do not have access to this ToDo" } }
                });

            todo.IsCompleted = setCompletedToDoDTO.IsCompleted;
            todo.UpdatedAt = DateTime.UtcNow;

            db.ToDos.Update(todo);
            db.SaveChanges();

            return Ok(new {
                isCompleted = todo.IsCompleted
            });
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteToDo([FromRoute] string id)
        {
            var todo = db.ToDos.FirstOrDefault(t => t.Id == id);

            if (todo == null)
                return NotFound(new ErrorModel {
                  Type = "https://httpstatuses.com/404",
                  Tittle = "Not found",
                  StatusCode = 404,
                  Errors = new ErrorDetails { Title = new string[] { "ToDo not found" } }
                });


            var userId = User.FindFirst("uid")?.Value;
            if (todo.UserId != userId)
                return StatusCode(403, new ErrorModel {
                  Type = "https://httpstatuses.com/403",
                  Tittle = "Forbidden",
                  StatusCode = 403,
                  Errors = new ErrorDetails { Title = new string[] { "You do not have access to this ToDo" } }
                });

            db.ToDos.Remove(todo);
            db.SaveChanges();

            return NoContent();
        }

        private IActionResult GetToDos(int page, int pageSize, string status, string? priority,
                                       string? search, string sortDir, string? sortBy, string? dueFrom, string? dueTo, bool isPublic = false)
        {
            if (page <= 0) return BadRequest(new { error = "400", message = "Page must be greater than 0" });
            if (pageSize <= 0 || pageSize > 50)
                return BadRequest(new ErrorModel {
                  Type = "https://httpstatuses.com/400",
                  Tittle = "Bad request",
                  StatusCode = 400,
                  Errors = new ErrorDetails { Title = new string[] { "PageSize must be greater than 0 and less than or equal to 50" } }
                });
            if (!String.IsNullOrEmpty(status) && status != "all" && status != "active" && status != "completed")
                return BadRequest(new ErrorModel {
                  Type = "https://httpstatuses.com/400",
                  Tittle = "Bad request",
                  StatusCode = 400,
                  Errors = new ErrorDetails { Title = new string[] { "Status must be 'all', 'active', or 'completed'" } }
                });
            if (!String.IsNullOrEmpty(sortDir) && sortDir != "asc" && sortDir != "desc")
                return BadRequest(new ErrorModel {
                  Type = "https://httpstatuses.com/400",
                  Tittle = "Bad request",
                  StatusCode = 400,
                  Errors = new ErrorDetails { Title = new string[] { "sortDir must be either 'asc' or 'desc'" } }
                });
            if (!String.IsNullOrEmpty(priority) && priority != "low" && priority != "medium" && priority != "high")
                return BadRequest(new ErrorModel {
                  Type = "https://httpstatuses.com/400",
                  Tittle = "Bad request",
                  StatusCode = 400,
                  Errors = new ErrorDetails { Title = new string[] { "Priority must be 'low', 'medium', or 'high'" } }
                });
            if (search != null && search.Length > 100)
                return BadRequest(new ErrorModel {
                  Type = "https://httpstatuses.com/400",
                  Tittle = "Bad request",
                  StatusCode = 400,
                  Errors = new ErrorDetails { Title = new string[] { "Search query must be less than or equal to 100 characters" } }
                });
            if (!String.IsNullOrEmpty(sortBy) && sortBy != "createdAt" && sortBy != "dueDate" && sortBy != "priority" && sortBy != "title")
                return BadRequest(new ErrorModel {
                  Type = "https://httpstatuses.com/400",
                  Tittle = "Bad request",
                  StatusCode = 400,
                  Errors = new ErrorDetails { Title = new string[] { "sortBy must be 'createdAt', 'dueDate', 'priority', or 'title'" } }
                });
            if (!String.IsNullOrEmpty(dueFrom) && !DateTime.TryParse(dueFrom, out _))
                return BadRequest(new ErrorModel {
                  Type = "https://httpstatuses.com/400",
                  Tittle = "Bad request",
                  StatusCode = 400,
                  Errors = new ErrorDetails { Title = new string[] { "dueFrom must be a valid date" } }
                });
            if (!String.IsNullOrEmpty(dueTo) && !DateTime.TryParse(dueTo, out _))
                return BadRequest(new ErrorModel {
                  Type = "https://httpstatuses.com/400",
                  Tittle = "Bad request",
                  StatusCode = 400,
                  Errors = new ErrorDetails { Title = new string[] { "dueTo must be a valid date" } }
                });

            var query = db.ToDos.AsQueryable();
            int totalItems;
            int totalPages;
            List<ToDoModel> todos;

            if (!String.IsNullOrEmpty(dueFrom) && DateTime.TryParse(dueFrom, out DateTime dueFromDate))
                query = query.Where(t => t.DueDate >= dueFromDate);

            if (!String.IsNullOrEmpty(dueTo) && DateTime.TryParse(dueTo, out DateTime dueToDate))
                query = query.Where(t => t.DueDate <= dueToDate);

            if (!String.IsNullOrEmpty(status))
            {
                if (status == "active")
                    query = query.Where(t => !t.IsCompleted);
                else if (status == "completed")
                    query = query.Where(t => t.IsCompleted);
            }

            if (!String.IsNullOrEmpty(priority))
                if (priority == "low" || priority == "medium" || priority == "high")
                    query = query.Where(t => t.Priority == priority);

            if (!String.IsNullOrEmpty(search))
                query = query.Where(t => t.Title.Contains(search));

            if (!String.IsNullOrEmpty(sortBy))
                switch (sortBy)
                {
                    case "createdAt":
                        if (sortDir == "asc")
                            query = query.OrderBy(t => t.CreatedAt);
                        else
                            query = query.OrderByDescending(t => t.CreatedAt);
                        break;
                    case "dueDate":
                        if (sortDir == "asc")
                            query = query.OrderBy(t => t.DueDate);
                        else
                            query = query.OrderByDescending(t => t.DueDate);
                        break;
                    case "priority":
                        if (sortDir == "asc")
                            query = query.OrderBy(t => t.Priority);
                        else
                            query = query.OrderByDescending(t => t.Priority);
                        break;
                    case "title":
                        if (sortDir == "asc")
                            query = query.OrderBy(t => t.Title);
                        else
                            query = query.OrderByDescending(t => t.Title);
                        break;
                }

            if (isPublic)
            {
                query = query.Where(t => t.IsPublic);
                totalItems = query.Count();
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                todos = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                var userId = User.FindFirst("uid")?.Value;
                query = query.Where(t => t.UserId == userId);
                totalItems = query.Count();
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                todos = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            return Ok(new {
                page,
                pageSize,
                totalItems,
                totalPages,
                items = todos
            });
        }
    }
}
