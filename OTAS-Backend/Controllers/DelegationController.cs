using Microsoft.AspNetCore.Mvc;
using OTAS.Interfaces.IRepository;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DelegationController : ControllerBase
    {
        private readonly IDelegationRepository _delegationRepository;
        public DelegationController(IDelegationRepository delegationRepository)
        {
            _delegationRepository = delegationRepository;
        }
    }




}
