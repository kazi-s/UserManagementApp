using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace UserManagementApi.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendConfirmationEmailAsync(string email, string name, string token)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("User Management System", _configuration["EmailSettings:SenderEmail"]));
            message.To.Add(new MailboxAddress(name, email));
            message.Subject = "Confirm Your Email Address";

            var confirmationLink = $"http://localhost:5080/api/auth/confirm-email?email={email}&token={token}";
            
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $@"
                <h2>Welcome, {name}!</h2>
                <p>Thank you for registering. Please confirm your email address by clicking the link below:</p>
                <p><a href='{confirmationLink}'>Confirm Email</a></p>
                <p>If the link doesn't work, copy and paste this URL into your browser:</p>
                <p>{confirmationLink}</p>
                <p>This link will expire in 24 hours.</p>
                <br>
                <p>Best regards,<br>User Management Team</p>
            ";

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["EmailSettings:SmtpServer"], 
                int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587"), 
                SecureSocketOptions.StartTls
            );
            
            await client.AuthenticateAsync(
                _configuration["EmailSettings:SenderEmail"],
                _configuration["EmailSettings:SenderPassword"]
            );
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation($"Confirmation email sent to {email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send confirmation email to {email}");
            throw;
        }
    }
}