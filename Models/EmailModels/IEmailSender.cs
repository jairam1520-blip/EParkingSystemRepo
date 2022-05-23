namespace ParkingSystem.Models.EmailModels
{
    public interface IEmailSender
    {
        void SendEmail(Message message);
    }
}
