using OTAS.DTO.Post;
using OTAS.Interfaces.IService;
using OTAS.Interfaces.IRepository;
using OTAS.Repository;
using AutoMapper;
using OTAS.Models;

namespace OTAS.Services
{
    public class ActualRequesterService : IActualRequesterService
    {
        private readonly IActualRequesterRepository _actualRequesterRepository;
        private readonly IMapper _mapper;

        public ActualRequesterService(IActualRequesterRepository actualRequesterRepository, IMapper mapper)
        {
            _actualRequesterRepository = actualRequesterRepository;
            _mapper = mapper;
        }


        public async Task<ServiceResult> AddActualRequesterInfoAsync(ActualRequester actualRequester)
        {
            var mappedActualRequester =  _mapper.Map<ActualRequester>(actualRequester);
            ServiceResult result = new()
            {
                Success = await _actualRequesterRepository.AddActualRequesterInfo(mappedActualRequester)
            };

            if (!result.Success) result.Message = "Something went wrong while Saving actual requester info";

            return result;
        }
    }
}
