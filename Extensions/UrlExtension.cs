using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiziSearch.Extensions
{
    public static class UrlExtension
    {
        public static string AliasUrl(this Microsoft.AspNetCore.Mvc.IUrlHelper helper,string url)
        {
            if(string.IsNullOrEmpty(url)) return "";

            url = url.Trim();//Removes Empty Spaces
            if(url.Length > 100)//if its bigger than 100 characters 
            {
                url = url.Substring(0, 100);//Take first 100 character
            }
            //Türkçe karakterden arındır
            url = url.Replace("İ", "I");
            url = url.Replace("ı", "i");
            url = url.Replace("Ğ", "G");
            url = url.Replace("ğ", "g");
            url = url.Replace("Ç", "C");
            url = url.Replace("ç", "c");
            url = url.Replace("Ö", "O");
            url = url.Replace("ö", "o");
            url = url.Replace("Ş", "S");
            url = url.Replace("ş", "s");
            url = url.Replace("Ü", "U");
            url = url.Replace("ü", "u");
            //Diğer işaretlerden arındır
            url = url.Replace("'", ""); //Example sal'am = salam
            url = url.Replace("\"",""); //Example "salam" = salam
            
            char[] degistir = @"!'^+%&/()=?_*""£#$½{[]}\~.,:;<>|€@".ToCharArray();
            for(int i =0;i< degistir.Length; i++)
            {
                if (url.Contains(degistir[i]))
                {
                    url = url.Replace(degistir[i].ToString(), string.Empty);
                }
            }
            //herhangi bir karakter a'dan z ye ve A'dan Z'ye 0'dan 9'a Altı çizili olanlar hariç
            Regex r = new Regex("[^a-zA-Z0-9_-]");

            url = r.Replace(url, "-");
            while (url.IndexOf("--") > -1)
                url = url.Replace("--", "-");
            return url;
        }
    }
}
