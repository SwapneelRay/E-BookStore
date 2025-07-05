using Microsoft.AspNetCore.Identity.UI.Services;


namespace E_BookStore.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //Email sender login Here
           return Task.CompletedTask;
        }
    }
}
