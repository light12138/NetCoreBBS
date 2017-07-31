using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCoreBBS.MyAttributes
{
    public class SameAsHandler : AuthorizationHandler<SameAsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SameAsRequirement requirement)
        {
            // 判断该身份是否存在
            //if (!context.User.HasClaim(c => c.Type == ClaimTypes. &&
            //                      c.Issuer == "http://contoso.com"))
            //{
            //    // .NET 4.x -> return Task.FromResult(0);
            //    return Task.CompletedTask;
            //}
            var cla= context.User.Claims.FirstOrDefault();
            if (cla.Value == requirement.Name)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
