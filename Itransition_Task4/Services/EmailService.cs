using MailKit.Security;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Itransition_Task4.Services
{
    public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
    {
        public async Task SendVerificationEmailAsync(string toEmail, string verifyLink)
        {
            try
            {
                var host = configuration["Smtp:Host"] ?? configuration["Smtp__Host"] ?? "smtp.gmail.com";

                // FORCE Port 465 for cloud hosting like Render
                var portString = configuration["Smtp:Port"] ?? configuration["Smtp__Port"] ?? "465";
                var port = int.Parse(portString);

                var senderEmail = configuration["Smtp:Email"] ?? configuration["Smtp__Email"];
                var senderPassword = configuration["Smtp:Password"] ?? configuration["Smtp__Password"];

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    logger.LogError("[EMAIL FAILURE] SMTP credentials missing in configuration!");
                    return;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Task4 App Security", senderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Verify Your Account Email";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 24px; background-color: #0f172a; color: #f8fafc; border-radius: 12px;'>
                            <h2 style='color: #38bdf8; margin-top: 0;'>Email Verification Required</h2>
                            <p style='color: #cbd5e1;'>Thank you for registering! Please click the button below to verify your email address:</p>
                            <div style='margin: 24px 0;'>
                                <a href='{verifyLink}' style='padding: 12px 24px; background-color: #6366f1; color: #ffffff; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>Verify Email Address</a>
                            </div>
                            <p style='font-size: 12px; color: #64748b;'>If you did not create an account, you can safely ignore this email.</p>
                        </div>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                // 10 second timeout so background worker threads don't hang
                client.Timeout = 10000;

                // Force SSL on Connect for port 465
                var socketOptions = (port == 465)
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                await client.ConnectAsync(host, port, socketOptions);
                await client.AuthenticateAsync(senderEmail, senderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                logger.LogInformation("Verification email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[EMAIL FAILURE] Exception while sending email to {Email}", toEmail);
            }
        }
    }
}