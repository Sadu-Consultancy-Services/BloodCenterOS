using System.Security.Claims;
using BloodCenterOS.API.Repositories;
using BloodCenterOS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingRepository _repo;
    public BillingController(IBillingRepository repo) => _repo = repo;

    private long CenterId => long.TryParse(User.FindFirst("CenterId")?.Value, out var id) ? id : 0;
    private long UserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetByCenterAsync(CenterId);
        return Ok(ApiResponse<IEnumerable<Billing>>.Ok(items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var invoice = await _repo.GetByIdAsync(id);
        if (invoice == null) return NotFound(ApiResponse<string>.Fail("Invoice not found"));
        var details = await _repo.GetDetailAsync(id);
        var result = new InvoiceWithDetails { Invoice = invoice, Details = details.ToList() };
        return Ok(ApiResponse<InvoiceWithDetails>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Billing billing)
    {
        billing.CenterId = CenterId;
        var id = await _repo.CreateBillingAsync(billing);
        billing.BillingTransactionId = id;
        return CreatedAtAction(null, ApiResponse<Billing>.Ok(billing, "Invoice created"));
    }

    [HttpPost("{billingId}/payment")]
    public async Task<IActionResult> AddPayment(long billingId, [FromQuery] decimal amount, [FromQuery] string mode, [FromQuery] string? reference)
    {
        var id = await _repo.AddPaymentAsync(billingId, CenterId, amount, mode, reference, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Payment recorded"));
    }

    [HttpGet("dues")]
    public async Task<IActionResult> GetDues([FromQuery] string? keyword)
    {
        var items = await _repo.GetDuesAsync(CenterId, keyword);
        return Ok(ApiResponse<IEnumerable<DuesRegisterItem>>.Ok(items));
    }

    [HttpPost("credit-note")]
    public async Task<IActionResult> CreateCreditNote([FromBody] CreditNoteRequest req)
    {
        var id = await _repo.CreateCreditNoteAsync(CenterId, req.OriginalInvoiceId, req.Amount, req.Reason, UserId);
        return Ok(ApiResponse<long>.Ok(id, "Credit note created"));
    }
}
