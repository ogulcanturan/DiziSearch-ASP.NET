using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiziSearch.Models.ViewModels
{
    public class DiziViewModel
    {
        public Dizi Dizi { get; set; }
        public List<Episode> Episodes { get; set; }
        public List<string> DizininCategorisi { get; set; }
    }
}
