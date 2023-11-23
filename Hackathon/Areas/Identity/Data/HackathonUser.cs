using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Hackathon.Areas.Identity.Data;

// Add profile data for application users by adding properties to the HackathonUser class
public class HackathonUser : IdentityUser
{
    public string Name { get; set; }

    public string SecondName { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;
}

