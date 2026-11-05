# Restaurant service implementation

## Schema evidence and preservation

The configured SQL Server database was inspected read-only through `sys.tables`, `sys.columns`, `sys.foreign_keys`, and view definitions before implementation. The runtime repositories use the `dbo.Rms*` schema; EF Core manages only ASP.NET Identity. There were no purchasing, supplier, dining-session, or audit tables.

Reused: `RmsSalesOrders`, `RmsSalesOrderItems`, `RmsDiningTables`, `RmsCustomers`, `RmsMenuItems`, `RmsFoodCategories`, `RmsInventoryItems`, `RmsStockTransactions`, `RmsInvoices`, `RmsPayments`, `RmsExpenses`, `RmsAppSettings`, `RmsPermissions`, `RmsRolePermissions`, and the existing `AspNet*` identity tables. Existing daily-sales and dashboard APIs remain available, with corrected invoice/credit accounting.

`MenuItemRecipes` has foreign keys to legacy `MenuItems` and `InventoryItems`, not the active `Rms*` tables. The four existing reporting views also reference legacy `Invoices`, `Orders`, `OrderItems`, `RestaurantTables`, and `InventoryItems`. Those tables, recipes, and views are preserved unchanged. They cannot safely supply active application data without an explicit legacy-ID reconciliation. Active reporting queries therefore reuse the current repository approach, rather than alter the legacy views.

`ServiceSchema.cs` adds nine missing tables, guarded by existence checks:

| Table | Purpose |
| --- | --- |
| RmsDiningSessions | Party visits and closing time |
| RmsSessionTables | Multiple tables per visit |
| RmsSeatAssignments | Customer/table/seat associations; shared seats and multiple seats per customer |
| RmsMenuItemRecipes | Ingredient requirements with foreign keys to the active menu/inventory |
| RmsSuppliers | Supplier identity and contact |
| RmsPurchases | Purchase reference, date, discounts, tax, and payments |
| RmsPurchaseItems | Ordered and received quantities and unit cost |
| RmsInvoiceItems | Selected order-line allocations to existing invoices |
| RmsAuditEvents | Actor, action, entity, timestamp, reason, request details, and retry key |

Additive columns provide table service status; order session/creator/approval; item preparation/customer/seat/instructions; stock movement order-item/cost/kind/actor; invoice session/subtotal/discount/tax/service/refund credit. Existing enum values are retained; invoice status 5 means fully refunded. No existing table is dropped, renamed, truncated, or recreated. No legacy rows are automatically reinterpreted or migrated.

## Connected UI and API entry points

| UI | Implemented flow |
| --- | --- |
| `/dashboard/tables` | Create/edit tables, open sessions, many-to-many seat assignments, remove assignments, move parties, combine sessions, statuses, settlement/authorized close exceptions |
| `/dashboard/orders` | Submit multi-line orders with customer/seat/instructions, manager edit/approval/rejection/cancellation, move an order between sessions, employee delivery, explicit adoption of legacy pending orders |
| `/dashboard/kitchen` | Approved item queue and Received → Preparing → Ready transitions; service employees mark Ready → Served |
| `/dashboard/menu` | Categories, menu price/availability, active ingredient recipes |
| `/dashboard/inventory` | Ingredients, units, thresholds, physical counts, reasoned adjustments, spoilage, movements, low-stock highlighting |
| `/dashboard/purchases` | Suppliers, purchase headers/lines, unreceived-line corrections, partial receipts, supplier payments, balances |
| `/dashboard/billing` | Party/customer/seat/selected-line bills, partial and multiple payments, refunds, voids, receipts/reprints |
| `/dashboard/customers` | Create/edit customers |
| `/dashboard/expenses` | Operating expenses and existing free-text categories |
| `/dashboard/settings` | Users, password resets, role assignments, permissions, restaurant/billing settings |
| `/dashboard/analytics` | Date-range/daily sales, item/category/table/employee grouping, payment method totals, recipe-cost estimates, gross/net profit, audit history |

Purchases and inventory reports are also available in their operational pages with date filters and record filtering. Bills display outstanding balances and payment state. No demo fallback is used in these pages. The dashboard and navigation link to implemented routes; the old messages placeholder redirects to orders.

The new service API is `GET /api/service/{section}` and `POST /api/service/{operation}`. Read sections and operations are explicitly whitelisted in `OperationsController` and `OperationsRepository`. Request DTOs are in `Backend.Application/Operations/OperationContracts.cs`; Swagger exposes their schema. `POST` requests require a nonempty UUID `requestKey`; retry the identical payload with that key after an ambiguous network failure. Reusing a key for a different payload or actor is rejected.

Examples of operations: `open-session`, `assign-seat`, `submit-order`, `approve-order`, `item-status`, `cancel-item`, `bill`, `pay`, `refund`, `void-bill`, `receive`, `stock-count`. Old order, invoice, payment, and stock-transaction write routes adapt to the transactional implementation; old order creation requires an open session, old invoice generation no longer accepts client tax amounts, and aggregate status changes other than approval direct callers to item-level operations. Compatibility routes accept `Idempotency-Key` for safe retries.

## Rules and accounting

- Server permission checks query current active user/role/permission mappings on each request. Employee order reads are restricted to their submissions. Kitchen-only users cannot approve, pay, or serve. Employees have no payment permission by default; owners can grant it.
- Default personas are initialized once: Owner, Manager, Employee, Kitchen Staff. Existing Admin retains administrative access. Manager excludes user, role, and restaurant-setting administration. Changes to persona grants persist across restarts.
- User creation and owner-initiated password resets use ASP.NET Identity. No previous password is required for an authorized reset. Passwords are excluded from audit bodies. New installations require an environment-provided bootstrap password; existing users are untouched.
- Service/financial commands use a SQL transaction, serializable isolation, and a database application lock. Audit/idempotency records commit in the same transaction. Approval consumes recipe ingredients once; insufficient stock rolls the entire approval back. Ingredients without recipes are not automatically inferred.
- Cancellation before preparation reverses the original ingredient quantities at their recorded costs. Cancellation after preparation reclassifies consumption as waste without restoring stock. Stock counts/adjustments append movements. Existing inventory edit APIs cannot overwrite stock or change established units.
- Bills include served lines only, with exclusive allocations to active invoices. To split quantities from the same dish between guests, submit separate order lines. Billed lines are locked against operational edits; refund and void first. Remaining unbilled lines can continue preparation and delivery. A closed session stays closed even after a later credit.
- Percent settings `TaxPercent`, `ServiceChargePercent`, and `DiscountPercent` default to zero. Rates are 0–100; discount is applied before tax/service; amounts round to two decimal places. `PaymentMethods` is a comma-separated selection of `1` cash, `2` card, `3` bank transfer, `4` wallet; all four are enabled when the setting is absent.
- Refunds are negative completed payment ledger entries plus an invoice credit. Original invoice amounts remain available; net balance subtracts both credits and net payments. Full credits have Refunded status. Voiding requires zero remaining net payments and a reason. Table settlement accepts paid/refunded bills; tables remain occupied until staff closes the session, then become Cleaning.
- Manager exceptions can close an unsettled session only with a reason. This releases the table but preserves unpaid bills, unbilled items, and audit evidence.
- Sales reports reflect current credits on invoices grouped by invoice date; they are not a cash-basis tax ledger. Ingredient cost uses recorded consumption/reversal/waste transactions in the selected period. Gross profit subtracts those costs from sales after discounts and credits; net profit also subtracts operating expenses. Recipe-cost reports are estimates using current costs. Historical orders without consumption records do not have reconstructed ingredient costs.

SignalR uses `/hubs/restaurant`, authenticated with the existing JWT. Events contain only change notifications. Clients reload authorized data after changes/reconnection, and poll every 30 seconds if the connection is unavailable. The schema has no restaurant/branch identity: one configured database is one restaurant, with no invented multi-branch isolation.

## Run and apply the additive schema

Use .NET 10, SQL Server, Python 3 for the integration harness, and Node 22.13+ for the frontend.

Set secrets through environment variables or existing development configuration outside committed source. `ConnectionStrings__DefaultConnection` overrides the connection string. `Jwt__Key` overrides the existing JWT signing key. For a new database only, set `Bootstrap__AdminPassword` to the initial admin password.

```powershell
dotnet restore backend/Backend.sln
dotnet build backend/Backend.sln --no-restore
dotnet run --project backend/src/Backend.Api
```

The repository's existing startup initializer applies its guarded base script, then the transactional additive `ServiceSchema.Sql`, then seeds permissions. No EF migration command is required. The SQL login needs the same schema-alteration rights as the existing initializer. The supplied restaurant database was inspected but has not been modified by the verification runs; additions apply when you run the updated API against it.

```powershell
cd frontend
npm ci
npm run dev
```

Use the existing `NEXT_PUBLIC_API_BASE_URL`, default `http://localhost:5179/api`. WebSockets must be forwarded to `/hubs/restaurant` when using a proxy. Build with `npm run build`; type-check with `npx tsc --noEmit`; lint with `npm run lint`.

## Verification and limits

`powershell -NoProfile -ExecutionPolicy Bypass -File backend/tests/verify-service.ps1` creates a uniquely named **isolated** database on the configured SQL Server, runs the built API at port 5187, and exercises the HTTP API. The harness never writes fixtures into the configured restaurant database. It leaves its named database for inspection and stops its API process. `verification.log` is gitignored. A SQL login with database creation rights is needed for this harness, not for normal service operation.

Coverage includes shared/multiple seats, approval rollback, concurrent retry safety, cancellation reversal/waste, partial and duplicate payments, paid-order protection, credits/voids, tax/discount/service calculations, configured payment methods, partial receipts, authorization, immediate revocation, password resets, read endpoints, and an actual SignalR WebSocket notification.

Known limits from the available schema: no multi-branch tenant model, no legacy-to-active ID reconciliation, no historical ingredient-cost reconstruction, and no structured priced modifier system. Existing recipes use one stock unit per ingredient; automatic unit conversion is not inferred. Only legacy pending/unbilled orders can be explicitly adopted into the new service lifecycle; previously prepared legacy orders require operational reconciliation because their preparation/consumption history is absent. This implementation does not claim that reconciliation is complete. Supplier payment history is represented by audit events and the purchase balance, not an accounts-payable ledger. Browser interaction/printing was not automated; TypeScript, lint, production build, backend build, HTTP integration, and SignalR checks provide the recorded validation.

The existing NuGet dependency graph emits vulnerability warnings for `Microsoft.OpenApi` and transitive `System.Security.Cryptography.Pkcs`; dependency remediation remains separate from these restaurant workflow changes.
