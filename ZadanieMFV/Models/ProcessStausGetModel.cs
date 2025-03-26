using ZadanieMFV.Enums;

namespace ZadanieMFV.Models
{


    public class ProcessStausGetModel
    {

        public Guid Guid { get; set; }
        public ProcessEnum Process { get; set; }
        public string Erorr { get; set; }

    }
}
