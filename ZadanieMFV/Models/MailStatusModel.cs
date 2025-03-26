using ZadanieMFV.Enums;

namespace ZadanieMFV.Models
{
    public class MailStatusModel
    {
        public int Id { get; set; }
        public MailErrorEnum Error { get; set; }

        public MailStatusEnum Status { get; set; }

    }
}
