using Xunit;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Bunit;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;
using MQTTnet.Extensions.ManagedClient;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.AspNetCore.Hosting;
using ZadanieMFV.Logic;
using ZadanieMFV.Models;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.Extensions.Logging;

namespace ProjectTest
{

    /*
     * Troche poszalalem . Ale chodzi o to zeby dynamicznie ladowac zaleznosci oraz  testowac funkcje bezposrednio  a nie przez request . 
     * Ladujemy zaleznosci poczym  budujemy model oraz uruchamiamy funkcje 
     * jest to troche inne podejscie niz  zazwyczaj 
     * Pozwala to testowac poza warstwa prezentacji . 
     * Oczywiscie w przypadku endpointow  jest to moze podejscie  na okolo  no bo prosciej przepytac api . Ale takie podejscie 
     * pozwala na czastkowe testowanie funkcji nie zaleznie 
     * A w przypadku aplikacji ktore nie posiadaja endpointow ( na przyklad blazor )  pozwala na ladowanie zaleznosci i testowanie 
     * W tym przypadku uzylem  tylko jednej klasy ale tak naprawde mozna zaladowac calosc 
     * 
     */
    public class Tests : IAsyncLifetime, IDisposable
    {
       


        public IManagedMqttClient MqttClient { get; private set; }
        public IHost Server { get; private set; }
        
        public Bunit.TestContext IndexPage { get; } // TODO : dispose

        public Tests()
        {

            // load secrets from .env file, or load them from env vars if running in a docker container
           // EnvironmentHelper.LoadEnvironmentVariables();

            // based upon the ASPNETCORE_ENVIRONMENT get the config meta data
            // ConfigMetaData = SettingsFile.GetConfigMetaData();

            var config = InitConfiguration();
           



            string[] args = new string[0];
            // build the host instance
           
            Server = new HostBuilder()
               .UseServiceProviderFactory(new AutofacServiceProviderFactory())
               .ConfigureLogging(logging =>
               {
                   logging.ClearProviders();
                   logging.AddConsole();
                   logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Information);
               })
               .ConfigureWebHost(webBuilder =>
               {
                   webBuilder
                    //.UseStartup<Startup>()
                   .UseKestrel()
                   
                   .UseUrls("https://127.0.0.1:5000"); // TODO: workout how to make dynamic and reference with NavigationManager
                   
               })
               .ConfigureServices((hostContext, service) => {
                   service.AddScoped<SendMail>();
              })
               .Build();
        }
        private void InitialisePageDependencies()
        {
            
        }

        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            return config;
        }


        [SetUp]
        public void Setup()
        {
            InitialisePageDependencies();
        }

        [Test]
        public void Test1()
        {
            MailToUserModel mailToUserModel = new MailToUserModel();
            mailToUserModel.Email = "piotrusu@gmail.com";
            mailToUserModel.Subject = "test";
            mailToUserModel.Body = "message";

            var cut = Server.Services.GetRequiredService<SendMail>();

            var result =  cut.SendMailToUser(mailToUserModel);
        }

        public async Task InitializeAsync()
        {
            //Poniewaz nie uruchamiamy tylko ladujemy zaleznosci nie ma potrzeby odpalac aplikacji 
            try
            {

                await Server.RunAsync();
                await Server.StartAsync();

                //  MqttClient = (Server.Services.GetService(typeof(IMqttService)) as IMqttService).Client;

                
            }
            catch (Exception ex)
            {
                var t = ex.ToString();
            }
        }
        public async Task DisposeAsync()
        {
            await Server?.StopAsync();
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Server?.Dispose();
            }

            Server = null;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
