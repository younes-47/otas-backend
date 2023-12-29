using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;

namespace OTAS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LiquidationController : ControllerBase
    {
        private readonly ILiquidationRepository _liquidationRepository;
        public LiquidationController(ILiquidationRepository liquidationRepository)
        {
            _liquidationRepository = liquidationRepository;
        }

        [Authorize(Roles = "requester, decider")]
        [HttpGet("Create")]
        public async Task<IActionResult> LiquidateAvanceVoyage(AvanceVoyageLiquidationPostDTO avanceVoyageLiquidation)
        {

        }


    }
}
