using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiziSearch.Models.ViewModels
{
    public class EpisodeListViewModel
    {
        public List<Episode> Episodes { get; set; }
        public List<Episode> SonEklenenEpisodes { get; set; }
        public List<Dizi> InFrontDiziler { get; set; }
        public List<Episode> AltyaziliEpisodes { get; set; }
        public List<Episode> AltyazisizEpisodes { get; set; } 
        public PagingInfo PagingInfo { get; set; }
    }
}
