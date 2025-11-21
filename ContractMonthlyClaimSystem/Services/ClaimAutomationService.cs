using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CMCS.Data;
using CMCS.Models;

namespace CMCS.Services
{
    public class ClaimAutomationService
    {
        private readonly ApplicationDbContext _context;
        private const decimal MAX_HOURS_PER_MONTH = 200;
        private const decimal APPROVAL_AMOUNT_THRESHOLD = 50000;

        public ClaimAutomationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Automated claim submission with validation
        public async Task<(bool Success, string Message, Claim Claim)> SubmitClaimAsync(Claim claim)
        {
            try
            {
                // Automated validation checks
                var validationResult = await ValidateClaimAsync(claim);
                if (!validationResult.IsValid)
                    return (false, validationResult.Message, null);

                // Auto-calculate total amount
                claim.CalculateTotal();
                claim.SubmissionDate = DateTime.Now;
                claim.Status = "Pending";

                // Check for automatic flagging based on business rules
                if (claim.HoursWorked > MAX_HOURS_PER_MONTH)
                {
                    claim.Status = "Flagged";
                    await AddApprovalHistoryAsync(claim, "System", "Flagged",
                        $"Hours exceed maximum limit of {MAX_HOURS_PER_MONTH}");
                }
                else if (claim.TotalAmount > APPROVAL_AMOUNT_THRESHOLD)
                {
                    claim.Status = "Flagged";
                    await AddApprovalHistoryAsync(claim, "System", "Flagged",
                        $"Amount exceeds threshold of R{APPROVAL_AMOUNT_THRESHOLD}");
                }

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                return (true, "Claim submitted successfully", claim);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", null);
            }
        }

        // Automated validation
        private async Task<(bool IsValid, string Message)> ValidateClaimAsync(Claim claim)
        {
            // Check for required fields
            if (claim.HoursWorked <= 0)
                return (false, "Hours worked must be greater than 0");

            if (claim.HourlyRate <= 0)
                return (false, "Hourly rate must be greater than 0");

            // Check for duplicate claims in the same month
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var existingClaim = await _context.Claims
                .Where(c => c.LecturerId == claim.LecturerId &&
                           c.SubmissionDate.Month == currentMonth &&
                           c.SubmissionDate.Year == currentYear)
                .FirstOrDefaultAsync();

            if (existingClaim != null)
                return (false, "A claim for this month has already been submitted");

            return (true, "Valid");
        }

        // Automated approval workflow
        public async Task<bool> ProcessApprovalWorkflowAsync(int claimId, int approverId, bool approve, string comments = "")
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null) return false;

            var approver = await _context.Users.FindAsync(approverId);
            if (approver == null) return false;

            // Automated approval logic based on role and amount
            if (approve)
            {
                // Auto-approve small claims for coordinators
                if (approver.Role == "Coordinator" && claim.TotalAmount <= 10000)
                {
                    claim.Status = "Approved";
                    await AddApprovalHistoryAsync(claim, approver.FullName, "Approved",
                        "Auto-approved: Amount within coordinator limit");
                }
                // Require manager approval for larger amounts
                else if (approver.Role == "Manager")
                {
                    claim.Status = "Approved";
                    await AddApprovalHistoryAsync(claim, approver.FullName, "Approved", comments);
                }
                else
                {
                    claim.Status = "Pending Manager Approval";
                    await AddApprovalHistoryAsync(claim, approver.FullName, "Escalated",
                        "Escalated to manager: Amount exceeds coordinator limit");
                }
            }
            else
            {
                claim.Status = "Rejected";
                await AddApprovalHistoryAsync(claim, approver.FullName, "Rejected", comments);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task AddApprovalHistoryAsync(Claim claim, string approverName, string action, string comments)
        {
            // For system actions, use system user ID (0)
            int approverId = 0;
            if (approverName != "System")
            {
                var approver = await _context.Users.FirstOrDefaultAsync(u =>
                    u.FirstName + " " + u.LastName == approverName);
                if (approver != null)
                    approverId = approver.UserId;
            }

            var history = new ApprovalHistory
            {
                ClaimId = claim.ClaimId > 0 ? claim.ClaimId : 0,
                ApproverId = approverId,
                Action = action,
                Comments = comments,
                ActionDate = DateTime.Now
            };

            if (claim.ClaimId > 0) // Only add if claim is already saved
                _context.ApprovalHistory.Add(history);
        }
    }
}