namespace Hospital.API.Services
{
    /// <summary>
    /// Pure leave-balance arithmetic used by <c>LeavesController</c>
    /// (POST deduct, PUT delta-adjust, DELETE restore).
    /// No DB access — safe for unit tests. All methods are behavior-identical
    /// to the inline expressions they replace.
    /// </summary>
    public static class LeaveBalanceCalculator
    {
        public static bool CanDeduct(int balance, int duration) => balance >= duration;

        public static int ApplyDeduction(int balance, int duration) => balance - duration;

        public static int ApplyDurationDelta(int balance, int oldDuration, int newDuration)
            => balance - (newDuration - oldDuration);

        public static int Restore(int balance, int duration) => balance + duration;
    }
}
