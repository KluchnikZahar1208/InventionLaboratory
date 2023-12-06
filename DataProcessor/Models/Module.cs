using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.Models
{
    public class Module
    {
        public int Id { get; set; }
        public string? ModuleCategoryID { get; set; }
        public string? ModuleState { get; set; }
    }
}
