using System.Collections.Generic;

namespace BlockM3.AEternity.SDK.Models
{
    public class Event
    {
        public string Address { get; set; }

        public string Name { get; set; }

        public List<object> Parameters { get; set; }
    }
}