using Microsoft.EntityFrameworkCore;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfStudentRepository : IStudentRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfStudentRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Students.SingleOrDefaultAsync(student => student.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Students.AsNoTracking().OrderBy(student => student.StudentNumber)
            .ToListAsync(cancellationToken);

    public async Task SaveAsync(Student student, CancellationToken cancellationToken = default)
    {
        await _context.Students.AddAsync(student, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<OpResult> RegisterAsync(string studentNumber, string fullName, string? contact,
        CancellationToken cancellationToken = default)
    {
        studentNumber = studentNumber.Trim();
        fullName = fullName.Trim();
        contact = string.IsNullOrWhiteSpace(contact) ? null : contact.Trim();
        if (studentNumber.Length == 0 || fullName.Length == 0)
            return new(OpOutcome.Rejected, "Student number and name are required.");
        if (await _context.Students.AnyAsync(
                student => student.StudentNumberNormalized == Normalizer.Normalize(studentNumber),
                cancellationToken))
            return new(OpOutcome.Conflict, "Student number already exists.");

        var id = await ReadableIdGenerator.NextAsync(_context, "StudentIdSequence", "ST-", 5, cancellationToken);
        _context.Students.Add(new Student
        {
            Id = id,
            StudentNumber = studentNumber,
            StudentNumberNormalized = Normalizer.Normalize(studentNumber),
            FullName = fullName,
            Contact = contact,
            IsEligible = true
        });
        await _context.SaveChangesAsync(cancellationToken);
        return new(OpOutcome.Success, "Student registered.");
    }

    public async Task<OpResult> UpdateAsync(string id, string studentNumber, string fullName, string? contact,
        CancellationToken cancellationToken = default)
    {
        studentNumber = studentNumber.Trim();
        fullName = fullName.Trim();
        contact = string.IsNullOrWhiteSpace(contact) ? null : contact.Trim();
        if (studentNumber.Length == 0 || fullName.Length == 0)
            return new(OpOutcome.Rejected, "Student number and name are required.");

        var student = await _context.Students.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (student is null)
            return new(OpOutcome.NotFound, "Student not found.");
        var normalized = Normalizer.Normalize(studentNumber);
        if (await _context.Students.AnyAsync(
                item => item.Id != id && item.StudentNumberNormalized == normalized,
                cancellationToken))
            return new(OpOutcome.Conflict, "Student number already exists.");

        student.StudentNumber = studentNumber;
        student.StudentNumberNormalized = normalized;
        student.FullName = fullName;
        student.Contact = contact;
        await _context.SaveChangesAsync(cancellationToken);
        return new(OpOutcome.Success, "Student updated.");
    }

    public async Task<OpResult> SetEligibilityAsync(string id, bool isEligible,
        CancellationToken cancellationToken = default)
    {
        var student = await _context.Students.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (student is null)
            return new(OpOutcome.NotFound, "Student not found.");
        if (student.IsEligible == isEligible)
            return new(OpOutcome.Rejected, isEligible ? "Already eligible." : "Already restricted.");

        student.IsEligible = isEligible;
        await _context.SaveChangesAsync(cancellationToken);
        return new(OpOutcome.Success, isEligible ? "Student permitted." : "Student restricted.");
    }
}
