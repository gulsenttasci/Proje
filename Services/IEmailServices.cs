using System.Threading.Tasks;
using Hearty_Bites.Models; 

namespace Hearty_Bites.Services
{
    public interface IEmailService
    {
        
        Task SendOrderContractEmailAsync(string toEmail, Order order, string customerName, string catererName, string menuComposition);
    }
}