using System.Collections.Generic;
using System;

namespace Frontend.Models
{
    public class ResultList
    {
        public int Id { get; set; }
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
        public List<Result> Results { get; set; } = new List<Result>();
    }
}