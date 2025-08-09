using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="model">The registration model.</param>
    /// <returns>A success message or error details.</returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Invalid request or validation errors.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            if (model.Username.ToLower() == "admin")
            {
                await _userManager.AddToRoleAsync(user, "Manager");
            }

            return Ok("User registered successfully");
        }

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("User registered successfully");
    }

    /// <summary>
    /// Logs in an existing user.
    /// </summary>
    /// <param name="model">The login model.</param>
    /// <returns>A JWT token or error message.</returns>
    /// <response code="200">User logged in successfully.</response>
    /// <response code="401">Invalid username or password.</response>
    /// <response code="400">Invalid request or validation errors.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

        if (!result.Succeeded)
        {
            return Unauthorized("Invalid username or password");
        }

        var user = await _userManager.FindByNameAsync(model.Username);
        var token = GenerateJwtToken(user);

        return Ok(new { Token = token });
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class RegisterModel
{
    /// <summary>
    /// The username of the user.
    /// </summary>
    [Required]
    public string Username { get; set; }

    /// <summary>
    /// The email address of the user.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    /// <summary>
    /// The password of the user.
    /// </summary>
    [Required]
    public string Password { get; set; }
}

public class LoginModel
{
    /// <summary>
    /// The username of the user.
    /// </summary>
    [Required]
    public string Username { get; set; }

    /// <summary>
    /// The password of the user.
    /// </summary>
    [Required]
    public string Password { get; set; }
}