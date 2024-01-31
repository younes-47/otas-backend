using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly OtasContext _context;
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IAvanceCaisseRepository _avanceCaisseRepository;
        private readonly IDepenseCaisseRepository _depenseCaisseRepository;
        private readonly IMapper _mapper;
        public UserController(IUserRepository userRepository,
            IDeciderRepository deciderRepository,
            ILdapAuthenticationService ldapAuthenticationService,
            OtasContext context,
            IAvanceVoyageRepository avanceVoyageRepository,
            IAvanceCaisseRepository avanceCaisseRepository,
            IDepenseCaisseRepository depenseCaisseRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _deciderRepository = deciderRepository;
            _ldapAuthenticationService = ldapAuthenticationService;
            _context = context;
            _avanceVoyageRepository = avanceVoyageRepository;
            _avanceCaisseRepository = avanceCaisseRepository;
            _depenseCaisseRepository = depenseCaisseRepository;
            _mapper = mapper;
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("info")]
        public async Task<UserInfoDTO> GetUserInformation()
        {
            string username = HttpContext.User.Identity.Name;
            var userInfo = _ldapAuthenticationService.GetUserInformation(username);
            var userId = await _userRepository.GetUserIdByUsernameAsync(username);
            userInfo.Level = await _deciderRepository.GetDeciderLevelByUserId(userId);
            userInfo.PreferredLanguage = await _userRepository.GetPreferredLanguageByUserIdAsync(userId);
            return userInfo;
        }


        [Authorize(Roles = "requester,decider")]
        [HttpGet("ActualRequesterStaticInfo/All")]
        public async Task<IActionResult> GetActualRequesterStaticInfo()
        {
            ActualRequesterStaticInfoDTO staticData = new();
            staticData.Departments = await _context.Departments.Select(dp => dp.Name).ToListAsync();
            staticData.ManagersUsernames = await _deciderRepository.GetManagersUsernames();
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
            moneyStats.AvanceCaissesAllTimeAmountMAD += ac_Totals[1];

            moneyStats.DepenseCaissesAllTimeAmountMAD += dc_Totals[0];
            moneyStats.DepenseCaissesAllTimeAmountMAD += dc_Totals[1];

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
                requestsStats.HoursPassedSinceLastRequest = difference.Hours;
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

            StatsDTO stats = new();

            decimal[] av_Totals = await _avanceVoyageRepository.GetRequesterAllTimeRequestedAmountsByUserIdAsync(userId);
            decimal[] ac_Totals = await _avanceCaisseRepository.GetRequesterAllTimeRequestedAmountsByUserIdAsync(userId);
            decimal[] dc_Totals = await _depenseCaisseRepository.GetRequesterAllTimeRequestedAmountsByUserIdAsync(userId);

            stats.MoneyStats.AvanceVoyagesAllTimeAmountMAD = av_Totals[0];
            stats.MoneyStats.AvanceVoyagesAllTimeAmountEUR = av_Totals[1];

            stats.MoneyStats.AvanceCaissesAllTimeAmountMAD = ac_Totals[0];
            stats.MoneyStats.AvanceCaissesAllTimeAmountMAD = ac_Totals[1];

            stats.MoneyStats.DepenseCaissesAllTimeAmountMAD = dc_Totals[0];
            stats.MoneyStats.DepenseCaissesAllTimeAmountMAD = dc_Totals[1];

            stats.MoneyStats.AllTimeAmountMAD = av_Totals[0] + ac_Totals[0] + dc_Totals[0];
            stats.MoneyStats.AllTimeAmountEUR = av_Totals[1] + ac_Totals[1] + dc_Totals[1];


            stats.RequestsStats.AvanceVoyagesAllTimeCount = _context.AvanceVoyages.Where(av => av.UserId == userId).Count();
            stats.RequestsStats.AvanceCaissesAllTimeCount = _context.AvanceCaisses.Where(ac => ac.UserId == userId).Count();
            stats.RequestsStats.DepenseCaissesAllTimeCount = _context.DepenseCaisses.Where(dc => dc.UserId == userId).Count();

            stats.RequestsStats.AllTimeCount = stats.RequestsStats.AvanceVoyagesAllTimeCount
                + stats.RequestsStats.AvanceCaissesAllTimeCount + stats.RequestsStats.DepenseCaissesAllTimeCount;

            //stats.RequestsStats.AllOngoingRequestsCount += _context.AvanceVoyages.Where(av => av.UserId == userId)
            //    .Where(av => av.LatestStatus != "Approved" && av.LatestStatus != "Approved")

            DateTime? lastCreatedReq = DateTime.MaxValue;

            DateTime? tempDate = await _avanceVoyageRepository.GetTheLatestCreatedRequestByUserIdAsync(userId);
            if (tempDate != null)
            {
                if (tempDate < lastCreatedReq)
                {
                    lastCreatedReq = tempDate;
                }
            }

            tempDate = await _avanceCaisseRepository.GetTheLatestCreatedRequestByUserIdAsync(userId);
            if (tempDate != null)
            {
                if (tempDate < lastCreatedReq)
                {
                    lastCreatedReq = tempDate;
                }
            }

            tempDate = await _depenseCaisseRepository.GetTheLatestCreatedRequestByUserIdAsync(userId);
            if (tempDate != null)
            {
                if (tempDate < lastCreatedReq)
                {
                    lastCreatedReq = tempDate;
                }
            }

            if (lastCreatedReq != DateTime.MaxValue)
            {
                TimeSpan difference = (TimeSpan)(DateTime.Now - lastCreatedReq);
                stats.RequestsStats.HoursPassedSinceLastRequest = difference.Days;
            }

            return Ok(stats);
        }
    }
}
