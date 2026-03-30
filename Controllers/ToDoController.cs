using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/todos")]
    public class ToDoController : ControllerBase
    {
        [HttpGet("/public")]
        public IActionResult GetPublicToDos()
        {
            var todos = new List<ToDoModel>
            {
                new ToDoModel { Id = "1", Title = "Buy groceries", IsCompleted = false },
                new ToDoModel { Id = "2", Title = "Walk the dog", IsCompleted = true }
            };

            return Ok(todos);
        }

        [HttpGet]
        public IActionResult GetToDos()
        {
            var todos = new List<ToDoModel>
            {
                new ToDoModel { Id = "1", Title = "Buy groceries", IsCompleted = false },
                new ToDoModel { Id = "2", Title = "Walk the dog", IsCompleted = true }
            };

            return Ok(todos);
        }

        [HttpPost]
        public IActionResult CreateToDo(CreateToDoDTO createToDoDTO)
        {
            var todo = new ToDoModel
            {
                Id = Guid.NewGuid().ToString(),
                Title = createToDoDTO.Title,
                IsCompleted = false
            };

            return Ok(new {
                todo.Title,
                todo.Details,
                todo.Priority,
                todo.DueDate,
                todo.IsPublic
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetToDo(string id)
        {
            var todo = new ToDoModel
            {
                Id = id,
                Title = "Buy groceries",
                IsCompleted = false
            };

            return Ok(todo);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateToDo(string id, UpdateToDoDTO updateToDoDTO)
        {
            var todo = new ToDoModel
            {
                Id = id,
                Title = updateToDoDTO.Title,
                IsCompleted = updateToDoDTO.IsCompleted
            };

            return Ok(todo);
        }

        [HttpPatch("{id}/completion")]
        public IActionResult SetCompletionToDo(string id, SetCompletedToDoDTO setCompletedToDoDTO)
        {
            var todo = new ToDoModel
            {
                Id = id,
                Title = "Buy groceries",
                IsCompleted = setCompletedToDoDTO.IsCompleted
            };

            return Ok(new {
                isCompleted = todo.IsCompleted
            });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteToDo(string id)
        {
            return NoContent();
        }
    }
}