using Backend.DTO;
using Backend.Models;
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
        public IActionResult GetPublicToDos([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string status = "")
        {
            return GetToDos(page, pageSize, status, true);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetToDos([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string status = "")
        {
            return GetToDos(page, pageSize, status, false);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CreateToDo(CreateToDoDTO createToDoDTO)
        {
            if (String.IsNullOrEmpty(createToDoDTO.Title))
                return BadRequest(new { error = "400", message = "Title is required" });

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
                DueDate = createToDoDTO.DueDate ?? null,
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
                return NotFound(new { error = "404", message = "ToDo not found" });

            var userId = User.FindFirst("uid")?.Value;
            if (todo.UserId != userId && !todo.IsPublic)
                return StatusCode(403, new { error = "403", message = "You do not have access to this ToDo" });

            return Ok(todo);
        }

        [HttpPut("{id}/{title}/{details}/{priority}/{dueDate}/{isPublic}/{isCompleted}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateToDo([FromRoute] string id, [FromRoute] UpdateToDoDTO updateToDoDTO)
        {
            var todo = db.ToDos.FirstOrDefault(t => t.Id == id);

            if (todo == null)
                return NotFound(new { error = "404", message = "ToDo not found" });

            var userId = User.FindFirst("uid")?.Value;
            if (todo.UserId != userId)
                return StatusCode(403, new { error = "403", message = "You do not have access to this ToDo" });

            todo.Title = updateToDoDTO.Title ?? todo.Title;
            todo.Details = updateToDoDTO.Details ?? todo.Details;
            todo.Priority = updateToDoDTO.Priority ?? todo.Priority;
            todo.DueDate = updateToDoDTO.DueDate != default(DateTime) ? updateToDoDTO.DueDate : todo.DueDate;
            todo.IsPublic = updateToDoDTO.IsPublic;
            todo.IsCompleted = updateToDoDTO.IsCompleted;
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
        public IActionResult SetCompletionToDo([FromRoute] string id, SetCompletedToDoDTO setCompletedToDoDTO)
        {
            var todo = db.ToDos.FirstOrDefault(t => t.Id == id);

            if (todo == null)
                return NotFound(new { error = "404", message = "ToDo not found" });

            var userId = User.FindFirst("uid")?.Value;
            if (todo.UserId != userId)
                return StatusCode(403, new { error = "403", message = "You do not have access to this ToDo" });

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
                return NotFound(new { error = "404", message = "ToDo not found" });

            var userId = User.FindFirst("uid")?.Value;
            if (todo.UserId != userId)
                return StatusCode(403, new { error = "403", message = "You do not have access to this ToDo" });

            db.ToDos.Remove(todo);
            db.SaveChanges();

            return NoContent();
        }

        private IActionResult GetToDos(int page, int pageSize, string status, bool isPublic)
        {
            var query = db.ToDos.AsQueryable();
            int totalItems;
            int totalPages;
            List<ToDoModel> todos;
    
            if (!String.IsNullOrEmpty(status))
            {
                if (status == "active")
                    query = query.Where(t => !t.IsCompleted);
                else if (status == "completed")
                    query = query.Where(t => t.IsCompleted);
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