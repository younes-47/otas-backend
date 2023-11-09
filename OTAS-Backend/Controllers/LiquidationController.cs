using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.Interfaces.IRepository;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiquidationController : ControllerBase
    {
        private readonly ILiquidationRepository _liquidationRepository;
        public LiquidationController(ILiquidationRepository liquidationRepository)
        {
            _liquidationRepository = liquidationRepository;
        }




    }
}
