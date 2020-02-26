using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DiziSearch.Models
{
    public class Episode
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:M}", ApplyFormatInEditMode = true)]
        [Display(Name="Bölüm Tarihi")]
        public DateTime Date { get; set; }

        [BindNever]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:M}", ApplyFormatInEditMode = false)]
        public DateTime AddedDate { get; set; }

        [Required]
        [Display(Name="Sezon")]
        public string Season { get; set; }

        [Required]
        [Display(Name="Bölüm")]
        public string Ep { get; set; }

        [Display(Name="Final")]//ADDED
        public string Durum { get; set; }
        public enum EDurum { Devam=0,SezonFinali=1,Final=2}
        
        [BindNever]//Controller'de değerleri atanacak
        public string Name { get; set; }                

        [BindNever]//Controller'de değerleri atanacak
        public string Alias { get; set; }

        public string Spoiler { get; set; }

        [Display(Name="Altyazı")]
        public string Subtitle { get; set; }
        public enum ESubtitle { NA=0,TR=1,EN=2}

        public bool Approved { get; set; }

        [Display(Name="Bölüm Adı")]
        public string EpName { get; set; }

        public string Link1 { get; set; }
        public string Link2 { get; set; }
        public string Link3 { get; set; }
        public string Link4 { get; set; }
        public string Link5 { get; set; }

        [BindNever]//Gelen id numarasına göre ilgili Link atanacak
        public string CurrentLink { get; set; }

        [BindNever]
        public string UploadedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string EditedBy { get; set; }

        [Display(Name="Dizi")]
        public int DiziId { get; set; }

        [ForeignKey("DiziId")]
        public virtual Dizi Dizi { get; set; }

    }
}
