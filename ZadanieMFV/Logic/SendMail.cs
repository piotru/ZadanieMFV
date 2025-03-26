using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Net.Mail;
using System.Net;
using ZadanieMFV.Database;
using ZadanieMFV.Models;
using Serilog;

namespace ZadanieMFV.Logic
{
    public class SendMail
    {

        public SendMail() { }

       public async Task<MailStatusModel> SendMailToUser(MailToUserModel message)
       {
          MailStatusModel status = new MailStatusModel();
            //ustawiamy na error bo dopiero wyslanie  ustawia na sukces 
          status.Status= Enums.MailStatusEnum.Error;
            //Sprawdzamy czy jest user 
            using var context = new ProcessContext();
            //Tu zastosowalem inna konstrukcje klauzuli where  , odpytujemy po email ale mozna rownie dobrze po id  wszystko zalezy jak wyglada logika biznesowa apki
          
                var user = context.Users.FirstOrDefault(a => a.Email == message.Email);

            if (!( context.Database.CanConnect()))
            {
                status.Error = Enums.MailErrorEnum.ErrorDatabaseConnection;
                return status;
                /*
                 * Jesli uruchamiamy aplikacje to ona sie laczy do bazy danych  i entity zarzadza polaczeniami , generalnie jest tak ze polaczenie jest utrzymywane 
                 * Oczywiscie moze sie zdarzyc ze baza jest nei dostepna ale w tym przypadku albo dostajemy exception z aplikacji i apka staje 
                 * Jesli laczymy sie i jest jakis blad to dostajemy SQL Exception i   wpka powinna sie zatrzymac
                 * Tak samo jest w przypadku obslugi bledow w momencie wykonywania jakies akcji  zwiazanej z baza danych 
                 * Oczywiscie w apkach desktopowych  uzywamy sprawdzenia polaczenia bo moze sie zdarzyc ale w tym przypadku zwracamy  uzyskany blad  ktory 
                 * pomaga ustalic przyczyne problemu 
                 * W apkach webowych zakladamy ze  dziala wszystko a jak nie  to  przy operacji dostaniemy blad 
                 */
            }

            if (user is null)
            {
                status.Error = Enums.MailErrorEnum.UserUnknown;
                return status;

            }
            //Pobieramy liste procesow do danego zadania ( guid) 
            var process = await context.ProcessStatus.Where(a => a.Guid == message.GuidProcess).ToListAsync();

            if (process is null)
            {
                //Mozna oczywiscie dodac specjalny blad 
                status.Error = Enums.MailErrorEnum.ErrorDataEntry;
                return status;
            }
            //Jesli ktorys dziala to blad 
            if (process.FirstOrDefault(a => a.ProcessStatus == Enums.ProcessStatus.Start) != null)
            {
                status.Error = Enums.MailErrorEnum.ProcessRun;
                return status;

            }
            //Jesli jakis ma status nieznany  to tez blad 
            if (process.FirstOrDefault(a => a.ProcessStatus == Enums.ProcessStatus.Unknown) != null)
            {
                status.Error = Enums.MailErrorEnum.ErrorDataEntry;
                return status;

            }
            /*
             * Specjalnie uzylem takiej konstrukcji gdyz w praktyce bledy sa duzo bardziej rozbudowane i tak naprawde zaleza od wymagan biznesowych 
             * lub tez  logiki aplikacji 
             * Z mailami jest tez taki problem ze tak naprawde wysylamy je do kolejki na serwerze  i  bledy ktore uzyskamy to najwyzej brak polaczenia , zly uzytkownik
             * zly host  a jak pojdzie to juz nie wiemy co sie z nim dzieje
             * Nie ma tez jak odpytac serwera o status maila 
             * Jak cos bedzie nie tak to serwer bedzie  probowal wyslac a my o tym nie bedziemy wiedzieli
             * Oczywisicie wroci do skrzynki odbiorczej i tu by mozna przez obsluge etykiet ( guid na przyklad ) odpytac czy nie ma bledu 
             * Lepiej i bezpieczniej jest uzyc innej metody na przyklad udostepnic  link unikalny do zasobu ktory wysylamy a userowi po zalogowaniu  pokazac informacje 
             * o tym ze plik jest 
             * Wtedy mamy pewnosc ze dojdzie do usera 
             */
            var statusmail = await MailSend(message);

            if (statusmail is null)
            {
                status.Error = Enums.MailErrorEnum.ErrorDataEntry;
                return status;
            }
            else if ( statusmail.Error==Enums.MailErrorEnum.ErrorDataEntry)
            {
                status.Error = statusmail.Error;
            }


            if (statusmail.Status==Enums.MailStatusEnum.Sucess)
            {
                status.Status= Enums.MailStatusEnum.Sucess;

                return status;
            }


            return status;
        
       }
        /// <summary>
        /// Wysylanie maila 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<MailStatusModel> MailSend(MailToUserModel message)
        {
            MailStatusModel mailStatusModel = new MailStatusModel();


            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("");
            mailMessage.To.Add(message.Email);
            mailMessage.Subject =message.Subject;
            mailMessage.Body = message.Body;
            /* Tu powinna byc obsluga  ustawien do wysylania  maili
             * Mozna uzyc appsettings lub innego pliku , mozna tez bazy danych ale ze jako mamy in memory  to troche bez sensu 
             * Nie mam ustawien wiec tak zostawie 
             */
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = "smtp.mywebsitedomain.com";
            smtpClient.Port = 587;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("username", "password");
            smtpClient.EnableSsl = true;

            try
            {

                await  smtpClient.SendMailAsync(mailMessage);
                mailStatusModel.Status= Enums.MailStatusEnum.Sucess;
            }
            catch (Exception ex)
            {

                mailStatusModel.Error = Enums.MailErrorEnum.ErrorDataEntry;
                Log.Error(ex, "An error occurred while processing the request.");
            }

            return mailStatusModel;
        }


    }
}
