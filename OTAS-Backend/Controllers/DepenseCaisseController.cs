using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
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
        //public async Task<IActionResult> AddDepenseCaisse([FromForm] IFormFile receipts, [FromForm] string depenseCaisse)
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
                    var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");

                    if (!Directory.Exists(uploadsFolderPath))
                    {
                        Directory.CreateDirectory(uploadsFolderPath);
                    }

                    var uniqueReceiptsFileName = Guid.NewGuid().ToString() + "_DP_" + depenseCaisse.Id + "_" + depenseCaisse.UserId;

                    var filePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);

                    //using var fileStream = new FileStream(filePath, FileMode.Create);

                    using System.IO.File.WriteAllBytes(filePath, depenseCaisse.ReceiptsFile);

                    //receipts.CopyTo(fileStream);

                    receiptsFilePath = filePath;
                }
                catch (Exception ex)
                {
                    return BadRequest($"ERROR:{ex.Message} |||||||||| {ex.Source} |||||||||| {ex.InnerException}");
                }
            }

            // File uploaded and path assigned now deal with the json

            //ServiceResult result = await _depenseCaisseService.AddDepenseCaisse(depenseCaisse, receiptsFilePath);
            //if (!result.Success) return BadRequest($"{result.Message}");

            //return Ok(result.Message);
            return Ok();
        }



    }
}
