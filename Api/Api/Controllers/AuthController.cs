using Api.Data;
using Api.Models;
using Api.Models.DTO;
using Api.utility;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private string secretKey;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(ApplicationDbContext context, IConfiguration configuration,
        RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequsetDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid data");

        // بررسی اینکه کاربر از قبل وجود دارد یا خیر
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
            return BadRequest("Email is already registered");

        // ایجاد کاربر جدید
        var newUser = new ApplicationUser
        {
            Email = model.Email,
            UserName = model.Email,
            Name = model.Name,
            NormalizedEmail = model.Email.ToUpper()
        };

        var result = await _userManager.CreateAsync(newUser, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // ایجاد نقش‌ها در صورت وجود نداشتن
        if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
            await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));

        if (!await _roleManager.RoleExistsAsync(SD.Role_Customer))
            await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));

        // اختصاص نقش به کاربر
        if (string.Equals(model.Role, SD.Role_Admin, StringComparison.OrdinalIgnoreCase))
            await _userManager.AddToRoleAsync(newUser, SD.Role_Admin);
        else
            await _userManager.AddToRoleAsync(newUser, SD.Role_Customer);

        return Ok("Register successfully");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequsetDto model)
    {
        var userFromDb = await _context.ApplicationUsers
            .FirstOrDefaultAsync(x => x.UserName.ToLower() == model.UserName.ToLower());

        if (userFromDb == null)
            return BadRequest("username or password is incorrect");

        bool isValid = await _userManager.CheckPasswordAsync(userFromDb, model.Password);
        if (!isValid)
            return BadRequest("username or password is incorrect");

        var roles = await _userManager.GetRolesAsync(userFromDb);
        var role = roles.FirstOrDefault() ?? "User";

        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim("fullName", userFromDb.Name ?? ""),
            new Claim("id", userFromDb.Id.ToString()),
            new Claim(ClaimTypes.Email, userFromDb.Email ?? ""),
            new Claim(ClaimTypes.Role, role)
        }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        var response = new LoginResonseDto
        {
            Email = userFromDb.Email,
            Token = tokenHandler.WriteToken(token),
            UserRole = role
        };

        return Ok(response);
    }
 

}
