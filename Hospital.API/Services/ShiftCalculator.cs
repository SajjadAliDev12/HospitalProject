namespace Hospital.API.Services
{
    /// <summary>
    /// Pure night-shift rotation math (Modulo-4 over <see cref="DateOnly.DayNumber"/>).
    /// Extracted from <see cref="ShiftService"/> so it can be unit-tested without EF/DB.
    /// Rotation: team 1 on the reference date, then 2, 3, 4, 1, ... Negative diffs
    /// (target before reference) wrap correctly via double-modulo.
    /// </summary>
    public static class ShiftCalculator
    {
        public static int GetTeamId(DateOnly referenceDate, DateOnly targetDate)
        {
            int daysDifference = targetDate.DayNumber - referenceDate.DayNumber;
            int teamIndex = ((daysDifference % 4) + 4) % 4;
            return teamIndex + 1;
        }
    }
}
