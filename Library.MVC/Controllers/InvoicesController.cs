using Library.Domain;
using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Library.MVC.Controllers;

public class InvoicesController(ApplicationDbContext context) : Controller
{
    // GET: Invoices
    public async Task<IActionResult> Index()
    {
        var invoices = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new InvoiceSummaryViewModel
            {
                Id = i.Id,
                InvoiceDate = i.InvoiceDate,
                CustomerName = i.Customer!.Name,
                LineCount = i.Lines.Count,
                Total = i.Lines.Sum(l => l.Quantity * l.UnitPrice)
            })
            .ToListAsync();

        return View(invoices);
    }

    // GET: Invoices/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var invoice = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null) return NotFound();

        var vm = new InvoiceDetailsViewModel
        {
            Id = invoice.Id,
            InvoiceDate = invoice.InvoiceDate,
            CustomerName = invoice.Customer!.Name,
            Lines = invoice.Lines.Select(l => new InvoiceLineDetailsModel
            {
                ProductName = l.Product!.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        return View(vm);
    }

    // GET: Invoices/Create
    public async Task<IActionResult> Create()
    {
        var vm = new InvoiceCreateViewModel();
        await PopulateViewModelAsync(vm);
        return View(vm);
    }

    // POST: Invoices/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceCreateViewModel vm)
    {
        if (vm.Lines.Count == 0)
            ModelState.AddModelError("Lines", "At least one invoice line is required.");

        if (!ModelState.IsValid)
        {
            await PopulateViewModelAsync(vm);
            return View(vm);
        }

        var invoice = new Invoice
        {
            CustomerId = vm.CustomerId,
            InvoiceDate = vm.InvoiceDate,
            Lines = vm.Lines.Select(l => new InvoiceLine
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    // GET: Invoices/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var invoice = await context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null) return NotFound();

        var vm = new InvoiceEditViewModel
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            InvoiceDate = invoice.InvoiceDate,
            Lines = invoice.Lines.Select(l => new InvoiceLineFormModel
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        await PopulateViewModelAsync(vm);
        return View(vm);
    }

    // POST: Invoices/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InvoiceEditViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (vm.Lines.Count == 0)
            ModelState.AddModelError("Lines", "At least one invoice line is required.");

        if (!ModelState.IsValid)
        {
            await PopulateViewModelAsync(vm);
            return View(vm);
        }

        var invoice = await context.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null) return NotFound();

        invoice.CustomerId = vm.CustomerId;
        invoice.InvoiceDate = vm.InvoiceDate;

        // Replace all lines (captures updated prices/products)
        context.InvoiceLines.RemoveRange(invoice.Lines);
        invoice.Lines = vm.Lines.Select(l => new InvoiceLine
        {
            InvoiceId = invoice.Id,
            ProductId = l.ProductId,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice
        }).ToList();

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    // GET: Invoices/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var invoice = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null) return NotFound();

        var vm = new InvoiceDetailsViewModel
        {
            Id = invoice.Id,
            InvoiceDate = invoice.InvoiceDate,
            CustomerName = invoice.Customer!.Name,
            Lines = invoice.Lines.Select(l => new InvoiceLineDetailsModel
            {
                ProductName = l.Product!.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        return View(vm);
    }

    // POST: Invoices/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice is not null)
            context.Invoices.Remove(invoice);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateViewModelAsync(InvoiceCreateViewModel vm)
    {
        var customers = await context.Customers.OrderBy(c => c.Name).ToListAsync();
        var products = await context.Products.OrderBy(p => p.Name).ToListAsync();

        vm.CustomerOptions = customers.Select(c => new SelectListItem(c.Name, c.Id.ToString()));
        vm.ProductsJson = JsonSerializer.Serialize(
            products.Select(p => new { p.Id, p.Name, p.UnitPrice }));
        vm.LinesJson = JsonSerializer.Serialize(
            vm.Lines.Select(l => new { l.ProductId, l.Quantity, l.UnitPrice }));
    }
}
