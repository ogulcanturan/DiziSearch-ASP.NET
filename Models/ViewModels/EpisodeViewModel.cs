using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiziSearch.Models.ViewModels
{
    public class EpisodeViewModel
    {
        public Episode Episode { get; set; }
        public IEnumerable<Dizi> Diziler { get; set; }
    }
}

