using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.DTOs.Auth
{
    public class AccessTokenResult
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
