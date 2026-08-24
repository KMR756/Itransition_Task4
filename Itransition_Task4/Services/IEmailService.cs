namespace Itransition_Task4.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string verifyLink);
    }
}