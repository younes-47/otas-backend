using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using OTAS.Models;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Repository;

namespace OTAS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IDeciderRepository _deciderRepository;
        private readonly ILdapAuthenticationService _ldapAuthenticationService;
        private readonly ILiquidationRepository _liquidationRepository;
        private readonly OtasContext _context;
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IAvanceCaisseRepository _avanceCaisseRepository;
        private readonly IDepenseCaisseRepository _depenseCaisseRepository;
        private readonly IMapper _mapper;
        public UserController(IUserRepository userRepository,
            IDeciderRepository deciderRepository,
            ILdapAuthenticationService ldapAuthenticationService,
            ILiquidationRepository liquidationRepository,
            OtasContext context,
            IAvanceVoyageRepository avanceVoyageRepository,
            IAvanceCaisseRepository avanceCaisseRepository,
            IDepenseCaisseRepository depenseCaisseRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _deciderRepository = deciderRepository;
            _ldapAuthenticationService = ldapAuthenticationService;
            _liquidationRepository = liquidationRepository;
            _context = context;
            _avanceVoyageRepository = avanceVoyageRepository;
            _avanceCaisseRepository = avanceCaisseRepository;
            _depenseCaisseRepository = depenseCaisseRepository;
            _mapper = mapper;
        }
        [Authorize(Roles = "requester,decider")]
        [HttpPost("preferredLanguage/{lang}")]
        public async Task<IActionResult> SetPreferredLanguage(string lang)
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (user == null) { return BadRequest(); }
            user.PreferredLanguage = lang;
            await _userRepository.UpdateUserAsync(user);
            var output = new { user.PreferredLanguage };
            return Ok(output);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("info")]
        public async Task<UserInfoDTO> GetUserInformation()
        {
            string username = HttpContext.User.Identity.Name;
            var userInfo = _ldapAuthenticationService.GetUserInformation(username);
            var userId = await _userRepository.GetUserIdByUsernameAsync(username);
            userInfo.PreferredLanguage = await _userRepository.GetPreferredLanguageByUserIdAsync(userId);
            return userInfo;
        }

        [Authorize(Roles = "decider")]
        [HttpGet("DeciderLevels")]
        public async Task<List<string>> GetDeciderLevels()
        {
            string username = HttpContext.User.Identity.Name;
            var userId = await _userRepository.GetUserIdByUsernameAsync(username);
            return await _deciderRepository.GetDeciderLevelsByUserId(userId);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("ActualRequesterStaticInfo/All")]
        public async Task<IActionResult> GetActualRequesterStaticInfo()
        {
            ActualRequesterStaticInfoDTO staticData = new();
            staticData.Departments = await _context.Departments.Select(dp => dp.Name).ToListAsync();
            staticData.Managers = await _deciderRepository.GetManagersInfo();
            staticData.JobTitles = _ldapAuthenticationService.GetJobTitles();

            return Ok(staticData);
        }

        //[Authorize(Roles = "requester,decider")]
        //[HttpGet("requester/Stats")]
        //public async Task<IActionResult> GetRequesterStats()
        //{
        //    ActualRequesterStaticInfoDTO staticData = new();
        //    staticData.Departments = await _context.Departments.Select(dp => dp.Name).ToListAsync();
        //    staticData.ManagersUsernames = await _deciderRepository.GetManagersUsernames();
        //    staticData.JobTitles = _ldapAuthenticationService.GetJobTitles();

        //    return Ok(staticData);

        [Authorize(Roles = "requester,decider")]
        [HttpGet("Requester/Stats")]
        public async Task<IActionResult> GetRequesterStats()
        {
            string username = HttpContext.User.Identity.Name;
            var userId = await _userRepository.GetUserIdByUsernameAsync(username);

            StatsDTO stats = new();

            stats.PendingOrdreMissionsCount = _context.OrdreMissions.Where(av => av.UserId == userId)
               .Where(av => av.LatestStatus != 99 && av.LatestStatus != 97 && av.LatestStatus != 16 && av.LatestStatus != 7).Count();

            stats.PendingAvanceVoyagesCount = _context.AvanceVoyages.Where(av => av.UserId == userId)
               .Where(av => av.LatestStatus != 99 && av.LatestStatus != 97 && av.LatestStatus != 16 && av.LatestStatus != 7).Count();

            stats.PendingAvanceCaissesCount = _context.AvanceCaisses.Where(av => av.UserId == userId)
               .Where(av => av.LatestStatus != 99 && av.LatestStatus != 97 && av.LatestStatus != 16 && av.LatestStatus != 7).Count();

            stats.PendingDepenseCaissesCount = _context.DepenseCaisses.Where(av => av.UserId == userId)
               .Where(av => av.LatestStatus != 99 && av.LatestStatus != 97 && av.LatestStatus != 16 && av.LatestStatus != 7).Count();

            stats.PendingLiquidationsCount = _context.Liquidations.Where(av => av.UserId == userId)
               .Where(av => av.LatestStatus != 99 && av.LatestStatus != 97 && av.LatestStatus != 16 && av.LatestStatus != 7).Count();


            var finalizedAvancesCaisses = await _avanceCaisseRepository.GetFinalizedAvanceCaissesForRequesterStats(userId);
            stats.FinalizedAvanceCaissesCount = finalizedAvancesCaisses.Count;
            foreach (var ac in finalizedAvancesCaisses)
            {
                if (ac.Currency == "MAD")
                {
                    stats.FinalizedAvanceCaissesMADCount += ac.EstimatedTotal;
                }
                else
                {
                    stats.FinalizedAvanceCaissesEURCount += ac.EstimatedTotal;
                }
            }

            var finalizedAvancesVoyages = await _avanceVoyageRepository.GetFinalizedAvanceVoyagesForRequesterStats(userId);
            stats.FinalizedAvanceVoyagesCount = finalizedAvancesVoyages.Count;
            foreach (var av in finalizedAvancesVoyages)
            {
                if (av.Currency == "MAD")
                {
                    stats.FinalizedAvanceVoyagesMADCount += av.EstimatedTotal;
                }
                else
                {
                    stats.FinalizedAvanceVoyagesEURCount += av.EstimatedTotal;
                }
            }

            var finalizedDepenseCaisses = await _depenseCaisseRepository.GetFinalizedDepenseCaissesForRequesterStats(userId);
            stats.FinalizedDepenseCaissesCount = finalizedDepenseCaisses.Count;
            foreach (var dc in finalizedDepenseCaisses)
            {
                if (dc.Currency == "MAD")
                {
                    stats.FinalizedDepenseCaissesMADCount += dc.Total;
                }
                else
                {
                    stats.FinalizedDepenseCaissesEURCount += dc.Total;
                }
            }

            stats.ToLiquidateRequestsCount = await _liquidationRepository.GetRequestsToLiquidateCountForRequester(userId);
            stats.ToLiquidateRequestsMADCount = await _liquidationRepository.GetRequestsToLiquidateTotalByCurrencyForRequester("MAD", userId);
            stats.ToLiquidateRequestsEURCount = await _liquidationRepository.GetRequestsToLiquidateTotalByCurrencyForRequester("MAD", userId);

            stats.OngoingLiquidationsCount = await _liquidationRepository.GetOngoingLiquidationsCountForRequester(userId);
            stats.OngoingLiquidationsMADCount = await _liquidationRepository.GetOngoingLiquidationsTotalByCurrencyForRequester("MAD", userId);
            stats.OngoingLiquidationsEURCount = await _liquidationRepository.GetOngoingLiquidationsTotalByCurrencyForRequester("EUR", userId);

            stats.FinalizedLiquidationsCount = await _liquidationRepository.GetFinalizedLiquidationsCountForRequester(userId);
            stats.FinalizedLiquidationsMADCount = await _liquidationRepository.GetFinalizedLiquidationsTotalByCurrencyForRequester("MAD", userId);
            stats.FinalizedLiquidationsEURCount = await _liquidationRepository.GetFinalizedLiquidationsTotalByCurrencyForRequester("EUR", userId);

            return Ok(stats);
        }

        [Authorize(Roles = "decider")]
        [HttpGet("Decider/Stats")]
        public async Task<IActionResult> GetDeciderStats()
        {
            string username = HttpContext.User.Identity.Name;
            var userId = await _userRepository.GetUserIdByUsernameAsync(username);

            StatsDTO stats = new();

            stats.PendingOrdreMissionsCount = _context.OrdreMissions.Where(om => om.NextDeciderUserId == userId).Count();
            stats.PendingAvanceVoyagesCount = _context.AvanceVoyages.Where(av => av.NextDeciderUserId == userId).Count();
            stats.PendingAvanceCaissesCount = _context.AvanceCaisses.Where(ac => ac.NextDeciderUserId == userId).Count();
            stats.PendingDepenseCaissesCount = _context.DepenseCaisses.Where(dc => dc.NextDeciderUserId == userId).Count();
            stats.PendingLiquidationsCount = _context.Liquidations.Where(l => l.NextDeciderUserId == userId).Count();


            var finalizedAvancesCaisses = await _avanceCaisseRepository.GetFinalizedAvanceCaissesForDeciderStats(userId);
            stats.FinalizedAvanceCaissesCount = finalizedAvancesCaisses.Count;
            foreach (var ac in finalizedAvancesCaisses)
            {
                if (ac.Currency == "MAD")
                {
                    stats.FinalizedAvanceCaissesMADCount += ac.EstimatedTotal;
                }
                else
                {
                    stats.FinalizedAvanceCaissesEURCount += ac.EstimatedTotal;
                }
            }

            var finalizedAvancesVoyages = await _avanceVoyageRepository.GetFinalizedAvanceVoyagesForDeciderStats(userId);
            stats.FinalizedAvanceVoyagesCount = finalizedAvancesVoyages.Count;
            foreach (var av in finalizedAvancesVoyages)
            {
                if (av.Currency == "MAD")
                {
                    stats.FinalizedAvanceVoyagesMADCount += av.EstimatedTotal;
                }
                else
                {
                    stats.FinalizedAvanceVoyagesEURCount += av.EstimatedTotal;
                }
            }

            var finalizedDepenseCaisses = await _depenseCaisseRepository.GetFinalizedDepenseCaissesForDeciderStats(userId);
            stats.FinalizedDepenseCaissesCount = finalizedDepenseCaisses.Count;
            foreach (var dc in finalizedDepenseCaisses)
            {
                if (dc.Currency == "MAD")
                {
                    stats.FinalizedDepenseCaissesMADCount += dc.Total;
                }
                else
                {
                    stats.FinalizedDepenseCaissesEURCount += dc.Total;
                }
            }

            stats.ToLiquidateRequestsCount = await _liquidationRepository.GetRequestsToLiquidateCountForDecider();
            stats.ToLiquidateRequestsMADCount = await _liquidationRepository.GetRequestsToLiquidateTotalByCurrencyForDecider("MAD");
            stats.ToLiquidateRequestsEURCount = await _liquidationRepository.GetRequestsToLiquidateTotalByCurrencyForDecider("EUR");

            stats.OngoingLiquidationsCount = await _liquidationRepository.GetOngoingLiquidationsCountForDecider();
            stats.OngoingLiquidationsMADCount = await _liquidationRepository.GetOngoingLiquidationsTotalByCurrencyForDecider("MAD");
            stats.OngoingLiquidationsEURCount = await _liquidationRepository.GetOngoingLiquidationsTotalByCurrencyForDecider("EUR");

            stats.FinalizedLiquidationsCount = await _liquidationRepository.GetFinalizedLiquidationsCountForDecider();
            stats.FinalizedLiquidationsMADCount = await _liquidationRepository.GetFinalizedLiquidationsTotalByCurrencyForDecider("MAD");
            stats.FinalizedLiquidationsEURCount = await _liquidationRepository.GetFinalizedLiquidationsTotalByCurrencyForDecider("EUR");

            return Ok(stats);
        }
    }
}
