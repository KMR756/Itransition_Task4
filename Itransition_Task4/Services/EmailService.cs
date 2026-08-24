using System.Net;
using System.Net.Mail;

namespace Itransition_Task4.Services
{
    public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
    {
        public async Task SendVerificationEmailAsync(string toEmail, string verifyLink)
        {
            try
            {
                var host = configuration["Smtp:Host"] ?? "smtp.gmail.com";
                var port = int.Parse(configuration["Smtp:Port"] ?? "587");
                var senderEmail = configuration["Smtp:Email"];
                var senderPassword = configuration["Smtp:Password"];

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    logger.LogWarning("SMTP credentials are missing in appsettings.json. Skipping email dispatch.");
                    return;
                }

                using var smtpClient = new SmtpClient(host)
                {
                    Port = port,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true,
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Task4 App Security"),
                    Subject = "Verify Your Account Email",
                    Body = $@"
                        <div style='font-family: Arial, sans-serif; padding: 24px; background-color: #0f172a; color: #f8fafc; border-radius: 12px;'>
                            <h2 style='color: #38bdf8; margin-top: 0;'>Email Verification Required</h2>
                            <p style='color: #cbd5e1;'>Thank you for registering! Please click the button below to verify your email address:</p>
                            <div style='margin: 24px 0;'>
                                <a href='{verifyLink}' style='padding: 12px 24px; background-color: #6366f1; color: #ffffff; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>Verify Email Address</a>
                            </div>
                            <p style='font-size: 12px; color: #64748b;'>If you did not create an account, you can safely ignore this email.</p>
                        </div>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);
                await smtpClient.SendMailAsync(mailMessage);

                logger.LogInformation("Verification email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send verification email to {Email}", toEmail);
            }
        }
    }
}