namespace SmartProctor.Shared.Responses
{
    public class UserDetailsResponseModel : BaseResponseModel
    {
        public string Id { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}