namespace Lanka.Modules.Users.Domain.Users
{
    public sealed class Permission
    {
        public static readonly Permission GetUser = new("users:read");
        
        public Permission(string code)
        {
            this.Code = code;
        }

        public string Code { get; }
    }
}
