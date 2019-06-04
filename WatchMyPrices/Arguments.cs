namespace WatchMyPrices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class Arguments
    {
        private readonly Regex watchModePattern = new Regex(@"-w(atch)?", RegexOptions.IgnoreCase);
        private readonly Regex timerNamePattern = new Regex(@"-t(imer)?", RegexOptions.IgnoreCase);
        private readonly Regex timerTimePattern = new Regex(@":(?:(\d{0,2})h)?(?:(\d{0,2})m)?", RegexOptions.IgnoreCase);

        public Arguments(string[] args)
        {
            this.OriginalArguments = string.Join(" ", args);
            this.IsWatchMode = args.Where(a => this.watchModePattern.IsMatch(a)).Any();

            var timerGiven = args.Where(a => this.timerNamePattern.IsMatch(a)).FirstOrDefault();

            if (timerGiven == null)
            {
                // Set a default 4h interval, if none given
                this.TimeInterval = new TimeSpan(4, 0, 0);
                return;
            }

            var intervalMatch = this.timerTimePattern.Match(timerGiven);

            if (!intervalMatch.Success)
            {
                throw new ArgumentException(
                    string.Format("Invalid timer interval argument, {0}, given", timerGiven),
                    timerGiven);
            }

            var hours = intervalMatch.Groups[1].Success ? int.Parse(intervalMatch.Groups[1].Value) : 0;
            var minutes = intervalMatch.Groups[2].Success ? int.Parse(intervalMatch.Groups[2].Value) : 0;

            this.TimeInterval = new TimeSpan(hours, minutes, 0);
        }

        public string OriginalArguments { get; }

        public bool IsWatchMode { get; }

        public TimeSpan TimeInterval { get; }
    }
}
