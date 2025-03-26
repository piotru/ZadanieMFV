using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using System;
using ZadanieMFV.Database;
using ZadanieMFV.Models;

namespace ZadanieMFV.Logic
{
    public class ProcessTools
    {
        //Uzywamy bazy in memory wiec nie inicjalizujemy polaczen z baza 
        //ProcessContext context;

        //public ProcessTools(ProcessContext _context) {
        //    this.context = _context;
        //}

        /// <summary>
        /// First Process 
        /// </summary>
        /// <returns></returns>
        public async Task<ProcessStatusModel> StartProcess(ProcessStartModel processStartModel)
        {
            Guid guid = Guid.NewGuid();

            ProcessStatusModel processStatusModel = new ProcessStatusModel();

            processStatusModel.Guid = guid;


            using var context = new ProcessContext();
            /*UWAGA tu jest cala  zabawa , uzywamy bazy in memory wiec nie mamy potrzeby sterowac z poziomu  aplikacji  transakcyjnosci 
             * Nie mniej jednak przy inicjacji bazy danych powinno sie wskazac strategie transakcji w zaleznosci od efektu  ktory chcemy osiagnac 
             * Bledy moga doprowadzic do drastycznego  spowolnienia naszej aplikacji . 
             * uzycie najbardziej restrykcyjnej strategi powodowac bedzie  zatrzymanie dzialania aplikacji do czasu az zrobi sie commit . Jesli to bedzie duza baza danych 
             * to   przebudowanie  indeksow bedzie chwile trwalo w efekcie  kazdy select bedzie czekal 
             * W tym przypadku raczej nie uzywal bym transakcji gdyz  dane  sa relatywnie male oraz sam entity  ma mechanizmy ktore pilnuja integralnosci 
             * wiecej https://learn.microsoft.com/en-us/ef/ef6/saving/transactions
            */
            using var dbContextTransaction = context.Database.BeginTransaction();
            try
            {
                context.ProcessStatus.Add(new ProcessStatusData
                {

                    Guid = guid,
                    Start = DateTime.Now,
                    ProcessStatus = Enums.ProcessStatus.Start,
                    ProcessEnum = Enums.ProcessEnum.ModulA,


                });

                await context.SaveChangesAsync();
                processStatusModel.Status = Enums.ProcessStatus.Start;
                dbContextTransaction.Commit();



            }
            catch
            {
                dbContextTransaction.Rollback();
                processStatusModel.Status = Enums.ProcessStatus.Unknown;

            }
            finally
            {
                //Generalnie nie trzeba uzywac tej metody, nie zamykamy polaczen 
                /*Beginning a transaction requires that the underlying store connection is open.
                 * So calling Database.BeginTransaction() will open the connection if it is not already opened.
                 * If DbContextTransaction opened the connection then it will close it when Dispose() is called.
                 */
                //  dbContextTransaction.Dispose();
            }

            return processStatusModel;

        }
        /*
         * Tu w zaleznosci od zaimplementowanej logiki  odpytujemy aplikacje o status  procesu  i w zaleznosci od danych w bazie zwracamy odpowieni status
         */
        /// <summary>
        /// Pobieramy informacje o procesach w jakim statusie jest 
        /// </summary>
        /// <param name="processStausGetModel"></param>
        /// <returns></returns>

        public async Task<ProcessStatusModel> GetStatusProcess(ProcessStausGetModel processStausGetModel)
        {
            ProcessStatusModel processStatusModel = new ProcessStatusModel();

            using var context = new ProcessContext();
            //Uzycie transakcji w selectach jest grubym bledem
            //szukamy procesu o podanym guid oraz typie ( w zalezenosci od przyjetej logiki ) 
            var process = context.ProcessStatus.Where(a => a.Guid == processStausGetModel.Guid && a.ProcessEnum == processStausGetModel.Process).FirstOrDefault();

            if (process != null)
            {
                processStatusModel.Status = process.ProcessStatus;
            }
            else
            {
                //jesli nie ma o takim guid to nieznany ( logika powinna obsluzyc ten problem ) 
                processStatusModel.Status = Enums.ProcessStatus.Unknown;
            }


            return processStatusModel;
        }
        /*
         * Zakladamy ze jakiekolwiek bledy koncza proces , oczywiscie moze byc inaczej wetedy trzeba dodac dodatkowa metode 
         */

        /// <summary>
        /// Zakonczenie procesu 
        /// </summary>
        /// <param name="processStausGetModel"></param>
        /// <returns></returns>
        public async Task<ProcessStatusModel> EndProcess(ProcessStausGetModel processStausGetModel)
        {
            ProcessStatusModel processStatusModel = new ProcessStatusModel();




            using var context = new ProcessContext();

            using var dbContextTransaction = context.Database.BeginTransaction();
            try
            {
                var process = context.ProcessStatus.Where(a => a.Guid == processStausGetModel.Guid && a.ProcessEnum == processStausGetModel.Process).FirstOrDefault();

                if (process != null)
                {
                    process.End = DateTime.Now;
                    process.ProcessStatus = Enums.ProcessStatus.End;
                    process.Errror = processStausGetModel.Erorr;

                    context.ProcessStatus.Entry(process).State = EntityState.Modified;

                    /*
                     * Mozna  context.ProcessStatus.Update(process);
                     */

                    await context.SaveChangesAsync();

                    dbContextTransaction.Commit();
                }
                else
                {
                    //jesli nie ma o takim guid to nieznany ( logika powinna obsluzyc ten problem ) 
                    processStatusModel.Status = Enums.ProcessStatus.Unknown;
                }



            }
            catch (Exception ex) 
            {
                dbContextTransaction.Rollback();
                processStatusModel.Status = Enums.ProcessStatus.Unknown;
                Log.Error(ex, "An error occurred while processing the request.");
            }
            finally
            {
                //Generalnie nie trzeba uzywac tej metody, nie zamykamy polaczen 
                /*Beginning a transaction requires that the underlying store connection is open.
                 * So calling Database.BeginTransaction() will open the connection if it is not already opened.
                 * If DbContextTransaction opened the connection then it will close it when Dispose() is called.
                 */
                //  dbContextTransaction.Dispose();
            }

            return processStatusModel;
        }




        /*
         * Generalnie w systemach nie powinno sie kasowac rekordow  z wielu wzgledow Nie mniej jednak zgodnie z zadaniem  CRUD (Create, Read, Update, Delete)
         * dodam ta funkcje . W praktyce  mozliwosc analizowania procesow pozwala zdobyc informacje o tym jak przebiega nasza logika  i czy nie ma jakis problemow
         * kasowanie oczywiscie pozbawilo by mozliwosci  analizy .
         * Tak wiec dla przykladu , zwracam standardowy model ale mozemy  dodac jakis inny w zaleznosci od potrzeb 
         */


        public async Task<ProcessStatusModel> RemoveProcess(ProcessStausGetModel processStausGetModel)
        {
            ProcessStatusModel processStatusModel = new ProcessStatusModel();




            using var context = new ProcessContext();

            using var dbContextTransaction = context.Database.BeginTransaction();
            try
            {
                var process = context.ProcessStatus.Where(a => a.Guid == processStausGetModel.Guid && a.ProcessEnum == processStausGetModel.Process).FirstOrDefault();

                if (process != null)
                {


                    context.ProcessStatus.Remove(process);

                  

                    await context.SaveChangesAsync();

                    dbContextTransaction.Commit();
                }
                else
                {
                    //jesli nie ma o takim guid to nieznany ( logika powinna obsluzyc ten problem ) 
                    processStatusModel.Status = Enums.ProcessStatus.Unknown;
                }



            }
            catch ( Exception ex ) 
            {
                dbContextTransaction.Rollback();
                processStatusModel.Status = Enums.ProcessStatus.Unknown;
                Log.Error(ex, "An error occurred while processing the request.");
            }
            finally
            {
                //Generalnie nie trzeba uzywac tej metody, nie zamykamy polaczen 
                /*Beginning a transaction requires that the underlying store connection is open.
                 * So calling Database.BeginTransaction() will open the connection if it is not already opened.
                 * If DbContextTransaction opened the connection then it will close it when Dispose() is called.
                 */
                //  dbContextTransaction.Dispose();
            }

            return processStatusModel;
        }


    }

}
