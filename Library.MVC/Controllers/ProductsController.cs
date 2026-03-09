using Library.Domain;
using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Controllers;

public class ProductsController(ApplicationDbContext context) : Controller
{
    // GET: Products
    public async Task<IActionResult> Index()
    {
        var products = await context.Products
            .OrderBy(p => p.Name)
            .Select(p => new ProductSummaryViewModel
            {
                Id = p.Id,
                Name = p.Name,
                UnitPrice = p.UnitPrice,
                UsageCount = p.InvoiceLines.Count
            })
            .ToListAsync();

        return View(products);
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var product = await context.Products
            .Select(p => new ProductSummaryViewModel
            {
                Id = p.Id,
                Name = p.Name,
                UnitPrice = p.UnitPrice,
                UsageCount = p.InvoiceLines.Count
            })
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return NotFound();
        return View(product);
    }

    // GET: Products/Create
    public IActionResult Create() => View(new ProductFormViewModel());

    // POST: Products/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var product = new Product { Name = vm.Name, UnitPrice = vm.UnitPrice };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = product.Id });
    }

    // GET: Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var product = await context.Products.FindAsync(id);
        if (product is null) return NotFound();

        return View(new ProductFormViewModel { Id = product.Id, Name = product.Name, UnitPrice = product.UnitPrice });
    }

    // POST: Products/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();
        if (!ModelState.IsValid) return View(vm);

        var product = await context.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.Name = vm.Name;
        product.UnitPrice = vm.UnitPrice;
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Products/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var vm = await context.Products
            .Select(p => new ProductDeleteViewModel
            {
                Id = p.Id,
                Name = p.Name,
                UnitPrice = p.UnitPrice,
                UsageCount = p.InvoiceLines.Count
            })
            .FirstOrDefaultAsync(p => p.Id == id);

        if (vm is null) return NotFound();
        return View(vm);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var usageCount = await context.InvoiceLines.CountAsync(l => l.ProductId == id);
        if (usageCount > 0)
        {
            var existing = await context.Products.FindAsync(id);
            var vm = new ProductDeleteViewModel
            {
                Id = id,
                Name = existing?.Name ?? "",
                UnitPrice = existing?.UnitPrice ?? 0,
                UsageCount = usageCount
            };
            ModelState.AddModelError(string.Empty,
                "This product is used in existing invoice lines and cannot be deleted.");
            return View(vm);
        }

        var product = await context.Products.FindAsync(id);
        if (product is not null)
            context.Products.Remove(product);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
