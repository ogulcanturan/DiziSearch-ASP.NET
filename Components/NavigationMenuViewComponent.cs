using DiziSearch.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiziSearch.Components
{
    public class NavigationMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public NavigationMenuViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public IViewComponentResult Invoke()
        {   //Highlighting the Current Category
            //ViewBag.SelectedCategory = RouteData?.Values["category"]; not working due to wrong Routing system
            //GET DIFFERENT ELEMENTS
            var a = _db.Diziler.Select(x => x.Genre).ToList();
            List<string> b = new List<string>();

            foreach (var item in a)
            {
                string forNow = item;
                forNow = forNow?.Replace(" ", "");
                if (forNow!=null)
                {
                    if(forNow?.IndexOf(',') != -1)
                    {
                        do
                        {
                            b.Add(forNow?.Substring(0, forNow.IndexOf(',')));
                            forNow = forNow?.Remove(0, forNow.IndexOf(',') + 1);
                            if (forNow?.IndexOf(',') == -1)
                            {
                                b.Add(forNow);
                            }

                        } while (forNow.IndexOf(',') != -1);
                    }
                    

                }
            }

            return View(b.Distinct().OrderBy(x => x));
        }
    }
}
