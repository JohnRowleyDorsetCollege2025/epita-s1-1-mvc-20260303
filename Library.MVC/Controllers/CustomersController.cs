using Library.Domain;
using Library.MVC.Data;
using Library.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Controllers;

public class CustomersController(ApplicationDbContext context) : Controller
{
    // GET: Customers
    public async Task<IActionResult> Index()
    {
        var customers = await context.Customers
            .OrderBy(c => c.Name)
            .Select(c => new CustomerSummaryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                InvoiceCount = c.Invoices.Count
            })
            .ToListAsync();

        return View(customers);
    }

    // GET: Customers/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var customer = await context.Customers
            .Select(c => new CustomerSummaryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                InvoiceCount = c.Invoices.Count
            })
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null) return NotFound();
        return View(customer);
    }

    // GET: Customers/Create
    public IActionResult Create() => View(new CustomerFormViewModel());

    // POST: Customers/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        context.Customers.Add(new Customer { Name = vm.Name });
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Customers/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var customer = await context.Customers.FindAsync(id);
        if (customer is null) return NotFound();

        return View(new CustomerFormViewModel { Id = customer.Id, Name = customer.Name });
    }

    // POST: Customers/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();
        if (!ModelState.IsValid) return View(vm);

        var customer = await context.Customers.FindAsync(id);
        if (customer is null) return NotFound();

        customer.Name = vm.Name;
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Customers/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var vm = await context.Customers
            .Select(c => new CustomerDeleteViewModel
            {
                Id = c.Id,
                Name = c.Name,
                InvoiceCount = c.Invoices.Count
            })
            .FirstOrDefaultAsync(c => c.Id == id);

        if (vm is null) return NotFound();
        return View(vm);
    }

    // POST: Customers/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var invoiceCount = await context.Invoices.CountAsync(i => i.CustomerId == id);
        if (invoiceCount > 0)
        {
            var vm = new CustomerDeleteViewModel
            {
                Id = id,
                Name = (await context.Customers.FindAsync(id))?.Name ?? "",
                InvoiceCount = invoiceCount
            };
            ModelState.AddModelError(string.Empty,
                "This customer has invoices and cannot be deleted. Remove all invoices first.");
            return View(vm);
        }

        var customer = await context.Customers.FindAsync(id);
        if (customer is not null)
            context.Customers.Remove(customer);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Customers/Invoices/5  — lists all invoices for a customer
    public async Task<IActionResult> Invoices(int? id)
    {
        if (id is null) return NotFound();

        var customer = await context.Customers
            .Include(c => c.Invoices)
                .ThenInclude(i => i.Lines)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null) return NotFound();

        var vm = new CustomerInvoicesViewModel
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            Invoices = customer.Invoices
                .OrderByDescending(i => i.InvoiceDate)
                .Select(i => new CustomerInvoiceRowViewModel
                {
                    InvoiceId = i.Id,
                    InvoiceDate = i.InvoiceDate,
                    LineCount = i.Lines.Count,
                    Total = i.Lines.Sum(l => l.Quantity * l.UnitPrice)
                })
                .ToList()
        };

        return View(vm);
    }

    // GET: Customers/InvoiceDetail/5  — detail of one invoice, with customer context
    public async Task<IActionResult> InvoiceDetail(int? id)
    {
        if (id is null) return NotFound();

        var invoice = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null) return NotFound();

        var vm = new CustomerInvoiceDetailViewModel
        {
            CustomerId = invoice.Customer!.Id,
            CustomerName = invoice.Customer.Name,
            InvoiceId = invoice.Id,
            InvoiceDate = invoice.InvoiceDate,
            Lines = invoice.Lines.Select(l => new InvoiceLineDetailsModel
            {
                ProductName = l.Product!.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        return View(vm);
    }
}
