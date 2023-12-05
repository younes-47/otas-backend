using System;
using System.Collections.Generic;

namespace OTAS.Models;

public partial class RoleCode
{
    public int RoleInt { get; set; }

    public string RoleString { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
