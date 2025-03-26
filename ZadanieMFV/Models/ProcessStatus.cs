using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZadanieMFV.Enums;

/*Tabela gdzie przechowujemy informacje  o statusach procesu . Polem laczacym procesy jest guid . 
 * W zaleznosci od intencji mozemy albo procesy wykonywac rownolegle albo sekwencynie
 * Oczywiscie jak chcemy rozbudowac ilosc procesow to  dodajemy identyfikator do listy i mamy mozliwosc prostej rozbudowy
 * To samo dotyczy logiki  czyli kolejnosci wykonania mozna zrobic rownolegle sekwencyjny ( najpierw A i B po zakonczeniu C i dalej D,E )
 * W praktyce  czesto jest tak ze z jakis powodow  procesy nam sie wykrzaczaja w zwiazku z tym w celu identyfikacji  zapisujemy start i koniec procesu tak zeby miec 
 * wiedze o na przyklad wydajnosci . 
 * Dodatkowo dodalem  kolumne Error ktore sluzy do przechowywania bledow wykonywanych przez dany proces   celem identyfikacji  blednego dzialania 
 * Tak jest latwiej niz sledzenie logow aplikacji 
*/
namespace ZadanieMFV.Models
{
    /// <summary>
    /// Prcocesy w systemie 
    /// </summary>
    public class ProcessStatusData
    {
        /// <summary>
        /// Id procesu
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        /// <summary>
        /// Guid Procesu
        /// </summary>
        public Guid Guid { get; set; }  
        /// <summary>
        /// Identyfikator procesu
        /// </summary>
        public ProcessEnum ProcessEnum { get; set; }
        /// <summary>
        /// Informacja o statusie 1 start , 2 zakonczony 
        /// </summary>
        public ProcessStatus ProcessStatus { get; set; }
        /// <summary>
        /// Start Procesu
        /// </summary>
        public DateTime Start { get; set; }
        /// <summary>
        /// Zakonczenie procesu 
        /// </summary>
        public DateTime ? End { get; set; }
        /// <summary>
        /// Ewentualne bledy ktore zapisujemy w momencie obslugi logiki procesu 
        /// </summary>
        [MaxLength(5000)]
        public string ? Errror { get; set; }

        //Ewentualne dodatkowe dane potrzebne w toku dzialania 
    }
}
