namespace ERP.Infrastructure.Auth
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = "ERP.Api";
        public string Audience { get; set; } = "ERP.Client";
        public string Key { get; set; } = "CHANGE_ME_SUPER_SECRET_256_KEY";
        public int ExpMinutes { get; set; } = 60;
    }
}
