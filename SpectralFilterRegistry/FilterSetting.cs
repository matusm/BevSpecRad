namespace SpectralFilterRegistry
{
    public class FilterSetting
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double CutoffLow { get; set; } // in nm
        public double CutoffHigh { get; set; } // in nm

        public override string ToString()
        {
            return $"{Name}: {Description} (Cutoff: {CutoffLow}nm - {CutoffHigh}nm)";
        }
    }
}
