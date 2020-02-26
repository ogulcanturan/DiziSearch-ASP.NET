using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiziSearch.Models.ViewModels
{
    public class DiziListViewModel
    {
        public List<Dizi> Diziler { get; set; }
        public PagingInfo PagingInfo { get; set; }
       
    }
}
