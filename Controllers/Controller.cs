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
        
        [HttpGet("{idClient}")]
        public async Task<IActionResult> GetClientWithSubscriptions(int idClient)
        {
        var clientData = await _context.Clients
        .Where(c => c.IdClient == idClient)
        .Select(c => new ClientDto
        {
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            Subscriptions = c.Payments
                .GroupBy(p => p.Subscription)
                .Select(g => new SubscriptionDto
                {
                    IdSubscription = g.Key.IdSubscription,
                    Name = g.Key.Name,
                    TotalPaidAmount = g.Sum(p => p.Subscription.Price)
                })
                .ToList()
                })
                .FirstOrDefaultAsync();

            if (clientData == null)
            {
                return NotFound();
            }

            return Ok(clientData);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDto request)
        {
            var client = await _context.Clients.FindAsync(request.IdClient);
            if (client == null)
            {
                return NotFound("Klient nie istnieje");
            }

            var subscription = await _context.Subscriptions.FindAsync(request.IdSubscription);
            if (subscription == null)
            {
                return NotFound("Subskrypcja nie istnieje.");
            }

            if (subscription.EndTime < DateTime.UtcNow)
            {
                return BadRequest("Subskrypcja nie aktywna.");
            }

            var lastSale = await _context.Sales
                .Where(s => s.IdClient == request.IdClient && s.IdSubscription == request.IdSubscription)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastSale == null)
            {
                return BadRequest("brak rekordu sprzedaży");
            }

            var nextPaymentStartDate = lastSale.CreatedAt.AddMonths(subscription.RenewalPeriod);

            var existingPayment = await _context.Payments
                .Where(p => p.IdClient == request.IdClient && p.IdSubscription == request.IdSubscription && p.Date >= nextPaymentStartDate)
                .FirstOrDefaultAsync();

            if (existingPayment != null)
            {
                return BadRequest("Płatność dokonana");
            }

            decimal finalPaymentAmount = subscription.Price;

            var highestDiscount = await _context.Discounts
                .Where(d => d.IdSubscription == request.IdSubscription && d.DateFrom <= DateTime.UtcNow && d.DateTo >= DateTime.UtcNow)
                .OrderByDescending(d => d.Value)
                .FirstOrDefaultAsync();

            if (highestDiscount != null)
            {
                finalPaymentAmount -= finalPaymentAmount * (highestDiscount.Value / 100.0m);
            }

            if (request.Payment != finalPaymentAmount)
            {
                return BadRequest($"Nieprawidłowa płatność. Powinno być: {finalPaymentAmount}");
            }

            var payment = new Payment
            {
                Date = DateTime.UtcNow,
                IdClient = request.IdClient,
                IdSubscription = request.IdSubscription
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new { IdPayment = payment.IdPayment });
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