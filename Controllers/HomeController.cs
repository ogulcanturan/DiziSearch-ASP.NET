using DiziSearch.Data;
using DiziSearch.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DiziSearch.Controllers
{
    public class HomeController : Controller
    {
        public int PageSize = 10; //Her bölmeye
        #region DependencyInjection
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }
        #endregion

        public IActionResult Index(string category,int page=1)
        {
            EpisodeListViewModel episodeLVM = new EpisodeListViewModel
            {
                Episodes = new List<Models.Episode>(),
                SonEklenenEpisodes = new List<Models.Episode>(),
                InFrontDiziler = new List<Models.Dizi>(),
                AltyaziliEpisodes = new List<Models.Episode>(),
                AltyazisizEpisodes = new List<Models.Episode>()
            };

            //Category'e göre listeleme 
            episodeLVM.Episodes = _db.Episodes
               .Include(m=> m.Dizi)
               .Where(m => m.Dizi.Approved == true)
               .Where(a => a.Approved == true)
               .Where(m => category == null || m.Dizi.Genre.Contains(category) == true)
               .OrderByDescending(m => m.Id)
               .Skip((page - 1) * PageSize)
               .Take(PageSize).ToList();
            //
            episodeLVM.SonEklenenEpisodes = _db.Episodes.Include(m => m.Dizi).Where(m=> m.Dizi.Approved ==true).Where(m=> m.Approved==true).OrderByDescending(m => m.Id).Skip((page-1)*PageSize).Take(PageSize).ToList(); // Son blümleri al
            episodeLVM.AltyaziliEpisodes = _db.Episodes.Include(m => m.Dizi).Where(m => m.Dizi.Approved == true).Where(m => m.Approved == true).Where(m => m.Subtitle == "images/FlagImages/1.png").OrderByDescending(m => m.Id).Skip((page - 1) * PageSize).Take(PageSize).ToList();//Türkçe olanları al
            episodeLVM.AltyazisizEpisodes = _db.Episodes.Include(m => m.Dizi).Where(m => m.Dizi.Approved == true).Where(m => m.Approved == true).Where(m => m.Subtitle == "images/FlagImages/0.png" || m.Subtitle == "images/FlagImages/2.png").OrderByDescending(m => m.Id).Skip((page - 1) * PageSize).Take(PageSize).ToList();//NA ve En olanları al
            episodeLVM.InFrontDiziler = _db.Diziler.Where(m => m.Approved == true && m.InFront == true).OrderByDescending(m=> m.Id).Take(8).ToList();
            //episodeLVM.Episodes = _db.Episodes.OrderByDescending(m => m.Id).ToList();
            return View(episodeLVM);
        }
        
    }
}
