using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Domain.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string? BuildingNumber { get; set; }
        public string? ApartmentNumber { get; set; }
        public string? AdditionalDetails { get; set; }
    }
}
