using AccessoriesStore.Application.DTOs.Addresses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Abstractions.Addresses
{
    public interface IAddressService
    {
        Task<AddressResponse> CreateAsync(
            string userId,
            CreateAddressRequest request);

        Task<List<AddressResponse>> GetAllAsync(
            string userId);

        Task<AddressResponse> GetByIdAsync(
            string userId,
            int addressId);

        Task<AddressResponse> UpdateAsync(
            string userId,
            int addressId,
            UpdateAddressRequest request);

        Task DeleteAsync(
            string userId,
            int addressId);
    }
}
