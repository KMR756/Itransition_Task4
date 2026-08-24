


using System.Text;
using System.Text.Json;

namespace Itransition_Task4.Services
{
    public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
    {
        private static readonly HttpClient HttpClient = new();

        public async Task SendVerificationEmailAsync(string toEmail, string verifyLink)
        {
            try
            {
                var apiKey = configuration["Resend:ApiKey"] ?? configuration["Resend__ApiKey"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    logger.LogError("[EMAIL FAILURE] Resend API Key is missing in Configuration!");
                    return;
                }

                // Resend free tier without a custom domain requires sending FROM onboarding@resend.dev
                var payload = new
                {
                    from = "Task4 Security <onboarding@resend.dev>",
                    to = new[] { toEmail },
                    subject = "Verify Your Account Email",
                    html = $@"
                        <div style='font-family: Arial, sans-serif; padding: 24px; background-color: #0f172a; color: #f8fafc; border-radius: 12px;'>
                            <h2 style='color: #38bdf8; margin-top: 0;'>Email Verification Required</h2>
                            <p style='color: #cbd5e1;'>Thank you for registering! Please click the button below to verify your email address:</p>
                            <div style='margin: 24px 0;'>
                                <a href='{verifyLink}' style='padding: 12px 24px; background-color: #6366f1; color: #ffffff; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>Verify Email Address</a>
                            </div>
                            <p style='font-size: 12px; color: #64748b;'>If you did not create an account, you can safely ignore this email.</p>
                        </div>"
                };

                var json = JsonSerializer.Serialize(payload);
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await HttpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Verification email sent successfully to {Email} via Resend", toEmail);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    logger.LogError("[EMAIL FAILURE] Resend API returned status {Status}: {Error}", response.StatusCode, errorResponse);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[EMAIL FAILURE] Exception while sending email to {Email}", toEmail);
            }
        }
    }
}