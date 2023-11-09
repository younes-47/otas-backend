using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.Interfaces.IRepository;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ITripRepository _tripRepository;
        public TripController(ITripRepository tripRepository)
        {
            _tripRepository = tripRepository;
        }



    }
}
