using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TaskManagerApi.Data;
using TaskManagerApi.Models;
using TaskManagerApi.Models.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskManagerApi.Models.ViewMoels;

namespace TaskManagerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuarioController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserViewModel userViewModel)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Username == userViewModel.Username))
                return BadRequest("Username já existe");

            var usuario = new Usuario
            {
                Username = userViewModel.Username,
                CreatedAt = DateTime.UtcNow
            };

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userViewModel.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuário registrado com sucesso");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserViewModel userViewModel)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == userViewModel.Username);

            if (usuario == null)
                return Unauthorized("Usuário ou senha incorretos");

            if (!BCrypt.Net.BCrypt.Verify(userViewModel.Password, usuario.PasswordHash))
            {
                return Unauthorized("Usuário ou senha incorretos");
            }

            var token = GenerateJwtToken(usuario);

            return Ok(new { token });
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
