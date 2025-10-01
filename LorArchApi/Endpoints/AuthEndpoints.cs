using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LorArchApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace LorArchApi.Endpoints;


public record RegisterDto(string Email, string Password);

public record LoginDto(string Email, string Password);

public record AuthResponseDto(string Token);


public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterDto registerDto, UserManager<Usuario> userManager) =>
        {
            var userExists = await userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return Results.BadRequest("Um usuário com este e-mail já existe.");
            }

            var newUser = new Usuario
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
            };

            var result = await userManager.CreateAsync(newUser, registerDto.Password);

            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }

            return Results.Ok("Usuário registrado com sucesso.");
            
        })
        .WithName("RegisterUser")
        .WithTags("Auth")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithSummary("Registrar um novo usuário")
        .WithDescription("Cria uma nova conta de usuário com e-mail e senha.");
        
        app.MapPost("/api/auth/login", async (LoginDto loginDto, UserManager<Usuario> userManager, IConfiguration configuration) =>
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Results.Unauthorized();
            }

            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(1), 
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Results.Ok(new AuthResponseDto(
                $"Bearer {tokenString}"
            ));
        })
        .WithName("LoginUser")
        .WithTags("Auth")
        .Produces<AuthResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithSummary("Fazer login")
        .WithDescription("Autentica um usuário e retorna um token JWT.");
    }
}
