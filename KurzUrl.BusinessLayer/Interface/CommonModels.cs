namespace KurzUrl.BusinessLayer.Interface
{
    public class PlanLimitReached
    {
        public bool IsPlanLimitReached { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string PlanType { get; set; } = string.Empty;
        public int PlanLimit { get; set; }
        public int CurrentCount { get; set; }
        public string? UpgradeTo { get; set; }
    }
}

