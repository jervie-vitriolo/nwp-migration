using NWP_DB_Migration.Article;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.Marshalling;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static System.Net.Mime.MediaTypeNames;




//1 - Clean up article
//CleanUpArticle();

//2 - Generate classes
GeneratePostClass();


//3 - Authors
//ProcessAuthors(fileName, "author:");

//4 - SQL
//GenerateInsertSql();

//5 - Extract fetured image
//ExtractFeaturedImage();

void GeneratePostClass()
{
    string section = string.Empty;
    string ClassInit = string.Empty;
    string XMas = string.Empty;


    for (int i = 0; i < 600; i++)
    {
        section = section + $"internal class section{i} {{ \n" +
       "public string primarytype { get; set; } \n" +
       "public string uuid { get; set; } \n" +
       "public string activationstatus { get; set; } \n" +
       "public DateTime created { get; set; } \n" +
       "public string createdby { get; set; } \n" +
       "public DateTime lastactivated { get; set; } \n" +
       "public string lastactivatedby { get; set; } \n" +
       "public DateTime lastmodified { get; set; } \n" +
       "public string lastmodifiedby { get; set; } \n" +
       "public string type { get; set; } \n" +
       "public string text { get; set; } \n" +
       "public string embedCode { get; set; } }\n\n\n";

        ClassInit = ClassInit + $"public section{i} section{i} = new section{i}();";

        XMas = XMas + $"finalString = finalString + CleanChecks(post.section{i}.text);\n";
    }

    string ClassDeclaration = $"namespace NWP_DB_Migration.Article\r\n {{ {section}  }}";

}

void CleanUpArticle()
{
    string subpath = "";
    // Get all subdirectories in the specified path
    string[] directories = { @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Articles\nwp - in progress\2024\5" };


    // Loop through each directory
    foreach (string directory in directories)
    {
        Console.WriteLine(directory);
        
        foreach (string filePath in Directory.EnumerateFiles(directory))
        {
            Console.WriteLine($"Clean up Section");
            string matchingLines = File.ReadAllText(filePath);

            //section
            for (int i = 0; i < 582; i++)
            {
                matchingLines = matchingLines.Replace($"'{i}':", $"section{i}:");
            }

            //small caps
            matchingLines = matchingLines.Replace("mixinTypes", "mixintypes");
            matchingLines = matchingLines.Replace("primaryType", "primarytype");
            matchingLines = matchingLines.Replace("createdBy", "createdby");
            matchingLines = matchingLines.Replace("lastActivated", "lastactivated");
            matchingLines = matchingLines.Replace("lastActivatedBy", "lastactivatedby");
            matchingLines = matchingLines.Replace("lastModified':", "lastmodified':");
            matchingLines = matchingLines.Replace("lastModifiedBy", "lastmodifiedby");
            matchingLines = matchingLines.Replace("visualType", "visualtype");
            matchingLines = matchingLines.Replace("activationStatus", "activationstatus");


            //clean
            matchingLines = matchingLines.Replace("'jcr:", "'");
            matchingLines = matchingLines.Replace("'mgnl:", "'");
            matchingLines = matchingLines.Replace("[]", "''");
            matchingLines = matchingLines.Replace(": ['", ": '");
            matchingLines = matchingLines.Replace("']", "'");
            matchingLines = matchingLines.Replace("  '", "  ");
            matchingLines = matchingLines.Replace("': '", ": '");
            matchingLines = matchingLines.Replace("': ", ": ");
            matchingLines = matchingLines.Replace("START HERE----->", "'START HERE----->");


            matchingLines = matchingLines.Replace("lastactivatedBy", "lastactivatedby");




            File.WriteAllText(filePath, matchingLines);
        }
    }
}

void CreatePostInsertSql(Post post,int PostID){

    
    string WP_Post_Article_InsertSql = $"INSERT INTO `wp_posts` ( `ID`,`post_author`, `post_date`, `post_date_gmt`, `post_content`, `post_title`, `post_excerpt`, `post_status`, `comment_status`, `ping_status`, `post_password`, `post_name`, `to_ping`, `pinged`, `post_modified`, `post_modified_gmt`, `post_content_filtered`, `post_parent`, `guid`, `menu_order`, `post_type`, `post_mime_type`, `comment_count`) " +
                          $"VALUES({PostID} ,'{getPostAuthorID(post.author)}', '{formatDateTime(post.created)}', '{formatDateTime(post.created)}', '{getPostContent(post)}', '{mysqlStringFormat(post.title)}', '{mysqlStringFormat(post.caption)}', '{getPostStatus(post)}', 'open', 'open', '', '{mysqlStringFormat(post.title).Replace(" ", "-")}', '', '', '{formatDateTime(post.lastmodified)}', '{formatDateTime(post.lastmodified)}', '', 0, 'https://newswatchplus-staging.azurewebsites.net/?p=', 0, 'post', '', 0);";

    string WP_PostMeta = $"INSERT INTO `wp_postmeta` ( `post_id`, `meta_key`, `meta_value`) VALUES( {PostID}, '_thumbnail_id', '{GetPostMetaValue(post.imagesource)}');";

    string WP_term_relationships = $"INSERT INTO wp_term_relationships(OBJECT_ID,TERM_TAXONOMY_ID,TERM_ORDER) VALUES({PostID},{getCategoryId(post.categories)},0);";

}

int getCategoryId(string categoryId)
{
    string categoryname = getFromNWPCategories(categoryId);

    return 36;
}

string getFromNWPCategories(string categoryId)
{
    return "news";
}

string mysqlStringFormat(string text)
{
    if(text  != null)
    {
        return Regex.Replace(text, @"[\r\n\x00\x1a\\'""]", @"\$0");
    }

    return string.Empty;
}

void GenerateInsertSql()
{
    string subpath = "";
    // Get all subdirectories in the specified path
    string[] directories = { @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Articles\nwp - in progress\5" };


    // Loop through each directory
    foreach (string directory in directories)
    {
        Console.WriteLine(directory);
        int PostID = 2888;
        foreach (string filePath in Directory.EnumerateFiles(directory))
        {
            Console.WriteLine($"Found file: {filePath}");

            var matchingLines = File.ReadLines(filePath)
                                .Select((line, index) => new { LineText = line, LineNumber = index + 1 })
                                .Where(item => item.LineText.Contains("START HERE----->"));

            int[] lineNumbers = matchingLines.Select(x => x.LineNumber).ToArray();

            for (int i = 0; i < lineNumbers.Length; i++)
            {
                int startLine = lineNumbers[i]+1;
                int endLine = 0;

                if (i==lineNumbers.Length-1)
                {
                     endLine = File.ReadAllLines(filePath).Length;
                }
                else
                {
                    endLine = lineNumbers[i+1] - 1;
                }

                List<string> articles = GetArticles(filePath, startLine, endLine);
                string yml = string.Join("\n", articles);

                //start parsing
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                    .Build();


                //yml contains a string containing your YAML
                var p = deserializer.Deserialize<Post>(yml);
                if (p != null)
                {
                    CreatePostInsertSql(p,PostID);
                    PostID ++;
                }

            }
            

        }

    }

}

int GetPostMetaValue(string imagesource)
{
    ImageList ImageList = new ImageList();
    var ImageId = ImageList.GetImage().FirstOrDefault(s => s.ImageTitle == imagesource).ImageId;
    return ImageId;
}

string formatDateTime(DateTime created)
{
    return created.ToString("yyyy-MM-dd HH:mm:ss");
}

string getPostStatus(Post post)
{
    if (post.activationstatus)
    {
        return "publish";
    }

    return "draft";
}

int getPostAuthorID(string name)
{
    if (name == null)
    {
        return 38;
    }
    else
    {
        AuthorsList AuthorsList = new AuthorsList();
        var ID = AuthorsList.GetAuthors().FirstOrDefault(s => s.Author == name).ID;
        return ID;
    }

}

string getPostContent(Post post)
{
    string finalString = string.Empty;

    try
    {
        finalString = CleanChecks(post.section0.text);
        finalString = finalString + CleanChecks(post.section1.text);
        finalString = finalString + CleanChecks(post.section2.text);
        finalString = finalString + CleanChecks(post.section3.text);
        finalString = finalString + CleanChecks(post.section4.text);
        finalString = finalString + CleanChecks(post.section5.text);
        finalString = finalString + CleanChecks(post.section6.text);
        finalString = finalString + CleanChecks(post.section7.text);
        finalString = finalString + CleanChecks(post.section8.text);
        finalString = finalString + CleanChecks(post.section9.text);
        finalString = finalString + CleanChecks(post.section10.text);
        finalString = finalString + CleanChecks(post.section11.text);
        finalString = finalString + CleanChecks(post.section12.text);
        finalString = finalString + CleanChecks(post.section13.text);
        finalString = finalString + CleanChecks(post.section0.text);
        finalString = finalString + CleanChecks(post.section1.text);
        finalString = finalString + CleanChecks(post.section2.text);
        finalString = finalString + CleanChecks(post.section3.text);
        finalString = finalString + CleanChecks(post.section4.text);
        finalString = finalString + CleanChecks(post.section5.text);
        finalString = finalString + CleanChecks(post.section6.text);
        finalString = finalString + CleanChecks(post.section7.text);
        finalString = finalString + CleanChecks(post.section8.text);
        finalString = finalString + CleanChecks(post.section9.text);
        finalString = finalString + CleanChecks(post.section10.text);
        finalString = finalString + CleanChecks(post.section11.text);
        finalString = finalString + CleanChecks(post.section12.text);
        finalString = finalString + CleanChecks(post.section13.text);
        finalString = finalString + CleanChecks(post.section14.text);
        finalString = finalString + CleanChecks(post.section15.text);
        finalString = finalString + CleanChecks(post.section16.text);
        finalString = finalString + CleanChecks(post.section17.text);
        finalString = finalString + CleanChecks(post.section18.text);
        finalString = finalString + CleanChecks(post.section19.text);
        finalString = finalString + CleanChecks(post.section20.text);
        finalString = finalString + CleanChecks(post.section21.text);
        finalString = finalString + CleanChecks(post.section22.text);
        finalString = finalString + CleanChecks(post.section23.text);
        finalString = finalString + CleanChecks(post.section24.text);
        finalString = finalString + CleanChecks(post.section25.text);
        finalString = finalString + CleanChecks(post.section26.text);
        finalString = finalString + CleanChecks(post.section27.text);
        finalString = finalString + CleanChecks(post.section28.text);
        finalString = finalString + CleanChecks(post.section29.text);
        finalString = finalString + CleanChecks(post.section30.text);
        finalString = finalString + CleanChecks(post.section31.text);
        finalString = finalString + CleanChecks(post.section32.text);
        finalString = finalString + CleanChecks(post.section33.text);
        finalString = finalString + CleanChecks(post.section34.text);
        finalString = finalString + CleanChecks(post.section35.text);
        finalString = finalString + CleanChecks(post.section36.text);
        finalString = finalString + CleanChecks(post.section37.text);
        finalString = finalString + CleanChecks(post.section38.text);
        finalString = finalString + CleanChecks(post.section39.text);
        finalString = finalString + CleanChecks(post.section40.text);
        finalString = finalString + CleanChecks(post.section41.text);
        finalString = finalString + CleanChecks(post.section42.text);
        finalString = finalString + CleanChecks(post.section43.text);
        finalString = finalString + CleanChecks(post.section44.text);
        finalString = finalString + CleanChecks(post.section45.text);
        finalString = finalString + CleanChecks(post.section46.text);
        finalString = finalString + CleanChecks(post.section47.text);
        finalString = finalString + CleanChecks(post.section48.text);
        finalString = finalString + CleanChecks(post.section49.text);
        finalString = finalString + CleanChecks(post.section50.text);
        finalString = finalString + CleanChecks(post.section51.text);
        finalString = finalString + CleanChecks(post.section52.text);
        finalString = finalString + CleanChecks(post.section53.text);
        finalString = finalString + CleanChecks(post.section54.text);
        finalString = finalString + CleanChecks(post.section55.text);
        finalString = finalString + CleanChecks(post.section56.text);
        finalString = finalString + CleanChecks(post.section57.text);
        finalString = finalString + CleanChecks(post.section58.text);
        finalString = finalString + CleanChecks(post.section59.text);
        finalString = finalString + CleanChecks(post.section60.text);
        finalString = finalString + CleanChecks(post.section61.text);
        finalString = finalString + CleanChecks(post.section62.text);
        finalString = finalString + CleanChecks(post.section63.text);
        finalString = finalString + CleanChecks(post.section64.text);
        finalString = finalString + CleanChecks(post.section65.text);
        finalString = finalString + CleanChecks(post.section66.text);
        finalString = finalString + CleanChecks(post.section67.text);
        finalString = finalString + CleanChecks(post.section68.text);
        finalString = finalString + CleanChecks(post.section69.text);
        finalString = finalString + CleanChecks(post.section70.text);
        finalString = finalString + CleanChecks(post.section71.text);
        finalString = finalString + CleanChecks(post.section72.text);
        finalString = finalString + CleanChecks(post.section73.text);
        finalString = finalString + CleanChecks(post.section74.text);
        finalString = finalString + CleanChecks(post.section75.text);
        finalString = finalString + CleanChecks(post.section76.text);
        finalString = finalString + CleanChecks(post.section77.text);
        finalString = finalString + CleanChecks(post.section78.text);
        finalString = finalString + CleanChecks(post.section79.text);
        finalString = finalString + CleanChecks(post.section80.text);
        finalString = finalString + CleanChecks(post.section81.text);
        finalString = finalString + CleanChecks(post.section82.text);
        finalString = finalString + CleanChecks(post.section83.text);
        finalString = finalString + CleanChecks(post.section84.text);
        finalString = finalString + CleanChecks(post.section85.text);
        finalString = finalString + CleanChecks(post.section86.text);
        finalString = finalString + CleanChecks(post.section87.text);
        finalString = finalString + CleanChecks(post.section88.text);
        finalString = finalString + CleanChecks(post.section89.text);
        finalString = finalString + CleanChecks(post.section90.text);
        finalString = finalString + CleanChecks(post.section91.text);
        finalString = finalString + CleanChecks(post.section92.text);
        finalString = finalString + CleanChecks(post.section93.text);
        finalString = finalString + CleanChecks(post.section94.text);
        finalString = finalString + CleanChecks(post.section95.text);
        finalString = finalString + CleanChecks(post.section96.text);
        finalString = finalString + CleanChecks(post.section97.text);
        finalString = finalString + CleanChecks(post.section98.text);
        finalString = finalString + CleanChecks(post.section99.text);
        finalString = finalString + CleanChecks(post.section100.text);
        finalString = finalString + CleanChecks(post.section101.text);
        finalString = finalString + CleanChecks(post.section102.text);
        finalString = finalString + CleanChecks(post.section103.text);
        finalString = finalString + CleanChecks(post.section104.text);
        finalString = finalString + CleanChecks(post.section105.text);
        finalString = finalString + CleanChecks(post.section106.text);
        finalString = finalString + CleanChecks(post.section107.text);
        finalString = finalString + CleanChecks(post.section108.text);
        finalString = finalString + CleanChecks(post.section109.text);
        finalString = finalString + CleanChecks(post.section110.text);
        finalString = finalString + CleanChecks(post.section111.text);
        finalString = finalString + CleanChecks(post.section112.text);
        finalString = finalString + CleanChecks(post.section113.text);
        finalString = finalString + CleanChecks(post.section114.text);
        finalString = finalString + CleanChecks(post.section115.text);
        finalString = finalString + CleanChecks(post.section116.text);
        finalString = finalString + CleanChecks(post.section117.text);
        finalString = finalString + CleanChecks(post.section118.text);
        finalString = finalString + CleanChecks(post.section119.text);
        finalString = finalString + CleanChecks(post.section120.text);
        finalString = finalString + CleanChecks(post.section121.text);
        finalString = finalString + CleanChecks(post.section122.text);
        finalString = finalString + CleanChecks(post.section123.text);
        finalString = finalString + CleanChecks(post.section124.text);
        finalString = finalString + CleanChecks(post.section125.text);
        finalString = finalString + CleanChecks(post.section126.text);
        finalString = finalString + CleanChecks(post.section127.text);
        finalString = finalString + CleanChecks(post.section128.text);
        finalString = finalString + CleanChecks(post.section129.text);
        finalString = finalString + CleanChecks(post.section130.text);
        finalString = finalString + CleanChecks(post.section131.text);
        finalString = finalString + CleanChecks(post.section132.text);
        finalString = finalString + CleanChecks(post.section133.text);
        finalString = finalString + CleanChecks(post.section134.text);
        finalString = finalString + CleanChecks(post.section135.text);
        finalString = finalString + CleanChecks(post.section136.text);
        finalString = finalString + CleanChecks(post.section137.text);
        finalString = finalString + CleanChecks(post.section138.text);
        finalString = finalString + CleanChecks(post.section139.text);
        finalString = finalString + CleanChecks(post.section140.text);
        finalString = finalString + CleanChecks(post.section141.text);
        finalString = finalString + CleanChecks(post.section142.text);
        finalString = finalString + CleanChecks(post.section143.text);
        finalString = finalString + CleanChecks(post.section144.text);
        finalString = finalString + CleanChecks(post.section145.text);
        finalString = finalString + CleanChecks(post.section146.text);
        finalString = finalString + CleanChecks(post.section147.text);
        finalString = finalString + CleanChecks(post.section148.text);
        finalString = finalString + CleanChecks(post.section149.text);
        finalString = finalString + CleanChecks(post.section150.text);
        finalString = finalString + CleanChecks(post.section151.text);
        finalString = finalString + CleanChecks(post.section152.text);
        finalString = finalString + CleanChecks(post.section153.text);
        finalString = finalString + CleanChecks(post.section154.text);
        finalString = finalString + CleanChecks(post.section155.text);
        finalString = finalString + CleanChecks(post.section156.text);
        finalString = finalString + CleanChecks(post.section157.text);
        finalString = finalString + CleanChecks(post.section158.text);
        finalString = finalString + CleanChecks(post.section159.text);
        finalString = finalString + CleanChecks(post.section160.text);
        finalString = finalString + CleanChecks(post.section161.text);
        finalString = finalString + CleanChecks(post.section162.text);
        finalString = finalString + CleanChecks(post.section163.text);
        finalString = finalString + CleanChecks(post.section164.text);
        finalString = finalString + CleanChecks(post.section165.text);
        finalString = finalString + CleanChecks(post.section166.text);
        finalString = finalString + CleanChecks(post.section167.text);
        finalString = finalString + CleanChecks(post.section168.text);
        finalString = finalString + CleanChecks(post.section169.text);
        finalString = finalString + CleanChecks(post.section170.text);
        finalString = finalString + CleanChecks(post.section171.text);
        finalString = finalString + CleanChecks(post.section172.text);
        finalString = finalString + CleanChecks(post.section173.text);
        finalString = finalString + CleanChecks(post.section174.text);
        finalString = finalString + CleanChecks(post.section175.text);
        finalString = finalString + CleanChecks(post.section176.text);
        finalString = finalString + CleanChecks(post.section177.text);
        finalString = finalString + CleanChecks(post.section178.text);
        finalString = finalString + CleanChecks(post.section179.text);
        finalString = finalString + CleanChecks(post.section180.text);
        finalString = finalString + CleanChecks(post.section181.text);
        finalString = finalString + CleanChecks(post.section182.text);
        finalString = finalString + CleanChecks(post.section183.text);
        finalString = finalString + CleanChecks(post.section184.text);
        finalString = finalString + CleanChecks(post.section185.text);
        finalString = finalString + CleanChecks(post.section186.text);
        finalString = finalString + CleanChecks(post.section187.text);
        finalString = finalString + CleanChecks(post.section188.text);
        finalString = finalString + CleanChecks(post.section189.text);
        finalString = finalString + CleanChecks(post.section190.text);
        finalString = finalString + CleanChecks(post.section191.text);
        finalString = finalString + CleanChecks(post.section192.text);
        finalString = finalString + CleanChecks(post.section193.text);
        finalString = finalString + CleanChecks(post.section194.text);
        finalString = finalString + CleanChecks(post.section195.text);
        finalString = finalString + CleanChecks(post.section196.text);
        finalString = finalString + CleanChecks(post.section197.text);
        finalString = finalString + CleanChecks(post.section198.text);
        finalString = finalString + CleanChecks(post.section199.text);
        finalString = finalString + CleanChecks(post.section200.text);
        finalString = finalString + CleanChecks(post.section201.text);
        finalString = finalString + CleanChecks(post.section202.text);
        finalString = finalString + CleanChecks(post.section203.text);
        finalString = finalString + CleanChecks(post.section204.text);
        finalString = finalString + CleanChecks(post.section205.text);
        finalString = finalString + CleanChecks(post.section206.text);
        finalString = finalString + CleanChecks(post.section207.text);
        finalString = finalString + CleanChecks(post.section208.text);
        finalString = finalString + CleanChecks(post.section209.text);
        finalString = finalString + CleanChecks(post.section210.text);
        finalString = finalString + CleanChecks(post.section211.text);
        finalString = finalString + CleanChecks(post.section212.text);
        finalString = finalString + CleanChecks(post.section213.text);
        finalString = finalString + CleanChecks(post.section214.text);
        finalString = finalString + CleanChecks(post.section215.text);
        finalString = finalString + CleanChecks(post.section216.text);
        finalString = finalString + CleanChecks(post.section217.text);
        finalString = finalString + CleanChecks(post.section218.text);
        finalString = finalString + CleanChecks(post.section219.text);
        finalString = finalString + CleanChecks(post.section220.text);
        finalString = finalString + CleanChecks(post.section221.text);
        finalString = finalString + CleanChecks(post.section222.text);
        finalString = finalString + CleanChecks(post.section223.text);
        finalString = finalString + CleanChecks(post.section224.text);
        finalString = finalString + CleanChecks(post.section225.text);
        finalString = finalString + CleanChecks(post.section226.text);
        finalString = finalString + CleanChecks(post.section227.text);
        finalString = finalString + CleanChecks(post.section228.text);
        finalString = finalString + CleanChecks(post.section229.text);
        finalString = finalString + CleanChecks(post.section230.text);
        finalString = finalString + CleanChecks(post.section231.text);
        finalString = finalString + CleanChecks(post.section232.text);
        finalString = finalString + CleanChecks(post.section233.text);
        finalString = finalString + CleanChecks(post.section234.text);
        finalString = finalString + CleanChecks(post.section235.text);
        finalString = finalString + CleanChecks(post.section236.text);
        finalString = finalString + CleanChecks(post.section237.text);
        finalString = finalString + CleanChecks(post.section238.text);
        finalString = finalString + CleanChecks(post.section239.text);
        finalString = finalString + CleanChecks(post.section240.text);
        finalString = finalString + CleanChecks(post.section241.text);
        finalString = finalString + CleanChecks(post.section242.text);
        finalString = finalString + CleanChecks(post.section243.text);
        finalString = finalString + CleanChecks(post.section244.text);
        finalString = finalString + CleanChecks(post.section245.text);
        finalString = finalString + CleanChecks(post.section246.text);
        finalString = finalString + CleanChecks(post.section247.text);
        finalString = finalString + CleanChecks(post.section248.text);
        finalString = finalString + CleanChecks(post.section249.text);
        finalString = finalString + CleanChecks(post.section250.text);
        finalString = finalString + CleanChecks(post.section251.text);
        finalString = finalString + CleanChecks(post.section252.text);
        finalString = finalString + CleanChecks(post.section253.text);
        finalString = finalString + CleanChecks(post.section254.text);
        finalString = finalString + CleanChecks(post.section255.text);
        finalString = finalString + CleanChecks(post.section256.text);
        finalString = finalString + CleanChecks(post.section257.text);
        finalString = finalString + CleanChecks(post.section258.text);
        finalString = finalString + CleanChecks(post.section259.text);
        finalString = finalString + CleanChecks(post.section260.text);
        finalString = finalString + CleanChecks(post.section261.text);
        finalString = finalString + CleanChecks(post.section262.text);
        finalString = finalString + CleanChecks(post.section263.text);
        finalString = finalString + CleanChecks(post.section264.text);
        finalString = finalString + CleanChecks(post.section265.text);
        finalString = finalString + CleanChecks(post.section266.text);
        finalString = finalString + CleanChecks(post.section267.text);
        finalString = finalString + CleanChecks(post.section268.text);
        finalString = finalString + CleanChecks(post.section269.text);
        finalString = finalString + CleanChecks(post.section270.text);
        finalString = finalString + CleanChecks(post.section271.text);
        finalString = finalString + CleanChecks(post.section272.text);
        finalString = finalString + CleanChecks(post.section273.text);
        finalString = finalString + CleanChecks(post.section274.text);
        finalString = finalString + CleanChecks(post.section275.text);
        finalString = finalString + CleanChecks(post.section276.text);
        finalString = finalString + CleanChecks(post.section277.text);
        finalString = finalString + CleanChecks(post.section278.text);
        finalString = finalString + CleanChecks(post.section279.text);
        finalString = finalString + CleanChecks(post.section280.text);
        finalString = finalString + CleanChecks(post.section281.text);
        finalString = finalString + CleanChecks(post.section282.text);
        finalString = finalString + CleanChecks(post.section283.text);
        finalString = finalString + CleanChecks(post.section284.text);
        finalString = finalString + CleanChecks(post.section285.text);
        finalString = finalString + CleanChecks(post.section286.text);
        finalString = finalString + CleanChecks(post.section287.text);
        finalString = finalString + CleanChecks(post.section288.text);
        finalString = finalString + CleanChecks(post.section289.text);
        finalString = finalString + CleanChecks(post.section290.text);
        finalString = finalString + CleanChecks(post.section291.text);
        finalString = finalString + CleanChecks(post.section292.text);
        finalString = finalString + CleanChecks(post.section293.text);
        finalString = finalString + CleanChecks(post.section294.text);
        finalString = finalString + CleanChecks(post.section295.text);
        finalString = finalString + CleanChecks(post.section296.text);
        finalString = finalString + CleanChecks(post.section297.text);
        finalString = finalString + CleanChecks(post.section298.text);
        finalString = finalString + CleanChecks(post.section299.text);
        finalString = finalString + CleanChecks(post.section300.text);
        finalString = finalString + CleanChecks(post.section301.text);
        finalString = finalString + CleanChecks(post.section302.text);
        finalString = finalString + CleanChecks(post.section303.text);
        finalString = finalString + CleanChecks(post.section304.text);
        finalString = finalString + CleanChecks(post.section305.text);
        finalString = finalString + CleanChecks(post.section306.text);
        finalString = finalString + CleanChecks(post.section307.text);
        finalString = finalString + CleanChecks(post.section308.text);
        finalString = finalString + CleanChecks(post.section309.text);
        finalString = finalString + CleanChecks(post.section310.text);
        finalString = finalString + CleanChecks(post.section311.text);
        finalString = finalString + CleanChecks(post.section312.text);
        finalString = finalString + CleanChecks(post.section313.text);
        finalString = finalString + CleanChecks(post.section314.text);
        finalString = finalString + CleanChecks(post.section315.text);
        finalString = finalString + CleanChecks(post.section316.text);
        finalString = finalString + CleanChecks(post.section317.text);
        finalString = finalString + CleanChecks(post.section318.text);
        finalString = finalString + CleanChecks(post.section319.text);
        finalString = finalString + CleanChecks(post.section320.text);
        finalString = finalString + CleanChecks(post.section321.text);
        finalString = finalString + CleanChecks(post.section322.text);
        finalString = finalString + CleanChecks(post.section323.text);
        finalString = finalString + CleanChecks(post.section324.text);
        finalString = finalString + CleanChecks(post.section325.text);
        finalString = finalString + CleanChecks(post.section326.text);
        finalString = finalString + CleanChecks(post.section327.text);
        finalString = finalString + CleanChecks(post.section328.text);
        finalString = finalString + CleanChecks(post.section329.text);
        finalString = finalString + CleanChecks(post.section330.text);
        finalString = finalString + CleanChecks(post.section331.text);
        finalString = finalString + CleanChecks(post.section332.text);
        finalString = finalString + CleanChecks(post.section333.text);
        finalString = finalString + CleanChecks(post.section334.text);
        finalString = finalString + CleanChecks(post.section335.text);
        finalString = finalString + CleanChecks(post.section336.text);
        finalString = finalString + CleanChecks(post.section337.text);
        finalString = finalString + CleanChecks(post.section338.text);
        finalString = finalString + CleanChecks(post.section339.text);
        finalString = finalString + CleanChecks(post.section340.text);
        finalString = finalString + CleanChecks(post.section341.text);
        finalString = finalString + CleanChecks(post.section342.text);
        finalString = finalString + CleanChecks(post.section343.text);
        finalString = finalString + CleanChecks(post.section344.text);
        finalString = finalString + CleanChecks(post.section345.text);
        finalString = finalString + CleanChecks(post.section346.text);
        finalString = finalString + CleanChecks(post.section347.text);
        finalString = finalString + CleanChecks(post.section348.text);
        finalString = finalString + CleanChecks(post.section349.text);
        finalString = finalString + CleanChecks(post.section350.text);
        finalString = finalString + CleanChecks(post.section351.text);
        finalString = finalString + CleanChecks(post.section352.text);
        finalString = finalString + CleanChecks(post.section353.text);
        finalString = finalString + CleanChecks(post.section354.text);
        finalString = finalString + CleanChecks(post.section355.text);
        finalString = finalString + CleanChecks(post.section356.text);
        finalString = finalString + CleanChecks(post.section357.text);
        finalString = finalString + CleanChecks(post.section358.text);
        finalString = finalString + CleanChecks(post.section359.text);
        finalString = finalString + CleanChecks(post.section360.text);
        finalString = finalString + CleanChecks(post.section361.text);
        finalString = finalString + CleanChecks(post.section362.text);
        finalString = finalString + CleanChecks(post.section363.text);
        finalString = finalString + CleanChecks(post.section364.text);
        finalString = finalString + CleanChecks(post.section365.text);
        finalString = finalString + CleanChecks(post.section366.text);
        finalString = finalString + CleanChecks(post.section367.text);
        finalString = finalString + CleanChecks(post.section368.text);
        finalString = finalString + CleanChecks(post.section369.text);
        finalString = finalString + CleanChecks(post.section370.text);
        finalString = finalString + CleanChecks(post.section371.text);
        finalString = finalString + CleanChecks(post.section372.text);
        finalString = finalString + CleanChecks(post.section373.text);
        finalString = finalString + CleanChecks(post.section374.text);
        finalString = finalString + CleanChecks(post.section375.text);
        finalString = finalString + CleanChecks(post.section376.text);
        finalString = finalString + CleanChecks(post.section377.text);
        finalString = finalString + CleanChecks(post.section378.text);
        finalString = finalString + CleanChecks(post.section379.text);
        finalString = finalString + CleanChecks(post.section380.text);
        finalString = finalString + CleanChecks(post.section381.text);
        finalString = finalString + CleanChecks(post.section382.text);
        finalString = finalString + CleanChecks(post.section383.text);
        finalString = finalString + CleanChecks(post.section384.text);
        finalString = finalString + CleanChecks(post.section385.text);
        finalString = finalString + CleanChecks(post.section386.text);
        finalString = finalString + CleanChecks(post.section387.text);
        finalString = finalString + CleanChecks(post.section388.text);
        finalString = finalString + CleanChecks(post.section389.text);
        finalString = finalString + CleanChecks(post.section390.text);
        finalString = finalString + CleanChecks(post.section391.text);
        finalString = finalString + CleanChecks(post.section392.text);
        finalString = finalString + CleanChecks(post.section393.text);
        finalString = finalString + CleanChecks(post.section394.text);
        finalString = finalString + CleanChecks(post.section395.text);
        finalString = finalString + CleanChecks(post.section396.text);
        finalString = finalString + CleanChecks(post.section397.text);
        finalString = finalString + CleanChecks(post.section398.text);
        finalString = finalString + CleanChecks(post.section399.text);
        finalString = finalString + CleanChecks(post.section400.text);
        finalString = finalString + CleanChecks(post.section401.text);
        finalString = finalString + CleanChecks(post.section402.text);
        finalString = finalString + CleanChecks(post.section403.text);
        finalString = finalString + CleanChecks(post.section404.text);
        finalString = finalString + CleanChecks(post.section405.text);
        finalString = finalString + CleanChecks(post.section406.text);
        finalString = finalString + CleanChecks(post.section407.text);
        finalString = finalString + CleanChecks(post.section408.text);
        finalString = finalString + CleanChecks(post.section409.text);
        finalString = finalString + CleanChecks(post.section410.text);
        finalString = finalString + CleanChecks(post.section411.text);
        finalString = finalString + CleanChecks(post.section412.text);
        finalString = finalString + CleanChecks(post.section413.text);
        finalString = finalString + CleanChecks(post.section414.text);
        finalString = finalString + CleanChecks(post.section415.text);
        finalString = finalString + CleanChecks(post.section416.text);
        finalString = finalString + CleanChecks(post.section417.text);
        finalString = finalString + CleanChecks(post.section418.text);
        finalString = finalString + CleanChecks(post.section419.text);
        finalString = finalString + CleanChecks(post.section420.text);
        finalString = finalString + CleanChecks(post.section421.text);
        finalString = finalString + CleanChecks(post.section422.text);
        finalString = finalString + CleanChecks(post.section423.text);
        finalString = finalString + CleanChecks(post.section424.text);
        finalString = finalString + CleanChecks(post.section425.text);
        finalString = finalString + CleanChecks(post.section426.text);
        finalString = finalString + CleanChecks(post.section427.text);
        finalString = finalString + CleanChecks(post.section428.text);
        finalString = finalString + CleanChecks(post.section429.text);
        finalString = finalString + CleanChecks(post.section430.text);
        finalString = finalString + CleanChecks(post.section431.text);
        finalString = finalString + CleanChecks(post.section432.text);
        finalString = finalString + CleanChecks(post.section433.text);
        finalString = finalString + CleanChecks(post.section434.text);
        finalString = finalString + CleanChecks(post.section435.text);
        finalString = finalString + CleanChecks(post.section436.text);
        finalString = finalString + CleanChecks(post.section437.text);
        finalString = finalString + CleanChecks(post.section438.text);
        finalString = finalString + CleanChecks(post.section439.text);
        finalString = finalString + CleanChecks(post.section440.text);
        finalString = finalString + CleanChecks(post.section441.text);
        finalString = finalString + CleanChecks(post.section442.text);
        finalString = finalString + CleanChecks(post.section443.text);
        finalString = finalString + CleanChecks(post.section444.text);
        finalString = finalString + CleanChecks(post.section445.text);
        finalString = finalString + CleanChecks(post.section446.text);
        finalString = finalString + CleanChecks(post.section447.text);
        finalString = finalString + CleanChecks(post.section448.text);
        finalString = finalString + CleanChecks(post.section449.text);
        finalString = finalString + CleanChecks(post.section450.text);
        finalString = finalString + CleanChecks(post.section451.text);
        finalString = finalString + CleanChecks(post.section452.text);
        finalString = finalString + CleanChecks(post.section453.text);
        finalString = finalString + CleanChecks(post.section454.text);
        finalString = finalString + CleanChecks(post.section455.text);
        finalString = finalString + CleanChecks(post.section456.text);
        finalString = finalString + CleanChecks(post.section457.text);
        finalString = finalString + CleanChecks(post.section458.text);
        finalString = finalString + CleanChecks(post.section459.text);
        finalString = finalString + CleanChecks(post.section460.text);
        finalString = finalString + CleanChecks(post.section461.text);
        finalString = finalString + CleanChecks(post.section462.text);
        finalString = finalString + CleanChecks(post.section463.text);
        finalString = finalString + CleanChecks(post.section464.text);
        finalString = finalString + CleanChecks(post.section465.text);
        finalString = finalString + CleanChecks(post.section466.text);
        finalString = finalString + CleanChecks(post.section467.text);
        finalString = finalString + CleanChecks(post.section468.text);
        finalString = finalString + CleanChecks(post.section469.text);
        finalString = finalString + CleanChecks(post.section470.text);
        finalString = finalString + CleanChecks(post.section471.text);
        finalString = finalString + CleanChecks(post.section472.text);
        finalString = finalString + CleanChecks(post.section473.text);
        finalString = finalString + CleanChecks(post.section474.text);
        finalString = finalString + CleanChecks(post.section475.text);
        finalString = finalString + CleanChecks(post.section476.text);
        finalString = finalString + CleanChecks(post.section477.text);
        finalString = finalString + CleanChecks(post.section478.text);
        finalString = finalString + CleanChecks(post.section479.text);
        finalString = finalString + CleanChecks(post.section480.text);
        finalString = finalString + CleanChecks(post.section481.text);
        finalString = finalString + CleanChecks(post.section482.text);
        finalString = finalString + CleanChecks(post.section483.text);
        finalString = finalString + CleanChecks(post.section484.text);
        finalString = finalString + CleanChecks(post.section485.text);
        finalString = finalString + CleanChecks(post.section486.text);
        finalString = finalString + CleanChecks(post.section487.text);
        finalString = finalString + CleanChecks(post.section488.text);
        finalString = finalString + CleanChecks(post.section489.text);
        finalString = finalString + CleanChecks(post.section490.text);
        finalString = finalString + CleanChecks(post.section491.text);
        finalString = finalString + CleanChecks(post.section492.text);
        finalString = finalString + CleanChecks(post.section493.text);
        finalString = finalString + CleanChecks(post.section494.text);
        finalString = finalString + CleanChecks(post.section495.text);
        finalString = finalString + CleanChecks(post.section496.text);
        finalString = finalString + CleanChecks(post.section497.text);
        finalString = finalString + CleanChecks(post.section498.text);
        finalString = finalString + CleanChecks(post.section499.text);
        finalString = finalString + CleanChecks(post.section500.text);
        finalString = finalString + CleanChecks(post.section501.text);
        finalString = finalString + CleanChecks(post.section502.text);
        finalString = finalString + CleanChecks(post.section503.text);
        finalString = finalString + CleanChecks(post.section504.text);
        finalString = finalString + CleanChecks(post.section505.text);
        finalString = finalString + CleanChecks(post.section506.text);
        finalString = finalString + CleanChecks(post.section507.text);
        finalString = finalString + CleanChecks(post.section508.text);
        finalString = finalString + CleanChecks(post.section509.text);
        finalString = finalString + CleanChecks(post.section510.text);
        finalString = finalString + CleanChecks(post.section511.text);
        finalString = finalString + CleanChecks(post.section512.text);
        finalString = finalString + CleanChecks(post.section513.text);
        finalString = finalString + CleanChecks(post.section514.text);
        finalString = finalString + CleanChecks(post.section515.text);
        finalString = finalString + CleanChecks(post.section516.text);
        finalString = finalString + CleanChecks(post.section517.text);
        finalString = finalString + CleanChecks(post.section518.text);
        finalString = finalString + CleanChecks(post.section519.text);
        finalString = finalString + CleanChecks(post.section520.text);
        finalString = finalString + CleanChecks(post.section521.text);
        finalString = finalString + CleanChecks(post.section522.text);
        finalString = finalString + CleanChecks(post.section523.text);
        finalString = finalString + CleanChecks(post.section524.text);
        finalString = finalString + CleanChecks(post.section525.text);
        finalString = finalString + CleanChecks(post.section526.text);
        finalString = finalString + CleanChecks(post.section527.text);
        finalString = finalString + CleanChecks(post.section528.text);
        finalString = finalString + CleanChecks(post.section529.text);
        finalString = finalString + CleanChecks(post.section530.text);
        finalString = finalString + CleanChecks(post.section531.text);
        finalString = finalString + CleanChecks(post.section532.text);
        finalString = finalString + CleanChecks(post.section533.text);
        finalString = finalString + CleanChecks(post.section534.text);
        finalString = finalString + CleanChecks(post.section535.text);
        finalString = finalString + CleanChecks(post.section536.text);
        finalString = finalString + CleanChecks(post.section537.text);
        finalString = finalString + CleanChecks(post.section538.text);
        finalString = finalString + CleanChecks(post.section539.text);
        finalString = finalString + CleanChecks(post.section540.text);
        finalString = finalString + CleanChecks(post.section541.text);
        finalString = finalString + CleanChecks(post.section542.text);
        finalString = finalString + CleanChecks(post.section543.text);
        finalString = finalString + CleanChecks(post.section544.text);
        finalString = finalString + CleanChecks(post.section545.text);
        finalString = finalString + CleanChecks(post.section546.text);
        finalString = finalString + CleanChecks(post.section547.text);
        finalString = finalString + CleanChecks(post.section548.text);
        finalString = finalString + CleanChecks(post.section549.text);
        finalString = finalString + CleanChecks(post.section550.text);
        finalString = finalString + CleanChecks(post.section551.text);
        finalString = finalString + CleanChecks(post.section552.text);
        finalString = finalString + CleanChecks(post.section553.text);
        finalString = finalString + CleanChecks(post.section554.text);
        finalString = finalString + CleanChecks(post.section555.text);
        finalString = finalString + CleanChecks(post.section556.text);
        finalString = finalString + CleanChecks(post.section557.text);
        finalString = finalString + CleanChecks(post.section558.text);
        finalString = finalString + CleanChecks(post.section559.text);
        finalString = finalString + CleanChecks(post.section560.text);
        finalString = finalString + CleanChecks(post.section561.text);
        finalString = finalString + CleanChecks(post.section562.text);
        finalString = finalString + CleanChecks(post.section563.text);
        finalString = finalString + CleanChecks(post.section564.text);
        finalString = finalString + CleanChecks(post.section565.text);
        finalString = finalString + CleanChecks(post.section566.text);
        finalString = finalString + CleanChecks(post.section567.text);
        finalString = finalString + CleanChecks(post.section568.text);
        finalString = finalString + CleanChecks(post.section569.text);
        finalString = finalString + CleanChecks(post.section570.text);
        finalString = finalString + CleanChecks(post.section571.text);
        finalString = finalString + CleanChecks(post.section572.text);
        finalString = finalString + CleanChecks(post.section573.text);
        finalString = finalString + CleanChecks(post.section574.text);
        finalString = finalString + CleanChecks(post.section575.text);
        finalString = finalString + CleanChecks(post.section576.text);
        finalString = finalString + CleanChecks(post.section577.text);
        finalString = finalString + CleanChecks(post.section578.text);
        finalString = finalString + CleanChecks(post.section579.text);
        finalString = finalString + CleanChecks(post.section580.text);
        finalString = finalString + CleanChecks(post.section581.text);
        finalString = finalString + CleanChecks(post.section582.text);
        finalString = finalString + CleanChecks(post.section583.text);
        finalString = finalString + CleanChecks(post.section584.text);
        finalString = finalString + CleanChecks(post.section585.text);
        finalString = finalString + CleanChecks(post.section586.text);
        finalString = finalString + CleanChecks(post.section587.text);
        finalString = finalString + CleanChecks(post.section588.text);
        finalString = finalString + CleanChecks(post.section589.text);
        finalString = finalString + CleanChecks(post.section590.text);
        finalString = finalString + CleanChecks(post.section591.text);
        finalString = finalString + CleanChecks(post.section592.text);
        finalString = finalString + CleanChecks(post.section593.text);
        finalString = finalString + CleanChecks(post.section594.text);
        finalString = finalString + CleanChecks(post.section595.text);
        finalString = finalString + CleanChecks(post.section596.text);
        finalString = finalString + CleanChecks(post.section597.text);
        finalString = finalString + CleanChecks(post.section598.text);
        finalString = finalString + CleanChecks(post.section599.text);

    }
    catch (Exception ex)
    {
        return finalString;
    }

    return finalString;
}

string CleanChecks(string text)
{
    try
    {

        text= Regex.Replace(text, @"[\r\n\x00\x1a\\'""]", @"\$0");

        if (!text.Contains("<p>") && !text.Contains("</p>"))
        {
            return $"<p>{text}</p> {Environment.NewLine}";

        }

        return text + Environment.NewLine;
    }
    catch (Exception)
    {
        throw;
    }

}

List<string> GetArticles(string filePath, int startLine, int endLine)
{
    // Adjust for 0-based indexing if your line numbers are 1-based.
    // If startLine and endLine are already 0-based, remove the -1.
    int skipCount = startLine - 1;
    int takeCount = endLine - startLine + 1;

    if (skipCount < 0 || takeCount < 0)
    {
        throw new ArgumentException("Invalid line range. Start line and end line must be positive, and endLine must be greater than or equal to startLine.");
    }

    try
    {
        // Use File.ReadLines for memory efficiency with large files.
        // Skip the lines before the start of the range, then take the specified number of lines.
        return File.ReadLines(filePath)
                   .Skip(skipCount)
                   .Take(takeCount)
                   .ToList();
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine($"Error: File not found at {filePath}");
        return new List<string>(); // Return empty list on error
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        return new List<string>(); // Return empty list on error
    }
}


void ExtractFeaturedImage()
{
    // Get all subdirectories in the specified path
    var DIR2024 = @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Assets\nwp\2024";
    string[] directories = { @$"{DIR2024}\5",
                             @$"{DIR2024}\6",
                             @$"{DIR2024}\7"};

    string subpath="";
    
    // Loop through each directory
    foreach (string directory in directories)
    {
        var tempArry = directory.Split("\\");
        subpath = $@"{tempArry[tempArry.Length-2]}\{tempArry[tempArry.Length-1]}";

        Console.WriteLine(directory);
        foreach (string filePath in Directory.EnumerateFiles(directory))
        {
            Console.WriteLine($"Found file: {filePath}");
            string[] lines = File.ReadAllLines(filePath);
            string imageSource = "";

            for (int i = 0; i < lines.Length; i++)
            { 

                //check for filename
                if (lines[i].Equals("        <sv:property sv:name=\"jcr:uuid\" sv:type=\"String\">"))
                {
                    imageSource = lines[i+1].Replace("<sv:value>", "").Replace("</sv:value>", "").Trim();
                }

                //check for actual base64 string images for parsing
                if (lines[i].Contains("<sv:property sv:name=\"jcr:data\" sv:type=\"Binary\">"))
                {
                    Base64StringToJpeg(lines[i+1].Replace("<sv:value>", "").Replace("</sv:value>", ""), imageSource, subpath);
                }

            }


        }

    }

}

void Base64StringToJpeg(string base64String, string fileName,string subPath)
{
    try
    {
        string dirDestination = $@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Assets\nwp-extracted\{subPath}";

        if(!Directory.Exists(dirDestination))
        {
            Directory.CreateDirectory(dirDestination);
        }

        string filePath = $@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Assets\nwp-extracted\{subPath}\{fileName}.jpg";
        File.WriteAllBytes(filePath, Convert.FromBase64String(base64String));
    }
    catch (FormatException ex)
    {
        Console.WriteLine($"Error: Invalid Base64 string. {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}

static void ProcessAuthors(string filePath, string searchText)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"Error: File not found at {filePath}");
        return;
    }

    try
    {
        // Use File.ReadLines to read lines lazily and efficiently
        var matchingLines = File.ReadLines(filePath)
                                .Select((line, index) => new { LineText = line, LineNumber = index + 1 })
                                .Where(item => item.LineText.Contains(searchText));

        string[] authors = matchingLines.Select(x => x.LineText.Replace("author:","").Replace("\'","").Trim()).Distinct().ToArray();
        List<string> wp_users = new List<string>();
        List<string> wp_usermeta = new List<string>();

        if (matchingLines.Any())
        {
            int i = 0;
            int user_id = 6;
            foreach (var author in authors)
            {
                user_id++;
                i++;
                wp_users.Add($"INSERT INTO `wp_users` ( `ID`,`user_login`, `user_pass`, `user_nicename`, `user_email`, `user_url`, `user_registered`, `user_activation_key`, `user_status`, `display_name`) VALUES({user_id},'migrateduser-{i}', '$wp$2y$10$GveTsPNj/qlcYltZ7sctouaGAgwxGsCs83u.HXbm.XabHUDa0w/jy', 'migrateduser-{i}', 'migrated.user.{i}@newswatchplus.ph', 'https://newswatchplus-staging.azurewebsites.net/author/migrateduser-{i}', '2025-09-05 07:22:19', '', 0, '{author}');");

                wp_usermeta.Add($"INSERT INTO `wp_usermeta` (`user_id`, `meta_key`, `meta_value`) VALUES" +
                $"( {user_id}, 'nickname', 'migrateduser-{i}')," +
                $"( {user_id}, 'first_name', '{author}'),"+
                $"( {user_id}, 'last_name', ''),"+
                $"( {user_id}, 'rich_editing', 'true'),"+
                $"( {user_id}, 'syntax_highlighting', 'true'),"+
                $"( {user_id}, 'comment_shortcuts', 'false'),"+
                $"( {user_id}, 'admin_color', 'fresh'),"+
                $"( {user_id}, 'show_admin_bar_front', 'true'),"+
                "(" + user_id + ", 'wp_capabilities', 'a:1:{s:6:\"author\";b:1;}' ),"+
                $"( {user_id}, 'wp_user_level', '2');");

            }

        }
        else
        {
            Console.WriteLine($"'{searchText}' not found in the file.");
        }
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine($"Error: File not found at '{filePath}'");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
}
