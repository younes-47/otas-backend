using Microsoft.AspNetCore.Mvc;
using OTAS.Interfaces.IRepository;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepenseCaisseController : ControllerBase
    {
        private readonly IDepenseCaisseRepository _depenseCaisseRepository;
        public DepenseCaisseController(IDepenseCaisseRepository depenseCaisseRepository)
        {
            _depenseCaisseRepository = depenseCaisseRepository;
        }



    }
}
