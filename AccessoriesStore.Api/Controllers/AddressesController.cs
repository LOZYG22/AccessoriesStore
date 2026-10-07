using AccessoriesStore.Application.Abstractions.Addresses;
using AccessoriesStore.Application.Common.Responses;
using AccessoriesStore.Application.DTOs.Addresses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccessoriesStore.Api.Controllers
{
    [ApiController]
    [Route("api/addresses")]
    [Authorize]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;
        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAddressRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _addressService.CreateAsync(
                userId!,
                request);

            return Ok(
                    ApiResponse<AddressResponse>.SuccessResponse(
                        result,
                        "Address created successfully.")
                );
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _addressService.GetAllAsync(userId!);

            return Ok(
                    ApiResponse<List<AddressResponse>>.SuccessResponse(result)
                );
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _addressService.GetByIdAsync(
                userId!,
                id);

            return Ok(
                    ApiResponse<AddressResponse>.SuccessResponse(result)
                );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateAddressRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _addressService.UpdateAsync(
                userId!,
                id,
                request);

            return Ok(
                    ApiResponse<AddressResponse>.SuccessResponse(
                        result,
                        "Address updated successfully.")
                );
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _addressService.DeleteAsync(userId!, id);

            return Ok(
                    ApiResponse<object>.SuccessResponse(
                        null,
                        "Address deleted successfully.")
                );
        }
    }
}
