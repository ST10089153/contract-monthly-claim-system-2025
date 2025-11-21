using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CMCS.Data;
using CMCS.Models;

namespace CMCS.Services
{
    public class InvoiceGenerationService
    {
        private readonly ApplicationDbContext _context;

        public InvoiceGenerationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Automated invoice generation for approved claims
        public async Task<Invoice> GenerateInvoiceAsync(int claimId)
        {
            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null || claim.Status != "Approved")
                return null;

            // Check if invoice already exists
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.ClaimId == claimId);

            if (existingInvoice != null)
                return existingInvoice;

            // Generate unique invoice number
            var invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{claimId:D4}";

            var invoice = new Invoice
            {
                ClaimId = claimId,
                InvoiceNumber = invoiceNumber,
                GeneratedDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                Amount = claim.TotalAmount,
                Status = "Generated"
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }

        // Batch invoice generation for multiple claims
        public async Task<int> GenerateBatchInvoicesAsync()
        {
            var approvedClaims = await _context.Claims
                .Where(c => c.Status == "Approved" &&
                           !_context.Invoices.Any(i => i.ClaimId == c.ClaimId))
                .ToListAsync();

            int invoicesGenerated = 0;

            foreach (var claim in approvedClaims)
            {
                var result = await GenerateInvoiceAsync(claim.ClaimId);
                if (result != null)
                    invoicesGenerated++;
            }

            return invoicesGenerated;
        }

        // Process invoice payment
        public async Task<bool> ProcessPaymentAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
                return false;

            invoice.Status = "Paid";
            await _context.SaveChangesAsync();

            return true;
        }
    }
}