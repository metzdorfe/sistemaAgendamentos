using System.Windows;
using EspacoFestas.Data;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EspacoFestas
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IConfiguration Configuracao { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Configuracao = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddIniFile("config.ini", optional: false)
                .Build();
        }

        public static EspacoFestasContext CriarContexto()
        {
            var connectionString = new FbConnectionStringBuilder
            {
                Database = Configuracao["Banco:Caminho"],
                DataSource = Configuracao["Banco:Servidor"] ?? "localhost",
                Port = int.TryParse(Configuracao["Banco:Porta"], out var porta) ? porta : 3050,
                UserID = Configuracao["Banco:Usuario"] ?? "SYSDBA",
                Password = Configuracao["Banco:Senha"] ?? "masterkey",
                Charset = "UTF8"
            }.ToString();

            var options = new DbContextOptionsBuilder<EspacoFestasContext>()
                .UseFirebird(connectionString)
                .Options;

            return new EspacoFestasContext(options);
        }
    }
}
