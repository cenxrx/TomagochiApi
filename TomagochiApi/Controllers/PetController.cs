using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TomagochiApi.Services;

namespace TomagochiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PetController : ControllerBase
{
    private readonly PetService _petService;

    public PetController(PetService petService)
    {
        _petService = petService;
    }

    /// <summary>
    /// Создать питомца для пользователя (требуется JWT)
    /// </summary>
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreatePet([FromQuery] string petName)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized("Пользователь не найден в токене");

        try
        {
            var pet = await _petService.CreatePetForUser(userIdClaim, petName);
            return CreatedAtAction(nameof(GetPet), new { userIdClaim }, pet);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при создании питомца: {ex.Message}");
        }
    }
    /// <summary>
    /// Переименовать питомца (требуется JWT)
    /// </summary>
    [HttpPost("update-name")]
    [Authorize]
    public async Task<IActionResult> UpdatePetName([FromQuery] string newName)
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Пользователь не найден в токене");

            var updatedPet = await _petService.UpdatePetName(userIdClaim, newName);
            return Ok(updatedPet);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); // 404
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message); // 400
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при переименовании: {ex.Message}"); // 500
        }
    }
    /// <summary>
    /// Получить питомца пользователя (требуется JWT)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetPet()
    {
        var userId = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Пользователь не найден в токене");

        try
        {
            var pet = await _petService.GetPetByUserId(userId);
            return Ok(pet);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при получении питомца: {ex.Message}");
        }
    }
    /// <summary>
    /// Покормить питомца (требуется JWT)
    /// </summary>
    [HttpPost("feed")]
    [Authorize]
    public async Task<IActionResult> FeedPet([FromQuery] string foodName)
    {
        var userId = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Пользователь не найден в токене");

        try
        {
            await _petService.FeedPet(userId, foodName);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при кормлении: {ex.Message}");
        }
    }

    /// <summary>
    /// Поиграть с питомцем (требуется JWT)
    /// </summary>
    [HttpPost("play")]
    [Authorize]
    public async Task<IActionResult> PlayWithPet()
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            await _petService.PlayWithPet(userIdClaim);
            return NoContent(); // 204 No Content
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); // 404 Not Found
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message); // 400 Bad Request
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при игре: {ex.Message}");
        }
    }

    /// <summary>
    /// Почистить питомца (требуется JWT)
    /// </summary>
    [HttpPost("clean")]
    [Authorize]
    public async Task<IActionResult> CleanPet( [FromQuery] string cleanItemName)
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            await _petService.CleanPet(userIdClaim, cleanItemName);
            return NoContent(); // 204 No Content
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); // 404 Not Found
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message); // 400 Bad Request
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при чистке: {ex.Message}");
        }
    }

    /// <summary>
    /// Начать сон питомца (требуется JWT)
    /// </summary>
    [HttpPost("sleep/start")]
    [Authorize]
    public async Task<IActionResult> StartSleep()
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            await _petService.StartSleep(userIdClaim);
            return NoContent(); // 204 No Content
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); // 404 Not Found
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message); // 400 Bad Request
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при укладывании спать: {ex.Message}");
        }
    }

    /// <summary>
    /// Удалить питомца пользователя (требуется JWT)
    /// </summary>
    [HttpDelete("{userId}")]
    [Authorize]
    public async Task<IActionResult> DeletePet()
    {
        try
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            await _petService.DeletePetForUser(userIdClaim);
            return NoContent(); // 204 No Content
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message); // 404 Not Found
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при удалении питомца: {ex.Message}");
        }
    }
}