using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.Data;
using TaskManagerApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManagerApi.Models.ViewModels;

namespace TaskManagerApi.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("api/[controller]")]
    public class TarefaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TarefaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tarefa>>> GetTarefas()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized("Usuário não autenticado");

            int parsedUserId = int.Parse(userId);

            var tarefas = await _context.Tarefas
                .Where(t => t.UserId == parsedUserId)
                .ToListAsync();

            return Ok(tarefas);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Tarefa>> GetTarefa(int id)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);

            if (tarefa == null)
                return NotFound("Tarefa não encontrada");

            return Ok(tarefa);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTarefa([FromBody] TarefaViewModel tarefaVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized("Usuário não autenticado");


            Tarefa tarefa = new Tarefa();

            tarefa.Title = tarefaVM.Title;
            tarefa.Description = tarefaVM.Description;
            tarefa.IsCompleted = tarefaVM.IsCompleted;
            tarefa.Category = tarefaVM.Category;
            tarefa.UpdatedAt = DateTime.UtcNow;
            tarefa.CreatedAt = DateTime.UtcNow;
            tarefa.UserId = int.Parse(userId);

            _context.Tarefas.Add(tarefa);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTarefa), new { id = tarefa.Id }, tarefa);
        }

        [HttpGet("GetTasks/")]
        public async Task<IActionResult> GetTasks([FromQuery] string category, [FromQuery] int? userId)
        {
            var query = _context.Tarefas.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.Category == category);
            }

            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId.Value);
            }

            var tasks = await query.ToListAsync();
            return Ok(tasks);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTarefa(int id, [FromBody] TarefaViewModel tarefaAtualizada)
        {
            var tarefaExistente = await _context.Tarefas.FindAsync(id);
            if (tarefaExistente == null)
                return NotFound("Tarefa não encontrada");

            tarefaExistente.Title = tarefaAtualizada.Title;
            tarefaExistente.Description = tarefaAtualizada.Description;
            tarefaExistente.IsCompleted = tarefaAtualizada.IsCompleted;
            tarefaExistente.Category = tarefaAtualizada.Category;
            tarefaExistente.UpdatedAt = DateTime.UtcNow;

            _context.Entry(tarefaExistente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarefa(int id)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);
            if (tarefa == null)
                return NotFound("Tarefa não encontrada");

            _context.Tarefas.Remove(tarefa);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPut("{id}/completa")]
        public async Task<IActionResult> AlterarCumprida(int id, [FromQuery] bool isCompleted)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);

            if (tarefa == null)
                return NotFound("Tarefa não encontrada");

            if (id != tarefa.Id)
                return BadRequest("ID da tarefa não coincide");

            tarefa.IsCompleted = isCompleted;

            _context.Entry(tarefa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
