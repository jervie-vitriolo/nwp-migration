using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWP_DB_Migration.Article
{
    internal class Post
    {

        public string author { get; set; }
        public string caption { get; set; }
        public string categories { get; set; }
        public DateTime created { get; set; }
        public string imagesource { get; set; }
        public string mixintypes { get; set; }
        public string primarytype { get; set; }
        public string uuid { get; set; }
        public string lead { get; set; }
        public bool activationstatus { get; set; }
        public string createdby { get; set; }
        public DateTime lastactivated { get; set; }
        public string lastactivatedby { get; set; }
        public DateTime lastmodified { get; set; }
        public string lastmodifiedby { get; set; }
        public string tags { get; set; }
        public string stories { get; set; }
        public string template { get; set; }
        public string title { get; set; }
        public string visualtype { get; set; }

        public section0 section0 = new section0();
        public section1 section1 = new section1();
        public section2 section2 = new section2();
        public section3 section3 = new section3();
        public section4 section4 = new section4();
        public section5 section5 = new section5();
        public section6 section6 = new section6();
        public section7 section7 = new section7();
        public section8 section8 = new section8();
        public section9 section9 = new section9();
        public section10 section10 = new section10();
        public section11 section11 = new section11();
        public section12 section12 = new section12();
        public section13 section13 = new section13();
    }
}
