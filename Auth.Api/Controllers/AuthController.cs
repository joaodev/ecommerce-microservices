using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Auth.Api.Data;
using Auth.Api.DTOs;
using Auth.Api.Models;
using Auth.Api.Services;

namespace Auth.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly PasswordHasher _hasher;
    private readonly TokenService _tokenService;

    public AuthController(AuthDbContext db, PasswordHasher hasher, TokenService tokenService)
    {
        _db = db;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
            return Conflict("E-mail ja cadastrado.");

        var user = new User { Email = request.Email };
        user.PasswordHash = _hasher.Hash(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Email));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
            return Unauthorized("Credenciais invalidas.");

        var valid = _hasher.Verify(user, user.PasswordHash, request.Password);
        if (!valid)
            return Unauthorized("Credenciais invalidas.");

        var token = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Email));
    }
}