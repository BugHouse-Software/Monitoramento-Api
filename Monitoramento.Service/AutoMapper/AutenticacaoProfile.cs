using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitoramento.Service.AutoMapper
{
    public static class AutoMapperSetup
    {
        public static void AddMapeamentos(this IServiceCollection services)
        {
            _ = services.AddAutoMapper(typeof(AutoMapperSetup));
        }

    }
}
