using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Post;
using OTAS.Interfaces.IService;
using OTAS.Services;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepenseCaisseController : ControllerBase
    {
        private readonly IDepenseCaisseService _depenseCaisseService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DepenseCaisseController(IDepenseCaisseService depenseCaisseService,
            IWebHostEnvironment webHostEnvironment)
        {
            _depenseCaisseService = depenseCaisseService;
            _webHostEnvironment = webHostEnvironment;
        }

        //Requester
        [HttpPost("Create")]
        public async Task<IActionResult> AddDepenseCaisse([FromBody] DepenseCaissePostDTO depenseCaisse)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            string receiptsFilePath;

            if (depenseCaisse.ReceiptsFile == null || depenseCaisse.ReceiptsFile.Length == 0)
            {
                return BadRequest("No file uploaded");
            }
            else
            {
                try
                {
                    // _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files)
                    var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Depense-Caisse");

                    if (!Directory.Exists(uploadsFolderPath))
                    {
                        Directory.CreateDirectory(uploadsFolderPath);
                    }
                    var uniqueReceiptsFileName = "DC_" + depenseCaisse.UserId + "__" + Guid.NewGuid().ToString() + ".pdf";
                    var filePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);
                    await System.IO.File.WriteAllBytesAsync(filePath, depenseCaisse.ReceiptsFile);
                    receiptsFilePath = filePath;
                }
                catch (Exception ex)
                {
                    return BadRequest($"ERROR: {ex.Message} |||||||||| {ex.Source} |||||||||| {ex.InnerException}");
                }
            }
            
            //File uploaded and path assigned now deal with the json

            ServiceResult result = await _depenseCaisseService.AddDepenseCaisse(depenseCaisse, receiptsFilePath);
            if (!result.Success) return BadRequest($"{result.Message}");

            return Ok(result.Message);
        }



    }
}
