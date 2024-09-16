namespace Monitoramento.Domain.Models.V1.Authorization
{
    public class AuthorizationRequest
    {
        public string Usuario { get; set; }
        public string UsuarioId { get; set; }
        public TipoLinguagemEnum Linguagem { get; set; }

    }
}
