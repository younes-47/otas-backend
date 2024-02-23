using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using org.apache.commons.codec.language.bm;
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
            StatsMoneyDTO moneyStats = new();
            StatsRequestsDTO requestsStats = new();

            decimal[] av_Totals = await _avanceVoyageRepository.GetRequesterAllTimeRequestedAmountsByUserIdAsync(userId);
            decimal[] ac_Totals = await _avanceCaisseRepository.GetRequesterAllTimeRequestedAmountsByUserIdAsync(userId);
            decimal[] dc_Totals = await _depenseCaisseRepository.GetRequesterAllTimeRequestedAmountsByUserIdAsync(userId);

            moneyStats.AvanceVoyagesAllTimeAmountMAD += av_Totals[0];
            moneyStats.AvanceVoyagesAllTimeAmountEUR += av_Totals[1];

            moneyStats.AvanceCaissesAllTimeAmountMAD += ac_Totals[0];
            moneyStats.AvanceCaissesAllTimeAmountEUR += ac_Totals[1];

            moneyStats.DepenseCaissesAllTimeAmountMAD += dc_Totals[0];
            moneyStats.DepenseCaissesAllTimeAmountEUR += dc_Totals[1];

            moneyStats.AllTimeAmountMAD += av_Totals[0] + ac_Totals[0] + dc_Totals[0];
            moneyStats.AllTimeAmountEUR += av_Totals[1] + ac_Totals[1] + dc_Totals[1];

            stats.MoneyStats = moneyStats;


            requestsStats.OrdreMissionsAllTimeCount = _context.OrdreMissions.Where(om => om.UserId == userId).Count();
            requestsStats.AvanceVoyagesAllTimeCount = _context.AvanceVoyages.Where(av => av.UserId == userId).Count();
            requestsStats.AvanceCaissesAllTimeCount = _context.AvanceCaisses.Where(ac => ac.UserId == userId).Count();
            requestsStats.DepenseCaissesAllTimeCount = _context.DepenseCaisses.Where(dc => dc.UserId == userId).Count();


            requestsStats.AllTimeCount = requestsStats.OrdreMissionsAllTimeCount + requestsStats.AvanceVoyagesAllTimeCount
                + requestsStats.AvanceCaissesAllTimeCount + requestsStats.DepenseCaissesAllTimeCount;

            requestsStats.AllOngoingRequestsCount = _context.AvanceVoyages.Where(av => av.UserId == userId)
                .Where(av => av.LatestStatus != 99 && av.LatestStatus != 97 && av.LatestStatus != 16 && av.LatestStatus != 7).Count();

            requestsStats.AllOngoingRequestsCount += _context.AvanceCaisses.Where(av => av.UserId == userId)
                .Where(av => av.LatestStatus != 99 && av.LatestStatus != 97 && av.LatestStatus != 16 && av.LatestStatus != 7).Count();

            requestsStats.AllOngoingRequestsCount += _context.DepenseCaisses.Where(av => av.UserId == userId)
                .Where(av => av.LatestStatus != 99 && av.LatestStatus != 97 && av.LatestStatus != 16 && av.LatestStatus != 7).Count();

            requestsStats.AllOngoingRequestsCount += _context.OrdreMissions.Where(av => av.UserId == userId)
                .Where(av => av.LatestStatus != 99 && av.LatestStatus != 97 && av.LatestStatus != 16 && av.LatestStatus != 7).Count();

            DateTime? lastCreatedReq = DateTime.MinValue;

            DateTime? tempDate = await _avanceVoyageRepository.GetTheLatestCreatedRequestByUserIdAsync(userId);
            if (tempDate != null)
            {
                if (tempDate > lastCreatedReq)
                {
                    lastCreatedReq = tempDate;
                }
            }

            tempDate = await _avanceCaisseRepository.GetTheLatestCreatedRequestByUserIdAsync(userId);
            if (tempDate != null)
            {
                if (tempDate > lastCreatedReq)
                {
                    lastCreatedReq = tempDate;
                }
            }

            tempDate = await _depenseCaisseRepository.GetTheLatestCreatedRequestByUserIdAsync(userId);
            if (tempDate != null)
            {
                if (tempDate > lastCreatedReq)
                {
                    lastCreatedReq = tempDate;
                }
            }

            if (lastCreatedReq != DateTime.MinValue)
            {
                TimeSpan difference = (TimeSpan)(DateTime.Now - lastCreatedReq);
                requestsStats.HoursPassedSinceLastRequest = (decimal)difference.TotalHours;
            }
            else
            {
                requestsStats.HoursPassedSinceLastRequest = null;
            }

            stats.RequestsStats = requestsStats;

            return Ok(stats);
        }

        [Authorize(Roles = "decider")]
        [HttpGet("Decider/Stats")]
        public async Task<IActionResult> GetDeciderStats()
        {
            string username = HttpContext.User.Identity.Name;
            var userId = await _userRepository.GetUserIdByUsernameAsync(username);

            DeciderStatsDTO stats = new();

            stats.PendingOrdreMissionsCount = _context.OrdreMissions.Where(om => om.NextDeciderUserId == userId).Count();
            stats.PendingAvanceVoyagesCount = _context.AvanceVoyages.Where(av => av.NextDeciderUserId == userId).Count();
            stats.PendingAvanceCaissesCount = _context.AvanceCaisses.Where(ac => ac.NextDeciderUserId == userId).Count();
            stats.PendingDepenseCaissesCount = _context.DepenseCaisses.Where(dc => dc.NextDeciderUserId == userId).Count();
            stats.PendingLiquidationsCount = _context.Liquidations.Where(l => l.NextDeciderUserId == userId).Count();


            var finalizedAvancesCaisses = await _avanceCaisseRepository.GetFinalizedAvanceCaissesForDeciderStats(userId);
            stats.FinalizedAvanceCaissesCount = finalizedAvancesCaisses.Count;
            foreach (var ac in finalizedAvancesCaisses)
            {
                stats.FinalizedAvanceCaissesCount += 1;
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
                stats.FinalizedAvanceVoyagesCount += 1;
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
                stats.FinalizedDepenseCaissesCount += 1;
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
            stats.ToLiquidateRequestsMADCount = await _liquidationRepository.GetRequestsToLiquidateTotalByCurrency("MAD");
            stats.ToLiquidateRequestsEURCount = await _liquidationRepository.GetRequestsToLiquidateTotalByCurrency("EUR");

            stats.OngoingLiquidationsCount = await _liquidationRepository.GetOngoingLiquidationsCount();
            stats.OngoingLiquidationsMADCount = await _liquidationRepository.GetOngoingLiquidationsTotalByCurrency("MAD");
            stats.OngoingLiquidationsEURCount = await _liquidationRepository.GetOngoingLiquidationsTotalByCurrency("EUR");

            stats.FinalizedLiquidationsCount = await _liquidationRepository.GetFinalizedLiquidationsCount();
            stats.FinalizedLiquidationsMADCount = await _liquidationRepository.GetFinalizedLiquidationsTotalByCurrency("MAD");
            stats.FinalizedLiquidationsEURCount = await _liquidationRepository.GetFinalizedLiquidationsTotalByCurrency("EUR");

            return Ok(stats);
        }
    }
}
