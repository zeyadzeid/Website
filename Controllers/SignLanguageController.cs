using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;

namespace UsersApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignLanguageController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;

        public SignLanguageController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> ConvertImageToText()
        {
            try
            {
                var form = await Request.ReadFormAsync();
                var file = form.Files.GetFile("image");

                if (file == null || file.Length == 0)
                    return BadRequest("لم يتم إرسال أي صورة.");

                // إرسال الملف إلى FastAPI
                using var client = _clientFactory.CreateClient();
                using var content = new MultipartFormDataContent();

                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "file", file.FileName);

                var response = await client.PostAsync("http://127.0.0.1:8000/predict", content); // عدّل لو FastAPI شغال على عنوان آخر

                if (!response.IsSuccessStatusCode)
                    return StatusCode(500, "فشل الاتصال بخدمة التعرف.");

                var resultJson = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonDocument.Parse(resultJson);
                var text = result.RootElement.GetProperty("result").GetString();

                return Ok(new { text });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "حدث خطأ داخلي: " + ex.Message);
            }
        }
    }
}
