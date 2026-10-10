# OrderProcessorAPI

A simple *NET  Web API (.NET 8)* that improves a legacy
order-processing application. It uses SQL Server, Entity Framework ,
dependency injection, the Repository Pattern, the Strategy Pattern, and
mock payment and email services.

The example products are meat products, and prices are in *South
African rand (ZAR)*.

*Whiteboard session:* The coding standards, design patterns,
and improvements adopted in this project were part of the discussions we
had during the whiteboard session. This implementation applies those
discussions to the legacy Order Processor, with a focus on *clear
responsibilities, maintainability, security, testability, and
straightforward code*.

## 1. How to run the project

### Requirements

-   Visual Studio 2022 with NET and web development support
-   .NET 8 SDK
-   SQL Server LocalDB (or an accessible SQL Server instance)
-   Internet access for restoring NuGet packages, if they are not
    already installed

### Steps

1.  Open the solution in *Visual Studio 2022*.

2.  Set *OrderProcessorAPI* as the startup project.

3.  Check the connection string in `appsettings.json`. For LocalDB, it
    should look like:

	json
	
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LuckyBeardOrdersDb;Trusted_Connection=True;TrustServerCertificate=True;"
      }
    }

    Keep the other existing settings in `appsettings.json`.

4.  Build the solution (*Ctrl + Shift + B*). Visual Studio should
    restore the required NuGet packages.

5.  Open *Tools → NuGet Package Manager → Package Manager Console*.
    Set *Default project* to `OrderProcessorAPI` and run:

    ``` powershell
    Update-Database
    ```

    This applies the existing EF  migrations. *Do not create a new
    migration just to run the project.*

6.  Press *F5* to start the API.

7.  Open `https://localhost:7102/swagger`. or use `http://localhost:5167/swagger` 

*Expected result:* The project builds, the `LuckyBeardOrdersDb`
database contains `Products`, `Orders`, and `OrderItems`, and Swagger
displays the API endpoints. The seeded product IDs and prices are listed
below.

If Swagger does not open, confirm the project is running. use dotnet add package Swashbuckle.AspNetCore 
on package manager if not installed alreay.

### Sample products

  Product ID   Product             Price
  ------------ ------------ ------------
  1            Beef Stew      R105.00/kg
  2            Beef Wors      R105.00/kg
  3            Lamb Chops     R215.00/kg
  4            Lamb Stew      R150.00/kg
  5            Lamb Shank     R170.00/kg
  6            Lamb Ribs      R170.00/kg
  7            Pork Ribs      R100.00/kg

## 2. Architecture --- how the project works

*Design approach:* The standards and architecture used here reflect
the points discussed during our *whiteboard session*. Rather than
introducing unnecessary layers, the refactoring focuses on the practical
issues in the supplied legacy code.

The request follows this simple flow:

``` text
Client / Swagger
      |
      v
OrdersController
      |
      v
OrderProcessor (business rules)
      |            |                 |
      v            v                 v
Repositories   Payment Strategy   Email Sender
      |            |                 |
      v            v                 v
EF /SQL    Mock payment       Mock email
```

  Part                                Responsibility
  
  `OrdersController`                  Receives HTTP requests and returns
                                      responses.

  `OrderProcessor`                    Checks the order, calculates the
                                      total, coordinates payment, saves
                                      the order and requests an email.

  `IOrderRepository` /                Saves orders and retrieves order
  `OrderRepository`                   history.

  `IProductRepository` /              Reads products and prices from SQL
  `ProductRepository`                 Server.

  `OrderDbContext`                    EF  database access, change
                                      tracking and SQL transactions.

  `IPaymentStrategy` and payment      Handles different mock payment
  classes                             methods.

  `PaymentStrategyFactory`            Selects the correct payment method.

  `IEmailSender` / `MockEmailSender`  Simulates sending an order
                                      confirmation.

  `GlobalExceptionHandler`            Logs errors and returns consistent
                                      error responses.

  `Program.cs`                        Registers dependencies and
                                      configures the API.
  -----------------------------------------------------------------------

### Real-life example: legacy code versus new architecture

*Imagine a customer orders 2 kg of Beef Stew and 1 kg of Lamb Chops and
pays by credit card.*

*Old legacy approach:* One large `DoProcess()` method does everything:
builds SQL strings, looks up prices, calculates the amount, chooses a
payment method using `if/else`, calls the payment service, saves the
order, and sends an email. If the payment provider changes, or the
database query breaks, the same large method must be edited. That makes
it harder to understand and test.

*New approach:*

1.  `OrdersController` receives the order.
2.  `OrderProcessor` asks `ProductRepository` for the real prices and
    calculates the total.
3.  `PaymentStrategyFactory` selects `CreditCardPaymentStrategy`; the
    order processor does not contain separate payment `if/else` blocks.
4.  `OrderRepository` saves the order and its items using EF .
5.  `MockEmailSender` simulates the confirmation.
6.  The controller returns a clear JSON result.

*What this resolves:*

  Legacy problem                      Improvement
  
  One god class doing everything      Separate controller, service,
                                      repositories, payments and email
                                      responsibilities (*Single
                                      Responsibility Principle*).

  SQL created by joining strings      EF  LINQ queries are
                                      parameterized, reducing SQL
                                      injection risk.

  Database credentials written in     Connection settings live in
  code                                configuration; LocalDB uses Windows
                                      authentication.

  Repeated payment `if/else`          *Strategy Pattern*: add a new
  statements                          payment class and register it
                                      without rewriting order-processing
                                      logic.

  Directly creating dependencies      *Dependency Injection*: classes
                                      receive interfaces/dependencies
                                      through constructors.

  Manual connection management        EF  manages database
                                      connections and tracks changes.

  Weak error handling /               Central exception handling and
  `Console.WriteLine`                 structured `ILogger` logging.

## 3. How to test the endpoints

Open Swagger at `https://localhost:7102/swagger`, expand
the endpoint, select *Try it out*, paste the JSON (where applicable),
and click *Execute*.

The examples below assume the sample products have been seeded
successfully.

### Test A --- Mock credit-card payment

*Endpoint:* `POST /api/mock/payment`

*Request:*

``` json
{
  "paymentMethod": "CreditCard",
  "amount": 350.00
}
```

*Expected:* HTTP *200 OK*.

``` json
{
  "paymentMethod": "CreditCard",
  "amount": 350.00,
  "currency": "ZAR",
  "success": true,
  "message": "Mock payment processed successfully"
}
```

Repeat with `"PayPal"` or `"BankTransfer"`; the expected status is also
*200 OK*, with the corresponding `paymentMethod`.

### Test B --- Unsupported payment method

*Endpoint:* `POST /api/mock/payment`

``` json
{
  "paymentMethod": "Bitcoin",
  "amount": 350.00
}
```

*Expected:* HTTP *400 Bad Request*, with the message
`Unsupported payment method: Bitcoin` (the mock controller returns this
as an error string).

### Test C --- Mock email

*Endpoint:* `POST /api/mock/email`

``` json
{
  "customerEmail": "customer@example.com",
  "orderId": 1001,
  "total": 420.50
}
```

*Expected:* HTTP *200 OK*.

``` json
{
  "success": true,
  "message": "Mock email sent successfully",
  "orderId": 1001,
  "customerEmail": "customer@example.com"
}
```

### Test D --- Create a successful order

*Endpoint:* `POST /api/orders`

``` json
{
  "customerId": 101,
  "customerEmail": "customer@example.com",
  "paymentMethod": "CreditCard",
  "discount": 10,
  "items": [
    { "productId": 1, "quantity": 2 },
    { "productId": 3, "quantity": 1 }
  ]
}
```


*Expected:* HTTP *200 OK*.

``` json
{
  "orderId": 1,
  "customerId": 101,
  "total": 382.50,
  "status": "Paid",
  "message": "Order processed successfully."
}
```

The `orderId` is an example; SQL Server assigns the actual ID. The order
and its two items should be saved in the database.

### Test E --- Get customer order history (replaces `FindHistory()`)

*Endpoint:* `GET /api/orders/customer/101`

*Expected:* HTTP *200 OK*, with a JSON array containing the orders
for customer 101. After Test D, one entry should look similar to:

``` json
[
  {
    "id": 1,
    "customerId": 101,
    "total": 382.50,
    "status": "Paid",
    "paymentMethod": "CreditCard",
    "createdAt": "2026-10-10T12:00:00Z",
    "items": [
      {
        "productId": 1,
        "productName": "Beef Stew",
        "quantity": 2,
        "unitPrice": 105.00,
        "lineTotal": 210.00
      },
      {
        "productId": 3,
        "productName": "Lamb Chops",
        "quantity": 1,
        "unitPrice": 215.00,
        "lineTotal": 215.00
      }
    ]
  }
]
```

The ID and timestamp are illustrative. If multiple orders exist for
customer 101, the array contains more entries. If there are none, the
result is `[]` with *200 OK*.

### Test F --- Get one order

*Endpoint:* `GET /api/orders/{orderId}`

Replace `{orderId}` with the actual ID returned by Test D, for example
`GET /api/orders/1`.

*Expected:* HTTP *200 OK*, returning that order's details and items.

*Not found test:* `GET /api/orders/999999` (assuming that ID does not
exist).

*Expected:* HTTP *404 Not Found*, with the message
`Order not found.`

### Test G --- Invalid product ID

*Endpoint:* `POST /api/orders`

``` json
{
  "customerId": 101,
  "customerEmail": "customer@example.com",
  "paymentMethod": "CreditCard",
  "discount": 0,
  "items": [
    { "productId": 9999, "quantity": 1 }
  ]
}
```

*Expected:* HTTP *400 Bad Request*. The global exception handler
returns a Problem Details response similar to:

``` json
{
  "title": "Invalid request",
  "status": 400,
  "detail": "One or more products do not exist.",
  "instance": "/api/orders"
}
```

No order should be created.

### Test H --- Invalid quantity

*Endpoint:* `POST /api/orders`

Use Test D's JSON, but change the first item's quantity to `-1`.

*Expected:* HTTP *400 Bad Request* with validation errors for
`Items[0].Quantity`. The response is generated automatically by NET
's `[ApiController]` validation and may include `type`, `title`,
`status`, `errors`, and `traceId`. The order is not processed.

### Test I --- Invalid email

*Endpoint:* `POST /api/orders`

Use Test D's JSON, but change `customerEmail` to `"not-an-email"`.

*Expected:* HTTP *400 Bad Request* with a validation error for
`CustomerEmail`. No order should be created.

## 4. How to test GlobalExceptionHandler

The global exception handler is registered in `Program.cs`:

``` csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
```

It maps errors as follows:

  Error                    Expected status         Expected result
  
  `ArgumentException`      *400*                 `Invalid request` with
                                                   the error message.

  `KeyNotFoundException`   *404*                 `Resource not found` with
                                                   the error message.

  Unexpected exception     *500*                 `Internal server error`
                                                   with a generic message.

### Test  --- Real validation/business exception (400)

POST /api/orders

{
  "customerId": 101,
  "customerEmail": "customer@example.com",
  "paymentMethod": "CreditCard",
  "discount": 10,
  "items": [
    {
      "productId": 9999,
      "quantity": 2
    }
  ]
}

*Expected:*
 {
  "title": "Invalid request",
  "status": 400,
  "detail": "One or more products do not exist.",
  "instance": "/api/orders"
}

*Note:* The existing `GET /api/orders/999999` returns `NotFound()`
directly from the controller. It tests the *404 HTTP response*, but it
does *not* exercise the global exception handler. The temporary
endpoint above specifically tests the handler.


## 5. Summary

This refactoring changes a difficult-to-maintain, tightly coupled legacy
order processor into smaller, focused components. It demonstrates
*SOLID principles, dependency injection, Repository Pattern, Strategy
Pattern, validation, structured logging, global exception handling,
async database access, and EF  transactions*, without adding
unnecessary complexity for the assessment.
