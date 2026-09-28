using AutoMapper;

using OnlineCasino.Entities;

using OnlineCasino.DTOs.Auth;
using OnlineCasino.DTOs.Games;
using OnlineCasino.DTOs.History;
using OnlineCasino.DTOs.Users;
using OnlineCasino.DTOs.Wallet;

namespace OnlineCasino.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<SignUpRequest, User>()
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.RoleId, o => o.Ignore())
                .ForMember(d => d.Role, o => o.Ignore())
                .ForMember(d => d.Wallet, o => o.Ignore())
                .ForMember(d => d.Transactions, o => o.Ignore())
                .ForMember(d => d.GameRounds, o => o.Ignore());

            CreateMap<User, AuthUserDto>()
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.Name));

            CreateMap<User, UserProfileDto>()
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.Name));

            CreateMap<User, UserListItemDto>()
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.Name))
                .ForMember(d => d.Balance, o => o.MapFrom(s => s.Wallet.Balance));

            // Wallet
            CreateMap<Wallet, WalletDto>();

            CreateMap<Transaction, TransactionDto>()
                .ForMember(d => d.Type, o => o.MapFrom(s => s.TransactionType.Name));

            // Games
            CreateMap<Game, GameDto>()
                .ForMember(d => d.Type, o => o.MapFrom(s => s.GameType.Name));

            // History
            CreateMap<GameRound, GameRoundDto>()
                .ForMember(d => d.RoundId, o => o.MapFrom(s => s.PublicId))
                .ForMember(d => d.Game, o => o.MapFrom(s => s.Game.Code))
                .ForMember(d => d.Result, o => o.MapFrom(s => s.RoundResult != null ? s.RoundResult.Name : null));
        }
    }
}