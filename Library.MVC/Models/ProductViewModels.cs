using System.ComponentModel.DataAnnotations;

namespace Library.MVC.Models;

public class ProductSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int UsageCount { get; set; }
}

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Unit price is required.")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Unit price must be greater than zero.")]
    [DataType(DataType.Currency)]
    [Display(Name = "Unit Price (£)")]
    public decimal UnitPrice { get; set; }
}

public class ProductDeleteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int UsageCount { get; set; }
    public bool CanDelete => UsageCount == 0;
}
