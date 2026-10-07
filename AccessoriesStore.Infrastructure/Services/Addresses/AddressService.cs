using AccessoriesStore.Application.Abstractions.Addresses;
using AccessoriesStore.Application.DTOs.Addresses;
using AccessoriesStore.Domain.Entities;
using AccessoriesStore.Domain.Exceptions;
using AccessoriesStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Infrastructure.Services.Addresses
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDbContext _context;

        public AddressService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AddressResponse> CreateAsync(
            string userId,
            CreateAddressRequest request)
        {
            var address = new Address
            {
                UserId = userId,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                City = request.City,
                Area = request.Area,
                Street = request.Street,
                BuildingNumber = request.BuildingNumber,
                ApartmentNumber = request.ApartmentNumber,
                AdditionalDetails = request.AdditionalDetails
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return MapToResponse(address);
        }

        public async Task<List<AddressResponse>> GetAllAsync(string userId)
        {
            var addresses = await _context.Addresses
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return addresses.Select(MapToResponse).ToList();
        }

        public async Task<AddressResponse> GetByIdAsync(
            string userId,
            int addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(x =>
                    x.Id == addressId &&
                    x.UserId == userId);

            if (address is null)
                throw new NotFoundException("Address not found.");

            return MapToResponse(address);
        }

        public async Task<AddressResponse> UpdateAsync(
            string userId,
            int addressId,
            UpdateAddressRequest request)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(x =>
                    x.Id == addressId &&
                    x.UserId == userId);

            if (address is null)
                throw new NotFoundException("Address not found.");

            address.FullName = request.FullName;
            address.PhoneNumber = request.PhoneNumber;
            address.City = request.City;
            address.Area = request.Area;
            address.Street = request.Street;
            address.BuildingNumber = request.BuildingNumber;
            address.ApartmentNumber = request.ApartmentNumber;
            address.AdditionalDetails = request.AdditionalDetails;

            await _context.SaveChangesAsync();

            return MapToResponse(address);
        }

        public async Task DeleteAsync(
            string userId,
            int addressId)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(x =>
                    x.Id == addressId &&
                    x.UserId == userId);

            if (address is null)
                throw new NotFoundException("Address not found.");

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
        }

        private static AddressResponse MapToResponse(Address address)
        {
            return new AddressResponse
            {
                Id = address.Id,
                FullName = address.FullName,
                PhoneNumber = address.PhoneNumber,
                City = address.City,
                Area = address.Area,
                Street = address.Street,
                BuildingNumber = address.BuildingNumber,
                ApartmentNumber = address.ApartmentNumber,
                AdditionalDetails = address.AdditionalDetails
            };
        }
    }
}
