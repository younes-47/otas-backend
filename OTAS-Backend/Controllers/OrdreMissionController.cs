using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdreMissionController : ControllerBase
    {
        private readonly IOrdreMissionService _ordreMissionService;

        public OrdreMissionController(IOrdreMissionService ordreMissionService)
        {
            _ordreMissionService = ordreMissionService;
        }

        /* This get /Table endpoint is within the Request section and it is shown for all the roles
         * since everyone can perform a request, it shows all the Missions ordered */
        //[HttpGet("Table")]
        //public IActionResult OrdreMissionTable(int status)
        //{
        //    var oms = _mapper.Map<List<OrdreMissionDTO>>(_ordreMissionRepository.GetOrdresMissionByRequesterUsername());

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    return Ok(oms);

        //}



        /* This post /Request endpoint is within the Request section and it is shown for all the roles
         * since everyone can perform a request */
        [HttpPost("Request")]
        public async Task<IActionResult> RequestOrdreMission([FromBody] OrdreMissionPostDTO ordreMissionRequest)
        {  
            if(!ModelState.IsValid) return BadRequest(ModelState);

            ServiceResult omResult = await _ordreMissionService.CreateOrdreMissionWithAvanceVoyage(ordreMissionRequest);

            if (!omResult.Success) return Ok(omResult.ErrorMessage);

            return Ok(omResult.SuccessMessage); 
        }





    }
}
