using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityService
{
    public class config
    {
        //定义系统中的资源
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            // 客户端凭据
            return new List<Client>
            {
                // OpenID Connect implicit 客户端 (MVC)
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.Implicit,

                    RedirectUris = { "http://localhost:5003/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5003" },
                    //运行访问的资源
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                }
            };
        }

        //测试用户
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "admin",
                    Password = "123456",

                    Claims = new List<Claim>
                    {
                        new Claim("admin", "wzl"),
                        new Claim("website", "https://www.cnblogs.com/linezero")
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "wss",
                    Password = "123456",

                    Claims = new List<Claim>
                    {
                        new Claim("Wss", "wss"),
                        new Claim("website", "https://github.com/linezero")
                    }
                }
            };
        }
    }
}
