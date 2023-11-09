using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.Interfaces.IRepository;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusHistoryController : ControllerBase
    {
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        public StatusHistoryController(IStatusHistoryRepository statusHistoryRepository)
        {
            _statusHistoryRepository = statusHistoryRepository;
        }




    }
}
