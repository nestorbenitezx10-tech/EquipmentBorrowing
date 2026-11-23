using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Desktop.Services;

public sealed class ApiUnavailableException(string message, Exception? inner = null) : Exception(message, inner);
public sealed class ApiServerException(string message) : Exception(message);

public static class ApiClientFactory
{
    public static HttpClient Create()
    {
        var address = Environment.GetEnvironmentVariable("EQUIPMENTBORROWING_API_URL") ?? "http://localhost:5090/";
        return new HttpClient { BaseAddress = new Uri(address, UriKind.Absolute), Timeout = TimeSpan.FromSeconds(15) };
    }
}

internal static class ApiHttp
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    internal static Equipment ToEquipment(EquipmentContract x) => new()
    {
        Id = x.Id,
        Identifier = x.EquipmentCode,
        IdentifierNormalized = x.EquipmentCode.ToUpperInvariant(),
        Name = x.Name,
        Description = x.Description,
        IsAvailable = x.IsAvailable
    };

    internal static Borrowing ToBorrowing(BorrowingContract x) => new()
    {
        Id = x.Id,
        StudentId = x.StudentId,
        EquipmentId = x.EquipmentId,
        CheckoutDate = x.BorrowedDate,
        ExpectedReturnDate = x.ExpectedReturnDate,
        ActualReturnDate = x.ReturnedDate,
        Status = x.Status == "Returned" ? BorrowingStatus.Returned : BorrowingStatus.Active,
        Student = new Student { Id = x.StudentId, StudentNumber = x.StudentNumber ?? string.Empty, FullName = x.StudentName ?? string.Empty },
        Equipment = new Equipment { Id = x.EquipmentId, Identifier = x.EquipmentCode ?? string.Empty, Name = x.EquipmentName ?? string.Empty }
    };

    internal static async Task<T> GetAsync<T>(HttpClient http, string path, CancellationToken ct)
    {
        HttpResponseMessage response;
        try { response = await http.GetAsync(path, ct); }
        catch (HttpRequestException ex) { throw new ApiUnavailableException("The API could not be reached. Check that it is running and the address is correct.", ex); }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested) { throw new ApiUnavailableException("The API did not respond in time.", ex); }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested) { throw new ApiUnavailableException("The API did not respond in time.", ex); }
        using (response)
        {
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                throw new ApiServerException("The API is running but reported a server or database error.");
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(await ErrorMessage(response, ct));
            return (await response.Content.ReadFromJsonAsync<T>(Json, ct))!;
        }
    }

    internal static async Task<T?> GetOptionalAsync<T>(HttpClient http, string path, CancellationToken ct)
    {
        HttpResponseMessage response;
        try { response = await http.GetAsync(path, ct); }
        catch (HttpRequestException ex) { throw new ApiUnavailableException("The API could not be reached. Check that it is running and the address is correct.", ex); }
        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound) return default;
            if (response.StatusCode == HttpStatusCode.InternalServerError) throw new ApiServerException("The API is running but reported a server or database error.");
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException(await ErrorMessage(response, ct));
            return await response.Content.ReadFromJsonAsync<T>(Json, ct);
        }
    }

    internal static async Task<OpResult> MutateAsync(HttpClient http, HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = new StringContent(JsonSerializer.Serialize(body, Json), Encoding.UTF8, "application/json");
        HttpResponseMessage response;
        try { response = await http.SendAsync(request, ct); }
        catch (HttpRequestException) { return new(OpOutcome.UnknownOutcome, "No HTTP response was received. The save outcome is unknown; refresh before retrying."); }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested) { return new(OpOutcome.UnknownOutcome, "The API timed out. The save outcome is unknown; refresh before retrying."); }
        using (response)
        {
            if (response.IsSuccessStatusCode) return new(OpOutcome.Success, "Saved successfully.");
            var apiError = await ReadError(response, ct);
            var message = apiError?.Message ?? "The API rejected the request.";
            return response.StatusCode switch
            {
                HttpStatusCode.NotFound => new(OpOutcome.NotFound, message),
                HttpStatusCode.Conflict => new(OpOutcome.Conflict, message),
                HttpStatusCode.InternalServerError when apiError?.Code == "unknown_outcome" => new(OpOutcome.UnknownOutcome, message),
                HttpStatusCode.InternalServerError => new(OpOutcome.Failed, "The API returned a server/database error. Refresh the data before trying again."),
                _ => new(OpOutcome.Rejected, message)
            };
        }
    }

    private static async Task<string> ErrorMessage(HttpResponseMessage response, CancellationToken ct)
    {
        var error = await ReadError(response, ct);
        if (!string.IsNullOrWhiteSpace(error?.Message)) return error.Message;
        return response.StatusCode == HttpStatusCode.NotFound ? "The requested record was not found." : "The API rejected the request.";
    }

    private static async Task<ApiError?> ReadError(HttpResponseMessage response, CancellationToken ct)
    {
        try { return await response.Content.ReadFromJsonAsync<ApiError>(Json, ct); }
        catch (JsonException) { return null; }
    }

    internal sealed record ApiError(string Code, string Message);
}

public sealed class HttpStudentRepository(HttpClient http) : IStudentRepository
{
    public async Task<Student?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        await ApiHttp.GetOptionalAsync<Student>(http, $"api/v1/students/{Uri.EscapeDataString(id)}", cancellationToken);
    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await ApiHttp.GetAsync<List<Student>>(http, "api/v1/students", cancellationToken);
    public async Task SaveAsync(Student student, CancellationToken cancellationToken = default) =>
        Ensure(await ApiHttp.MutateAsync(http, HttpMethod.Post, "api/v1/students", new { studentNumber = student.StudentNumber, fullName = student.FullName, contact = student.Contact }, cancellationToken));
    public Task<OpResult> RegisterAsync(string studentNumber, string fullName, string? contact, CancellationToken cancellationToken = default) =>
        ApiHttp.MutateAsync(http, HttpMethod.Post, "api/v1/students", new { studentNumber, fullName, contact }, cancellationToken);
    public Task<OpResult> UpdateAsync(string id, string studentNumber, string fullName, string? contact, CancellationToken cancellationToken = default) =>
        ApiHttp.MutateAsync(http, HttpMethod.Put, $"api/v1/students/{Uri.EscapeDataString(id)}", new { studentNumber, fullName, contact }, cancellationToken);
    public Task<OpResult> SetEligibilityAsync(string id, bool isEligible, CancellationToken cancellationToken = default) =>
        ApiHttp.MutateAsync(http, HttpMethod.Patch, $"api/v1/students/{Uri.EscapeDataString(id)}/eligibility", new { isEligible }, cancellationToken);
    private static void Ensure(OpResult result) { if (!result.IsSuccess) throw new InvalidOperationException(result.Message); }
}

public sealed class HttpEquipmentRepository(HttpClient http) : IEquipmentRepository
{
    public async Task<Equipment?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await ApiHttp.GetOptionalAsync<EquipmentContract>(http, $"api/v1/equipment/{Uri.EscapeDataString(id)}", cancellationToken);
        return item is null ? null : ApiHttp.ToEquipment(item);
    }
    public async Task<IReadOnlyList<Equipment>> GetAllAsync(bool availableOnly = false, CancellationToken cancellationToken = default) =>
        (await ApiHttp.GetAsync<List<EquipmentContract>>(http, availableOnly ? "api/v1/equipment?available=true" : "api/v1/equipment", cancellationToken)).Select(ApiHttp.ToEquipment).ToArray();
    public async Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default) =>
        await Ensure(ApiHttp.MutateAsync(http, HttpMethod.Put, $"api/v1/equipment/{Uri.EscapeDataString(equipment.Id)}", new { equipmentCode = equipment.Identifier, name = equipment.Name, description = equipment.Description }, cancellationToken));
    public async Task SaveAsync(Equipment equipment, CancellationToken cancellationToken = default) =>
        await Ensure(ApiHttp.MutateAsync(http, HttpMethod.Post, "api/v1/equipment", new { equipmentCode = equipment.Identifier, name = equipment.Name, description = equipment.Description }, cancellationToken));
    public Task<OpResult> RegisterAsync(string identifier, string name, string? description, CancellationToken cancellationToken = default) =>
        ApiHttp.MutateAsync(http, HttpMethod.Post, "api/v1/equipment", new { equipmentCode = identifier, name, description }, cancellationToken);
    public Task<OpResult> UpdateAsync(string id, string identifier, string name, string? description, CancellationToken cancellationToken = default) =>
        ApiHttp.MutateAsync(http, HttpMethod.Put, $"api/v1/equipment/{Uri.EscapeDataString(id)}", new { equipmentCode = identifier, name, description }, cancellationToken);
    private static async Task Ensure(Task<OpResult> task) { var result = await task; if (!result.IsSuccess) throw new InvalidOperationException(result.Message); }
}

public sealed class HttpBorrowingRepository(HttpClient http) : IBorrowingRepository
{
    public async Task<Borrowing?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = await ApiHttp.GetOptionalAsync<BorrowingContract>(http, $"api/v1/borrowings/{Uri.EscapeDataString(id)}", cancellationToken);
        return item is null ? null : ApiHttp.ToBorrowing(item);
    }
    public async Task<IEnumerable<Borrowing>> GetActiveBorrowingsByStudentIdAsync(string studentId, CancellationToken cancellationToken = default) =>
        (await GetStudentHistoryAsync(studentId, cancellationToken)).Where(x => x.Status == BorrowingStatus.Active);
    public async Task SaveAsync(Borrowing borrowing, CancellationToken cancellationToken = default) =>
        Ensure(await ApiHttp.MutateAsync(http, HttpMethod.Post, "api/v1/borrowings", new { borrowing.StudentId, borrowing.EquipmentId, expectedReturnDate = borrowing.ExpectedReturnDate }, cancellationToken));
    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        if (borrowing.Status == BorrowingStatus.Returned) { Ensure(await ApiHttp.MutateAsync(http, HttpMethod.Post, $"api/v1/borrowings/{Uri.EscapeDataString(borrowing.Id)}/return", new { }, cancellationToken)); return; }
        throw new NotSupportedException("Borrowing updates are not exposed by the API contract.");
    }
    public async Task<IEnumerable<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default) => await GetActiveAsync(cancellationToken);
    public async Task<IReadOnlyList<Borrowing>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        (await ApiHttp.GetAsync<List<BorrowingContract>>(http, "api/v1/borrowings?status=Active", cancellationToken)).Select(ApiHttp.ToBorrowing).ToArray();
    public async Task<IReadOnlyList<Borrowing>> SearchAsync(string? searchText, BorrowingStatusFilter statusFilter, BorrowingSortField sortBy, bool ascending, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(searchText)) query.Add("search=" + Uri.EscapeDataString(searchText));
        query.Add("status=" + statusFilter);
        query.Add("sort=" + (sortBy == BorrowingSortField.ExpectedReturnDate ? "expectedReturnDate" : "borrowedDate"));
        query.Add("direction=" + (ascending ? "asc" : "desc"));
        return (await ApiHttp.GetAsync<List<BorrowingContract>>(http, "api/v1/borrowings?" + string.Join("&", query), cancellationToken)).Select(ApiHttp.ToBorrowing).ToArray();
    }
    public async Task<IReadOnlyList<Borrowing>> GetStudentHistoryAsync(string studentId, CancellationToken cancellationToken = default) =>
        (await ApiHttp.GetAsync<List<BorrowingContract>>(http, $"api/v1/students/{Uri.EscapeDataString(studentId)}/borrowings", cancellationToken)).Select(ApiHttp.ToBorrowing).ToArray();
    public async Task<IReadOnlyList<Borrowing>> GetEquipmentHistoryAsync(string equipmentId, CancellationToken cancellationToken = default) =>
        (await ApiHttp.GetAsync<List<BorrowingContract>>(http, $"api/v1/equipment/{Uri.EscapeDataString(equipmentId)}/borrowings", cancellationToken)).Select(ApiHttp.ToBorrowing).ToArray();
    public async Task<OpResult> CheckoutAsync(string studentId, IReadOnlyCollection<string> equipmentIds, DateOnly checkoutDate, DateOnly expectedReturnDate, CancellationToken cancellationToken = default)
    {
        if (equipmentIds.Count == 1)
            return await ApiHttp.MutateAsync(http, HttpMethod.Post, "api/v1/borrowings", new { studentId, equipmentId = equipmentIds.Single(), expectedReturnDate }, cancellationToken);
        return await ApiHttp.MutateAsync(http, HttpMethod.Post, "api/v1/checkout-requests", new { studentId, equipmentIds, expectedReturnDate }, cancellationToken);
    }
    public Task<OpResult> ReturnAsync(string borrowingId, DateOnly returnDate, CancellationToken cancellationToken = default) =>
        ApiHttp.MutateAsync(http, HttpMethod.Post, $"api/v1/borrowings/{Uri.EscapeDataString(borrowingId)}/return", new { }, cancellationToken);
    private static void Ensure(OpResult result) { if (!result.IsSuccess) throw new InvalidOperationException(result.Message); }
}

internal sealed record EquipmentContract(string Id, string EquipmentCode, string Name, string? Description, bool IsAvailable);
internal sealed record BorrowingContract(string Id, string StudentId, string EquipmentId, DateOnly BorrowedDate,
    DateOnly ExpectedReturnDate, DateOnly? ReturnedDate, string Status, string? StudentNumber,
    string? StudentName, string? EquipmentCode, string? EquipmentName);
