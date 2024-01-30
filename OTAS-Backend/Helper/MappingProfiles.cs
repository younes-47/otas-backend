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
            // Mapping from post objects
            CreateMap<OrdreMission, OrdreMissionDTO>().ReverseMap();
            CreateMap<OrdreMission, OrdreMissionViewDTO>()
                .ForMember(dest => dest.StatusHistory, opt => opt.MapFrom(src => src.StatusHistories));

            CreateMap<OrdreMission, OrdreMissionDeciderTableDTO>()
                .ForMember(dest => dest.RequestedAmountMAD, opt => opt.MapFrom(src => src.AvanceVoyages.Where(av => av.Currency == "MAD").Select(av => av.EstimatedTotal).FirstOrDefault()))
                .ForMember(dest => dest.RequestedAmountEUR, opt => opt.MapFrom(src => src.AvanceVoyages.Where(av => av.Currency == "EUR").Select(av => av.EstimatedTotal).FirstOrDefault()));
                   

            CreateMap<OrdreMission, OrdreMissionDeciderViewDTO>().ReverseMap();
            CreateMap<OrdreMission, OrdreMissionDeciderViewDTO>()
                .ForMember(dest => dest.StatusHistory, opt => opt.MapFrom(src => src.StatusHistories));

            CreateMap<AvanceCaisse, AvanceCaisseViewDTO>()
                .ForMember(dest => dest.Expenses, opt => opt.MapFrom(src => src.Expenses));



            //CreateMap<AvanceVoyage, AvanceVoyageViewDTO>()
            //    .ForMember(dest => dest.Expenses, opt => opt.MapFrom(src => src.Expenses))
            //    .ForMember(dest => dest.Trips, opt => opt.MapFrom(src => src.Trips));

            CreateMap<AvanceVoyageViewDTO, AvanceVoyage>();
            CreateMap<AvanceVoyage, AvanceVoyageTableDTO>().ReverseMap();

            CreateMap<AvanceVoyageViewDTO, AvanceVoyage>().ReverseMap();
            CreateMap<AvanceCaisse, AvanceCaisseDTO>().ReverseMap();


            CreateMap<DepenseCaisse, DepenseCaisseDTO>().ReverseMap();
            CreateMap<Liquidation, LiquidationDTO>().ReverseMap();
            CreateMap<Expense, ExpenseDTO>().ReverseMap();
            CreateMap<Trip, TripDTO>().ReverseMap();
            CreateMap<StatusHistory, StatusHistoryDTO>().ReverseMap();
            CreateMap<Delegation, DelegationDTO>().ReverseMap();
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<ActualRequester, ActualRequesterDTO>().ReverseMap();

            // Mapping from put objects
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
