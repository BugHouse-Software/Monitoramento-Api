using System.Collections.Generic;

namespace Monitoramento.Domain.Models.V1.Authorization
{
    public class UsuarioDataTokenDTO
    {
        public string Usuario { get; set; }
        public string CodigoUsuario { get; set; }

        public List<LocaisUsuarioAcessoDTO> LocaisAcesso { get; set; }
    }
}
