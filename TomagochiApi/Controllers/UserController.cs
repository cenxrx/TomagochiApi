using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomagochiApi.Models;
using TomagochiApi.Services;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Регистрация нового пользователя (без JWT)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(User user)
    {
        try
        {
            var createdUser = await _userService.CreateUser(user);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при регистрации: {ex.Message}");
        }
    }

    /// <summary>
    /// Получить пользователя по ID (требуется JWT)
    /// </summary>
    [HttpGet("")]
    [Authorize]
    public async Task<IActionResult> GetUser()
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim != null)
            {
                var user = await _userService.GetUserById(userIdClaim);
                return Ok(user);
            }
            else
            {
                return Unauthorized();
            }
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Пользователь не найден");
        }
    }

    /// <summary>
    /// Получить инвентарь пользователя (требуется JWT)
    /// </summary>
    [HttpGet("/inventory")]
    [Authorize]
    public async Task<IActionResult> GetUserInventory()
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim != null)
            {
                var inventory = await _userService.GetInventoryByUserId(userIdClaim);
                return Ok(inventory);
            }
            else
                return Unauthorized();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Получить питомца пользователя (требуется JWT)
    /// </summary>
    [HttpGet("/pet")]
    [Authorize]
    public async Task<IActionResult> GetUserPet()
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            var pet = await _userService.GetPetByUserId(userIdClaim);
            return Ok(pet);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Обновить данные пользователя (требуется JWT)
    /// </summary>
    [HttpPut()]
    [Authorize]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto updateDto)
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim != null)
                await _userService.UpdateUser(userIdClaim, updateDto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Пользователь не найден");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Удалить пользователя (требуется JWT)
    /// </summary>
    [HttpDelete()]
    [Authorize]
    public async Task<IActionResult> DeleteUser()
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim != null) await _userService.DeleteUser(userIdClaim);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Пользователь не найден");
        }
    }

    /// <summary>
    /// Вход в систему (без JWT)
    /// </summary>
    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        try
        {
            var token = await _userService.AuthenticateUser(login.Email, login.Password);

            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            // Логируйте ошибку (например, в файл или консоль)
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}

// DTO для входа
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}