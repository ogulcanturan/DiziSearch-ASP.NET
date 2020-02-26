using DiziSearch.Data;
using DiziSearch.Extensions;
using DiziSearch.Models;
using DiziSearch.Models.ViewModels;
using DiziSearch.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DiziSearch.Controllers
{
    [Authorize]
    public class DiziController : Controller
    {
        public int PageSize = 18;
        #region DependencyInjection
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _hostingEnvironment;
        public DiziController(ApplicationDbContext db, IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion
        [AllowAnonymous]
        public IActionResult Index(string category,int page=1)
        {
            DiziListViewModel diziLVM = new DiziListViewModel()
            {
                Diziler = new List<Dizi>()
            };
            var count =0;
            if(category == "puan")
            {
                diziLVM.Diziler = _db.Diziler
             .Where(a => a.Approved == true)
             .OrderByDescending(m => m.IMDBScore)
             .Skip((page - 1) * PageSize)
             .Take(PageSize).ToList();

                count = _db.Diziler
                   .Where(m => m.Approved == true).Count();
            }
            else { 
            diziLVM.Diziler = _db.Diziler
              .Where(a => a.Approved == true)
              .Where(m => category == null || m.Genre.Contains(category) == true || m.Name.Contains(category)==true || m.Year.Year.ToString().Contains(category) == true)
              .OrderByDescending(m => m.Id)
              .Skip((page - 1) * PageSize)
              .Take(PageSize).ToList();

                count = _db.Diziler
               .Where(m => m.Approved == true)
               .Where(m => category == null || m.Genre.Contains(category) == true || m.Name.Contains(category) == true || m.Year.Year.ToString().Contains(category) == true).Count();

            }


            diziLVM.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = PageSize,
                TotalItems = count
            };

            if(category == null)
            {
                ViewBag.CategoryIs = "";
            }
            else
            {
                ViewBag.CategoryIs = category;
            }

            return View(diziLVM);
        }

        public IActionResult WaitedList()
        {
            DiziListViewModel diziLVM = new DiziListViewModel()
            {
                Diziler = new List<Dizi>()
            };
            diziLVM.Diziler = _db.Diziler.Where(m => m.Approved == false).ToList();
            return View(diziLVM);
        }

        public IActionResult AdminIndex(int page=1)
        {
            DiziListViewModel diziLVM = new DiziListViewModel()
            {
                Diziler = new List<Dizi>()
            };

            diziLVM.Diziler = _db.Diziler.
                OrderByDescending(m=> m.Id)
                .Skip((page-1)*7)
                .Take(7).ToList();

            var count = _db.Diziler.Count();

            diziLVM.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = 7,
                TotalItems = count
            };

            return View(diziLVM);
        }
        #region Create
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dizi dizi)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }

            var IsDiziExists = _db.Diziler.Where(m => m.Name == dizi.Name).Count();
            if (IsDiziExists > 0)
            {
                ModelState.AddModelError("", "İlgili dizi zaten daha önce yaratıldı.");
                return View();
            }
            else
            {
                _db.Diziler.Add(dizi);
                await _db.SaveChangesAsync();
                //C:\Users\StrongTURK\source\repos\DiziSearch\DiziSearch\wwwroot
                string webRootPath = _hostingEnvironment.WebRootPath; //wwwroot
                var files = HttpContext.Request.Form.Files;//Yüklenen type file'ın url'sini al

                var diziFromDb = _db.Diziler.Find(dizi.Id);

                if (files.Count > 0) //Eğer dizi yüklenmişse
                {
                    //C:\Users\StrongTURK\source\repos\DiziSearch\DiziSearch\wwwroot'\'images\DiziImages
                    var uploads = Path.Combine(webRootPath, Constants.ImageFolder);
                    var extension = Path.GetExtension(files[0].FileName);//Yüklediğimiz dosyanın extension'ı al

                    //ilgili uploads'taki yere Id.extension isimli bir dosya yarat
                    using (var filestream = new FileStream(Path.Combine(uploads, dizi.Id + extension), FileMode.Create))
                    {
                        files[0].CopyTo(filestream); //yaratılan dosyaya ilgili yüklenen dosyayı kopyala.
                    }
                    //Image' ilgili url'yi ata
                    diziFromDb.Image = @"\" + Constants.ImageFolder + @"\" + dizi.Id + extension;
                }
                else //Eğer Dizi yüklenmemişse
                {
                    var uploads = Path.Combine(webRootPath, Constants.ImageFolder + @"\" + Constants.DefaultImage);
                    //default-image.png'i = kopyala ve ilgili isim ile kayıt et
                    System.IO.File.Copy(uploads, webRootPath + @"\" + Constants.ImageFolder + @"\" + dizi.Id + ".png");
                    //Image'i ilgili url'yi ata
                    diziFromDb.Image = @"\" + Constants.ImageFolder + @"\" + dizi.Id + ".png";
                }
                //AddedTime
                diziFromDb.AddedDate = DateTime.Now;
                diziFromDb.Alias = Url.AliasUrl(diziFromDb.Name); //Alias atama normal ismi uygun biçimde düzenleyerek.
                var a = diziFromDb.IMDBScore.ToString();
                diziFromDb.IMDBScoreStr = a[0] + "." + a[1];
                #region AdminIslemler
                //Şuanki kullanıcıyı bulma
                ClaimsPrincipal currentUser = this.User;
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // This is ID
                //UploadedBy kısmında kim eklemişse onu atama işlemini gerçekleştir
                diziFromDb.UploadedBy = currentUser.Identity.Name; // this is name

                //Master Admin yada Normal admin değilse otomatik olarak approved false olarak ayarlasın.
                if (User.IsInRole(Constants.ModeratorUser))
                {
                    diziFromDb.Approved = false;
                }
                //Eğer onaylanmış vaziyette ise kimin onayladığını alalım
                if(diziFromDb.Approved == true)
                {
                    diziFromDb.ApprovedBy = currentUser.Identity.Name;
                }
                #endregion
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion
        #region Display
        [AllowAnonymous]
        public async Task<IActionResult> Display(string alias)
        {
            if(alias == null)
            {
                return NotFound();
            }

            DiziViewModel diziVM = new DiziViewModel()
            {
                Dizi = new Dizi(),
                Episodes = new List<Episode>(),
                DizininCategorisi = new List<string>()
            };

            var diziFromDb = await _db.Diziler.Where(m=> m.Approved==true).Where(m => m.Alias == alias).FirstOrDefaultAsync();

            var episodesFromDb = await _db.Episodes.Include(m => m.Dizi).Where(m => m.Approved==true).Where(m => m.Dizi.Alias == alias).OrderBy(m => m.Season).ToListAsync();

            if (diziFromDb == null)
            {
                return NotFound();
            }

            string a;
            //KATEGORİ BÖLME MEKANİZMASI
            if(diziFromDb.Genre == null)
            {
                a = "Not categorized";
            }
            else
            {
                a = diziFromDb.Genre;
            }

                a = a.Replace(" ", "");
            if (a != null)
            {
                if (a.IndexOf(',') != -1)
                {
                    do
                    {
                        diziVM.DizininCategorisi.Add(a.Substring(0, a.IndexOf(',')));
                        a = a.Remove(0, a.IndexOf(',') + 1);
                        if (a.IndexOf(',') == -1)
                        {
                            diziVM.DizininCategorisi.Add(a);
                        }

                    } while (a.IndexOf(',') != -1);
                }
            }
               

          
            diziVM.Episodes = episodesFromDb;
            //Gösterilmesini istediğim verileri ekledim
            diziVM.Dizi.Name = diziFromDb.Name;
            diziVM.Dizi.Cast = diziFromDb.Cast;
            diziVM.Dizi.Year = diziFromDb.Year;
            diziVM.Dizi.Image = diziFromDb.Image;
            diziVM.Dizi.Country = diziFromDb.Country;
            diziVM.Dizi.Description = diziFromDb.Description;
            diziVM.Dizi.Genre = diziFromDb.Genre;
            diziVM.Dizi.IMDBScoreStr = diziFromDb.IMDBScoreStr;

            return View(diziVM);
        }
        #endregion

        #region Details
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var diziFromDb = _db.Diziler.Find(id);
            if (diziFromDb == null)
            {
                return NotFound();
            }
            //İlgili dizinin detaylarındaki toplam bölüm ve toplam sezon sayısı.
            var toplamblm = (_db.Episodes.Include(m => m.Dizi).Where(m => m.Dizi.Id == diziFromDb.Id).Select(m => m.Ep).ToList()).Count();
            ViewBag.toplamBolum = toplamblm;
            var b = (_db.Episodes.Include(m => m.Dizi).Where(m => m.Dizi.Id == diziFromDb.Id).Select(m => m.Season).ToList());
            if (b.Count==0)
            {
                ViewBag.toplamSezon = 0;
            }
            else
            {
            int buyukSayi = Convert.ToInt32(b[0]);
            for(int i = 0; i < toplamblm; i++)
            {
                buyukSayi = buyukSayi >= (Convert.ToInt32(b[i])) ? buyukSayi : (Convert.ToInt32(b[i]));
            }
            ViewBag.toplamSezon = buyukSayi;
            }
            //Toplam bölüm sayısı
            return View(diziFromDb);

        }
        #endregion

        public IActionResult Error() => View();

        #region Edit
        public IActionResult Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var diziFromDb = _db.Diziler.Find(id);
            if (diziFromDb == null)
            {
                return NotFound();
            }
            if (diziFromDb.Approved == true && User.IsInRole(Constants.ModeratorUser)) // Moderator'un onaylanmış bir postu değiştirmesine izin vermiyoruz.
            {
                return RedirectToAction(nameof(Error));
            }
            ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // This is ID
            var claimName = currentUser.Identity.Name;
            if (diziFromDb.ApprovedBy == claimName || diziFromDb.UploadedBy == claimName || User.IsInRole(Constants.MasterAdminUser) || (User.IsInRole(Constants.NormalAdminUser) && diziFromDb.Approved == false))
            {
                return View(diziFromDb);
            }
            return RedirectToAction(nameof(Error));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,Dizi dizi)
        {
            var diziFromDb = await _db.Diziler.Where(m => m.Id == dizi.Id).FirstOrDefaultAsync();
            dizi.Image = diziFromDb.Image;
            if (diziFromDb.Approved == true && User.IsInRole(Constants.ModeratorUser)) // Moderator'un onaylanmış bir postu değiştirmesine izin vermiyoruz.
            {
                return RedirectToAction(nameof(Index));
            }
            else if (ModelState.IsValid)
            {
                //Şuanki kullanıcıyı bulma
                ClaimsPrincipal currentUser = this.User;
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // This is ID
                var claimName = currentUser.Identity.Name;
                //Eğer Onaylanan kişi tarafından yada yüklenen kişi tarafından yada Master admin ise yada (Onaylanmamış olup şuanki kullanıcı Normal admin ise) değiştirme işlemini gerçekleştir.
                if (diziFromDb.ApprovedBy == claimName || diziFromDb.UploadedBy == claimName || User.IsInRole(Constants.MasterAdminUser) || (User.IsInRole(Constants.NormalAdminUser) && diziFromDb.Approved == false))
                {
                    //C:\Users\StrongTURK\source\repos\DiziSearch\DiziSearch\wwwroot
                    string webRootPath = _hostingEnvironment.WebRootPath; //wwwroot
                    var files = HttpContext.Request.Form.Files;//Yüklenen type file'ın url'sini al

                    // var diziFromDb = await _db.Diziler.Where(m => m.Id == dizi.Id).FirstOrDefaultAsync();

                    if (files.Count > 0 && files[0] != null) //Dosya 0'dan büyük ve ilgili image yüklenmiş ise
                    {
                        //C:\Users\StrongTURK\source\repos\DiziSearch\DiziSearch\wwwroot'\'images\DiziImages
                        var uploads = Path.Combine(webRootPath, Constants.ImageFolder);
                        var extension_new = Path.GetExtension(files[0].FileName);//Yüklediğimiz dosyanın extension'ı al
                        var extension_old = Path.GetExtension(diziFromDb.Image);

                        //Eğer ilgili Image var ise o image'i sil
                        if (System.IO.File.Exists(Path.Combine(uploads, dizi.Id + extension_old)))
                        {
                            System.IO.File.Delete(Path.Combine(uploads, dizi.Id + extension_old));
                        }

                        //ilgili uploads'taki yere Id.extension isimli bir dosya yarat
                        using (var filestream = new FileStream(Path.Combine(uploads, dizi.Id + extension_new), FileMode.Create))
                        {
                            files[0].CopyTo(filestream); //yaratılan dosyaya ilgili yüklenen dosyayı kopyala.
                        }
                        //Image' ilgili url'yi ata
                        dizi.Image = @"\" + Constants.ImageFolder + @"\" + dizi.Id + extension_new;
                    }
                    if (dizi.Image != null) //eğer image yüklenmiş ise databasedeki image bölmesine ata
                    {
                        diziFromDb.Image = dizi.Image;
                    }
                    //This changes applying if its been changing like _db.Update(); await _db.SaveChangesAsync();
                    diziFromDb.Name = dizi.Name;
                    diziFromDb.Year = dizi.Year;
                    diziFromDb.Country = dizi.Country;
                    diziFromDb.IMDBScore = dizi.IMDBScore;
                    diziFromDb.Description = dizi.Description;
                    diziFromDb.Alias = Url.AliasUrl(dizi.Name);
                    diziFromDb.Genre = dizi.Genre;
                    diziFromDb.InFront = dizi.InFront;
                    #region AdminIslemler

                    //UploadedBy kısmında kim eklemişse onu atama işlemini gerçekleştir

                    //Master Admin yada Normal admin değilse otomatik olarak approved false olarak ayarlasın.
                    if (User.IsInRole(Constants.ModeratorUser))
                    {
                        diziFromDb.Approved = false;
                    }
                    else
                    { 
                        //Eğer önceki onay durumuyla şuanki onay durumu eşleşmiyorsa yeniden biri onaylamıştır.
                        if (diziFromDb.Approved == false || (diziFromDb.Approved != dizi.Approved))
                        {
                            diziFromDb.ApprovedBy = currentUser.Identity.Name; // this is name
                        }
                        diziFromDb.Approved = dizi.Approved;
                    }
                    #endregion

                    var a = diziFromDb.IMDBScore.ToString();
                    diziFromDb.IMDBScoreStr = a[0] + "." + a[1];
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Bunu yapacak izniniz bulunmamaktadır.");
                }
            }
            return View(dizi);
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAuto(int id, Dizi dizi,string returnUrl)
        {
            var diziFromDb = await _db.Diziler.Where(m => m.Id == dizi.Id).FirstOrDefaultAsync();
            if (User.IsInRole(Constants.ModeratorUser)) // Moderator ise izin verme
            {
                //return Redirect(returnUrl ?? "Index");
                return RedirectToAction(nameof(Error));
            }
            else if (ModelState.IsValid)
            {
                //Şuanki kullanıcıyı bulma
                ClaimsPrincipal currentUser = this.User;
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // This is ID
                var claimName = currentUser.Identity.Name;
                if (diziFromDb.ApprovedBy == claimName || diziFromDb.UploadedBy == claimName || User.IsInRole(Constants.MasterAdminUser) || (User.IsInRole(Constants.NormalAdminUser) && diziFromDb.Approved == false))
                {
                    diziFromDb.ApprovedBy = currentUser.Identity.Name; // this is name
                    diziFromDb.Approved = !(diziFromDb.Approved);
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


        #region Delete
        [Authorize(Roles = Constants.MasterAdminUser)]
        public ActionResult Delete(int? id) 
        {
            if (id == null)
            {
                return NotFound();
            }
            //Return only element of a sequence
            var diziFromDb = _db.Diziler.Find(id);

            if (diziFromDb == null)
            {
                return NotFound();
            }
            return View(diziFromDb);
        }
        [HttpPost]
        [Authorize(Roles = Constants.MasterAdminUser)]
        public async Task<IActionResult> Delete(int id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            var diziFromDb = await _db.Diziler.FindAsync(id);
            if (diziFromDb == null)
            {
                return NotFound();
            }
            else
            {
                var uploads = Path.Combine(webRootPath, Constants.ImageFolder);
                var extension = Path.GetExtension(diziFromDb.Image);
                if (System.IO.File.Exists(Path.Combine(uploads, diziFromDb.Id + extension)))
                {
                    System.IO.File.Delete(Path.Combine(uploads, diziFromDb.Id + extension));
                }

                _db.Diziler.Remove(diziFromDb);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion

    }
}
