using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreBBS.MyAttributes
{
    public class SameAsRequirement: IAuthorizationRequirement
    {
        public string Name { get; set; }

        public SameAsRequirement(string name)
        {
            Name = name;
        }
    }
}
