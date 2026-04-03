using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CountryRiskService : ICountryRiskService
    {
        private readonly ICountryRiskRepository _countryRiskRepo;

        public CountryRiskService(ICountryRiskRepository countryRiskRepo)
        {
            _countryRiskRepo = countryRiskRepo;
        }

        public async Task<IEnumerable<CountryRiskDTO>> GetAllAsync()
        {
            var countries = await _countryRiskRepo.GetAllAsync();
            return countries.Select(MapToDTO);
        }

        public async Task<IEnumerable<CountryRiskDTO>> GetActiveAsync()
        {
            var countries = await _countryRiskRepo.GetActiveAsync();
            return countries.Select(MapToDTO);
        }

        public async Task<CountryRiskDTO> GetByIdAsync(int id)
        {
            var country = await _countryRiskRepo.GetByIdAsync(id);
            if (country == null) throw new Exception("Country not found");
            return MapToDTO(country);
        }

        public async Task<CountryRiskDTO> CreateAsync(CreateCountryRiskDTO dto)
        {
            var existing = await _countryRiskRepo.GetByNameAsync(dto.Name);
            if (existing != null) throw new Exception("Country already exists");

            var country = new CountryRisk
            {
                Name = dto.Name,
                Multiplier = dto.Multiplier,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _countryRiskRepo.AddAsync(country);
            return MapToDTO(created);
        }

        public async Task UpdateAsync(int id, UpdateCountryRiskDTO dto)
        {
            var country = await _countryRiskRepo.GetByIdAsync(id);
            if (country == null) throw new Exception("Country not found");

            country.Multiplier = dto.Multiplier;
            country.IsActive = dto.IsActive;

            await _countryRiskRepo.UpdateAsync(country);
        }

        public async Task DeleteAsync(int id)
        {
            var country = await _countryRiskRepo.GetByIdAsync(id);
            if (country == null) throw new Exception("Country not found");

            var isUsed = await _countryRiskRepo.IsCountryUsedInPoliciesAsync(country.Name);
            if (isUsed) throw new Exception("Cannot delete country as it is used in policies");

            await _countryRiskRepo.DeleteAsync(id);
        }

        private CountryRiskDTO MapToDTO(CountryRisk country)
        {
            return new CountryRiskDTO
            {
                Id = country.Id,
                Name = country.Name,
                Multiplier = country.Multiplier,
                IsActive = country.IsActive,
                CreatedAt = country.CreatedAt
            };
        }
    }
}
