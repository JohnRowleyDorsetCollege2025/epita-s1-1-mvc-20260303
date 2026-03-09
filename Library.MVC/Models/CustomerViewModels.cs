using System.ComponentModel.DataAnnotations;

namespace Library.MVC.Models;

public class CustomerSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int InvoiceCount { get; set; }
}

public class CustomerFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = "";
}

public class CustomerDeleteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int InvoiceCount { get; set; }
    public bool CanDelete => InvoiceCount == 0;
}

// ── Customer → Invoices list page ──────────────────────────────────────────

public class CustomerInvoicesViewModel
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public List<CustomerInvoiceRowViewModel> Invoices { get; set; } = [];
}

public class CustomerInvoiceRowViewModel
{
    public int InvoiceId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public int LineCount { get; set; }
    public decimal Total { get; set; }
}

// ── Customer → Invoice detail page ─────────────────────────────────────────

public class CustomerInvoiceDetailViewModel
{
    // Customer section
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";

    // Invoice section
    public int InvoiceId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public List<InvoiceLineDetailsModel> Lines { get; set; } = [];
    public decimal Total => Lines.Sum(l => l.LineTotal);
}
