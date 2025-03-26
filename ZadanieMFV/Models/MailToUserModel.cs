namespace ZadanieMFV.Models
{

    /// <summary>
    /// Model do wyslania maila 
    /// </summary>
    public class MailToUserModel
    {
        public int IdUser { get; set; }
        public Guid GuidProcess { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

    }
}
