
namespace VoiceX.Models
{
    public class User_data
    {
        public string UserID { get; set; }
        public string CompanyID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public User_data()
        {
            CompanyID = "";
            UserID = "";
            Email = "";
            Name = "";
        }
    }
}
