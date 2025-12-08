using System;

namespace BevSpecRad.Helpers
{
    public class ConsoleProgressBar
    {
        private readonly int width;
        private readonly string left = "[";
        private readonly string right = "]";

        public ConsoleProgressBar(int width = 68)
        {
            this.width = Math.Max(10, width);
        }

        public void Report(int current, int total)
        {
            if (total <= 0)
            {
                Report(0.0);
                return;
            }
            double percent = ((double)current / (double)total) * 100.0;
            Report(percent);
        }

        public void Report(double percent) // percent: 0.0..100.0
        {
            percent = Math_Clamp(percent, 0.0, 100.0);
            int filled = (int)Math.Round((percent / 100.0) * width);
            string hashes = new string('#', filled);
            string spaces = new string(' ', width - filled);
            string pctText = $"{percent,6:0.0} %"; // formats like " 100.0%"

            // Build bar similar to: [####################..........]  42.3%
            string bar = $"{left}{hashes}{spaces}{right} {pctText}";

            Console.Write("\r" + bar);
            if (percent >= 100.0)
            {
                Console.WriteLine();
            }
        }

        private static double Math_Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
