using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4Demo1
{
    public class Config
    {

        public static IEnumerable<ApiResource> GetApiResource()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1","My Api")
            };
        }

        public static IEnumerable<Client> GetClient()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId="client",
                    //没有交互式用户，使用clientid / secret进行身份验证
                    AllowedGrantTypes=GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("Secret".Sha256())
                    },
                      //客户端可以访问的 api的范围
                    AllowedScopes={ "api1"}
                }
            };
        }
    }
}
