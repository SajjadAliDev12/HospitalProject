using FluentAssertions;
using Hospital.API.Services;

namespace Hospital.Tests;

/// <summary>
/// Covers leave-balance arithmetic used by LeavesController
/// (POST deduct / PUT delta / DELETE restore).
/// </summary>
public class LeaveBalanceCalculatorTests
{
    [Theory]
    [InlineData(10, 5, true)]
    [InlineData(5, 5, true)]   // exact balance is sufficient
    [InlineData(4, 5, false)]
    [InlineData(0, 1, false)]
    public void CanDeduct_respects_balance(int balance, int duration, bool expected)
    {
        LeaveBalanceCalculator.CanDeduct(balance, duration).Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 3, 7)]
    [InlineData(5, 5, 0)]
    public void ApplyDeduction_subtracts(int balance, int duration, int expected)
    {
        LeaveBalanceCalculator.ApplyDeduction(balance, duration).Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 3, 5, 8)]   // extension costs the difference
    [InlineData(10, 5, 3, 12)]  // shortening refunds the difference
    [InlineData(10, 4, 4, 10)]  // unchanged duration is identity
    public void ApplyDurationDelta_adjusts_by_difference(int balance, int oldDuration, int newDuration, int expected)
    {
        LeaveBalanceCalculator.ApplyDurationDelta(balance, oldDuration, newDuration).Should().Be(expected);
    }

    [Fact]
    public void Restore_returns_borrowed_days()
    {
        LeaveBalanceCalculator.Restore(7, 3).Should().Be(10);
    }

    [Fact]
    public void Full_leave_lifecycle_conserves_balance()
    {
        // Mirrors LeavesController: POST 4 days, PUT 4 -> 6, DELETE restores 6.
        int balance = 10;
        balance = LeaveBalanceCalculator.ApplyDeduction(balance, 4);   // 6
        balance = LeaveBalanceCalculator.ApplyDurationDelta(balance, 4, 6); // 4
        balance = LeaveBalanceCalculator.Restore(balance, 6);         // 10
        balance.Should().Be(10);
    }
}
