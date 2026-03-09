# Invoice Controller Implementation

## Files Created

### 1. `Library.MVC/Models/InvoiceViewModels.cs`
All ViewModels for the invoice feature.

| Class | Purpose |
|---|---|
| `InvoiceLineFormModel` | Bound per-line form data: `ProductId`, `Quantity`, `UnitPrice` with validation |
| `InvoiceCreateViewModel` | Create form: customer dropdown, invoice date, list of lines, `ProductsJson`, `LinesJson` |
| `InvoiceEditViewModel` | Extends `InvoiceCreateViewModel` with `Id` |
| `InvoiceLineDetailsModel` | Read-only line: product name, qty, unit price, computed `LineTotal` |
| `InvoiceDetailsViewModel` | Read-only invoice: customer, date, lines, computed `Total` |
| `InvoiceSummaryViewModel` | Lightweight row for the index list |

### 2. `Library.MVC/Controllers/InvoicesController.cs`
Full CRUD controller using EF Core via `ApplicationDbContext`.

| Action | Method | Description |
|---|---|---|
| `Index` | GET | Lists all invoices with customer name and computed total (EF projection) |
| `Details(id)` | GET | Eager-loads Customer → Lines → Product |
| `Create` | GET | Populates customer dropdown and products JSON for JS |
| `Create(vm)` | POST | Validates ≥1 line, saves `Invoice` + `InvoiceLine` records |
| `Edit(id)` | GET | Loads existing invoice lines, serialises to `LinesJson` for JS |
| `Edit(id, vm)` | POST | Removes all old lines and recreates from posted data |
| `Delete(id)` | GET | Confirmation page showing full invoice detail |
| `DeleteConfirmed(id)` | POST | Removes the invoice (cascade deletes lines via EF config) |

**Price snapshot:** `UnitPrice` is copied from the product onto the `InvoiceLine` at creation/edit time, so historical invoices are unaffected by future product price changes.

### 3. `Library.MVC/Views/Invoices/Index.cshtml`
Table listing all invoices with date, customer, line count, total, and View/Edit/Delete buttons.

### 4. `Library.MVC/Views/Invoices/Create.cshtml`
Form with:
- Invoice date picker
- Customer dropdown (populated server-side)
- Dynamic invoice lines table (JavaScript-driven)
- Add Line / Remove buttons
- Live line total and grand total calculation

### 5. `Library.MVC/Views/Invoices/Edit.cshtml`
Same structure as Create. Existing lines are serialised to `LinesJson` by the controller and rebuilt in the browser on load.

### 6. `Library.MVC/Views/Invoices/Details.cshtml`
Read-only view showing customer, date, all lines, and grand total.

### 7. `Library.MVC/Views/Invoices/Delete.cshtml`
Confirmation page showing full invoice summary before deletion.

### 8. `Library.MVC/Views/Shared/_Layout.cshtml` (modified)
Added **Invoices** nav link to the top navigation bar.

---

## How the Dynamic Lines Table Works

Products are embedded as JSON in the page by the controller (`ProductsJson`). No AJAX calls are needed.

```
Controller → Model.ProductsJson → <script> const products = [...] </script>
Controller → Model.LinesJson   → <script> const existingLines = [...] </script>
```

On page load, JavaScript builds `<tr>` rows from `existingLines` (empty array for Create, populated for Edit/failed POST).

### Interactions
| User action | JavaScript response |
|---|---|
| Select a product | Auto-fills Unit Price from embedded products data; updates line total |
| Change Qty or Unit Price | Recalculates line total and grand total live |
| Click "+ Add Line" | Appends a new empty row; increments index counter |
| Click "Remove" | Removes the row; re-indexes all `name` attributes sequentially |
| Submit with 0 lines | Prevented client-side; server also validates |

### Model Binding
Rows use sequential `name` attributes (`Lines[0].ProductId`, `Lines[1].ProductId`, …). The `reindex()` function updates these after any row is removed so ASP.NET Core's model binder always receives a contiguous sequence.

### Failed POST Recovery
If server-side validation fails, the controller re-serialises `vm.Lines` back into `LinesJson` before returning the view, so JavaScript can rebuild the table with the user's previously entered data intact.

---

## Entity Relationships Used

```
Customer  1──* Invoice  1──* InvoiceLine *──1 Product
```

- `DeleteBehavior.Restrict` on Invoice → Customer (safe delete)
- Cascade delete on Invoice → InvoiceLine (lines removed with invoice)
- `UnitPrice` stored as `decimal(10,2)` on both `Product` and `InvoiceLine`
