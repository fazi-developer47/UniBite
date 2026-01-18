using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Threading.Tasks;

namespace UniversityCanteenFoodOrderingSystem.Services
{
	public class DummyEmailSender : IEmailSender
	{
		public static string EmailMessage { get; private set; }
		public Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			EmailMessage = htmlMessage;
			// Console output for demo purposes
			Console.WriteLine($"[Dummy Email] To: {email}, Subject: {subject}");
			Console.WriteLine($"Message: {htmlMessage}");
			return Task.CompletedTask;
		}
	}
}
