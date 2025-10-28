namespace ShiftPlanner.Helpers
{
    public static class ShiftTimeConfig
    {
        // Opening & closing times per day
        public static readonly Dictionary<string, (TimeSpan Open, TimeSpan Close)> OpeningHours =
            new()
            {
                ["Sunday"] = (TimeSpan.Parse("10:00"), TimeSpan.Parse("17:00")),
                ["Monday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("17:30")),
                ["Tuesday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("17:30")),
                ["Wednesday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("17:30")),
                ["Thursday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("21:00")),
                ["Friday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("21:00")),
                ["Saturday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("17:00"))
            };

        // Optional: For easy list use in views/templates
        public static readonly List<(string Day, string From, string To, double Hours)> Template =
            OpeningHours.Select(d =>
            {
                var hours = (d.Value.Close - d.Value.Open).TotalHours;
                return (d.Key, d.Value.Open.ToString(@"hh\:mm"), d.Value.Close.ToString(@"hh\:mm"), hours);
            }).ToList();
    }
}
