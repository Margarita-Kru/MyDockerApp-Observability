using Microsoft.AspNetCore.Mvc;
using MyDockerApp.Dtos;

namespace MyDockerApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public RegistrationController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterNewUser([FromForm] UserRegistrationDto dto)
        {
            if (dto.Avatar == null || dto.Avatar.Length == 0)
            {
                return BadRequest("Файл не прикреплен.");
            }

            if (dto.Avatar.Length > 2 * 1024 * 1024)
            {
                return BadRequest("Размер файла не должен превышать 2 МБ.");
            }

            var extension = Path.GetExtension(dto.Avatar.FileName).ToLower();
            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png") {

                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadFolder);

                var newFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadFolder, newFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.Avatar.CopyToAsync(stream);
            }
            else
            {
                return BadRequest("Разрешены только файлы .jpg, .jpeg, .png");
            }
            return RedirectToAction(nameof(Success), new { userName = dto.UserName });
        }

        [HttpGet("success")]
        public IActionResult Success([FromQuery] string userName)
        {
            return Ok($"Пользователь {userName} успешно зарегистрирован.");
        }
    }
}
