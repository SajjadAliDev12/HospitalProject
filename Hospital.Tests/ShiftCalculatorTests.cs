using FluentAssertions;
using Hospital.API.Services;

namespace Hospital.Tests;

/// <summary>
/// Covers the Modulo-4 night-shift rotation (TASK-SH-01 / user spec).
/// Reference implementation: <see cref="ShiftCalculator.GetTeamId"/>.
/// </summary>
public class ShiftCalculatorTests
{
    private static readonly DateOnly Ref = new(2024, 1, 1);

    [Theory]
    [InlineData(0, 1)]   // reference day -> team 1
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 4)]
    [InlineData(4, 1)]   // cycle wraps
    [InlineData(5, 2)]
    [InlineData(7, 4)]
    [InlineData(8, 1)]
    public void Forward_dates_rotate_1_2_3_4(int offsetDays, int expectedTeam)
    {
        ShiftCalculator.GetTeamId(Ref, Ref.AddDays(offsetDays)).Should().Be(expectedTeam);
    }

    [Theory]
    [InlineData(-1, 4)]  // dates before the reference wrap backwards
    [InlineData(-2, 3)]
    [InlineData(-3, 2)]
    [InlineData(-4, 1)]
    [InlineData(-5, 4)]
    public void Backward_dates_wrap_correctly(int offsetDays, int expectedTeam)
    {
        ShiftCalculator.GetTeamId(Ref, Ref.AddDays(offsetDays)).Should().Be(expectedTeam);
    }

    [Fact]
    public void Full_year_offset_follows_modulo()
    {
        // 365 % 4 == 1 -> team 2
        ShiftCalculator.GetTeamId(Ref, Ref.AddDays(365)).Should().Be(2);
    }

    [Fact]
    public void Leap_day_boundary_is_exact()
    {
        // 2024 is a leap year: Feb28 -> Feb29 -> Mar01 = +2 days -> team 3
        var feb28 = new DateOnly(2024, 2, 28);
        ShiftCalculator.GetTeamId(feb28, new DateOnly(2024, 3, 1)).Should().Be(3);
    }

    [Fact]
    public void Large_offset_stays_in_range_1_to_4()
    {
        for (int d = -1000; d <= 1000; d++)
            ShiftCalculator.GetTeamId(Ref, Ref.AddDays(d)).Should().BeInRange(1, 4);
    }

    [Fact]
    public void Reference_date_is_independent_of_calendar_origin()
    {
        var alt = new DateOnly(2026, 9, 20);
        ShiftCalculator.GetTeamId(alt, alt).Should().Be(1);
        ShiftCalculator.GetTeamId(alt, alt.AddDays(4)).Should().Be(1);
        ShiftCalculator.GetTeamId(alt, alt.AddDays(-4)).Should().Be(1);
    }
}
