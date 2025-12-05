using Microsoft.Extensions.Configuration;
using System;

namespace SpectralFilterRegistry
{
    public class FilterRegistry
    {
        public int Count => filterSettings.Length;

        // 1-based index (like in FilterWheel)
        public FilterSetting GetFilter(int index)
        {
            if (index < 1 || index > filterSettings.Length)
            {
                throw new IndexOutOfRangeException("Filter index is out of range.");
            }
            return filterSettings[index-1];
        }

        public FilterRegistry() : this("appsettings.json") { }
        
        public FilterRegistry(string jsonFilePath)
        {
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile(jsonFilePath).Build();
            filterSettings = config.GetSection("Filters").Get<FilterSetting[]>(); // This works with the Binder namespace
        }

        private FilterSetting[] filterSettings;
    }
}
