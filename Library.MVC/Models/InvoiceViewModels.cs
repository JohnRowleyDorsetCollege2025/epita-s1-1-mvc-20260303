using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Library.MVC.Models;

public class InvoiceLineFormModel
{
    [Required(ErrorMessage = "Please select a product.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a product.")]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; } = 1;

    [Required]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Unit price must be greater than 0.")]
    public decimal UnitPrice { get; set; }
}

public class InvoiceCreateViewModel
{
    [Required(ErrorMessage = "Please select a customer.")]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Invoice Date")]
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    public List<InvoiceLineFormModel> Lines { get; set; } = [];

    // Populated by controller, not round-tripped via POST
    public IEnumerable<SelectListItem> CustomerOptions { get; set; } = [];
    public string ProductsJson { get; set; } = "[]";
    public string LinesJson { get; set; } = "[]";
}

public class InvoiceEditViewModel : InvoiceCreateViewModel
{
    public int Id { get; set; }
}

public class InvoiceLineDetailsModel
{
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
}

public class InvoiceDetailsViewModel
{
    public int Id { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = "";
    public List<InvoiceLineDetailsModel> Lines { get; set; } = [];
    public decimal Total => Lines.Sum(l => l.LineTotal);
}

public class InvoiceSummaryViewModel
{
    public int Id { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = "";
    public int LineCount { get; set; }
    public decimal Total { get; set; }
}
