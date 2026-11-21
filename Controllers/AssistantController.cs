using Microsoft.AspNetCore.Mvc;
using NyayaSetu.Interfaces;

namespace NyayaSetu.Controllers
{
    public class AssistantController : Controller
    {
        private readonly IAiAssistantService _assistant;

        public AssistantController(IAiAssistantService assistant)
        {
            _assistant = assistant;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Ask([FromBody] AssistantPromptRequest request)
        {
            var lang = Request.Cookies["NyayaSetu.Lang"] ?? "en";
            var reply = _assistant.Ask(request?.Question ?? string.Empty, lang);
            return Json(reply);
        }

        public record AssistantPromptRequest(string Question);
    }
}
