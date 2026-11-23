namespace EquipmentBorrowing.Application;

public enum OpOutcome
{
    Success,
    Rejected,
    NotFound,
    Conflict,
    Failed,
    UnknownOutcome
}

public sealed record OpResult(OpOutcome Outcome, string Message)
{
    public bool IsSuccess => Outcome == OpOutcome.Success;
}

public enum BorrowingStatusFilter
{
    All,
    Active,
    Returned
}

public enum BorrowingSortField
{
    CheckoutDate,
    ExpectedReturnDate
}
