using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DiziSearch.Models
{
    public class Dizi
    {
        public int Id { get; set; }

        [Display(Name ="Adı")]
        public string Name { get; set; }
        public string Alias { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString ="{0:yyyy}",ApplyFormatInEditMode =true)]
        [Display(Name = "Yıl")]
        public DateTime Year { get; set; }

        [BindNever]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:M}", ApplyFormatInEditMode = true)]
        [Display(Name="Eklenme Tarihi")]
        public DateTime AddedDate { get; set; }

        [Display(Name = "Ülke")]
        public string Country { get; set; }
        [Display(Name = "Oyuncular")]
        public string Cast { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
        [Display(Name = "IMDB Puanı")]
        public decimal IMDBScore { get; set; }

        [BindNever]
        public string IMDBScoreStr { get; set; }

        public string Image { get; set; }
        [Display(Name ="Özet")]
        public string Description { get; set; }
        [Display(Name = "Tür")]
        public string Genre { get; set; }
        [Display(Name = "Onay")]
        public bool Approved { get; set; }
        [Display(Name = "Öne Çıksın")]
        public bool InFront { get; set; }

        [BindNever]
        public string UploadedBy { get; set; }
        [BindNever]
        public string ApprovedBy { get; set; }
    }
}
