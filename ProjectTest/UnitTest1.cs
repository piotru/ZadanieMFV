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

namespace ProjectTest
{
    public class Tests : IAsyncLifetime, IDisposable
    {
       


        public IManagedMqttClient MqttClient { get; private set; }
        public IHost Server { get; private set; }

        public Bunit.TestContext IndexPage { get; } // TODO : dispose

        public Tests()
        {

            // load secrets from .env file, or load them from env vars if running in a docker container
            //EnvironmentHelper.LoadEnvironmentVariables();

            // based upon the ASPNETCORE_ENVIRONMENT get the config meta data
            // ConfigMetaData = SettingsFile.GetConfigMetaData();

            var config = InitConfiguration();
            //zeby zaladowac zmienne srodowiskowe 
            //ladujemy wszystkie zaleznosci , tak prosciej niz ladowac  potrzebne nam tylko . 
            //Nalezy pamietac ze korzystamy z appsettings i uruchamia nam sie jako production.
           

            //var mockApplicationPrincipal = new Mock<IApplicationPrincipal>();
            //mockApplicationPrincipal.Setup(ap => ap.Id).Returns(1);
            //mockApplicationPrincipal.Setup(ap => ap.CurrentContextFacilityId).Returns(7);

            //var appPrincipal = new TestApplicationPrincipal(null);

            string[] args = new string[0];
            // build the host instance
            Server = (IHost?)Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseConsoleLifetime()
                .UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext())
            .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                     .UseKestrel()
                    .UseUrls("http://127.0.0.1:5000"); // TODO: workout how to make dynamic and reference with NavigationManager


                })
                .ConfigureServices((hostContext, service) =>
                {
                    
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                }).Build();

            
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
            //try
            //{

            //    await Server.RunAsync();
            //    await Server.StartAsync();

            //    //  MqttClient = (Server.Services.GetService(typeof(IMqttService)) as IMqttService).Client;

            //    InitialisePageDependencies();
            //}
            //catch (Exception ex)
            //{ 
            //    var t =ex.ToString();
            //}
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
