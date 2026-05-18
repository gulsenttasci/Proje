using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Hearty_Bites.Models;
using Microsoft.Extensions.Configuration;

namespace Hearty_Bites.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        
        public EmailService(IConfiguration configuration) 
        {
            _configuration = configuration;
        }
        public async Task SendOrderContractEmailAsync(string toEmail, Order order, string customerName, string catererName, string menuComposition)
        {
            
            string appPassword = _configuration["Email:AppPassword"] ?? string.Empty;
            var smtpClient = new SmtpClient("smtp.gmail.com") 
            {
                Port = 587, 
                Credentials = new NetworkCredential("gulsentasci47@gmail.com", appPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("no-reply@thecaterist.com", "The Caterist Platform Engine"),
                Subject = $"🧾 Order Confirmed & Catering Agreement - Order #{order.Id}",
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(toEmail);

            
            string htmlTemplate = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; color: #333; }}
                    .header {{ text-align: center; border-bottom: 2px solid #000; padding-bottom: 10px; }}
                    .brand {{ font-size: 24pt; font-weight: bold; }}
                    .section {{ font-size: 11pt; font-weight: bold; margin-top: 15px; border-bottom: 1px solid #ccc; }}
                    .content {{ font-size: 10pt; text-align: justify; line-height: 1.5; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <div class='brand'>THE CATERIST</div>
                    <div>CATERING MARKETPLACE</div>
                </div>
                
                <h3 style='text-align:center;'>DIGITAL TRANSACTION RECEIPT & AGREEMENT</h3>
                <p><strong>Order ID:</strong> #{order.Id}<br>
                   <strong>Transaction Date:</strong> {order.OrderDate:dd.MM.yyyy HH:mm}<br>
                   <strong>Target Event Date:</strong> {order.EventDate:dd.MM.yyyy}<br>
                   <strong>Delivery Address:</strong> {order.DeliveryAddress}</p>

                <div class='section'>ARTICLE 1 - PARTIES</div>
                <p class='content'>This agreement is signed digitally between the Client (<strong>{customerName}</strong>) and the authorized Marketplace Platform Network (<strong>The Caterist</strong>) on behalf of the assigned vendor (<strong>{catererName}</strong>).</p>

                <div class='section'>ARTICLE 4 - APPLIED MENU COMPOSITION</div>
                <p class='content'>The assigned Caterer (<strong>{catererName}</strong>) shall deliver the following exact configuration structure: <br>
                   <span style='color:#6b1d2f; font-weight:bold;'>{menuComposition}</span></p>

                <div class='section'>ARTICLE 9 - FINANCIAL TRANSACTION STRUCTURE</div>
                <p class='content'>This transaction functions under a unit price agreement metric. <br>
                   <strong>Total Amount Securely Settled:</strong> <span style='font-size:12pt; font-weight:bold; color:#2e7d32;'>{order.TotalAmount:N2} ₺</span> (TRY)</p>

                <div class='section'>ARTICLE 12 - JURISDICTION</div>
                <p class='content'>Ankara Batı Courts and Execution Directorates are authorized for all disputes regarding the execution of this automated system record.</p>
                
                <hr style='border: 1px dashed #000; margin-top:30px;'>
                <p style='font-size: 8.5pt; color: #666; text-align: center; font-style: italic;'>
                    Thank you for using The Caterist Core Infrastructure. This automated log stands as a validated transaction handshake record.
                </p>
            </body>
            </html>";

            mailMessage.Body = htmlTemplate;

            try
            {
                
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("--> Success: Agreement and Invoice email dispatched safely to " + toEmail);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"--> Non-Fatal Warning: Email delivery paused. Reason: {ex.Message}");
            }
        }
    }
}