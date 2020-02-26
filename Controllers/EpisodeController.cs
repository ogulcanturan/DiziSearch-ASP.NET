using DiziSearch.Data;
using DiziSearch.Extensions;
using DiziSearch.Models;
using DiziSearch.Models.ViewModels;
using DiziSearch.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace DiziSearch.Controllers
{
    [Authorize]
    public class EpisodeController : Controller
    {
        #region DependencyInjection
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public EpisodeViewModel epVM { get; set; }

        public EpisodeController(ApplicationDbContext db)
        {
            _db = db;

            epVM = new EpisodeViewModel()
            {
                Diziler = _db.Diziler.ToList(), //Tüm db'deki dizileri Al
                Episode = new Models.Episode()
            };
        }
        #endregion

        public async Task<IActionResult> Index(int page=1)
        {
            EpisodeListViewModel epLVM = new EpisodeListViewModel
            {
                Episodes = new List<Episode>(),

            };

            //Sondan ilke doğru şekilde listeleme yapıldı
            epLVM.Episodes = await _db.Episodes.Include(m => m.Dizi).OrderByDescending(m => m.Id).Skip((page-1)*7).Take(7).ToListAsync();

            var count = _db.Episodes.Count(); // Toplam dizi sayısı


            epLVM.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = 7,
                TotalItems = count
            };

            return View(epLVM);
        }

        public async Task<IActionResult> WaitedList()
        {
            return View(await _db.Episodes.Include(m => m.Dizi).Where(m => m.Approved == false).ToListAsync());
        }

        #region Create
        public IActionResult Create()
        {
            return View(epVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePOST()
        {
            if (ModelState.IsValid)
            {
                epVM.Episode.AddedDate = DateTime.Now;
                epVM.Episode.Name = epVM.Episode.Season + ". Sezon " + epVM.Episode.Ep + ". Bölüm";
                epVM.Episode.Alias = Url.AliasUrl(epVM.Episode.Name + " izle");
                if (epVM.Episode.Link1 == null)
                {
                    epVM.Episode.Link1 = "-";
                }
                epVM.Episode.CurrentLink = epVM.Episode.Link1;
                epVM.Episode.Subtitle = @"images/FlagImages/" + epVM.Episode.Subtitle + ".png";
                _db.Episodes.Add(epVM.Episode);
                await _db.SaveChangesAsync();
                var episodeFromDb = _db.Episodes.Find(epVM.Episode.Id);
                #region LinklerinDurumu
                if (episodeFromDb.Link1 != null)
                {
                    if (episodeFromDb.Link1.IndexOf('-') == -1)
                    {
                        episodeFromDb.Link1 = episodeFromDb.Link1.Insert(0, "-"); //Eğer '-' çizgiyi bulamadıysa başına - koy.
                    }
                }
                if (episodeFromDb.Link2 != null)
                {
                    if (episodeFromDb.Link2.IndexOf('-') == -1)
                    {
                        episodeFromDb.Link2 = episodeFromDb.Link2.Insert(0, "-"); //Eğer '-' çizgiyi bulamadıysa başına - koy.
                    }
                }
                if (episodeFromDb.Link3 != null)
                {
                    if (episodeFromDb.Link3.IndexOf('-') == -1)
                    {
                        episodeFromDb.Link3 = episodeFromDb.Link3.Insert(0, "-"); //Eğer '-' çizgiyi bulamadıysa başına - koy.
                    }
                }
                if (episodeFromDb.Link4 != null)
                {
                    if (episodeFromDb.Link4.IndexOf('-') == -1)
                    {
                        episodeFromDb.Link4 = episodeFromDb.Link4.Insert(0, "-"); //Eğer '-' çizgiyi bulamadıysa başına - koy.
                    }
                }
                if (episodeFromDb.Link5 != null)
                {
                    if (episodeFromDb.Link5.IndexOf('-') == -1)
                    {
                        episodeFromDb.Link5 = episodeFromDb.Link5.Insert(0, "-"); //Eğer '-' çizgiyi bulamadıysa başına - koy.
                    }
                }
                #endregion
                #region AdminIslemler
                //Şuanki kullanıcıyı bulma
                ClaimsPrincipal currentUser = this.User;
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // This is ID
                //UploadedBy kısmında kim eklemişse onu atama işlemini gerçekleştir
                epVM.Episode.UploadedBy = currentUser.Identity.Name; // this is name

                //Master Admin yada Normal admin değilse otomatik olarak approved false olarak ayarlasın.
                if (User.IsInRole(Constants.ModeratorUser))
                {
                    epVM.Episode.Approved = false;
                }
                //Eğer onaylanmış vaziyette ise kimin onayladığını alalım
                if (epVM.Episode.Approved == true)
                {
                    epVM.Episode.ApprovedBy = currentUser.Identity.Name;
                }
                #endregion
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        #endregion

        public IActionResult Error() => View();

        #region Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //or epVM.Episode = await _db.Episodes.Include(m => m.Dizi).Where(m => m.Id == id).FirstOrDefaultAsync();
            epVM.Episode = await _db.Episodes.Include(m => m.Dizi).SingleOrDefaultAsync(m => m.Id == id);
            if (epVM.Episode == null)
            {
                return NotFound();
            }

            if (epVM.Episode.Approved == true && User.IsInRole(Constants.ModeratorUser)) // Moderator'un onaylanmış bir postu değiştirmesine izin vermiyoruz.
            {
                return RedirectToAction(nameof(Error));
            }
            ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // This is ID
            var claimName = currentUser.Identity.Name;
            if (epVM.Episode.ApprovedBy == claimName || epVM.Episode.UploadedBy == claimName || User.IsInRole(Constants.MasterAdminUser) || (User.IsInRole(Constants.NormalAdminUser) && epVM.Episode.Approved == false))
            {
                return View(epVM);
            }
            return RedirectToAction(nameof(Error));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (id != epVM.Episode.Id) //Fazlalık Check yapmıştım gelen id şuanki Id'nin eşleşmemesi durumunda
            {
                return NotFound();
            }
            var episodeFromDb = _db.Episodes.Find(id);
            if (episodeFromDb.Approved == true && User.IsInRole(Constants.ModeratorUser)) // Moderator'un onaylanmış bir postu değiştirmesine izin vermiyoruz.
            {
                ModelState.AddModelError("", "Moderatorler onaylanmış bölümleri düzeltemezler");
            }
            else if (ModelState.IsValid)
            {
                //var episodeFromDb = _db.Episodes.SingleOrDefault(m => m.Id == epVM.Episode.Id); // same result or m => m.Id == id)
                //var episodeFromDb = _db.Episodes.Where(m => m.Id == epVM.Episode.Id).FirstOrDefault();
                //var episodeFromDb = _db.Episodes.Find(id); //same result or Find(epVM.Episode.Id
                //Şuanki kullanıcıyı bulma
                ClaimsPrincipal currentUser = this.User;
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // This is ID
                var claimName = currentUser.Identity.Name;
                //Eğer Onaylanan kişi tarafından yada yüklenen kişi tarafından yada Master admin ise yada (Onaylanmamış olup şuanki kullanıcı Normal admin ise) değiştirme işlemini gerçekleştir.
                if (episodeFromDb.ApprovedBy == claimName || episodeFromDb.UploadedBy == claimName || User.IsInRole(Constants.MasterAdminUser) || (User.IsInRole(Constants.NormalAdminUser) && episodeFromDb.Approved == false))
                {
                    episodeFromDb.DiziId = epVM.Episode.DiziId;
                    episodeFromDb.Season = epVM.Episode.Season;
                    episodeFromDb.Ep = epVM.Episode.Ep;
                    episodeFromDb.Durum = epVM.Episode.Durum;
                    episodeFromDb.Date = epVM.Episode.Date;
                    episodeFromDb.EpName = epVM.Episode.EpName;
                    if (epVM.Episode.Link1 == null)
                    {
                        epVM.Episode.Link1 = "-";
                    }
                    episodeFromDb.Link1 = epVM.Episode.Link1;
                    episodeFromDb.Link2 = epVM.Episode.Link2;
                    episodeFromDb.Link3 = epVM.Episode.Link3;
                    episodeFromDb.Link4 = epVM.Episode.Link4;
                    episodeFromDb.Link5 = epVM.Episode.Link5;
                    #region LinklerinDurumu
                    if (episodeFromDb.Link1 != null)
                    {
                        if (episodeFromDb.Link1.IndexOf('-') == -1)
                        {
                            episodeFromDb.Link1 = episodeFromDb.Link1.Insert(0, "-"); //Eğer '-' çizgiyi bulamadıysa başına - koy.
                        }
                    }
                    if (episodeFromDb.Link2 != null)
                    {
                        if (episodeFromDb.Link2.IndexOf('-') == -1)
                        {
                            episodeFromDb.Link2 = episodeFromDb.Link2.Insert(0, "-"); //Eğer '-' çizgiyi bulamadıysa başına - koy.
                        }
                    }
                    if (episodeFromDb.Link3 != null)
                    {
                        if (episodeFromDb.Link3.IndexOf('-') == -1)
                        {
                            episodeFromDb.Link3 = episodeFromDb.Link3.Insert(0, "-"); //Eğer '-' çizgiyi bulamadıysa başına - koy.
                        }
                    }
                    if (episodeFromDb.Link4 != null)
                    {
                        if (episodeFromDb.Link4.IndexOf('-') == -1)
                        {
                            episodeFromDb.Link4 = episodeFromDb.Link4.Insert(0, "-"); //Eğer '-' çizgiyi bulamadıysa başına - koy.
                        }
                    }
                    if (episodeFromDb.Link5 != null)
                    {
                        if (episodeFromDb.Link5.IndexOf('-') == -1)
                        {
                            episodeFromDb.Link5 = episodeFromDb.Link5.Insert(0, "-"); //Eğer '-' çizgiyi bulamadıysa başına - koy.
                        }
                    }
                    #endregion
                    episodeFromDb.Spoiler = epVM.Episode.Spoiler;
                    episodeFromDb.Subtitle = epVM.Episode.Subtitle;
                    //KULLANICININ GIRMESINI ISTEMEDIGIMIZ DEGERLERİ KONTROLLERDA ATAMA YAPTIK
                    episodeFromDb.Name = epVM.Episode.Season + ". Sezon " + epVM.Episode.Ep + ". Bölüm";
                    episodeFromDb.Alias = Url.AliasUrl(episodeFromDb.Name + " izle");
                    episodeFromDb.CurrentLink = epVM.Episode.Link1;
                    episodeFromDb.Subtitle = @"images/FlagImages/" + epVM.Episode.Subtitle + ".png";

                    #region AdminIslemler
                    //Şuanki kullanıcıyı bulma
                    //Master Admin yada Normal admin değilse otomatik olarak approved false olarak ayarlasın.
                    if (User.IsInRole(Constants.ModeratorUser))
                    {
                        episodeFromDb.Approved = false;
                    }
                    else
                    {
                        //Eğer önceki onay durumuyla şuanki onay durumu eşleşmiyorsa yeniden biri onaylamıştır.
                        if (episodeFromDb.Approved == false || (episodeFromDb.Approved != epVM.Episode.Approved))
                        {
                            episodeFromDb.ApprovedBy = currentUser.Identity.Name; // this is name
                        }
                        episodeFromDb.Approved = epVM.Episode.Approved;
                    }

                    episodeFromDb.EditedBy = currentUser.Identity.Name;  // this is name
                    #endregion

                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Bunu yapacak yetkiniz bulunmamakta");
                }
            }
            return View(epVM); //Modelde sıkıntı var eski modeli geri yönlendir.
        }
        #endregion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAuto(int id,string returnUrl,string imgUrl)
        {
            var episodeFromDb = _db.Episodes.Find(id);
            if (User.IsInRole(Constants.ModeratorUser)) // Moderator ise izin verme
            {
                //return Redirect(returnUrl ?? "Index");
                return RedirectToAction(nameof(Error));
            }
            else if (ModelState.IsValid)
            {
                ClaimsPrincipal currentUser = this.User;
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // This is ID
                var claimName = currentUser.Identity.Name;
               
                //Eğer Onaylanan kişi tarafından yada yüklenen kişi tarafından yada Master admin ise yada (Onaylanmamış olup şuanki kullanıcı Normal admin ise) değiştirme işlemini gerçekleştir.
                if ( episodeFromDb.ApprovedBy == claimName || episodeFromDb.UploadedBy == claimName || User.IsInRole(Constants.MasterAdminUser) || (User.IsInRole(Constants.NormalAdminUser) && episodeFromDb.Approved==false))
                {
                    if(imgUrl != null)
                    {
                        episodeFromDb.Subtitle = imgUrl;
                        episodeFromDb.EditedBy = currentUser.Identity.Name;
                    }
                    else
                    {
                    episodeFromDb.ApprovedBy = currentUser.Identity.Name; // this is name
                    episodeFromDb.Approved = !(episodeFromDb.Approved);
                    }

                    await _db.SaveChangesAsync();
                    return Redirect(returnUrl ?? "Index");
                }
                else
                {
                    return RedirectToAction(nameof(Error));
                }
            }
            return Redirect(returnUrl ?? "Index");
        }

        #region Details
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            epVM.Episode = _db.Episodes.Include(m => m.Dizi).SingleOrDefault(m => m.Id == id);

            if (epVM.Episode == null)
            {
                return NotFound();
            }
            return View(epVM);
        }
        #endregion

        #region Delete
        [Authorize(Roles = Constants.MasterAdminUser)]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            epVM.Episode = _db.Episodes.Include(m => m.Dizi).SingleOrDefault(m => m.Id == id);
            if (epVM.Episode == null)
            {
                return NotFound();
            }
            return View(epVM);
        }
        [HttpPost]
        [Authorize(Roles = Constants.MasterAdminUser)]
        public async Task<IActionResult> Delete(int id,string returnUrl)
        {
            //Gerek yok fazladan check yapıyorum
            //if (id != epVM.Episode.Id) //Eğer View'da gelen Id ile Post yapıldığı vakitde ki Id aynı değilse.
            //{
            //    return NotFound();
            //}
            var episodeFromDb = await _db.Episodes.FindAsync(id);
            if (episodeFromDb == null)
            {
                return NotFound();
            }
            _db.Episodes.Remove(episodeFromDb);
            await _db.SaveChangesAsync();
            return Redirect(returnUrl ?? "~/Episode/Index");
        }
        #endregion

        #region Display
        [AllowAnonymous]
        public async Task<IActionResult> Display(string dizi, string alias, int? id)
        {
            if (alias == null)
            {
                return NotFound();
            }

            epVM.Episode = await _db.Episodes.Include(m => m.Dizi).Where(m=> m.Dizi.Approved == true).Where(m => m.Approved == true).Where(m => m.Dizi.Alias == dizi).Where(m => m.Alias == alias).FirstOrDefaultAsync();

            if (epVM.Episode == null)
            { return NotFound(); }

            #region IleriGeriMekanizmasi
            var a = await _db.Episodes.Include(m => m.Dizi).Where(m => m.Approved == true).Where(m => m.Dizi.Alias == dizi).Select(m => m.Season).ToListAsync();

            #region Son Sezon'u bulmak
            int buyukSayi = Convert.ToInt32(a[0]);
            for (int i = 0; i < a.Count(); i++)
            {
                buyukSayi = buyukSayi >= (Convert.ToInt32(a[i])) ? buyukSayi : (Convert.ToInt32(a[i]));
            }
            #endregion

            int b = buyukSayi; // Son sezon
            var c = await _db.Episodes.Include(m => m.Dizi).Where(m => m.Approved == true).Where(m => m.Dizi.Alias == dizi).Where(m => m.Season == epVM.Episode.Season).Select(m => m.Ep).ToListAsync();
            var d = c.Count(); // O anki sezondaki son bölüm sayısı
            ViewBag.dene = c.Count();

            //Eğer bölüm devam ediyor ve Son bölüm  şuanki bölüme eşit değilse
            if (epVM.Episode.Durum == "0" && d.ToString() != epVM.Episode.Ep)
            {   //Bölümü arttır
                int n = Convert.ToInt32(epVM.Episode.Ep) + 1; //şuanki bölüme 1 ekler
                ViewBag.next = Url.AliasUrl(epVM.Episode.Season + ". Sezon " + n.ToString() + ". Bölüm" + " izle");
                //Eğer ilk bölüm değil ise geri gitme option'ını sağla
                if (epVM.Episode.Ep != "1")
                {
                    ViewBag.back = Url.AliasUrl(epVM.Episode.Season + ". Sezon " + (n - 2).ToString() + ". Bölüm" + " izle");
                }
                else if (epVM.Episode.Ep == "1" && epVM.Episode.Season != "1")
                {
                    var f = await _db.Episodes.Include(m => m.Dizi).Where(m => m.Dizi.Alias == dizi).Where(m => m.Season == (Convert.ToInt32(epVM.Episode.Season) - 1).ToString()).Select(m => m.Ep).ToListAsync();
                    var g = f.Count(); // O anki sezonun bir önceki sezondaki toplam bölüm sayısı
                    //Sezonu indir en sonki bölümü patlat.
                    ViewBag.back = Url.AliasUrl((Convert.ToInt32(epVM.Episode.Season) - 1).ToString() + ". Sezon " + g.ToString() + ". Bölüm" + " izle");
                }
            }
            else if (epVM.Episode.Durum == "0" && epVM.Episode.Ep != "1")
            {
                int n = Convert.ToInt32(epVM.Episode.Ep) + 1; //şuanki bölüme 1 ekler
                ViewBag.back = Url.AliasUrl(epVM.Episode.Season + ". Sezon " + (n - 2).ToString() + ". Bölüm" + " izle");

            }   //Eğer Sezon finali ve Son sezon şuanki sezona eşit değilse = > SEZON FİNALİ VE BİR SONRAKİ SEZON VAR
            else if (epVM.Episode.Durum == "1" && b > Convert.ToInt32(epVM.Episode.Season))
            {
                //Bir Sonraki sezona geç
                int n = Convert.ToInt32(epVM.Episode.Season) + 1; //şuanki sezon'a 1 ekler
                ViewBag.next = Url.AliasUrl(n.ToString() + ". Sezon " + "1" + ". Bölüm" + " izle");
                //Bir önceki bölüme geri dön
                int k = Convert.ToInt32(epVM.Episode.Ep) + 1; //şuanki bölüme 1 ekler
                ViewBag.back = Url.AliasUrl(epVM.Episode.Season + ". Sezon " + (k - 2).ToString() + ". Bölüm" + " izle");

            }   //Eğer Sezon finali ve son sezon şuanki sezona eşit geri git sadece    
            else if (epVM.Episode.Durum == "1" && b.ToString() == epVM.Episode.Season)
            {
                int k = Convert.ToInt32(epVM.Episode.Ep) + 1; //şuanki bölüme 1 ekler
                ViewBag.back = Url.AliasUrl(epVM.Episode.Season + ". Sezon " + (k - 2).ToString() + ". Bölüm" + " izle");

            }   //Eğer Final ise
            else if (epVM.Episode.Durum == "2") //
            {
                //İlerlemene gerek yok.
                //Bir önceki bölüme geri dön
                int n = Convert.ToInt32(epVM.Episode.Ep) + 1; //şuanki bölüme 1 ekler
                if (epVM.Episode.Ep != "1") // EĞER FİNAL BÖLÜMÜ 1.BÖLÜM DEĞİLSE
                {
                    ViewBag.back = Url.AliasUrl(epVM.Episode.Season + ". Sezon " + (n - 2).ToString() + ". Bölüm" + " izle");
                }
                else if (epVM.Episode.Ep == "1" && epVM.Episode.Season != "1") // BÖLÜM 1.BÖLÜM VE S1.SEZON DEĞİL İse
                {
                    var f = await _db.Episodes.Include(m => m.Dizi).Where(m => m.Dizi.Alias == dizi).Where(m => m.Season == (Convert.ToInt32(epVM.Episode.Season) - 1).ToString()).Select(m => m.Ep).ToListAsync();
                    var g = f.Count(); // O anki sezonun bir önceki sezondaki toplam bölüm sayısı
                    //Sezonu indir en sonki bölümü patlat.
                    ViewBag.back = Url.AliasUrl((Convert.ToInt32(epVM.Episode.Season) - 1).ToString() + ". Sezon " + g.ToString() + ". Bölüm" + " izle");
                }

            }
            #endregion

            if (epVM.Episode == null)
            {
                return NotFound();
            }
            if (id == null)
            {
                epVM.Episode.CurrentLink = epVM.Episode.Link1;
            }
            else if (id == 1)
            {
                epVM.Episode.CurrentLink = epVM.Episode.Link1;
            }
            else if (id == 2)
            {

                epVM.Episode.CurrentLink = epVM.Episode.Link2;
            }
            else if (id == 3)
            {
                epVM.Episode.CurrentLink = epVM.Episode.Link3;
            }
            else if (id == 4)
            {
                epVM.Episode.CurrentLink = epVM.Episode.Link4;
            }
            else if (id == 5)
            {
                epVM.Episode.CurrentLink = epVM.Episode.Link5;
            }
            return View(epVM);
        }
        #endregion

    }
}
