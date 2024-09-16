using Microsoft.Extensions.Logging;
using Monitoramento.Domain.Models.V1.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Monitoramento.Service.V1.Authorization
{
    public class ApiAuthorizationService : IApiAuthorizationService
    {
        private readonly ICripitografarDataService _serviceCripitografar;
        private readonly IJwtService _service;

        private readonly IMapper _mapper;
        private readonly SessionService _session;
        private readonly ILogger<ApiAuthorizationService> _logger;

        public ApiAuthorizationService(ICripitografarDataService serviceCripitografar, IJwtService service, IConfiguration configuration, IMapper mapper, ILogger<ApiAuthorizationService> logger, SessionService session)
        {
            this._serviceCripitografar = serviceCripitografar;
            this._service = service;

            this._mapper=mapper;
            this._logger=logger;
            this._session=session;
        }





        public async Task<AuthorizationResponse> PostAuthorization(AuthorizationConfigurationRequest value)
        {
            var valuePassJson = await File.ReadAllTextAsync(Path.Combine(".", "Pass.json"));
            var passDto = valuePassJson.ToDeserialize<PassDTO>();

            if (value.Password != passDto.Pass || value.User  != passDto.User)
                throw new AthorizationException("User or Passwoworld is invalid values.");


            var expiresDate = DateTime.UtcNow.AddHours(2);
            var tokenValue = _service.GenerateTokenJwt(userId: value.User,
                                                       usuario: value.User,
                                                       expiresDate: expiresDate,
                                                       criptograpyData: String.Empty,
                                                       tpLinguagem: null,
                                                       type: "Admin",
                                                       securityAlgorithms: SecurityAlgorithms.HmacSha512Signature,
                                                       codeSecurity: passDto.CodeSecurity);

            if (tokenValue.IsNullOrWhiteSpace())
                throw new Exception("The error occurred while generating the authentication token.");

            return new AuthorizationResponse()
            {
                CreateDate = DateTime.UtcNow,
                ExpiresDate = expiresDate,
                Token = tokenValue,
                Type = "Admin",
                Usuario = value.User
            };
        }
        public async Task<AuthorizationResponse> PostAuthorization(AuthorizationRequest value)
        {
            UsuarioDataTokenDTO dadosUsuario = null;
            try
            {
                dadosUsuario = await GetDataUser(value.UsuarioId.ToInt());
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(), ex, "GetDataUser", null);
                throw;
            }


            var expiresDate = DateTime.UtcNow.AddHours(24);
            var stringObject = _serviceCripitografar.Criptograr(value, dadosUsuario.Usuario, expiresDate);

            var tokenValue = _service.GenerateTokenJwt(value.UsuarioId, dadosUsuario.Usuario, expiresDate, stringObject, value.Linguagem.ToInt(), dadosUsuario);

            if (tokenValue.IsNullOrWhiteSpace())
                throw new Exception("The error occurred while generating the authentication token.");

            return new AuthorizationResponse()
            {
                CreateDate = DateTime.UtcNow,
                ExpiresDate = expiresDate,
                Token = tokenValue,
                Type = "Bearer",
                Usuario = value.Usuario
            };
        }

        private static string DecryptStringFromBytes(string textValue, string key, string iv)
        {
            if (string.IsNullOrEmpty(textValue))
                throw new ArgumentNullException(nameof(textValue));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrEmpty(iv))
                throw new ArgumentNullException(nameof(iv));

            string result = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key);
                aesAlg.IV = Convert.FromBase64String(iv);

                var valueBytes = Convert.FromBase64String(textValue);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(valueBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            result = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return result;

        }



        private async Task<UsuarioDataTokenDTO> GetDataUser(decimal numeroUsuario)
        {
            try
            {
                DataBaseController dataBase = _session.DataBase;

                var users = await dataBase.Usuarios
                              .AsNoTracking()
                              .Where(s => s.NumeroUsuario.Equals(numeroUsuario))
                              .Select(s => new
                              {
                                  LocalUsuario = s.LocalUsuario.ToInt(),
                                  CodigoUsuario = s.CodigoUsuario,
                                  NomeUsuario = s.NomeUsuario,
                                  NumeroUsuario = s.NumeroUsuario
                              }).ToListAsync();
                var user = users.FirstOrDefault();

                if (user.IsNull())
                    throw new BadRequestException("The user's code was not found.");


                List<HomcenEntity> homcens;

                if (user.LocalUsuario ==0)
                {
                    homcens= await dataBase.Homcens.AsNoTracking().ToListAsync();
                }
                else
                {
                    homcens = await dataBase.Homcens.AsNoTracking().Where(s => s.LocalHomcen == user.LocalUsuario)
                                                    .ToListAsync();
                }

                var @return = new UsuarioDataTokenDTO()
                {
                    CodigoUsuario = user.CodigoUsuario,
                    Usuario = user.NomeUsuario.IsNullOrWhiteSpace() ? user.CodigoUsuario : user.NomeUsuario,
                    LocaisAcesso = new()
                };

                var homcnesAgrupado = homcens.GroupBy(s => s.CCustoHomcen).Select(s => s.Key).ToHashSet();

                if (homcnesAgrupado.IsNullOrEmpty())
                    throw new BadRequestException("It was not possible to generate the user's permission.");

                @return.LocaisAcesso = await dataBase.CadCens.AsNoTracking()
                                                      .Where(s => homcnesAgrupado.Contains(s.CodCenCadcen))
                                                      .Select(s => new LocaisUsuarioAcessoDTO()
                                                      {
                                                          CodCentroCusto= s.CodCenCadcen.ToInt(),
                                                          DescricaoLocal = s.DescrCenCadcen
                                                      }).ToListAsync();

                return @return;

            }
            catch (Exception ex)
            {


                throw;
            }
        }



    }
}
