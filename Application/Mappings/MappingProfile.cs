using AutoMapper.Execution;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTOs.Responses;
using Persistence.DBModels;
using Common.DTOs.Requests;
using Common.Pagination;
using Microsoft.AspNetCore.Identity;

namespace Application.Maapings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mappings between Account model on the database and client response models
            CreateMap<Account, AccountDTO>()
                .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.AccountType.ToString()))
                .ForMember(dest => dest.PensionAccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.CurrentBalance))
                .ForMember(dest => dest.PensionAccountNumber, opt => opt.MapFrom(src => src.PensionAccountNumber))
                .ReverseMap();

            // Mappings between Contribution model on the database and client response model
            CreateMap<Contribution, ContributionDTO>()
                .ForMember(dest => dest.ContributionType, opt => opt.MapFrom(src => src.ContributionType.ToString()))
                .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => $"{src.Member.FirstName} {src.Member.LastName}"))
                .ForMember(dest => dest.MemberProfilePicture, opt => opt.MapFrom(src => src.Member.ProfilePicture))
                .ForMember(dest => dest.ContributionDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ReverseMap();

            CreateMap<PaginatedList<Contribution>, PaginatedList<ContributionDTO>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items.Select(item => new ContributionDTO
                {
                    ContributionId = item.ContributionId,
                    MemberId = item.MemberId,
                    MemberName = $"{item.Member.FirstName} {item.Member.LastName}",
                    MemberProfilePicture = item.Member.ProfilePicture,
                    PensionAccountNumber = item.PensionAccountNumber,
                    Amount = item.Amount,
                    ContributionType = item.ContributionType,
                    ContributionDate = item.CreatedDate,
                    IsValidated = item.IsValidated
                }).ToList()))
                .ReverseMap();

            // Mappings between Contribution model on the database and client request model
            CreateMap<Contribution, ContributionRequest>().ReverseMap();

            // Mappings between Employer model on the database and client response model
            CreateMap<Employer, EmployerDTO>().ReverseMap();

            CreateMap<PaginatedList<Employer>, PaginatedList<EmployerDTO>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items.Select(item => new EmployerDTO
                {
                    EmployerId = item.EmployerId,
                    CompanyName = item.CompanyName,
                    CompanyProfileImage = item.CompanyProfileImage,
                    RegistrationNumber = item.RegistrationNumber,
                    TaxIdentificationNumber = item.TaxIdentificationNumber,
                    Industry = item.Industry,
                    ContactEmail = item.ContactEmail,
                    ContactPhone = item.ContactPhone,
                    WebsiteUrl = item.WebsiteUrl,
                    Address = item.Address,
                    Country = item.Country,
                    State = item.State,
                    City = item.City,
                    BankAccountNumber = item.BankAccountNumber,
                    BankName = item.BankName,
                    NumberOfEmployees = item.Employees.Count,
                    PensionContributionRate = item.PensionContributionRate
                }).ToList()))
                .ReverseMap();

            // Mappings between Employer model on the database and client request model
            CreateMap<Employer, EmployerRequest>()
                .ForMember(dest => dest.CompanyProfileImage, opt => opt.Ignore())
                .ReverseMap();

            // Mappings between Member model on the database and client response model
            CreateMap<Persistence.DBModels.Member, MemberDTO>()
                .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.MembershipType, opt => opt.MapFrom(src => src.MembershipType.ToString()))
                .ReverseMap();

            CreateMap<PaginatedList<Persistence.DBModels.Member>, PaginatedList<MemberDTO>>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items.Select(item => new MemberDTO
                {
                    MemberId = item.Id,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    ProfilePicture = item.ProfilePicture,
                    DateOfBirth = item.DateOfBirth,
                    NationalIdentificationNumber = item.NationalIdentificationNumber,
                    Address = item.Address,
                    BankAccountNumber = item.BankAccountNumber,
                    BankName = item.BankName,
                    EmployerId = item.EmployerId,
                    Employer = item.Employer,
                    IsActive = item.IsActive,
                    CreatedDate = item.CreatedDate,
                    ModifiedDate = item.ModifiedDate,
                    DeletedDate = item.DeletedDate,
                    MembershipType = item.MembershipType,
                    IsEligibleForBenefits = item.IsEligibleForBenefits,
                    Accounts = item.Accounts,
                    TotalContributions = item.Accounts.Sum(s => s.TotalContributions)
                }).ToList()))
                .ReverseMap();

            // Mappings between Member model on the database and client request model
            CreateMap<Persistence.DBModels.Member, MemberRequest>()
                .ForMember(dest => dest.ProfilePicture, opt => opt.Ignore())
                .ReverseMap();

            // Custom Mapping Expressions for paginated response
            CreateMap<PaginatedList<Persistence.DBModels.Member>, PaginatedList<MemberDTO>>()
                .ConvertUsing((src, dest, context) =>
                {
                    var mappedItems = context.Mapper.Map<List<MemberDTO>>(src.Items);
                    return new PaginatedList<MemberDTO>(mappedItems, src.TotalCount, src.PageIndex, src.Items.Count);
                });

            CreateMap<PaginatedList<Employer>, PaginatedList<EmployerDTO>>()
                .ConvertUsing((src, dest, context) =>
                {
                    var mappedItems = context.Mapper.Map<List<EmployerDTO>>(src.Items);
                    return new PaginatedList<EmployerDTO>(mappedItems, src.TotalCount, src.PageIndex, src.Items.Count);
                });

            CreateMap<PaginatedList<Contribution>, PaginatedList<ContributionDTO>>()
                .ConvertUsing((src, dest, context) =>
                {
                    var mappedItems = context.Mapper.Map<List<ContributionDTO>>(src.Items);
                    return new PaginatedList<ContributionDTO>(mappedItems, src.TotalCount, src.PageIndex, src.Items.Count);
                });

            CreateMap<PaginatedList<Account>, PaginatedList<AccountDTO>>()
                .ConvertUsing((src, dest, context) =>
                {
                    var mappedItems = context.Mapper.Map<List<AccountDTO>>(src.Items);
                    return new PaginatedList<AccountDTO>(mappedItems, src.TotalCount, src.PageIndex, src.Items.Count);
                });
        }
    }
}
