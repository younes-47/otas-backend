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
            CreateMap<OrdreMission, OrdreMissionFullDetailsDTO>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.AvanceVoyages, opt => opt.MapFrom(src => src.AvanceVoyages))
                .ForMember(dest => dest.ActualRequester, opt => opt.MapFrom(src => src.ActualRequester))
                .ForMember(dest => dest.StatusHistories, opt => opt.MapFrom(src => src.StatusHistories));

            CreateMap<AvanceCaisse, AvanceCaisseFullDetailsDTO>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Expenses, opt => opt.MapFrom(src => src.Expenses))
                .ForMember(dest => dest.ActualRequester, opt => opt.MapFrom(src => src.ActualRequester))
                .ForMember(dest => dest.StatusHistories, opt => opt.MapFrom(src => src.StatusHistories));
                

            CreateMap<AvanceVoyage, AvanceVoyageDTO>()
                .ForMember(dest => dest.Expenses, opt => opt.MapFrom(src => src.Expenses))
                .ForMember(dest => dest.Trips, opt => opt.MapFrom(src => src.Trips));

            CreateMap<AvanceVoyageDTO, AvanceVoyage>();
            CreateMap<AvanceVoyage, AvanceVoyageTableDTO>().ReverseMap();

            CreateMap<AvanceVoyageDTO, AvanceVoyage>().ReverseMap();
            CreateMap<AvanceCaisse, AvanceCaisseDTO>().ReverseMap();


            CreateMap<DepenseCaisse, DepenseCaisseDTO>().ReverseMap();
            CreateMap<Liquidation, LiquidationDTO>().ReverseMap();
            CreateMap<Expense, ExpenseDTO>().ReverseMap();
            CreateMap<Trip, TripDTO>().ReverseMap();
            CreateMap<StatusHistory, StatusHistoryDTO>().ReverseMap();
            CreateMap<Delegation, DelegationDTO>().ReverseMap();
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<ActualRequester, ActualRequesterDTO>().ReverseMap();

            // When we handle post methods
            CreateMap<OrdreMissionPostDTO, OrdreMission>()
                .ForMember(dest => dest.ActualRequester, opt => opt.Ignore());

            CreateMap<AvanceCaissePostDTO, AvanceCaisse>()
                .ForMember(dest => dest.ActualRequester, opt => opt.Ignore())
                .ForMember(dest => dest.Expenses, opt => opt.Ignore());

            CreateMap<ExpensePostDTO, Expense>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<ExpenseNoCurrencyPostDTO, Expense>().ReverseMap();
            CreateMap<TripPostDTO, Trip>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<ActualRequesterPostDTO, ActualRequester>().ReverseMap();

            CreateMap<DepenseCaissePostDTO, DepenseCaisse>()
                .ForMember(dest => dest.ActualRequester, opt => opt.Ignore())
                .ForMember(dest => dest.Expenses, opt => opt.Ignore());

        }
    }
}
