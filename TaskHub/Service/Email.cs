namespace TaskHub.Service
{
    public class Email : IEmailSender
    {
        private readonly ILogger<Email> _logger;

        public Email(ILogger<Email> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation("========== EMAIL ==========");
            _logger.LogInformation("To: {Email}", toEmail);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body:\n{Body}", body);
            _logger.LogInformation("===========================");

            return Task.CompletedTask;
        }
    }
}