using System.Text.Json;
using System.Text.Json.Serialization;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("CampusDb")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__CampusDb");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Configure ConnectionStrings:CampusDb before starting the API.");

builder.Services.AddDbContext<EquipmentBorrowingDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IStudentRepository, EfStudentRepository>();
builder.Services.AddScoped<IEquipmentRepository, EfEquipmentRepository>();
builder.Services.AddScoped<IBorrowingRepository, EfBorrowingRepository>();
builder.Services.AddScoped<StudentManagementService>();
builder.Services.AddScoped<EquipmentManagementService>();
builder.Services.AddScoped<BorrowEquipmentService>();
builder.Services.AddScoped<ReturnEquipmentService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
});

var app = builder.Build();
app.Use(async (context, next) =>
{
    try { await next(); }
    catch (Exception exception)
    {
        app.Logger.LogError(exception, "Unhandled API request failure");
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ApiError("server_error", "The request could not be completed."));
    }
});

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EquipmentBorrowingDbContext>();
    var migrationSql = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Migrations", "001_initial_postgresql.sql"));
    try { await db.Database.ExecuteSqlRawAsync(migrationSql); }
    catch (Exception exception) { app.Logger.LogWarning(exception, "Database setup was not available at API startup; database-backed requests will return a safe error."); }
}

app.MapGet("/openapi/v1.json", async (CancellationToken ct) =>
    Results.Content(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "openapi.json"), ct), "application/json"));
var apiRouteMethods = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
{
    ["/api/v1/equipment"] = ["GET", "POST"],
    ["/api/v1/equipment/{id}"] = ["GET", "PUT"],
    ["/api/v1/equipment/{id}/borrowings"] = ["GET"],
    ["/api/v1/students"] = ["GET", "POST"],
    ["/api/v1/students/{id}"] = ["GET", "PUT"],
    ["/api/v1/students/{id}/eligibility"] = ["PATCH"],
    ["/api/v1/students/{id}/borrowings"] = ["GET"],
    ["/api/v1/borrowings"] = ["GET", "POST"],
    ["/api/v1/borrowings/{id}"] = ["GET"],
    ["/api/v1/borrowings/{id}/return"] = ["POST"],
    ["/api/v1/checkout-requests"] = ["POST"]
};
app.MapFallback((HttpContext context) =>
{
    var requestPath = context.Request.Path.Value ?? string.Empty;
    var route = apiRouteMethods.FirstOrDefault(entry => RouteMatches(entry.Key, requestPath));
    if (route.Key is not null)
    {
        context.Response.Headers["Allow"] = string.Join(", ", route.Value);
        return Results.StatusCode(StatusCodes.Status405MethodNotAllowed);
    }

    return Results.NotFound(new ApiError("not_found", "Route not found."));
});
app.MapGet("/api/v1/equipment", async (bool? available, EquipmentManagementService service, CancellationToken ct) =>
{
    var items = await service.GetEquipmentAsync(available == true, ct);
    return Results.Ok(items.Select(ToEquipment));
});
app.MapGet("/api/v1/students", async (string? search, StudentManagementService service, CancellationToken ct) =>
{
    var students = await service.GetAllStudentsAsync(ct);
    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.Trim();
        students = students.Where(x => x.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            x.StudentNumber.Contains(term, StringComparison.OrdinalIgnoreCase)).ToArray();
    }
    return Results.Ok(students.Select(ToStudent));
});
app.MapGet("/api/v1/students/{id}", async (string id, StudentManagementService service, CancellationToken ct) =>
    (await service.GetStudentByIdAsync(id, ct)) is { } student ? Results.Ok(ToStudent(student)) : Results.NotFound(new ApiError("not_found", "Student not found.")));
app.MapPost("/api/v1/students", async (StudentInput input, StudentManagementService service, IStudentRepository repo, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(input.StudentNumber) || string.IsNullOrWhiteSpace(input.FullName)) return BadRequest("required", "Student number and full name are required.");
    if (input.StudentNumber.Length > 32 || input.FullName.Length > 120 || input.Contact?.Length > 254) return BadRequest("too_long", "Student fields exceed their maximum lengths.");
    var result = await service.RegisterStudentAsync(input.StudentNumber, input.FullName, input.Contact, ct);
    if (!result.IsSuccess) return FromOperation(result);
    var normalized = Normalizer.Normalize(input.StudentNumber);
    var student = (await repo.GetAllAsync(ct)).Single(x => x.StudentNumberNormalized == normalized);
    return Results.Created($"/api/v1/students/{student.Id}", ToStudent(student));
});
app.MapPut("/api/v1/students/{id}", async (string id, StudentInput input, StudentManagementService service, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(input.StudentNumber) || string.IsNullOrWhiteSpace(input.FullName)) return BadRequest("required", "Student number and full name are required.");
    if (input.StudentNumber.Length > 32 || input.FullName.Length > 120 || input.Contact?.Length > 254) return BadRequest("too_long", "Student fields exceed their maximum lengths.");
    var result = await service.UpdateStudentAsync(id, input.StudentNumber, input.FullName, input.Contact, ct);
    if (!result.IsSuccess) return FromOperation(result);
    return (await service.GetStudentByIdAsync(id, ct)) is { } student ? Results.Ok(ToStudent(student)) : Results.NotFound();
});
app.MapPatch("/api/v1/students/{id}/eligibility", async (string id, EligibilityInput input, StudentManagementService service, CancellationToken ct) =>
{
    var current = await service.GetStudentByIdAsync(id, ct);
    if (current is null) return Results.NotFound(new ApiError("not_found", "Student not found."));
    if (current.IsEligible == input.IsEligible) return Results.Ok(ToStudent(current));
    var result = await service.SetEligibilityAsync(id, input.IsEligible, ct);
    if (!result.IsSuccess) return FromOperation(result);
    return Results.Ok(ToStudent((await service.GetStudentByIdAsync(id, ct))!));
});
app.MapGet("/api/v1/equipment/{id}", async (string id, EquipmentManagementService service, CancellationToken ct) =>
    (await service.GetEquipmentByIdAsync(id, ct)) is { } equipment ? Results.Ok(ToEquipment(equipment)) : Results.NotFound(new ApiError("not_found", "Equipment not found.")));
app.MapPost("/api/v1/equipment", async (EquipmentInput input, EquipmentManagementService service, IEquipmentRepository repo, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(input.EquipmentCode) || string.IsNullOrWhiteSpace(input.Name)) return BadRequest("required", "Equipment code and name are required.");
    if (input.EquipmentCode.Length > 32 || input.Name.Length > 120 || input.Description?.Length > 500) return BadRequest("too_long", "Equipment fields exceed their maximum lengths.");
    var result = await service.RegisterEquipmentAsync(input.EquipmentCode, input.Name, input.Description, ct);
    if (!result.IsSuccess) return FromOperation(result);
    var normalized = Normalizer.Normalize(input.EquipmentCode);
    var equipment = (await repo.GetAllAsync(cancellationToken: ct)).Single(x => x.IdentifierNormalized == normalized);
    return Results.Created($"/api/v1/equipment/{equipment.Id}", ToEquipment(equipment));
});
app.MapPut("/api/v1/equipment/{id}", async (string id, EquipmentInput input, EquipmentManagementService service, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(input.EquipmentCode) || string.IsNullOrWhiteSpace(input.Name)) return BadRequest("required", "Equipment code and name are required.");
    if (input.EquipmentCode.Length > 32 || input.Name.Length > 120 || input.Description?.Length > 500) return BadRequest("too_long", "Equipment fields exceed their maximum lengths.");
    var result = await service.UpdateEquipmentAsync(id, input.EquipmentCode, input.Name, input.Description, ct);
    if (!result.IsSuccess) return FromOperation(result);
    return (await service.GetEquipmentByIdAsync(id, ct)) is { } equipment ? Results.Ok(ToEquipment(equipment)) : Results.NotFound();
});
app.MapPost("/api/v1/borrowings", async (CheckoutInput input, BorrowEquipmentService service, IBorrowingRepository repo, CancellationToken ct) =>
{
    var today = DateOnly.FromDateTime(DateTime.Today);
    if (input.ExpectedReturnDate <= today) return BadRequest("invalid_date", "Expected return date must be after today.");
    var result = await service.RequestCheckoutAsync(input.StudentId, [input.EquipmentId], today, input.ExpectedReturnDate, ct);
    if (!result.IsSuccess) return FromOperation(result);
    var borrowing = (await repo.GetActiveAsync(ct)).Single(x => x.StudentId == input.StudentId && x.EquipmentId == input.EquipmentId);
    return Results.Created($"/api/v1/borrowings/{borrowing.Id}", ToBorrowing(borrowing));
});
app.MapPost("/api/v1/checkout-requests", async (MultiCheckoutInput input, BorrowEquipmentService service, IBorrowingRepository repo, CancellationToken ct) =>
{
    var today = DateOnly.FromDateTime(DateTime.Today);
    if (input.EquipmentIds is null || input.EquipmentIds.Count < 2 || input.EquipmentIds.Distinct(StringComparer.Ordinal).Count() != input.EquipmentIds.Count)
        return BadRequest("invalid_items", "Choose at least two different equipment items.");
    if (input.ExpectedReturnDate <= today) return BadRequest("invalid_date", "Expected return date must be after today.");
    var result = await service.RequestMultiBorrowingAsync(input.StudentId, input.EquipmentIds, input.ExpectedReturnDate, ct);
    if (!result.IsSuccess) return FromOperation(result);
    var rows = await repo.GetActiveAsync(ct);
    var created = rows.Where(x => x.StudentId == input.StudentId && input.EquipmentIds.Contains(x.EquipmentId)).ToArray();
    return Results.Created("/api/v1/borrowings", new { borrowings = created.Select(x => ToBorrowing(x)) });
});
app.MapPost("/api/v1/borrowings/{id}/return", async (string id, EmptyInput _, ReturnEquipmentService service, IBorrowingRepository repo, CancellationToken ct) =>
{
    var result = await service.ReturnAsync(id, ct);
    if (!result.IsSuccess) return FromOperation(result);
    return (await repo.GetByIdAsync(id, ct)) is { } borrowing ? Results.Ok(ToBorrowing(borrowing)) : Results.NotFound(new ApiError("not_found", "Borrowing not found."));
});
app.MapGet("/api/v1/borrowings/{id}", async (string id, IBorrowingRepository repo, CancellationToken ct) =>
    (await repo.GetByIdAsync(id, ct)) is { } borrowing ? Results.Ok(ToBorrowing(borrowing)) : Results.NotFound(new ApiError("not_found", "Borrowing not found.")));
app.MapGet("/api/v1/borrowings", async (string? search, string? status, string? sort, string? direction, BorrowEquipmentService service, CancellationToken ct) =>
{
    var statusValue = status ?? "All";
    var sortValue = sort ?? "borrowedDate";
    var directionValue = direction ?? "asc";
    if (!(statusValue.Equals("All", StringComparison.OrdinalIgnoreCase) || statusValue.Equals("Active", StringComparison.OrdinalIgnoreCase) || statusValue.Equals("Returned", StringComparison.OrdinalIgnoreCase)) ||
        sortValue is not ("borrowedDate" or "expectedReturnDate") || directionValue is not ("asc" or "desc"))
        return BadRequest("invalid_query", "Use status=Active|Returned|All, sort=borrowedDate|expectedReturnDate, direction=asc|desc.");
    Enum.TryParse<BorrowingStatusFilter>(statusValue, true, out var filter);
    var sortField = sortValue == "expectedReturnDate" ? BorrowingSortField.ExpectedReturnDate : BorrowingSortField.CheckoutDate;
    var result = await service.SearchBorrowingsAsync(search, filter, sortField, directionValue == "asc", ct);
    return Results.Ok(result.Select(x => ToBorrowing(x)));
});
app.MapGet("/api/v1/students/{id}/borrowings", async (string id, StudentManagementService students, StudentManagementService service, CancellationToken ct) =>
{
    var student = await students.GetStudentByIdAsync(id, ct);
    if (student is null) return Results.NotFound(new ApiError("not_found", "Student not found."));
    return Results.Ok((await service.GetStudentHistoryAsync(id, ct)).Select(x => ToBorrowing(x, student)));
});
app.MapGet("/api/v1/equipment/{id}/borrowings", async (string id, EquipmentManagementService equipment, EquipmentManagementService service, CancellationToken ct) =>
{
    var item = await equipment.GetEquipmentByIdAsync(id, ct);
    if (item is null) return Results.NotFound(new ApiError("not_found", "Equipment not found."));
    return Results.Ok((await service.GetEquipmentHistoryAsync(id, ct)).Select(x => ToBorrowing(x, equipment: item)));
});

app.Run();

static bool RouteMatches(string template, string path)
{
    var templateSegments = template.Trim('/').Split('/');
    var pathSegments = path.Trim('/').Split('/');
    if (templateSegments.Length != pathSegments.Length) return false;

    for (var index = 0; index < templateSegments.Length; index++)
    {
        var segment = templateSegments[index];
        if (segment.StartsWith('{') && segment.EndsWith('}'))
        {
            if (pathSegments[index].Length == 0) return false;
            continue;
        }

        if (!segment.Equals(pathSegments[index], StringComparison.OrdinalIgnoreCase)) return false;
    }

    return true;
}

static IResult BadRequest(string code, string message) => Results.BadRequest(new ApiError(code, message));
static IResult FromOperation(OpResult result) => result.Outcome switch
{
    OpOutcome.NotFound => Results.NotFound(new ApiError("not_found", result.Message)),
    OpOutcome.Conflict or OpOutcome.Rejected => Results.Conflict(new ApiError("conflict", result.Message)),
    OpOutcome.UnknownOutcome => Results.Json(new ApiError("unknown_outcome", "The commit result is uncertain. Refresh records before retrying."), statusCode: StatusCodes.Status500InternalServerError),
    _ => Results.Json(new ApiError("server_error", "The request could not be completed."), statusCode: StatusCodes.Status500InternalServerError)
};
static StudentResponse ToStudent(Student x) => new(x.Id, x.StudentNumber, x.FullName, x.Contact, x.IsEligible);
static EquipmentResponse ToEquipment(Equipment x) => new(x.Id, x.Identifier, x.Name, x.Description, x.IsAvailable);
static BorrowingResponse ToBorrowing(Borrowing x, Student? student = null, Equipment? equipment = null) => new(x.Id, x.StudentId, x.EquipmentId,
    x.CheckoutDate, x.ExpectedReturnDate, x.ActualReturnDate,
    x.Status == BorrowingStatus.Returned ? "Returned" : "Active",
    student?.StudentNumber ?? x.Student?.StudentNumber, student?.FullName ?? x.Student?.FullName,
    equipment?.Identifier ?? x.Equipment?.Identifier, equipment?.Name ?? x.Equipment?.Name);

record StudentInput(string StudentNumber, string FullName, string? Contact);
record EquipmentInput(string EquipmentCode, string Name, string? Description);
record EligibilityInput(bool IsEligible);
record CheckoutInput(string StudentId, string EquipmentId, DateOnly ExpectedReturnDate);
record MultiCheckoutInput(string StudentId, IReadOnlyList<string> EquipmentIds, DateOnly ExpectedReturnDate);
record EmptyInput;
record ApiError(string Code, string Message);
record StudentResponse(string Id, string StudentNumber, string FullName, string? Contact, bool IsEligible);
record EquipmentResponse(string Id, string EquipmentCode, string Name, string? Description, bool IsAvailable);
record BorrowingResponse(string Id, string StudentId, string EquipmentId, DateOnly BorrowedDate,
    DateOnly ExpectedReturnDate, DateOnly? ReturnedDate, string Status, string? StudentNumber,
    string? StudentName, string? EquipmentCode, string? EquipmentName);
