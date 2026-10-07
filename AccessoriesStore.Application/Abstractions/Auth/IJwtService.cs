using AccessoriesStore.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessoriesStore.Application.Abstractions.Auth
{
    public interface IJwtService
    {
        AccessTokenResult GenerateAccessToken(
            string userId,
            string email,
            IEnumerable<string> roles);

        string GenerateRefreshToken();
    }
}
