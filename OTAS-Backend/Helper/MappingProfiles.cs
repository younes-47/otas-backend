using AutoMapper;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.Models;

namespace OTAS.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles() 
        {
            // When we handle get methods
            CreateMap<OrdreMission, OrdreMissionDTO>().ReverseMap();
            CreateMap<AvanceVoyage, AvanceVoyageDTO>().ReverseMap();
            CreateMap<AvanceCaisse, AvanceCaisseDTO>().ReverseMap();
            CreateMap<DepenseCaisse, DepenseCaisseDTO>().ReverseMap();
            CreateMap<Liquidation, LiquidationDTO>().ReverseMap();
            CreateMap<Expense, ExpenseDTO>().ReverseMap();
            CreateMap<Trip, TripDTO>().ReverseMap();
            CreateMap<StatusHistory, StatusHistoryDTO>().ReverseMap();
            CreateMap<Delegation, DelegationDTO>().ReverseMap();
            CreateMap<User, UserDTO>().ReverseMap();

            // When we handle post methods
            CreateMap<OrdreMissionPostDTO, OrdreMission>().ReverseMap();
            CreateMap<ExpensePostDTO, Expense>().ReverseMap();
            CreateMap<TripPostDTO, Trip>().ReverseMap();
            CreateMap<ActualRequesterPostDTO, ActualRequester>().ReverseMap();

        }
    }
}
