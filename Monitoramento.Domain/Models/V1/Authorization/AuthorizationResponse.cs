using System;

namespace Monitoramento.Domain.Models.V1.Authorization
{
    public class AuthorizationResponse
    {
        public string Type { get; set; }
        public string Token { get; set; }
        public string Usuario { get; set; }
        public DateTime ExpiresDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
