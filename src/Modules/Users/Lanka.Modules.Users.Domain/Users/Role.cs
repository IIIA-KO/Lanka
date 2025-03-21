﻿namespace Lanka.Modules.Users.Domain.Users;

public sealed class Role
{
    public static readonly Role Administrator = new("Administrator");
    public static readonly Role Member = new("Member");

    private Role(string name)
    {
        this.Name = name;
    }

    private Role()
    {
    }

    public string Name { get; private set; }
}
