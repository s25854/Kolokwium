using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ClientController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet("GetClientData/{id}")]
        public async Task<IActionResult> GetClientData(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Sales)
                    .ThenInclude(s => s.Subscription)
                .Include(c => c.Payments)
                    .ThenInclude(p => p.Subscription)
                .FirstOrDefaultAsync(c => c.IdClient == id);

            if (client == null)
            {
                return NotFound("Client not found.");
            }

            var subscriptions = client.Sales.GroupBy(s => s.IdSubscription)
                .Select(group => new SubscriptionResponse
                {
                    IdSubscription = group.Key,
                    Name = group.First().Subscription.Name,
                    TotalPaidAmount = client.Payments
                        .Where(p => p.IdSubscription == group.Key)
                        .Sum(p => p.Amount)
                }).ToList();

            var result = new ClientDataResponse
            {
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                Phone = client.Phone,
                Subscriptions = subscriptions
            };

            return Ok(result);
        }

        [HttpPost("AddPayment")]
        public async Task<IActionResult> AddPayment([FromBody] PaymentRequest request)
        {
            var client = await _context.Clients.FindAsync(request.IdClient);
            if (client == null)
            {
                return BadRequest("Client not found.");
            }

            var subscription = await _context.Subscriptions.FindAsync(request.IdSubscription);
            if (subscription == null)
            {
                return BadRequest("Subscription not found.");
            }

            var payment = new Payment
            {
                IdClient = request.IdClient,
                IdSubscription = request.IdSubscription,
                Amount = request.Amount,
                Date = request.Date
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok("Payment added successfully.");
        }
    }

    public class ClientDataResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<SubscriptionResponse> Subscriptions { get; set; }
    }

    public class SubscriptionResponse
    {
        public int IdSubscription { get; set; }
        public string Name { get; set; }
        public decimal TotalPaidAmount { get; set; }
    }

    public class PaymentRequest
    {
        public int IdClient { get; set; }
        public int IdSubscription { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}