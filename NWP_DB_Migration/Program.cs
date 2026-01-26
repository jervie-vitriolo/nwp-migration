using MySql.Data.MySqlClient;
using NWP_DB_Migration.Article;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static System.Net.Mime.MediaTypeNames;

internal class Program
{
    private static int SavedCount = 0;
    private static void Main(string[] args)
    {
        List<string> WP_Post_Article_InsertSql_list;
        List<string> WP_PostMeta_list;
        List<string> WP_term_relationships_list;
        List<string> WP_Post_Attachment_caption;
        
        int ErrorCount = 0;
        string[] directories = {
                                //@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\CNN new\Articles\archived\2015",
                                //@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\CNN new\Articles\archived\2016",
                                //@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\CNN new\Articles\archived\2017",
                                //@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\CNN new\Articles\archived\2018",
                                //@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\CNN new\Articles\archived\2019",
                                //@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\CNN new\Articles\archived\2020",
                                //@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\CNN new\Articles\archived\2021",
                                //@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\CNN new\Articles\archived\2022",
                                //@"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\CNN new\Articles\archived\2023",
                                @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\CNN new\Articles\archived\2024"
                                 };




        
        //Migrate Authors
        //ProcessAuthors();

        //Convert base64string to image file
        //ExtractFeaturedImage();

        //Add title starting point 
        //AddTItleMarking();

        GenerateInsertSql();



        void AddTItleMarking()
        {

            // Loop through each directory
            foreach (string rootdirectory in directories)
            {
                foreach (string dir in Directory.GetDirectories(rootdirectory))
                {
                    Console.WriteLine(dir);
                    foreach (string filePath in Directory.EnumerateFiles(dir))
                    {
                        Console.WriteLine($"Found file: {filePath}");
                        AppendSingleQoute(filePath);

                        string[] matchingLines = File.ReadAllLines(filePath);

                        //add START HERE----->
                        for (int i = 0; i < matchingLines.Count(); i++)
                        {
                            if (!matchingLines[i].Contains("'  'mgnl:")
                                && !matchingLines[i].Contains("    'author")
                                && !matchingLines[i].Contains("    'categories")
                                && !matchingLines[i].Contains("    'created")
                                && !matchingLines[i].Contains("    'imagesource")
                                && !matchingLines[i].Contains("    'jcr")
                                && !matchingLines[i].Contains("    'lead")
                                && !matchingLines[i].Contains("    'stories")
                                && !matchingLines[i].Contains("    'template")
                                && !matchingLines[i].Contains("    'title")
                                && !matchingLines[i].Contains("    'visualType")
                                && !matchingLines[i].Contains("'jcr:")
                                && !matchingLines[i].Contains("  'mgnl")
                                )
                            {
                                matchingLines[i] = matchingLines[i].Replace("'  '", "'  'START HERE----->");
                            }

                            //Tags
                            if (matchingLines[i].Contains("'mgnl:tags': ["))
                            {
                                matchingLines[i] = "    'mgnl:tags': []";
                            }

                        }

                        File.WriteAllLines(filePath, matchingLines);
                        RemoveSingleQoute(filePath);
                    }
                }           
            }
            Console.WriteLine($"Completed");
        }

        void AppendSingleQoute(string filePath)
        {
            string[] matchingLines = File.ReadAllLines(filePath);

            for (int i = 0; i < matchingLines.Count(); i++)
            {
                //if (matchingLines[i].Equals(""))
                //{
                //    continue;
                //}
                matchingLines[i] = $"'{matchingLines[i]}";
            }

            File.WriteAllLines(filePath, matchingLines);
        }

        void RemoveSingleQoute(string filePath)
        {
            string matchingLines = File.ReadAllText(filePath);

            matchingLines = matchingLines.Replace("''", "'");
            matchingLines = matchingLines.Replace("'  '", "  '");
            matchingLines = matchingLines.Replace("'    '", "    '");
            matchingLines = matchingLines.Replace("'      '", "      '");
            matchingLines = matchingLines.Replace("'        ", "        ");
            matchingLines = matchingLines.Replace("'      ", "      ");

            File.WriteAllText(filePath, matchingLines);
        }

        void  GenerateInsertSql()
        {
            WP_Post_Article_InsertSql_list = new List<string>();
            WP_PostMeta_list = new List<string>();
            WP_term_relationships_list = new List<string>();
            WP_Post_Attachment_caption = new List<string>();
            
            // Loop through each directory
            foreach (string rootdirectory in directories)
            {
                
                foreach (string dir in Directory.GetDirectories(rootdirectory))
                {
                    //test only 1 directory
                    string lastDirectoryName = Path.GetFileName(dir);
                    //if (!lastDirectoryName.Equals("2")) continue;

                    Post post;
                    Console.WriteLine(dir);
                    foreach (string filePath in Directory.EnumerateFiles(dir))
                    {
                        Console.WriteLine(filePath);

                        var matchingLines = File.ReadLines(filePath)
                                            .Select((line, index) => new { LineText = line, LineNumber = index + 1 })
                                            .Where(item => item.LineText.Contains("START HERE----->"));

                        int[] lineNumbers = matchingLines.Select(x => x.LineNumber).ToArray();

                        
                        for (int i = 0; i < lineNumbers.Length; i++)
                        {
                            post = new Post();
                            try
                            {
                                int startLine = lineNumbers[i] + 1;
                                int endLine = 0;

                                if (i == lineNumbers.Length - 1)
                                {
                                    endLine = File.ReadAllLines(filePath).Length;
                                }
                                else
                                {
                                    endLine = lineNumbers[i + 1] - 1;
                                }

                                List<string> articles = GetArticles(filePath, startLine, endLine);

                                
                                post = processPostData(articles, post);


                                if (post != null)
                                {
                                    CreatePostInsertSql(post);
                                    //GeRedirectUrl(post);
                                }
                            }
                            catch (Exception ex)
                            {
                                ErrorCount++;
                      
                                    Console.WriteLine($"Error at line {lineNumbers[i]} - {ex} - {post.title}");
                                
                                continue;
                            }

                        }


                    }

                    Console.WriteLine($"Total Error {ErrorCount}");
                    Console.WriteLine($"Total Saved {SavedCount}");
                }


            }

            Console.WriteLine($"Completed");
        }


        int GetPostMaxId()
        {
            try
            {
                string connStr = "server=nwpproduct-146b913ef7-wpdbserver.mysql.database.azure.com;user=qrdxngegwd;database=nwpproduct_146b913ef7_database;password=rgq6$jWrkQvsx3hL;";
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();

                var sql = $"select max(ID) maxId from wp_posts;";
                int id = 0;
                var wp_post = new MySqlCommand(sql, conn);
                MySqlDataReader reader = wp_post.ExecuteReader();

                while (reader.Read())
                {
                    id = reader.GetInt32("maxId");
                }

                conn.Close();

                return id+1;
            }
            catch (Exception)
            {
                //log the imagesource id if can't be found
                return 0;
            }
        }

        int GetPostMetaValue(string imagesource)
        {
            try
            {
                string connStr = "server=nwpproduct-146b913ef7-wpdbserver.mysql.database.azure.com;user=qrdxngegwd;database=nwpproduct_146b913ef7_database;password=rgq6$jWrkQvsx3hL;";
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();

                var sql = $"select ID from wp_posts where post_title='{imagesource}';";
                int id = 0;
                var wp_post = new MySqlCommand(sql, conn);
                MySqlDataReader reader = wp_post.ExecuteReader();

                while (reader.Read())
                {
                    id = reader.GetInt32("ID");
                }

                conn.Close();

                return id;
            }
            catch (Exception)
            {
                //log the imagesource id if can't be found
                return 0;
            }
        }

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
               "public string image { get; set; } \n" +
               "public string contentwidth { get; set; } \n" +
               "public string imagecaption { get; set; } \n" +
               "public string embed { get; set; } \n" +
               "public string scale { get; set; } \n" +
               "public string imagecredit { get; set; } \n" +
               "public string imagealttext { get; set; } \n" +
               "public string url { get; set; } \n" +
               "public string videocredit { get; set; } \n" +
               "public string imagelist { get; set; } \n" +
               "public string authorimage { get; set; } \n" +
               "public string customlead { get; set; } \n" +
               "public string embedcode { get; set; } \n" +
               "public string assetautoplay { get; set; } \n" +
               "public string quotation { get; set; } \n" +
               "public string imageAltText { get; set; } \n" +
               "public string videoCredit { get; set; } \n" +
               "public string assetcontrols { get; set; } \n" +
               "public string assetloop { get; set; } \n" +
               "public string mixintypes { get; set; } }\n\n\n";

                ClassInit = ClassInit + $"public section{i} section{i} = new section{i}();\n";
                XMas = XMas + $"finalString = finalString + CleanChecks(getBlock(post.title, post.section{i}.type, post.section{i}.embedcode,post.section{i}.text,post.section{i}.url,post.section{i}.embed,post.section{i}.image));\n";
            }
            
            string ClassDeclaration = $"namespace NWP_DB_Migration.Article\r\n {{ {section}  }}";

        }

        void CleanUpArticle()
        {
            // Loop through each directory
            foreach (string rootdirectory in directories)
            {
                
                foreach (string dir in Directory.GetDirectories(rootdirectory))
                {
                    Console.WriteLine(dir);
                
                    foreach (string filePath in Directory.EnumerateFiles(dir))
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
                        matchingLines = matchingLines.Replace("contentWidth", "contentwidth");
                        matchingLines = matchingLines.Replace("embedCode", "embedcode");
                        matchingLines = matchingLines.Replace("'imageCaption':", "imagecaption:");
                        matchingLines = matchingLines.Replace("authorImage", "authorimage");
                        matchingLines = matchingLines.Replace("imageCredit", "imagecredit");
                        matchingLines = matchingLines.Replace("imageList", "imagelist");
                        matchingLines = matchingLines.Replace("customLead", "customlead");
                        matchingLines = matchingLines.Replace("imagealtText", "imagealttext");


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
                        matchingLines = matchingLines.Replace("lastactivatedVersionCreated", "lastactivatedversioncreated");
                        matchingLines = matchingLines.Replace("lastactivatedVersion", "lastactivatedversion");
                    

                        File.WriteAllText(filePath, matchingLines);
                    }
                }
            }
        }

        void CreatePostInsertSql(Post post)
        {
            string image_caption_sql = string.Empty;

            var featuredImage = post.imagesource == "" ? post.embedimage : post.imagesource;
            int featuredImageId = 0;
            int PostID = GetPostMaxId();
            if (!string.IsNullOrEmpty(featuredImage))
            {
                featuredImageId = GetPostMetaValue(featuredImage);
            }
            

            string WP_Post_Article_InsertSql = $"INSERT INTO `wp_posts` ( `ID`,`post_author`, `post_date`, `post_date_gmt`, `post_content`, `post_title`, `post_excerpt`, `post_status`, `comment_status`, `ping_status`, `post_password`, `post_name`, `to_ping`, `pinged`, `post_modified`, `post_modified_gmt`, `post_content_filtered`, `post_parent`, `guid`, `menu_order`, `post_type`, `post_mime_type`, `comment_count`) " +
                                  $"VALUES({PostID} ,'{getPostAuthorID(post.author)}', '{formatDateTime(post.created)}', '{formatDateTime(post.created)}', '{mysqlStringFormat(post.Content)}', '{mysqlStringFormat(post.title)}', '{mysqlStringFormat(post.caption)}', '{getPostStatus(post)}', 'closed', 'open','', '{mysqlStringFormat(GenerateWordPressSlug(post.title))}', '', '', '{formatDateTime(post.lastmodified)}', '{formatDateTime(post.lastmodified)}', '', 0, 'https://www.newswatchplus.ph/?p=', 0, 'post', '', 0);";

            
            string WP_PostMeta = $"INSERT INTO `wp_postmeta` ( `post_id`, `meta_key`, `meta_value`) VALUES( {PostID}, '_thumbnail_id', '{featuredImageId}');";

            //Category
            string WP_term_relationships = $"INSERT INTO wp_term_relationships(OBJECT_ID,TERM_TAXONOMY_ID,TERM_ORDER) VALUES({PostID},{getCategoryId(post.categories)},0);";

            //CNN
             string WP_term_relationships_CNN_category = $"INSERT INTO wp_term_relationships(OBJECT_ID,TERM_TAXONOMY_ID,TERM_ORDER) VALUES({PostID},505,0);";
            //WP_term_relationships_CNN_category = "";

            //Tag
            string  WP_term_relationships_CNN_tag = $"INSERT INTO wp_term_relationships(OBJECT_ID,TERM_TAXONOMY_ID,TERM_ORDER) VALUES({PostID},500,0);";
            //WP_term_relationships_CNN_tag = "";

            if (!string.IsNullOrEmpty(post.caption ))
            {
                image_caption_sql = $"update wp_posts set post_excerpt='{mysqlStringFormat(post.caption)}'  where post_title ='{featuredImage}' and post_type='attachment';";
                WP_Post_Attachment_caption.Add(image_caption_sql);
            }
            


            WP_Post_Article_InsertSql_list.Add(WP_Post_Article_InsertSql);
            WP_PostMeta_list.Add(WP_PostMeta);
            WP_term_relationships_list.Add(WP_term_relationships);
            
            if (featuredImageId > 0)
            {
                SaveDataToDatabase(WP_Post_Article_InsertSql, WP_PostMeta, WP_term_relationships, image_caption_sql, WP_term_relationships_CNN_category, WP_term_relationships_CNN_tag);
            }

            
        }


        int getCategoryId(string categoryId)
        {
            if (categoryId == null)
            {
                //Console.WriteLine($"UNCATEGORISED");
                return 1;
            }
            else
            {
                NWPCategoryList NWPCategoryList = new NWPCategoryList();
                var nwpCategory = NWPCategoryList.GetCategory().FirstOrDefault(s => s.ID.ToUpper().Trim().Equals(categoryId.ToUpper().Trim()));

                if (nwpCategory != null)
                {
                    CategoryList CategoryList = new CategoryList();


                    try
                    {
                        CategoryList cat =  CategoryList.GetCategoryList().FirstOrDefault(s => s.Category.ToUpper().Trim().Equals(nwpCategory.Category.ToUpper().Trim()));

                        if(cat != null)
                        {
                            if (cat.ID != 0)
                            {
                                //Console.WriteLine($"{nwpCategory.Category}");
                                return cat.ID;
                            }
                        }


                        return 1;

                    }
                    catch (Exception)
                    {

                        return 1;
                    }
                    
                }
                //Console.WriteLine($"NEWS");
                return 36;
            }
        }

        string mysqlStringFormat(string text)
        {
            if (text != null)
            {
                return Regex.Replace(text, @"[\r\n\x00\x1a\\'""]", @"\$0");
            }

            return string.Empty;
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
            if (name == null || name==string.Empty)
            {
                return 4446; //4446- CNN Philippines Staff //1332 - Newswatch plus 
            }
            else
            {
                AuthorsList AuthorsList = new AuthorsList();
                var ID = AuthorsList.GetAuthors(name);
                return ID;
            }

        }

        string getBlock(string title, string type, string embedcode,string text, string url, string embed,string image)
        {

            switch (type)
            {
                case "text":
                    return text;
                case "embedcode":
                    return embedcode;
                case "embed":
                    return embed;
                case "externalLink":
                    return url;
                case "image":
                    return $"<figure class=\"wp-block-image alignwide size-full\"><img src=\"/wp-content/uploads/2025/10/{image}.jpg\" alt=\"\" /></figure>";
                default:
                    if(type != null)
                    {
                       Console.WriteLine($"{title} : BLOCK TYPE NOT FOUND: {type}");
                    }
                    return string.Empty;
            }

        }

        #region getPostContent


        string getPostContent(Post post)
        {
            string finalString = string.Empty;

            try
            {
                if (post.visualtype == "video" && post.embedsource.Contains("https://www.youtube.com/embed"))
                {
                    finalString = post.embedsource.Replace("width=\"560\"", "width=\"800\"").Replace("height=\"315\"", "height=\"500\"");
                }
                else if(post.visualtype == "embed")
                {
                    finalString = post.embedsource;
                }

                finalString = finalString + CleanChecks(getBlock(post.title, post.section0.type, post.section0.embedcode, post.section0.text, post.section0.url, post.section0.embed, post.section0.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section1.type, post.section1.embedcode, post.section1.text, post.section1.url, post.section1.embed, post.section1.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section2.type, post.section2.embedcode, post.section2.text, post.section2.url, post.section2.embed, post.section2.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section3.type, post.section3.embedcode, post.section3.text, post.section3.url, post.section3.embed, post.section3.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section4.type, post.section4.embedcode, post.section4.text, post.section4.url, post.section4.embed, post.section4.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section5.type, post.section5.embedcode, post.section5.text, post.section5.url, post.section5.embed, post.section5.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section6.type, post.section6.embedcode, post.section6.text, post.section6.url, post.section6.embed, post.section6.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section7.type, post.section7.embedcode, post.section7.text, post.section7.url, post.section7.embed, post.section7.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section8.type, post.section8.embedcode, post.section8.text, post.section8.url, post.section8.embed, post.section8.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section9.type, post.section9.embedcode, post.section9.text, post.section9.url, post.section9.embed, post.section9.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section10.type, post.section10.embedcode, post.section10.text, post.section10.url, post.section10.embed, post.section10.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section11.type, post.section11.embedcode, post.section11.text, post.section11.url, post.section11.embed, post.section11.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section12.type, post.section12.embedcode, post.section12.text, post.section12.url, post.section12.embed, post.section12.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section13.type, post.section13.embedcode, post.section13.text, post.section13.url, post.section13.embed, post.section13.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section14.type, post.section14.embedcode, post.section14.text, post.section14.url, post.section14.embed, post.section14.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section15.type, post.section15.embedcode, post.section15.text, post.section15.url, post.section15.embed, post.section15.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section16.type, post.section16.embedcode, post.section16.text, post.section16.url, post.section16.embed, post.section16.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section17.type, post.section17.embedcode, post.section17.text, post.section17.url, post.section17.embed, post.section17.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section18.type, post.section18.embedcode, post.section18.text, post.section18.url, post.section18.embed, post.section18.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section19.type, post.section19.embedcode, post.section19.text, post.section19.url, post.section19.embed, post.section19.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section20.type, post.section20.embedcode, post.section20.text, post.section20.url, post.section20.embed, post.section20.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section21.type, post.section21.embedcode, post.section21.text, post.section21.url, post.section21.embed, post.section21.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section22.type, post.section22.embedcode, post.section22.text, post.section22.url, post.section22.embed, post.section22.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section23.type, post.section23.embedcode, post.section23.text, post.section23.url, post.section23.embed, post.section23.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section24.type, post.section24.embedcode, post.section24.text, post.section24.url, post.section24.embed, post.section24.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section25.type, post.section25.embedcode, post.section25.text, post.section25.url, post.section25.embed, post.section25.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section26.type, post.section26.embedcode, post.section26.text, post.section26.url, post.section26.embed, post.section26.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section27.type, post.section27.embedcode, post.section27.text, post.section27.url, post.section27.embed, post.section27.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section28.type, post.section28.embedcode, post.section28.text, post.section28.url, post.section28.embed, post.section28.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section29.type, post.section29.embedcode, post.section29.text, post.section29.url, post.section29.embed, post.section29.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section30.type, post.section30.embedcode, post.section30.text, post.section30.url, post.section30.embed, post.section30.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section31.type, post.section31.embedcode, post.section31.text, post.section31.url, post.section31.embed, post.section31.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section32.type, post.section32.embedcode, post.section32.text, post.section32.url, post.section32.embed, post.section32.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section33.type, post.section33.embedcode, post.section33.text, post.section33.url, post.section33.embed, post.section33.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section34.type, post.section34.embedcode, post.section34.text, post.section34.url, post.section34.embed, post.section34.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section35.type, post.section35.embedcode, post.section35.text, post.section35.url, post.section35.embed, post.section35.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section36.type, post.section36.embedcode, post.section36.text, post.section36.url, post.section36.embed, post.section36.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section37.type, post.section37.embedcode, post.section37.text, post.section37.url, post.section37.embed, post.section37.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section38.type, post.section38.embedcode, post.section38.text, post.section38.url, post.section38.embed, post.section38.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section39.type, post.section39.embedcode, post.section39.text, post.section39.url, post.section39.embed, post.section39.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section40.type, post.section40.embedcode, post.section40.text, post.section40.url, post.section40.embed, post.section40.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section41.type, post.section41.embedcode, post.section41.text, post.section41.url, post.section41.embed, post.section41.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section42.type, post.section42.embedcode, post.section42.text, post.section42.url, post.section42.embed, post.section42.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section43.type, post.section43.embedcode, post.section43.text, post.section43.url, post.section43.embed, post.section43.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section44.type, post.section44.embedcode, post.section44.text, post.section44.url, post.section44.embed, post.section44.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section45.type, post.section45.embedcode, post.section45.text, post.section45.url, post.section45.embed, post.section45.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section46.type, post.section46.embedcode, post.section46.text, post.section46.url, post.section46.embed, post.section46.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section47.type, post.section47.embedcode, post.section47.text, post.section47.url, post.section47.embed, post.section47.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section48.type, post.section48.embedcode, post.section48.text, post.section48.url, post.section48.embed, post.section48.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section49.type, post.section49.embedcode, post.section49.text, post.section49.url, post.section49.embed, post.section49.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section50.type, post.section50.embedcode, post.section50.text, post.section50.url, post.section50.embed, post.section50.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section51.type, post.section51.embedcode, post.section51.text, post.section51.url, post.section51.embed, post.section51.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section52.type, post.section52.embedcode, post.section52.text, post.section52.url, post.section52.embed, post.section52.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section53.type, post.section53.embedcode, post.section53.text, post.section53.url, post.section53.embed, post.section53.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section54.type, post.section54.embedcode, post.section54.text, post.section54.url, post.section54.embed, post.section54.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section55.type, post.section55.embedcode, post.section55.text, post.section55.url, post.section55.embed, post.section55.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section56.type, post.section56.embedcode, post.section56.text, post.section56.url, post.section56.embed, post.section56.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section57.type, post.section57.embedcode, post.section57.text, post.section57.url, post.section57.embed, post.section57.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section58.type, post.section58.embedcode, post.section58.text, post.section58.url, post.section58.embed, post.section58.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section59.type, post.section59.embedcode, post.section59.text, post.section59.url, post.section59.embed, post.section59.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section60.type, post.section60.embedcode, post.section60.text, post.section60.url, post.section60.embed, post.section60.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section61.type, post.section61.embedcode, post.section61.text, post.section61.url, post.section61.embed, post.section61.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section62.type, post.section62.embedcode, post.section62.text, post.section62.url, post.section62.embed, post.section62.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section63.type, post.section63.embedcode, post.section63.text, post.section63.url, post.section63.embed, post.section63.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section64.type, post.section64.embedcode, post.section64.text, post.section64.url, post.section64.embed, post.section64.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section65.type, post.section65.embedcode, post.section65.text, post.section65.url, post.section65.embed, post.section65.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section66.type, post.section66.embedcode, post.section66.text, post.section66.url, post.section66.embed, post.section66.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section67.type, post.section67.embedcode, post.section67.text, post.section67.url, post.section67.embed, post.section67.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section68.type, post.section68.embedcode, post.section68.text, post.section68.url, post.section68.embed, post.section68.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section69.type, post.section69.embedcode, post.section69.text, post.section69.url, post.section69.embed, post.section69.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section70.type, post.section70.embedcode, post.section70.text, post.section70.url, post.section70.embed, post.section70.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section71.type, post.section71.embedcode, post.section71.text, post.section71.url, post.section71.embed, post.section71.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section72.type, post.section72.embedcode, post.section72.text, post.section72.url, post.section72.embed, post.section72.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section73.type, post.section73.embedcode, post.section73.text, post.section73.url, post.section73.embed, post.section73.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section74.type, post.section74.embedcode, post.section74.text, post.section74.url, post.section74.embed, post.section74.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section75.type, post.section75.embedcode, post.section75.text, post.section75.url, post.section75.embed, post.section75.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section76.type, post.section76.embedcode, post.section76.text, post.section76.url, post.section76.embed, post.section76.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section77.type, post.section77.embedcode, post.section77.text, post.section77.url, post.section77.embed, post.section77.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section78.type, post.section78.embedcode, post.section78.text, post.section78.url, post.section78.embed, post.section78.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section79.type, post.section79.embedcode, post.section79.text, post.section79.url, post.section79.embed, post.section79.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section80.type, post.section80.embedcode, post.section80.text, post.section80.url, post.section80.embed, post.section80.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section81.type, post.section81.embedcode, post.section81.text, post.section81.url, post.section81.embed, post.section81.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section82.type, post.section82.embedcode, post.section82.text, post.section82.url, post.section82.embed, post.section82.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section83.type, post.section83.embedcode, post.section83.text, post.section83.url, post.section83.embed, post.section83.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section84.type, post.section84.embedcode, post.section84.text, post.section84.url, post.section84.embed, post.section84.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section85.type, post.section85.embedcode, post.section85.text, post.section85.url, post.section85.embed, post.section85.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section86.type, post.section86.embedcode, post.section86.text, post.section86.url, post.section86.embed, post.section86.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section87.type, post.section87.embedcode, post.section87.text, post.section87.url, post.section87.embed, post.section87.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section88.type, post.section88.embedcode, post.section88.text, post.section88.url, post.section88.embed, post.section88.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section89.type, post.section89.embedcode, post.section89.text, post.section89.url, post.section89.embed, post.section89.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section90.type, post.section90.embedcode, post.section90.text, post.section90.url, post.section90.embed, post.section90.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section91.type, post.section91.embedcode, post.section91.text, post.section91.url, post.section91.embed, post.section91.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section92.type, post.section92.embedcode, post.section92.text, post.section92.url, post.section92.embed, post.section92.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section93.type, post.section93.embedcode, post.section93.text, post.section93.url, post.section93.embed, post.section93.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section94.type, post.section94.embedcode, post.section94.text, post.section94.url, post.section94.embed, post.section94.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section95.type, post.section95.embedcode, post.section95.text, post.section95.url, post.section95.embed, post.section95.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section96.type, post.section96.embedcode, post.section96.text, post.section96.url, post.section96.embed, post.section96.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section97.type, post.section97.embedcode, post.section97.text, post.section97.url, post.section97.embed, post.section97.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section98.type, post.section98.embedcode, post.section98.text, post.section98.url, post.section98.embed, post.section98.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section99.type, post.section99.embedcode, post.section99.text, post.section99.url, post.section99.embed, post.section99.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section100.type, post.section100.embedcode, post.section100.text, post.section100.url, post.section100.embed, post.section100.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section101.type, post.section101.embedcode, post.section101.text, post.section101.url, post.section101.embed, post.section101.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section102.type, post.section102.embedcode, post.section102.text, post.section102.url, post.section102.embed, post.section102.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section103.type, post.section103.embedcode, post.section103.text, post.section103.url, post.section103.embed, post.section103.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section104.type, post.section104.embedcode, post.section104.text, post.section104.url, post.section104.embed, post.section104.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section105.type, post.section105.embedcode, post.section105.text, post.section105.url, post.section105.embed, post.section105.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section106.type, post.section106.embedcode, post.section106.text, post.section106.url, post.section106.embed, post.section106.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section107.type, post.section107.embedcode, post.section107.text, post.section107.url, post.section107.embed, post.section107.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section108.type, post.section108.embedcode, post.section108.text, post.section108.url, post.section108.embed, post.section108.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section109.type, post.section109.embedcode, post.section109.text, post.section109.url, post.section109.embed, post.section109.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section110.type, post.section110.embedcode, post.section110.text, post.section110.url, post.section110.embed, post.section110.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section111.type, post.section111.embedcode, post.section111.text, post.section111.url, post.section111.embed, post.section111.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section112.type, post.section112.embedcode, post.section112.text, post.section112.url, post.section112.embed, post.section112.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section113.type, post.section113.embedcode, post.section113.text, post.section113.url, post.section113.embed, post.section113.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section114.type, post.section114.embedcode, post.section114.text, post.section114.url, post.section114.embed, post.section114.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section115.type, post.section115.embedcode, post.section115.text, post.section115.url, post.section115.embed, post.section115.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section116.type, post.section116.embedcode, post.section116.text, post.section116.url, post.section116.embed, post.section116.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section117.type, post.section117.embedcode, post.section117.text, post.section117.url, post.section117.embed, post.section117.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section118.type, post.section118.embedcode, post.section118.text, post.section118.url, post.section118.embed, post.section118.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section119.type, post.section119.embedcode, post.section119.text, post.section119.url, post.section119.embed, post.section119.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section120.type, post.section120.embedcode, post.section120.text, post.section120.url, post.section120.embed, post.section120.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section121.type, post.section121.embedcode, post.section121.text, post.section121.url, post.section121.embed, post.section121.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section122.type, post.section122.embedcode, post.section122.text, post.section122.url, post.section122.embed, post.section122.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section123.type, post.section123.embedcode, post.section123.text, post.section123.url, post.section123.embed, post.section123.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section124.type, post.section124.embedcode, post.section124.text, post.section124.url, post.section124.embed, post.section124.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section125.type, post.section125.embedcode, post.section125.text, post.section125.url, post.section125.embed, post.section125.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section126.type, post.section126.embedcode, post.section126.text, post.section126.url, post.section126.embed, post.section126.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section127.type, post.section127.embedcode, post.section127.text, post.section127.url, post.section127.embed, post.section127.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section128.type, post.section128.embedcode, post.section128.text, post.section128.url, post.section128.embed, post.section128.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section129.type, post.section129.embedcode, post.section129.text, post.section129.url, post.section129.embed, post.section129.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section130.type, post.section130.embedcode, post.section130.text, post.section130.url, post.section130.embed, post.section130.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section131.type, post.section131.embedcode, post.section131.text, post.section131.url, post.section131.embed, post.section131.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section132.type, post.section132.embedcode, post.section132.text, post.section132.url, post.section132.embed, post.section132.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section133.type, post.section133.embedcode, post.section133.text, post.section133.url, post.section133.embed, post.section133.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section134.type, post.section134.embedcode, post.section134.text, post.section134.url, post.section134.embed, post.section134.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section135.type, post.section135.embedcode, post.section135.text, post.section135.url, post.section135.embed, post.section135.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section136.type, post.section136.embedcode, post.section136.text, post.section136.url, post.section136.embed, post.section136.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section137.type, post.section137.embedcode, post.section137.text, post.section137.url, post.section137.embed, post.section137.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section138.type, post.section138.embedcode, post.section138.text, post.section138.url, post.section138.embed, post.section138.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section139.type, post.section139.embedcode, post.section139.text, post.section139.url, post.section139.embed, post.section139.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section140.type, post.section140.embedcode, post.section140.text, post.section140.url, post.section140.embed, post.section140.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section141.type, post.section141.embedcode, post.section141.text, post.section141.url, post.section141.embed, post.section141.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section142.type, post.section142.embedcode, post.section142.text, post.section142.url, post.section142.embed, post.section142.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section143.type, post.section143.embedcode, post.section143.text, post.section143.url, post.section143.embed, post.section143.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section144.type, post.section144.embedcode, post.section144.text, post.section144.url, post.section144.embed, post.section144.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section145.type, post.section145.embedcode, post.section145.text, post.section145.url, post.section145.embed, post.section145.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section146.type, post.section146.embedcode, post.section146.text, post.section146.url, post.section146.embed, post.section146.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section147.type, post.section147.embedcode, post.section147.text, post.section147.url, post.section147.embed, post.section147.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section148.type, post.section148.embedcode, post.section148.text, post.section148.url, post.section148.embed, post.section148.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section149.type, post.section149.embedcode, post.section149.text, post.section149.url, post.section149.embed, post.section149.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section150.type, post.section150.embedcode, post.section150.text, post.section150.url, post.section150.embed, post.section150.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section151.type, post.section151.embedcode, post.section151.text, post.section151.url, post.section151.embed, post.section151.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section152.type, post.section152.embedcode, post.section152.text, post.section152.url, post.section152.embed, post.section152.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section153.type, post.section153.embedcode, post.section153.text, post.section153.url, post.section153.embed, post.section153.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section154.type, post.section154.embedcode, post.section154.text, post.section154.url, post.section154.embed, post.section154.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section155.type, post.section155.embedcode, post.section155.text, post.section155.url, post.section155.embed, post.section155.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section156.type, post.section156.embedcode, post.section156.text, post.section156.url, post.section156.embed, post.section156.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section157.type, post.section157.embedcode, post.section157.text, post.section157.url, post.section157.embed, post.section157.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section158.type, post.section158.embedcode, post.section158.text, post.section158.url, post.section158.embed, post.section158.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section159.type, post.section159.embedcode, post.section159.text, post.section159.url, post.section159.embed, post.section159.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section160.type, post.section160.embedcode, post.section160.text, post.section160.url, post.section160.embed, post.section160.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section161.type, post.section161.embedcode, post.section161.text, post.section161.url, post.section161.embed, post.section161.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section162.type, post.section162.embedcode, post.section162.text, post.section162.url, post.section162.embed, post.section162.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section163.type, post.section163.embedcode, post.section163.text, post.section163.url, post.section163.embed, post.section163.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section164.type, post.section164.embedcode, post.section164.text, post.section164.url, post.section164.embed, post.section164.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section165.type, post.section165.embedcode, post.section165.text, post.section165.url, post.section165.embed, post.section165.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section166.type, post.section166.embedcode, post.section166.text, post.section166.url, post.section166.embed, post.section166.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section167.type, post.section167.embedcode, post.section167.text, post.section167.url, post.section167.embed, post.section167.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section168.type, post.section168.embedcode, post.section168.text, post.section168.url, post.section168.embed, post.section168.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section169.type, post.section169.embedcode, post.section169.text, post.section169.url, post.section169.embed, post.section169.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section170.type, post.section170.embedcode, post.section170.text, post.section170.url, post.section170.embed, post.section170.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section171.type, post.section171.embedcode, post.section171.text, post.section171.url, post.section171.embed, post.section171.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section172.type, post.section172.embedcode, post.section172.text, post.section172.url, post.section172.embed, post.section172.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section173.type, post.section173.embedcode, post.section173.text, post.section173.url, post.section173.embed, post.section173.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section174.type, post.section174.embedcode, post.section174.text, post.section174.url, post.section174.embed, post.section174.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section175.type, post.section175.embedcode, post.section175.text, post.section175.url, post.section175.embed, post.section175.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section176.type, post.section176.embedcode, post.section176.text, post.section176.url, post.section176.embed, post.section176.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section177.type, post.section177.embedcode, post.section177.text, post.section177.url, post.section177.embed, post.section177.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section178.type, post.section178.embedcode, post.section178.text, post.section178.url, post.section178.embed, post.section178.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section179.type, post.section179.embedcode, post.section179.text, post.section179.url, post.section179.embed, post.section179.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section180.type, post.section180.embedcode, post.section180.text, post.section180.url, post.section180.embed, post.section180.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section181.type, post.section181.embedcode, post.section181.text, post.section181.url, post.section181.embed, post.section181.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section182.type, post.section182.embedcode, post.section182.text, post.section182.url, post.section182.embed, post.section182.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section183.type, post.section183.embedcode, post.section183.text, post.section183.url, post.section183.embed, post.section183.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section184.type, post.section184.embedcode, post.section184.text, post.section184.url, post.section184.embed, post.section184.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section185.type, post.section185.embedcode, post.section185.text, post.section185.url, post.section185.embed, post.section185.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section186.type, post.section186.embedcode, post.section186.text, post.section186.url, post.section186.embed, post.section186.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section187.type, post.section187.embedcode, post.section187.text, post.section187.url, post.section187.embed, post.section187.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section188.type, post.section188.embedcode, post.section188.text, post.section188.url, post.section188.embed, post.section188.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section189.type, post.section189.embedcode, post.section189.text, post.section189.url, post.section189.embed, post.section189.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section190.type, post.section190.embedcode, post.section190.text, post.section190.url, post.section190.embed, post.section190.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section191.type, post.section191.embedcode, post.section191.text, post.section191.url, post.section191.embed, post.section191.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section192.type, post.section192.embedcode, post.section192.text, post.section192.url, post.section192.embed, post.section192.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section193.type, post.section193.embedcode, post.section193.text, post.section193.url, post.section193.embed, post.section193.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section194.type, post.section194.embedcode, post.section194.text, post.section194.url, post.section194.embed, post.section194.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section195.type, post.section195.embedcode, post.section195.text, post.section195.url, post.section195.embed, post.section195.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section196.type, post.section196.embedcode, post.section196.text, post.section196.url, post.section196.embed, post.section196.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section197.type, post.section197.embedcode, post.section197.text, post.section197.url, post.section197.embed, post.section197.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section198.type, post.section198.embedcode, post.section198.text, post.section198.url, post.section198.embed, post.section198.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section199.type, post.section199.embedcode, post.section199.text, post.section199.url, post.section199.embed, post.section199.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section200.type, post.section200.embedcode, post.section200.text, post.section200.url, post.section200.embed, post.section200.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section201.type, post.section201.embedcode, post.section201.text, post.section201.url, post.section201.embed, post.section201.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section202.type, post.section202.embedcode, post.section202.text, post.section202.url, post.section202.embed, post.section202.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section203.type, post.section203.embedcode, post.section203.text, post.section203.url, post.section203.embed, post.section203.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section204.type, post.section204.embedcode, post.section204.text, post.section204.url, post.section204.embed, post.section204.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section205.type, post.section205.embedcode, post.section205.text, post.section205.url, post.section205.embed, post.section205.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section206.type, post.section206.embedcode, post.section206.text, post.section206.url, post.section206.embed, post.section206.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section207.type, post.section207.embedcode, post.section207.text, post.section207.url, post.section207.embed, post.section207.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section208.type, post.section208.embedcode, post.section208.text, post.section208.url, post.section208.embed, post.section208.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section209.type, post.section209.embedcode, post.section209.text, post.section209.url, post.section209.embed, post.section209.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section210.type, post.section210.embedcode, post.section210.text, post.section210.url, post.section210.embed, post.section210.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section211.type, post.section211.embedcode, post.section211.text, post.section211.url, post.section211.embed, post.section211.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section212.type, post.section212.embedcode, post.section212.text, post.section212.url, post.section212.embed, post.section212.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section213.type, post.section213.embedcode, post.section213.text, post.section213.url, post.section213.embed, post.section213.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section214.type, post.section214.embedcode, post.section214.text, post.section214.url, post.section214.embed, post.section214.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section215.type, post.section215.embedcode, post.section215.text, post.section215.url, post.section215.embed, post.section215.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section216.type, post.section216.embedcode, post.section216.text, post.section216.url, post.section216.embed, post.section216.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section217.type, post.section217.embedcode, post.section217.text, post.section217.url, post.section217.embed, post.section217.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section218.type, post.section218.embedcode, post.section218.text, post.section218.url, post.section218.embed, post.section218.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section219.type, post.section219.embedcode, post.section219.text, post.section219.url, post.section219.embed, post.section219.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section220.type, post.section220.embedcode, post.section220.text, post.section220.url, post.section220.embed, post.section220.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section221.type, post.section221.embedcode, post.section221.text, post.section221.url, post.section221.embed, post.section221.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section222.type, post.section222.embedcode, post.section222.text, post.section222.url, post.section222.embed, post.section222.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section223.type, post.section223.embedcode, post.section223.text, post.section223.url, post.section223.embed, post.section223.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section224.type, post.section224.embedcode, post.section224.text, post.section224.url, post.section224.embed, post.section224.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section225.type, post.section225.embedcode, post.section225.text, post.section225.url, post.section225.embed, post.section225.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section226.type, post.section226.embedcode, post.section226.text, post.section226.url, post.section226.embed, post.section226.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section227.type, post.section227.embedcode, post.section227.text, post.section227.url, post.section227.embed, post.section227.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section228.type, post.section228.embedcode, post.section228.text, post.section228.url, post.section228.embed, post.section228.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section229.type, post.section229.embedcode, post.section229.text, post.section229.url, post.section229.embed, post.section229.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section230.type, post.section230.embedcode, post.section230.text, post.section230.url, post.section230.embed, post.section230.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section231.type, post.section231.embedcode, post.section231.text, post.section231.url, post.section231.embed, post.section231.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section232.type, post.section232.embedcode, post.section232.text, post.section232.url, post.section232.embed, post.section232.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section233.type, post.section233.embedcode, post.section233.text, post.section233.url, post.section233.embed, post.section233.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section234.type, post.section234.embedcode, post.section234.text, post.section234.url, post.section234.embed, post.section234.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section235.type, post.section235.embedcode, post.section235.text, post.section235.url, post.section235.embed, post.section235.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section236.type, post.section236.embedcode, post.section236.text, post.section236.url, post.section236.embed, post.section236.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section237.type, post.section237.embedcode, post.section237.text, post.section237.url, post.section237.embed, post.section237.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section238.type, post.section238.embedcode, post.section238.text, post.section238.url, post.section238.embed, post.section238.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section239.type, post.section239.embedcode, post.section239.text, post.section239.url, post.section239.embed, post.section239.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section240.type, post.section240.embedcode, post.section240.text, post.section240.url, post.section240.embed, post.section240.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section241.type, post.section241.embedcode, post.section241.text, post.section241.url, post.section241.embed, post.section241.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section242.type, post.section242.embedcode, post.section242.text, post.section242.url, post.section242.embed, post.section242.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section243.type, post.section243.embedcode, post.section243.text, post.section243.url, post.section243.embed, post.section243.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section244.type, post.section244.embedcode, post.section244.text, post.section244.url, post.section244.embed, post.section244.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section245.type, post.section245.embedcode, post.section245.text, post.section245.url, post.section245.embed, post.section245.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section246.type, post.section246.embedcode, post.section246.text, post.section246.url, post.section246.embed, post.section246.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section247.type, post.section247.embedcode, post.section247.text, post.section247.url, post.section247.embed, post.section247.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section248.type, post.section248.embedcode, post.section248.text, post.section248.url, post.section248.embed, post.section248.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section249.type, post.section249.embedcode, post.section249.text, post.section249.url, post.section249.embed, post.section249.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section250.type, post.section250.embedcode, post.section250.text, post.section250.url, post.section250.embed, post.section250.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section251.type, post.section251.embedcode, post.section251.text, post.section251.url, post.section251.embed, post.section251.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section252.type, post.section252.embedcode, post.section252.text, post.section252.url, post.section252.embed, post.section252.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section253.type, post.section253.embedcode, post.section253.text, post.section253.url, post.section253.embed, post.section253.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section254.type, post.section254.embedcode, post.section254.text, post.section254.url, post.section254.embed, post.section254.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section255.type, post.section255.embedcode, post.section255.text, post.section255.url, post.section255.embed, post.section255.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section256.type, post.section256.embedcode, post.section256.text, post.section256.url, post.section256.embed, post.section256.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section257.type, post.section257.embedcode, post.section257.text, post.section257.url, post.section257.embed, post.section257.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section258.type, post.section258.embedcode, post.section258.text, post.section258.url, post.section258.embed, post.section258.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section259.type, post.section259.embedcode, post.section259.text, post.section259.url, post.section259.embed, post.section259.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section260.type, post.section260.embedcode, post.section260.text, post.section260.url, post.section260.embed, post.section260.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section261.type, post.section261.embedcode, post.section261.text, post.section261.url, post.section261.embed, post.section261.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section262.type, post.section262.embedcode, post.section262.text, post.section262.url, post.section262.embed, post.section262.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section263.type, post.section263.embedcode, post.section263.text, post.section263.url, post.section263.embed, post.section263.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section264.type, post.section264.embedcode, post.section264.text, post.section264.url, post.section264.embed, post.section264.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section265.type, post.section265.embedcode, post.section265.text, post.section265.url, post.section265.embed, post.section265.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section266.type, post.section266.embedcode, post.section266.text, post.section266.url, post.section266.embed, post.section266.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section267.type, post.section267.embedcode, post.section267.text, post.section267.url, post.section267.embed, post.section267.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section268.type, post.section268.embedcode, post.section268.text, post.section268.url, post.section268.embed, post.section268.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section269.type, post.section269.embedcode, post.section269.text, post.section269.url, post.section269.embed, post.section269.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section270.type, post.section270.embedcode, post.section270.text, post.section270.url, post.section270.embed, post.section270.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section271.type, post.section271.embedcode, post.section271.text, post.section271.url, post.section271.embed, post.section271.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section272.type, post.section272.embedcode, post.section272.text, post.section272.url, post.section272.embed, post.section272.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section273.type, post.section273.embedcode, post.section273.text, post.section273.url, post.section273.embed, post.section273.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section274.type, post.section274.embedcode, post.section274.text, post.section274.url, post.section274.embed, post.section274.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section275.type, post.section275.embedcode, post.section275.text, post.section275.url, post.section275.embed, post.section275.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section276.type, post.section276.embedcode, post.section276.text, post.section276.url, post.section276.embed, post.section276.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section277.type, post.section277.embedcode, post.section277.text, post.section277.url, post.section277.embed, post.section277.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section278.type, post.section278.embedcode, post.section278.text, post.section278.url, post.section278.embed, post.section278.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section279.type, post.section279.embedcode, post.section279.text, post.section279.url, post.section279.embed, post.section279.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section280.type, post.section280.embedcode, post.section280.text, post.section280.url, post.section280.embed, post.section280.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section281.type, post.section281.embedcode, post.section281.text, post.section281.url, post.section281.embed, post.section281.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section282.type, post.section282.embedcode, post.section282.text, post.section282.url, post.section282.embed, post.section282.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section283.type, post.section283.embedcode, post.section283.text, post.section283.url, post.section283.embed, post.section283.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section284.type, post.section284.embedcode, post.section284.text, post.section284.url, post.section284.embed, post.section284.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section285.type, post.section285.embedcode, post.section285.text, post.section285.url, post.section285.embed, post.section285.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section286.type, post.section286.embedcode, post.section286.text, post.section286.url, post.section286.embed, post.section286.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section287.type, post.section287.embedcode, post.section287.text, post.section287.url, post.section287.embed, post.section287.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section288.type, post.section288.embedcode, post.section288.text, post.section288.url, post.section288.embed, post.section288.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section289.type, post.section289.embedcode, post.section289.text, post.section289.url, post.section289.embed, post.section289.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section290.type, post.section290.embedcode, post.section290.text, post.section290.url, post.section290.embed, post.section290.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section291.type, post.section291.embedcode, post.section291.text, post.section291.url, post.section291.embed, post.section291.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section292.type, post.section292.embedcode, post.section292.text, post.section292.url, post.section292.embed, post.section292.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section293.type, post.section293.embedcode, post.section293.text, post.section293.url, post.section293.embed, post.section293.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section294.type, post.section294.embedcode, post.section294.text, post.section294.url, post.section294.embed, post.section294.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section295.type, post.section295.embedcode, post.section295.text, post.section295.url, post.section295.embed, post.section295.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section296.type, post.section296.embedcode, post.section296.text, post.section296.url, post.section296.embed, post.section296.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section297.type, post.section297.embedcode, post.section297.text, post.section297.url, post.section297.embed, post.section297.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section298.type, post.section298.embedcode, post.section298.text, post.section298.url, post.section298.embed, post.section298.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section299.type, post.section299.embedcode, post.section299.text, post.section299.url, post.section299.embed, post.section299.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section300.type, post.section300.embedcode, post.section300.text, post.section300.url, post.section300.embed, post.section300.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section301.type, post.section301.embedcode, post.section301.text, post.section301.url, post.section301.embed, post.section301.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section302.type, post.section302.embedcode, post.section302.text, post.section302.url, post.section302.embed, post.section302.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section303.type, post.section303.embedcode, post.section303.text, post.section303.url, post.section303.embed, post.section303.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section304.type, post.section304.embedcode, post.section304.text, post.section304.url, post.section304.embed, post.section304.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section305.type, post.section305.embedcode, post.section305.text, post.section305.url, post.section305.embed, post.section305.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section306.type, post.section306.embedcode, post.section306.text, post.section306.url, post.section306.embed, post.section306.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section307.type, post.section307.embedcode, post.section307.text, post.section307.url, post.section307.embed, post.section307.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section308.type, post.section308.embedcode, post.section308.text, post.section308.url, post.section308.embed, post.section308.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section309.type, post.section309.embedcode, post.section309.text, post.section309.url, post.section309.embed, post.section309.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section310.type, post.section310.embedcode, post.section310.text, post.section310.url, post.section310.embed, post.section310.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section311.type, post.section311.embedcode, post.section311.text, post.section311.url, post.section311.embed, post.section311.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section312.type, post.section312.embedcode, post.section312.text, post.section312.url, post.section312.embed, post.section312.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section313.type, post.section313.embedcode, post.section313.text, post.section313.url, post.section313.embed, post.section313.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section314.type, post.section314.embedcode, post.section314.text, post.section314.url, post.section314.embed, post.section314.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section315.type, post.section315.embedcode, post.section315.text, post.section315.url, post.section315.embed, post.section315.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section316.type, post.section316.embedcode, post.section316.text, post.section316.url, post.section316.embed, post.section316.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section317.type, post.section317.embedcode, post.section317.text, post.section317.url, post.section317.embed, post.section317.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section318.type, post.section318.embedcode, post.section318.text, post.section318.url, post.section318.embed, post.section318.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section319.type, post.section319.embedcode, post.section319.text, post.section319.url, post.section319.embed, post.section319.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section320.type, post.section320.embedcode, post.section320.text, post.section320.url, post.section320.embed, post.section320.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section321.type, post.section321.embedcode, post.section321.text, post.section321.url, post.section321.embed, post.section321.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section322.type, post.section322.embedcode, post.section322.text, post.section322.url, post.section322.embed, post.section322.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section323.type, post.section323.embedcode, post.section323.text, post.section323.url, post.section323.embed, post.section323.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section324.type, post.section324.embedcode, post.section324.text, post.section324.url, post.section324.embed, post.section324.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section325.type, post.section325.embedcode, post.section325.text, post.section325.url, post.section325.embed, post.section325.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section326.type, post.section326.embedcode, post.section326.text, post.section326.url, post.section326.embed, post.section326.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section327.type, post.section327.embedcode, post.section327.text, post.section327.url, post.section327.embed, post.section327.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section328.type, post.section328.embedcode, post.section328.text, post.section328.url, post.section328.embed, post.section328.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section329.type, post.section329.embedcode, post.section329.text, post.section329.url, post.section329.embed, post.section329.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section330.type, post.section330.embedcode, post.section330.text, post.section330.url, post.section330.embed, post.section330.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section331.type, post.section331.embedcode, post.section331.text, post.section331.url, post.section331.embed, post.section331.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section332.type, post.section332.embedcode, post.section332.text, post.section332.url, post.section332.embed, post.section332.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section333.type, post.section333.embedcode, post.section333.text, post.section333.url, post.section333.embed, post.section333.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section334.type, post.section334.embedcode, post.section334.text, post.section334.url, post.section334.embed, post.section334.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section335.type, post.section335.embedcode, post.section335.text, post.section335.url, post.section335.embed, post.section335.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section336.type, post.section336.embedcode, post.section336.text, post.section336.url, post.section336.embed, post.section336.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section337.type, post.section337.embedcode, post.section337.text, post.section337.url, post.section337.embed, post.section337.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section338.type, post.section338.embedcode, post.section338.text, post.section338.url, post.section338.embed, post.section338.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section339.type, post.section339.embedcode, post.section339.text, post.section339.url, post.section339.embed, post.section339.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section340.type, post.section340.embedcode, post.section340.text, post.section340.url, post.section340.embed, post.section340.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section341.type, post.section341.embedcode, post.section341.text, post.section341.url, post.section341.embed, post.section341.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section342.type, post.section342.embedcode, post.section342.text, post.section342.url, post.section342.embed, post.section342.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section343.type, post.section343.embedcode, post.section343.text, post.section343.url, post.section343.embed, post.section343.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section344.type, post.section344.embedcode, post.section344.text, post.section344.url, post.section344.embed, post.section344.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section345.type, post.section345.embedcode, post.section345.text, post.section345.url, post.section345.embed, post.section345.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section346.type, post.section346.embedcode, post.section346.text, post.section346.url, post.section346.embed, post.section346.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section347.type, post.section347.embedcode, post.section347.text, post.section347.url, post.section347.embed, post.section347.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section348.type, post.section348.embedcode, post.section348.text, post.section348.url, post.section348.embed, post.section348.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section349.type, post.section349.embedcode, post.section349.text, post.section349.url, post.section349.embed, post.section349.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section350.type, post.section350.embedcode, post.section350.text, post.section350.url, post.section350.embed, post.section350.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section351.type, post.section351.embedcode, post.section351.text, post.section351.url, post.section351.embed, post.section351.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section352.type, post.section352.embedcode, post.section352.text, post.section352.url, post.section352.embed, post.section352.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section353.type, post.section353.embedcode, post.section353.text, post.section353.url, post.section353.embed, post.section353.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section354.type, post.section354.embedcode, post.section354.text, post.section354.url, post.section354.embed, post.section354.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section355.type, post.section355.embedcode, post.section355.text, post.section355.url, post.section355.embed, post.section355.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section356.type, post.section356.embedcode, post.section356.text, post.section356.url, post.section356.embed, post.section356.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section357.type, post.section357.embedcode, post.section357.text, post.section357.url, post.section357.embed, post.section357.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section358.type, post.section358.embedcode, post.section358.text, post.section358.url, post.section358.embed, post.section358.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section359.type, post.section359.embedcode, post.section359.text, post.section359.url, post.section359.embed, post.section359.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section360.type, post.section360.embedcode, post.section360.text, post.section360.url, post.section360.embed, post.section360.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section361.type, post.section361.embedcode, post.section361.text, post.section361.url, post.section361.embed, post.section361.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section362.type, post.section362.embedcode, post.section362.text, post.section362.url, post.section362.embed, post.section362.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section363.type, post.section363.embedcode, post.section363.text, post.section363.url, post.section363.embed, post.section363.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section364.type, post.section364.embedcode, post.section364.text, post.section364.url, post.section364.embed, post.section364.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section365.type, post.section365.embedcode, post.section365.text, post.section365.url, post.section365.embed, post.section365.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section366.type, post.section366.embedcode, post.section366.text, post.section366.url, post.section366.embed, post.section366.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section367.type, post.section367.embedcode, post.section367.text, post.section367.url, post.section367.embed, post.section367.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section368.type, post.section368.embedcode, post.section368.text, post.section368.url, post.section368.embed, post.section368.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section369.type, post.section369.embedcode, post.section369.text, post.section369.url, post.section369.embed, post.section369.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section370.type, post.section370.embedcode, post.section370.text, post.section370.url, post.section370.embed, post.section370.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section371.type, post.section371.embedcode, post.section371.text, post.section371.url, post.section371.embed, post.section371.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section372.type, post.section372.embedcode, post.section372.text, post.section372.url, post.section372.embed, post.section372.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section373.type, post.section373.embedcode, post.section373.text, post.section373.url, post.section373.embed, post.section373.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section374.type, post.section374.embedcode, post.section374.text, post.section374.url, post.section374.embed, post.section374.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section375.type, post.section375.embedcode, post.section375.text, post.section375.url, post.section375.embed, post.section375.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section376.type, post.section376.embedcode, post.section376.text, post.section376.url, post.section376.embed, post.section376.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section377.type, post.section377.embedcode, post.section377.text, post.section377.url, post.section377.embed, post.section377.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section378.type, post.section378.embedcode, post.section378.text, post.section378.url, post.section378.embed, post.section378.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section379.type, post.section379.embedcode, post.section379.text, post.section379.url, post.section379.embed, post.section379.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section380.type, post.section380.embedcode, post.section380.text, post.section380.url, post.section380.embed, post.section380.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section381.type, post.section381.embedcode, post.section381.text, post.section381.url, post.section381.embed, post.section381.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section382.type, post.section382.embedcode, post.section382.text, post.section382.url, post.section382.embed, post.section382.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section383.type, post.section383.embedcode, post.section383.text, post.section383.url, post.section383.embed, post.section383.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section384.type, post.section384.embedcode, post.section384.text, post.section384.url, post.section384.embed, post.section384.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section385.type, post.section385.embedcode, post.section385.text, post.section385.url, post.section385.embed, post.section385.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section386.type, post.section386.embedcode, post.section386.text, post.section386.url, post.section386.embed, post.section386.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section387.type, post.section387.embedcode, post.section387.text, post.section387.url, post.section387.embed, post.section387.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section388.type, post.section388.embedcode, post.section388.text, post.section388.url, post.section388.embed, post.section388.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section389.type, post.section389.embedcode, post.section389.text, post.section389.url, post.section389.embed, post.section389.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section390.type, post.section390.embedcode, post.section390.text, post.section390.url, post.section390.embed, post.section390.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section391.type, post.section391.embedcode, post.section391.text, post.section391.url, post.section391.embed, post.section391.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section392.type, post.section392.embedcode, post.section392.text, post.section392.url, post.section392.embed, post.section392.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section393.type, post.section393.embedcode, post.section393.text, post.section393.url, post.section393.embed, post.section393.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section394.type, post.section394.embedcode, post.section394.text, post.section394.url, post.section394.embed, post.section394.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section395.type, post.section395.embedcode, post.section395.text, post.section395.url, post.section395.embed, post.section395.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section396.type, post.section396.embedcode, post.section396.text, post.section396.url, post.section396.embed, post.section396.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section397.type, post.section397.embedcode, post.section397.text, post.section397.url, post.section397.embed, post.section397.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section398.type, post.section398.embedcode, post.section398.text, post.section398.url, post.section398.embed, post.section398.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section399.type, post.section399.embedcode, post.section399.text, post.section399.url, post.section399.embed, post.section399.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section400.type, post.section400.embedcode, post.section400.text, post.section400.url, post.section400.embed, post.section400.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section401.type, post.section401.embedcode, post.section401.text, post.section401.url, post.section401.embed, post.section401.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section402.type, post.section402.embedcode, post.section402.text, post.section402.url, post.section402.embed, post.section402.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section403.type, post.section403.embedcode, post.section403.text, post.section403.url, post.section403.embed, post.section403.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section404.type, post.section404.embedcode, post.section404.text, post.section404.url, post.section404.embed, post.section404.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section405.type, post.section405.embedcode, post.section405.text, post.section405.url, post.section405.embed, post.section405.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section406.type, post.section406.embedcode, post.section406.text, post.section406.url, post.section406.embed, post.section406.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section407.type, post.section407.embedcode, post.section407.text, post.section407.url, post.section407.embed, post.section407.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section408.type, post.section408.embedcode, post.section408.text, post.section408.url, post.section408.embed, post.section408.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section409.type, post.section409.embedcode, post.section409.text, post.section409.url, post.section409.embed, post.section409.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section410.type, post.section410.embedcode, post.section410.text, post.section410.url, post.section410.embed, post.section410.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section411.type, post.section411.embedcode, post.section411.text, post.section411.url, post.section411.embed, post.section411.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section412.type, post.section412.embedcode, post.section412.text, post.section412.url, post.section412.embed, post.section412.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section413.type, post.section413.embedcode, post.section413.text, post.section413.url, post.section413.embed, post.section413.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section414.type, post.section414.embedcode, post.section414.text, post.section414.url, post.section414.embed, post.section414.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section415.type, post.section415.embedcode, post.section415.text, post.section415.url, post.section415.embed, post.section415.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section416.type, post.section416.embedcode, post.section416.text, post.section416.url, post.section416.embed, post.section416.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section417.type, post.section417.embedcode, post.section417.text, post.section417.url, post.section417.embed, post.section417.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section418.type, post.section418.embedcode, post.section418.text, post.section418.url, post.section418.embed, post.section418.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section419.type, post.section419.embedcode, post.section419.text, post.section419.url, post.section419.embed, post.section419.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section420.type, post.section420.embedcode, post.section420.text, post.section420.url, post.section420.embed, post.section420.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section421.type, post.section421.embedcode, post.section421.text, post.section421.url, post.section421.embed, post.section421.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section422.type, post.section422.embedcode, post.section422.text, post.section422.url, post.section422.embed, post.section422.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section423.type, post.section423.embedcode, post.section423.text, post.section423.url, post.section423.embed, post.section423.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section424.type, post.section424.embedcode, post.section424.text, post.section424.url, post.section424.embed, post.section424.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section425.type, post.section425.embedcode, post.section425.text, post.section425.url, post.section425.embed, post.section425.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section426.type, post.section426.embedcode, post.section426.text, post.section426.url, post.section426.embed, post.section426.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section427.type, post.section427.embedcode, post.section427.text, post.section427.url, post.section427.embed, post.section427.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section428.type, post.section428.embedcode, post.section428.text, post.section428.url, post.section428.embed, post.section428.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section429.type, post.section429.embedcode, post.section429.text, post.section429.url, post.section429.embed, post.section429.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section430.type, post.section430.embedcode, post.section430.text, post.section430.url, post.section430.embed, post.section430.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section431.type, post.section431.embedcode, post.section431.text, post.section431.url, post.section431.embed, post.section431.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section432.type, post.section432.embedcode, post.section432.text, post.section432.url, post.section432.embed, post.section432.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section433.type, post.section433.embedcode, post.section433.text, post.section433.url, post.section433.embed, post.section433.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section434.type, post.section434.embedcode, post.section434.text, post.section434.url, post.section434.embed, post.section434.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section435.type, post.section435.embedcode, post.section435.text, post.section435.url, post.section435.embed, post.section435.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section436.type, post.section436.embedcode, post.section436.text, post.section436.url, post.section436.embed, post.section436.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section437.type, post.section437.embedcode, post.section437.text, post.section437.url, post.section437.embed, post.section437.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section438.type, post.section438.embedcode, post.section438.text, post.section438.url, post.section438.embed, post.section438.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section439.type, post.section439.embedcode, post.section439.text, post.section439.url, post.section439.embed, post.section439.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section440.type, post.section440.embedcode, post.section440.text, post.section440.url, post.section440.embed, post.section440.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section441.type, post.section441.embedcode, post.section441.text, post.section441.url, post.section441.embed, post.section441.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section442.type, post.section442.embedcode, post.section442.text, post.section442.url, post.section442.embed, post.section442.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section443.type, post.section443.embedcode, post.section443.text, post.section443.url, post.section443.embed, post.section443.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section444.type, post.section444.embedcode, post.section444.text, post.section444.url, post.section444.embed, post.section444.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section445.type, post.section445.embedcode, post.section445.text, post.section445.url, post.section445.embed, post.section445.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section446.type, post.section446.embedcode, post.section446.text, post.section446.url, post.section446.embed, post.section446.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section447.type, post.section447.embedcode, post.section447.text, post.section447.url, post.section447.embed, post.section447.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section448.type, post.section448.embedcode, post.section448.text, post.section448.url, post.section448.embed, post.section448.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section449.type, post.section449.embedcode, post.section449.text, post.section449.url, post.section449.embed, post.section449.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section450.type, post.section450.embedcode, post.section450.text, post.section450.url, post.section450.embed, post.section450.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section451.type, post.section451.embedcode, post.section451.text, post.section451.url, post.section451.embed, post.section451.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section452.type, post.section452.embedcode, post.section452.text, post.section452.url, post.section452.embed, post.section452.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section453.type, post.section453.embedcode, post.section453.text, post.section453.url, post.section453.embed, post.section453.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section454.type, post.section454.embedcode, post.section454.text, post.section454.url, post.section454.embed, post.section454.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section455.type, post.section455.embedcode, post.section455.text, post.section455.url, post.section455.embed, post.section455.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section456.type, post.section456.embedcode, post.section456.text, post.section456.url, post.section456.embed, post.section456.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section457.type, post.section457.embedcode, post.section457.text, post.section457.url, post.section457.embed, post.section457.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section458.type, post.section458.embedcode, post.section458.text, post.section458.url, post.section458.embed, post.section458.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section459.type, post.section459.embedcode, post.section459.text, post.section459.url, post.section459.embed, post.section459.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section460.type, post.section460.embedcode, post.section460.text, post.section460.url, post.section460.embed, post.section460.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section461.type, post.section461.embedcode, post.section461.text, post.section461.url, post.section461.embed, post.section461.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section462.type, post.section462.embedcode, post.section462.text, post.section462.url, post.section462.embed, post.section462.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section463.type, post.section463.embedcode, post.section463.text, post.section463.url, post.section463.embed, post.section463.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section464.type, post.section464.embedcode, post.section464.text, post.section464.url, post.section464.embed, post.section464.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section465.type, post.section465.embedcode, post.section465.text, post.section465.url, post.section465.embed, post.section465.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section466.type, post.section466.embedcode, post.section466.text, post.section466.url, post.section466.embed, post.section466.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section467.type, post.section467.embedcode, post.section467.text, post.section467.url, post.section467.embed, post.section467.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section468.type, post.section468.embedcode, post.section468.text, post.section468.url, post.section468.embed, post.section468.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section469.type, post.section469.embedcode, post.section469.text, post.section469.url, post.section469.embed, post.section469.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section470.type, post.section470.embedcode, post.section470.text, post.section470.url, post.section470.embed, post.section470.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section471.type, post.section471.embedcode, post.section471.text, post.section471.url, post.section471.embed, post.section471.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section472.type, post.section472.embedcode, post.section472.text, post.section472.url, post.section472.embed, post.section472.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section473.type, post.section473.embedcode, post.section473.text, post.section473.url, post.section473.embed, post.section473.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section474.type, post.section474.embedcode, post.section474.text, post.section474.url, post.section474.embed, post.section474.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section475.type, post.section475.embedcode, post.section475.text, post.section475.url, post.section475.embed, post.section475.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section476.type, post.section476.embedcode, post.section476.text, post.section476.url, post.section476.embed, post.section476.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section477.type, post.section477.embedcode, post.section477.text, post.section477.url, post.section477.embed, post.section477.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section478.type, post.section478.embedcode, post.section478.text, post.section478.url, post.section478.embed, post.section478.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section479.type, post.section479.embedcode, post.section479.text, post.section479.url, post.section479.embed, post.section479.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section480.type, post.section480.embedcode, post.section480.text, post.section480.url, post.section480.embed, post.section480.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section481.type, post.section481.embedcode, post.section481.text, post.section481.url, post.section481.embed, post.section481.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section482.type, post.section482.embedcode, post.section482.text, post.section482.url, post.section482.embed, post.section482.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section483.type, post.section483.embedcode, post.section483.text, post.section483.url, post.section483.embed, post.section483.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section484.type, post.section484.embedcode, post.section484.text, post.section484.url, post.section484.embed, post.section484.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section485.type, post.section485.embedcode, post.section485.text, post.section485.url, post.section485.embed, post.section485.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section486.type, post.section486.embedcode, post.section486.text, post.section486.url, post.section486.embed, post.section486.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section487.type, post.section487.embedcode, post.section487.text, post.section487.url, post.section487.embed, post.section487.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section488.type, post.section488.embedcode, post.section488.text, post.section488.url, post.section488.embed, post.section488.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section489.type, post.section489.embedcode, post.section489.text, post.section489.url, post.section489.embed, post.section489.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section490.type, post.section490.embedcode, post.section490.text, post.section490.url, post.section490.embed, post.section490.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section491.type, post.section491.embedcode, post.section491.text, post.section491.url, post.section491.embed, post.section491.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section492.type, post.section492.embedcode, post.section492.text, post.section492.url, post.section492.embed, post.section492.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section493.type, post.section493.embedcode, post.section493.text, post.section493.url, post.section493.embed, post.section493.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section494.type, post.section494.embedcode, post.section494.text, post.section494.url, post.section494.embed, post.section494.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section495.type, post.section495.embedcode, post.section495.text, post.section495.url, post.section495.embed, post.section495.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section496.type, post.section496.embedcode, post.section496.text, post.section496.url, post.section496.embed, post.section496.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section497.type, post.section497.embedcode, post.section497.text, post.section497.url, post.section497.embed, post.section497.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section498.type, post.section498.embedcode, post.section498.text, post.section498.url, post.section498.embed, post.section498.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section499.type, post.section499.embedcode, post.section499.text, post.section499.url, post.section499.embed, post.section499.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section500.type, post.section500.embedcode, post.section500.text, post.section500.url, post.section500.embed, post.section500.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section501.type, post.section501.embedcode, post.section501.text, post.section501.url, post.section501.embed, post.section501.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section502.type, post.section502.embedcode, post.section502.text, post.section502.url, post.section502.embed, post.section502.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section503.type, post.section503.embedcode, post.section503.text, post.section503.url, post.section503.embed, post.section503.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section504.type, post.section504.embedcode, post.section504.text, post.section504.url, post.section504.embed, post.section504.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section505.type, post.section505.embedcode, post.section505.text, post.section505.url, post.section505.embed, post.section505.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section506.type, post.section506.embedcode, post.section506.text, post.section506.url, post.section506.embed, post.section506.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section507.type, post.section507.embedcode, post.section507.text, post.section507.url, post.section507.embed, post.section507.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section508.type, post.section508.embedcode, post.section508.text, post.section508.url, post.section508.embed, post.section508.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section509.type, post.section509.embedcode, post.section509.text, post.section509.url, post.section509.embed, post.section509.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section510.type, post.section510.embedcode, post.section510.text, post.section510.url, post.section510.embed, post.section510.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section511.type, post.section511.embedcode, post.section511.text, post.section511.url, post.section511.embed, post.section511.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section512.type, post.section512.embedcode, post.section512.text, post.section512.url, post.section512.embed, post.section512.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section513.type, post.section513.embedcode, post.section513.text, post.section513.url, post.section513.embed, post.section513.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section514.type, post.section514.embedcode, post.section514.text, post.section514.url, post.section514.embed, post.section514.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section515.type, post.section515.embedcode, post.section515.text, post.section515.url, post.section515.embed, post.section515.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section516.type, post.section516.embedcode, post.section516.text, post.section516.url, post.section516.embed, post.section516.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section517.type, post.section517.embedcode, post.section517.text, post.section517.url, post.section517.embed, post.section517.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section518.type, post.section518.embedcode, post.section518.text, post.section518.url, post.section518.embed, post.section518.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section519.type, post.section519.embedcode, post.section519.text, post.section519.url, post.section519.embed, post.section519.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section520.type, post.section520.embedcode, post.section520.text, post.section520.url, post.section520.embed, post.section520.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section521.type, post.section521.embedcode, post.section521.text, post.section521.url, post.section521.embed, post.section521.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section522.type, post.section522.embedcode, post.section522.text, post.section522.url, post.section522.embed, post.section522.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section523.type, post.section523.embedcode, post.section523.text, post.section523.url, post.section523.embed, post.section523.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section524.type, post.section524.embedcode, post.section524.text, post.section524.url, post.section524.embed, post.section524.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section525.type, post.section525.embedcode, post.section525.text, post.section525.url, post.section525.embed, post.section525.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section526.type, post.section526.embedcode, post.section526.text, post.section526.url, post.section526.embed, post.section526.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section527.type, post.section527.embedcode, post.section527.text, post.section527.url, post.section527.embed, post.section527.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section528.type, post.section528.embedcode, post.section528.text, post.section528.url, post.section528.embed, post.section528.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section529.type, post.section529.embedcode, post.section529.text, post.section529.url, post.section529.embed, post.section529.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section530.type, post.section530.embedcode, post.section530.text, post.section530.url, post.section530.embed, post.section530.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section531.type, post.section531.embedcode, post.section531.text, post.section531.url, post.section531.embed, post.section531.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section532.type, post.section532.embedcode, post.section532.text, post.section532.url, post.section532.embed, post.section532.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section533.type, post.section533.embedcode, post.section533.text, post.section533.url, post.section533.embed, post.section533.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section534.type, post.section534.embedcode, post.section534.text, post.section534.url, post.section534.embed, post.section534.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section535.type, post.section535.embedcode, post.section535.text, post.section535.url, post.section535.embed, post.section535.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section536.type, post.section536.embedcode, post.section536.text, post.section536.url, post.section536.embed, post.section536.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section537.type, post.section537.embedcode, post.section537.text, post.section537.url, post.section537.embed, post.section537.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section538.type, post.section538.embedcode, post.section538.text, post.section538.url, post.section538.embed, post.section538.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section539.type, post.section539.embedcode, post.section539.text, post.section539.url, post.section539.embed, post.section539.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section540.type, post.section540.embedcode, post.section540.text, post.section540.url, post.section540.embed, post.section540.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section541.type, post.section541.embedcode, post.section541.text, post.section541.url, post.section541.embed, post.section541.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section542.type, post.section542.embedcode, post.section542.text, post.section542.url, post.section542.embed, post.section542.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section543.type, post.section543.embedcode, post.section543.text, post.section543.url, post.section543.embed, post.section543.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section544.type, post.section544.embedcode, post.section544.text, post.section544.url, post.section544.embed, post.section544.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section545.type, post.section545.embedcode, post.section545.text, post.section545.url, post.section545.embed, post.section545.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section546.type, post.section546.embedcode, post.section546.text, post.section546.url, post.section546.embed, post.section546.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section547.type, post.section547.embedcode, post.section547.text, post.section547.url, post.section547.embed, post.section547.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section548.type, post.section548.embedcode, post.section548.text, post.section548.url, post.section548.embed, post.section548.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section549.type, post.section549.embedcode, post.section549.text, post.section549.url, post.section549.embed, post.section549.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section550.type, post.section550.embedcode, post.section550.text, post.section550.url, post.section550.embed, post.section550.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section551.type, post.section551.embedcode, post.section551.text, post.section551.url, post.section551.embed, post.section551.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section552.type, post.section552.embedcode, post.section552.text, post.section552.url, post.section552.embed, post.section552.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section553.type, post.section553.embedcode, post.section553.text, post.section553.url, post.section553.embed, post.section553.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section554.type, post.section554.embedcode, post.section554.text, post.section554.url, post.section554.embed, post.section554.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section555.type, post.section555.embedcode, post.section555.text, post.section555.url, post.section555.embed, post.section555.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section556.type, post.section556.embedcode, post.section556.text, post.section556.url, post.section556.embed, post.section556.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section557.type, post.section557.embedcode, post.section557.text, post.section557.url, post.section557.embed, post.section557.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section558.type, post.section558.embedcode, post.section558.text, post.section558.url, post.section558.embed, post.section558.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section559.type, post.section559.embedcode, post.section559.text, post.section559.url, post.section559.embed, post.section559.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section560.type, post.section560.embedcode, post.section560.text, post.section560.url, post.section560.embed, post.section560.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section561.type, post.section561.embedcode, post.section561.text, post.section561.url, post.section561.embed, post.section561.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section562.type, post.section562.embedcode, post.section562.text, post.section562.url, post.section562.embed, post.section562.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section563.type, post.section563.embedcode, post.section563.text, post.section563.url, post.section563.embed, post.section563.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section564.type, post.section564.embedcode, post.section564.text, post.section564.url, post.section564.embed, post.section564.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section565.type, post.section565.embedcode, post.section565.text, post.section565.url, post.section565.embed, post.section565.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section566.type, post.section566.embedcode, post.section566.text, post.section566.url, post.section566.embed, post.section566.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section567.type, post.section567.embedcode, post.section567.text, post.section567.url, post.section567.embed, post.section567.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section568.type, post.section568.embedcode, post.section568.text, post.section568.url, post.section568.embed, post.section568.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section569.type, post.section569.embedcode, post.section569.text, post.section569.url, post.section569.embed, post.section569.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section570.type, post.section570.embedcode, post.section570.text, post.section570.url, post.section570.embed, post.section570.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section571.type, post.section571.embedcode, post.section571.text, post.section571.url, post.section571.embed, post.section571.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section572.type, post.section572.embedcode, post.section572.text, post.section572.url, post.section572.embed, post.section572.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section573.type, post.section573.embedcode, post.section573.text, post.section573.url, post.section573.embed, post.section573.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section574.type, post.section574.embedcode, post.section574.text, post.section574.url, post.section574.embed, post.section574.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section575.type, post.section575.embedcode, post.section575.text, post.section575.url, post.section575.embed, post.section575.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section576.type, post.section576.embedcode, post.section576.text, post.section576.url, post.section576.embed, post.section576.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section577.type, post.section577.embedcode, post.section577.text, post.section577.url, post.section577.embed, post.section577.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section578.type, post.section578.embedcode, post.section578.text, post.section578.url, post.section578.embed, post.section578.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section579.type, post.section579.embedcode, post.section579.text, post.section579.url, post.section579.embed, post.section579.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section580.type, post.section580.embedcode, post.section580.text, post.section580.url, post.section580.embed, post.section580.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section581.type, post.section581.embedcode, post.section581.text, post.section581.url, post.section581.embed, post.section581.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section582.type, post.section582.embedcode, post.section582.text, post.section582.url, post.section582.embed, post.section582.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section583.type, post.section583.embedcode, post.section583.text, post.section583.url, post.section583.embed, post.section583.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section584.type, post.section584.embedcode, post.section584.text, post.section584.url, post.section584.embed, post.section584.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section585.type, post.section585.embedcode, post.section585.text, post.section585.url, post.section585.embed, post.section585.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section586.type, post.section586.embedcode, post.section586.text, post.section586.url, post.section586.embed, post.section586.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section587.type, post.section587.embedcode, post.section587.text, post.section587.url, post.section587.embed, post.section587.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section588.type, post.section588.embedcode, post.section588.text, post.section588.url, post.section588.embed, post.section588.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section589.type, post.section589.embedcode, post.section589.text, post.section589.url, post.section589.embed, post.section589.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section590.type, post.section590.embedcode, post.section590.text, post.section590.url, post.section590.embed, post.section590.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section591.type, post.section591.embedcode, post.section591.text, post.section591.url, post.section591.embed, post.section591.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section592.type, post.section592.embedcode, post.section592.text, post.section592.url, post.section592.embed, post.section592.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section593.type, post.section593.embedcode, post.section593.text, post.section593.url, post.section593.embed, post.section593.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section594.type, post.section594.embedcode, post.section594.text, post.section594.url, post.section594.embed, post.section594.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section595.type, post.section595.embedcode, post.section595.text, post.section595.url, post.section595.embed, post.section595.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section596.type, post.section596.embedcode, post.section596.text, post.section596.url, post.section596.embed, post.section596.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section597.type, post.section597.embedcode, post.section597.text, post.section597.url, post.section597.embed, post.section597.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section598.type, post.section598.embedcode, post.section598.text, post.section598.url, post.section598.embed, post.section598.image));
                finalString = finalString + CleanChecks(getBlock(post.title, post.section599.type, post.section599.embedcode, post.section599.text, post.section599.url, post.section599.embed, post.section599.image));


            }
            catch (Exception ex)
            {
                return finalString;
            }

            return finalString;
        }


        #endregion


        string CleanChecks(string text)
        {
            try
            {
                if (text == string.Empty) return string.Empty;

                text = Regex.Replace(text, @"[\r\n\x00\x1a\\'""]", @"\$0");

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
            string currfilePath = "";
            string currdirectory = "";
            try
            {

                // Get all subdirectories in the specified path
                var DIR2015 = @"D:\NWP\Assets\Assets\cnn\2015";
                var DIR2016 = @"D:\NWP\Assets\Assets\cnn\2016";
                var DIR2017 = @"D:\NWP\Assets\Assets\cnn\2017";
                var DIR2018 = @"D:\NWP\Assets\Assets\cnn\2018";
                var DIR2019 = @"D:\NWP\Assets\Assets\cnn\2019";
                var ElectionResult = @"D:\NWP\Assets\Assets\cnn\2019ElectionResult";
                var DIR2020 = @"D:\NWP\Assets\Assets\cnn\2020";
                var DIR2021 = @"D:\NWP\Assets\Assets\cnn\2021";
                var DIR2022 = @"D:\NWP\Assets\Assets\cnn\2022";
                var DIR2023 = @"D:\NWP\Assets\Assets\cnn\2023";
                var DIR2024 = @"D:\NWP\Assets\Assets\cnn\2024";

                var ADVERTORIAL_IMAGES = @"D:\NWP\Assets\Assets\other";
                var ADVERTORIA_LOGOS = @"ADVERTORIAL-LOGOS";
                var ALFREDAT_COST = @"ALFRED AT COST";
                var Assets = @"D:\NWP\Assets\Assets\other";

                string[] directories = {
                                 @$"{DIR2015} = {DIR2015}",
                                 @$"{DIR2016}",
                                 @$"{DIR2017}",
                                 @$"{DIR2018}",
                                 @$"{DIR2019}",
                                 @$"{ElectionResult}",
                                 @$"{DIR2020}",
                                 @$"{DIR2021}",
                                 @$"{DIR2022}",
                                 @$"{DIR2023}",
                                 @$"{DIR2024}",
                };

                string subpath = "";
                bool Is2ndLevel = false;
                bool IsImageNext = false;

                // Loop through each directory

                foreach (var dir in directories)
                {
                    foreach (string directory in Directory.GetDirectories(dir))
                    {
                        currdirectory = directory;

                        var tempArry = directory.Split("\\");
                        subpath = $@"{tempArry[tempArry.Length - 2]}\{tempArry[tempArry.Length - 1]}";

                        Console.WriteLine(directory);
                        foreach (string filePath in Directory.EnumerateFiles(directory))
                        {
                            Console.WriteLine($"Found file: {filePath}");

                            currfilePath = filePath;


                            BeautifyXML(filePath);
                            DeleteFolderTypeNode(filePath);
                            AppendSingleQoute(filePath);

                            string[] lines = File.ReadAllLines(filePath);
                            string imageSource = "";

                            for (int i = 0; i < lines.Length; i++)
                            {

                                //check for filename
                                if (!IsImageNext && lines[i].Equals("'    <sv:property sv:name=\"jcr:uuid\" sv:type=\"String\">"))
                                {
                                    imageSource = lines[i + 1].Replace("<sv:value>", "").Replace("</sv:value>", "").Replace("'", "").Trim();
                                    Is2ndLevel = false;
                                    IsImageNext = true;
                                }

                                if (!IsImageNext && lines[i].Equals("'      <sv:property sv:name=\"jcr:uuid\" sv:type=\"String\">"))
                                {
                                    imageSource = lines[i + 1].Replace("<sv:value>", "").Replace("</sv:value>", "").Replace("'", "").Trim();
                                    Is2ndLevel = true;
                                    IsImageNext = true;
                                }

                                //check for actual base64 string images for parsing

                                if (lines[i].Contains("<sv:property sv:name=\"jcr:data\" sv:type=\"Binary\">"))
                                {
                                    Base64StringToJpeg(lines[i + 1].Replace("'", "").Trim().Replace("<sv:value>", "").Replace("</sv:value>", ""), imageSource.Trim(), subpath);

                                    IsImageNext = false;
                                }

                            }

                            RemoveSingleQoute(filePath);
                        }

                    }
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}  - {currdirectory} - {currfilePath}");
            }

        }

        void Base64StringToJpeg(string base64String, string fileName, string subPath)
        {
            try
            {
                string dirDestination = $@"D:\NWP\Assets\cnn-extracted\{subPath}";

                if (!Directory.Exists(dirDestination))
                {
                    Directory.CreateDirectory(dirDestination);
                }

                string filePath = $@"D:\NWP\Assets\cnn-extracted\{subPath}\{fileName}.jpg";
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

        static void ProcessAuthors()
        {

            // Get all subdirectories in the specified path
            var nwp_dir = @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\CNN\Articles\archived";
            string[] directories = {

                             @$"{nwp_dir}\2015",
                             @$"{nwp_dir}\2016",
                             @$"{nwp_dir}\2017",
                             @$"{nwp_dir}\2018",
                             @$"{nwp_dir}\2019",
                             @$"{nwp_dir}\2020",
                             @$"{nwp_dir}\2021",
                             @$"{nwp_dir}\2022",
                             @$"{nwp_dir}\2023",
                             @$"{nwp_dir}\2024",

    };

            var searchText = "author:";
            // Loop through each directory
            List<string> authorsList = new List<string>();
            foreach (string dir in directories)
            {
                Console.WriteLine(dir);
                foreach (string directory in Directory.GetDirectories(dir))
                {
                    foreach (string filePath in Directory.EnumerateFiles(directory))
                    {
                        Console.WriteLine($"Found file: {filePath}");

                        // Use File.ReadLines to read lines lazily and efficiently
                        var matchingLines = File.ReadLines(filePath)
                                                .Where(item => item.Contains(searchText));

                        string[] authors = matchingLines.Select(x => x.Replace($"{searchText}", "").Replace("\'", "").Trim()).Distinct().ToArray();
                        if (authors.Length > 0)
                        {
                            authorsList.AddRange(authors);
                        }

                    }
                }

                
            }


            int i = 1357;
            int user_id = 1357;
            List<string> wp_users = new List<string>();
            List<string> wp_usermeta = new List<string>();

            authorsList = authorsList.Distinct().OrderBy(authorsList => authorsList).ToList();

            foreach (var author in authorsList.Distinct())
            {

                user_id++;
                i++;
                wp_users.Add($"INSERT INTO `wp_users` ( `ID`,`user_login`, `user_pass`, `user_nicename`, `user_email`, `user_url`, `user_registered`, `user_activation_key`, `user_status`, `display_name`) VALUES({user_id},'migrateduser-{i}', '$wp$2y$10$GveTsPNj/qlcYltZ7sctouaGAgwxGsCs83u.HXbm.XabHUDa0w/jy', 'migrateduser-{i}', 'migrated.user.{i}@newswatchplus.ph', 'https://newswatchplus-staging.azurewebsites.net/author/migrateduser-{i}', '2025-09-05 07:22:19', '', 0, '{author}');");

                wp_usermeta.Add($"INSERT INTO `wp_usermeta` (`user_id`, `meta_key`, `meta_value`) VALUES" +
                $"( {user_id}, 'nickname', 'migrateduser-{i}')," +
                $"( {user_id}, 'first_name', '{author}')," +
                $"( {user_id}, 'last_name', '')," +
                $"( {user_id}, 'rich_editing', 'true')," +
                $"( {user_id}, 'syntax_highlighting', 'true')," +
                $"( {user_id}, 'comment_shortcuts', 'false')," +
                $"( {user_id}, 'admin_color', 'fresh')," +
                $"( {user_id}, 'show_admin_bar_front', 'true')," +
                "(" + user_id + ", 'wp_capabilities', 'a:1:{s:6:\"author\";b:1;}' )," +
                $"( {user_id}, 'wp_user_level', '2');");

            }

            var _user = wp_users;
            var _usermeta = wp_usermeta;
        }
    }
    private static string trimProperty(string selectedString, string property)
    {
        try
        {
            selectedString = selectedString.Replace($"{property}: '", string.Empty);
            selectedString = selectedString.Replace($"{property}: ", string.Empty);

            var text = selectedString.Replace($"{property}: ['", string.Empty).Trim();

            if (text.EndsWith("]"))
                text = text.Remove(text.Length - 1);

            if (text.StartsWith("["))
                text = text.Remove(0,1);

            if (text.EndsWith("'"))
                text = text.Remove(text.Length - 1);

            if (text.StartsWith("'"))
                text = text.Remove(0, 1);

            if (text.EndsWith("+08:00"))
                text = text.Remove(text.Length - 6);

            return text;
        }
        catch (Exception)
        {

            throw;
        }
        
    }


    private static Post processPostData(List<string> article, Post post)
    {

        //header
        for (int i = 0; i < article.Count; i++)
        {
            string currentValue = article[i];

            if (currentValue.Contains("'author'"))
            {
                post.author = trimProperty(currentValue,"'author'");
            }
            else if (currentValue.Contains("'imagesource'"))
            {
                post.imagesource = trimProperty(currentValue, "'imagesource'").Replace("jcr:",string.Empty);
            }
            else if(currentValue.Contains("'caption'"))
            {
                post.caption = GetMultiLineCaption(article, i);
            }
            else if (currentValue.Contains("'categories'"))
            {
                post.categories = trimProperty(currentValue, "'categories'");
            }
            else if (currentValue.Contains("'embedsource'"))
            {
                post.embedsource = GetMultiLineValue(article, i);
            }
            else if (currentValue.Contains("'created'"))
            {
                post.created = DateTime.Parse(trimProperty(currentValue, "'created'"));
            }
            else if (currentValue.Contains("'mgnl:lastModified'"))
            {
                post.lastmodified = DateTime.Parse(trimProperty(currentValue, "'mgnl:lastModified'"));
            }
            else if (currentValue.Contains("'title'"))
            {
                post.title = GetMultiLineTitle(article, i);
            }
            else if (currentValue.Contains("'mgnl:activationStatus'"))
            {
                post.activationstatus = trimProperty(currentValue, "'mgnl:activationStatus'") =="true"? true : false;
            }
            else if (currentValue.Contains("'visualType'"))
            {
                post.visualtype = trimProperty(currentValue, "'visualType'");
                break;
            }

        }

        //content

        var matchingLines = article.Select((line, index) => new { LineText = line, LineNumber = index + 1 })
                                            .Where(item => item.LineText.Contains("'jcr:primaryType': 'mgnl:block'"));

        int[] blocks = matchingLines.Select(x => x.LineNumber).ToArray();
        string finalString = string.Empty;

        for (int i = 0; i < blocks.Length; i++)
        {
            try
            {
                

                int startLine = blocks[i];
                int endLine = i == blocks.Length-1 ? blocks[i] : blocks[i + 1] - 2;
                string type = string.Empty;
                string text = string.Empty;
                string image = string.Empty;
                string embedCode = string.Empty;
                string embed = string.Empty;

                for (int currentLineNumber = startLine; currentLineNumber <= endLine; currentLineNumber++)
                {
                    string currentLineText = article[currentLineNumber];

                    if (currentLineText.Contains("type"))
                    {
                        if (currentLineText.Contains("text"))
                        {
                            type = "text";
                        }
                        else if (currentLineText.Contains("image"))
                        {
                            type = "image";
                        }
                        else if (currentLineText.Contains("video"))
                        {
                            type = "video";
                        }
                        else if (currentLineText.Contains("embedCode"))
                        {
                            type = "embedCode";
                        }
                    }
                    else if (currentLineText.Contains("'text':"))
                    {
                        
                        var multitext = article.Skip(currentLineNumber).Take(endLine - currentLineNumber);
                        text = TrimProperty(multitext, "'text':");

                    }
                    else if (currentLineText.Contains("'image':"))
                    {
                        image = TrimProperty(currentLineText, "'image':");
                        image = TrimProperty(image, "jcr:");
                    }
                    else if (currentLineText.Contains("'embedCode':"))
                    {
                        embedCode = GetMultiLineValue(article,currentLineNumber);
                    }
                    else if (currentLineText.Contains("'embed':"))
                    {
                        embed = GetMultiLineValue(article, currentLineNumber);
                    }

                }


                if (post.visualtype == "video" && post.embedsource.Contains("https://www.youtube.com/embed"))
                {
                    finalString = post.embedsource.Replace("width=\"560\"", "width=\"800\"").Replace("height=\"315\"", "height=\"500\"");
                    post.visualtype = string.Empty;
                }
                else if (post.visualtype == "embed")
                {
                    //finalString = post.embedsource;
                    post.visualtype = string.Empty;
                }
                else if(type == "text")
                {
                    finalString = finalString + text;
                }
                else if (type == "embedCode")
                {
                    finalString = finalString + embedCode;
                }
                    
                else if(type == "embed")
                {
                    finalString = finalString + embed;
                }
                else if(type== "image")
                {
                    finalString = finalString + $"<figure class=\"wp-block-image alignwide size-full\"><img src=\"/wp-content/uploads/2025/10/{image}.jpg\" alt=\"\" /></figure>";
                }


            }
            catch (Exception ex)
            {
                //ErrorCount++;

                Console.WriteLine($"Error at line {blocks[i]} - {ex} - {post.title}");

                continue;
            }

        }


        post.Content = finalString;

        return post;
    }

    private static string TrimProperty(IEnumerable<string> multitext,string toReplace)
    {
        multitext = multitext.Select(x => x.Replace(toReplace, string.Empty)).ToArray();
        List<string> trimmedStrings = multitext.Select(s => s.Trim()).ToList();

        for (int i = 0; i < trimmedStrings.Count(); i++)
        {
            var text = trimmedStrings[i];

            if (text.EndsWith("\\r\""))
            {
                text = text.Replace("\\r\"", "<br>");
            }
            else if (text.Equals("\"\\r\""))
            {
                text = "<br>";
            }

            if (text.EndsWith('\\'))
            {
                text = text.Remove(text.Length - 1);
            }

            if (text.StartsWith('\\'))
            {
                text = text.Remove(0,1);
            }
            text = text.Replace("\\ \\", string.Empty);

            trimmedStrings[i] = text;
        }

        

        return string.Join(" ", trimmedStrings).Trim('\'').Trim('\"');
    }

    private static string TrimProperty(string text, string toReplace)
    {
        return text.Replace(toReplace, string.Empty).Trim().Trim('\'');
    }


    private static string GetMultiLineTitle(IEnumerable<string> multitext, int LineStart)
    {
        for (int i = LineStart; i < multitext.ToArray().Length; i++)
        {
            if (multitext.ToArray()[i].Contains("'visualType':") || multitext.ToArray()[i].Contains("'updated':"))
            {
                //clean sigle qoute
                var uncleaned = multitext.Skip(LineStart).Take(i - LineStart);
                List<string> newList = new List<string>();
                foreach (var item in uncleaned)
                {
                    if (item.Trim().Equals("'"))
                    {
                        newList.Add(item.Replace("'", string.Empty).Trim().Trim('\''));
                    }
                    else
                    {
                        newList.Add(item.Trim().Trim('\''));
                    }
                }

                var cleaned = string.Join(" ", newList);
                return TrimProperty(cleaned, "title':");
            }
        }


        return string.Empty;
    }
    private static string GetMultiLineCaption(IEnumerable<string> multitext, int LineStart)
    {
        for (int i = LineStart; i < multitext.ToArray().Length; i++)
        {
            if (multitext.ToArray()[i].Contains("'categories':"))
            {
                //clean sigle qoute
                var uncleaned = multitext.Skip(LineStart).Take(i - LineStart);
                List<string> newList = new List<string>();
                foreach (var item in uncleaned)
                {
                    if (item.Trim().Equals("'"))
                    {
                        newList.Add(item.Replace("'", string.Empty).Trim().Trim('\''));
                    }
                    else
                    {
                        newList.Add(item.Trim().Trim('\''));
                    }
                }

                var cleaned = string.Join(" ", newList);
                return TrimProperty(cleaned, "caption':");
            }
        }


        return string.Empty;
    }




    private static string GetMultiLineValue(IEnumerable<string> multitext, int LineStart)
    {
        for (int i = LineStart; i < multitext.ToArray().Length; i++)
        {
            if (multitext.ToArray()[i].Contains("'jcr:") || multitext.ToArray()[i].Contains("'mgnl:"))
            {
                //clean sigle qoute
                var uncleaned = multitext.Skip(LineStart).Take(i - LineStart);
                List<string> newList = new List<string>();
                foreach (var item in uncleaned)
                {
                    if (item.Trim().Equals("'"))
                    {
                        newList.Add(item.Replace("'", string.Empty));
                    }
                    else
                    {
                        newList.Add(item);
                    }   
                }

                var cleaned = string.Join("\n", newList);
                return TrimProperty(TrimProperty(TrimProperty(cleaned, "'embedsource':"), "embed':"), "embedCode':").Trim().Trim('\'') + "\n";
            }
        }
        

        return string.Empty;
    }

    public static string GenerateWordPressSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        // 1. Lowercase the string
        string slug = title.ToLower();

        // 2. Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // 3. Remove special characters (keep letters, numbers, and hyphens)
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // 4. Remove duplicate hyphens
        slug = Regex.Replace(slug, @"-+", "-");

        // 5. Trim leading/trailing hyphens
        slug = slug.Trim('-');

        return slug;
    }


    private static string CleanApostrophe(string yml)
    {
        string[] lines = yml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);


        for (int i = 0; i < lines.Count(); i++)
        {
            string pattern = @"(\w+'s)";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(lines[i]);

            foreach (Match match in matches)
            {
                var newValue = match.Value.Replace("'", "’");
                lines[i] = lines[i].Replace(match.Value, newValue);
            }


        }

        // Iterate through each line using a foreach loop
        for (int i = 0; i < lines.Count(); i++)
        {
            string pattern = @"(\w+'\w+)|(\w+' \w+)|(\w+''\w+)|(\w+'' \w+)";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(lines[i]);

            foreach (Match match in matches)
            {
                var newValue = match.Value.Replace("'", "’");
                lines[i] = lines[i].Replace(match.Value, newValue);
            }

        }

        for (int i = 0; i < lines.Count(); i++)
        {
            string pattern = @"(\w+ '\w+)|(\w+ ''\w+)";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(lines[i]);

            foreach (Match match in matches)
            {
                var newValue = match.Value.Replace("'", "‘");
                lines[i] = lines[i].Replace(match.Value, newValue);
            }


        }

        

        var revertFormat = string.Join(Environment.NewLine, lines);
        return revertFormat;
    }

    private static string CleanTags(string yml)
    {

        string[] lines = yml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        // Iterate through each line using a foreach loop
        for (int i = 0; i < lines.Count(); i++)
        {
            string pattern = @"tags: '\w+";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(lines[i]);

            foreach (Match match in matches)
            {
                lines[i] = "    tags: ''";
            }

        }


        var revertFormat = string.Join(Environment.NewLine, lines);
        return revertFormat;
    }

    private static string CleanStories(string yml)
    {

        string[] lines = yml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        // Iterate through each line using a foreach loop
        for (int i = 0; i < lines.Count(); i++)
        {
            string pattern = @"stories: '\w+";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(lines[i]);

            foreach (Match match in matches)
            {
                var newValue = match.Value.Replace("'", "’");
                lines[i] = "    stories: ''";
            }

        }


        var revertFormat = string.Join(Environment.NewLine, lines);
        return revertFormat;
    }
    private static void SaveDataToDatabase(string wP_Post_Article_InsertSql, string wP_PostMeta, string wP_term_relationships,string image_captionSql,
                                            string wP_term_relationships_category, string wP_term_relationships_tag)
    {
        try
        {

        //Prod
        string connStr = "server=nwpproduct-146b913ef7-wpdbserver.mysql.database.azure.com;user=qrdxngegwd;database=nwpproduct_146b913ef7_database;password=rgq6$jWrkQvsx3hL;";

        MySqlConnection conn = new MySqlConnection(connStr);
        conn.Open();


        var wp_post = new MySqlCommand(wP_Post_Article_InsertSql, conn);
        wp_post.ExecuteNonQuery();

        var wp_postMeta = new MySqlCommand(wP_PostMeta, conn);
        wp_postMeta.ExecuteNonQuery();


        var wP_term = new MySqlCommand(wP_term_relationships, conn);
        wP_term.ExecuteNonQuery();

        if(!string.IsNullOrEmpty(wP_term_relationships_category))
        {
            var wP_term_category = new MySqlCommand(wP_term_relationships_category, conn);
            wP_term_category.ExecuteNonQuery();
        }
        

        if (!string.IsNullOrEmpty(wP_term_relationships_tag))
        {
            var wP_term_tag = new MySqlCommand(wP_term_relationships_tag, conn);
            wP_term_tag.ExecuteNonQuery();
        }
        

        if (!string.IsNullOrEmpty(image_captionSql))
        {
            var image_caption = new MySqlCommand(image_captionSql, conn);
            image_caption.ExecuteNonQuery();
        }


        conn.Close();

        SavedCount++;
        Console.WriteLine("Saved");


        }
        catch (Exception ex)
        {

            Console.WriteLine("Error on saving");
        }

    }

    /// <summary>
    /// Todo
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void BeautifyXML(string filePath)
    {
        try
        {
            string xml = File.ReadAllText(filePath);
            XDocument doc = XDocument.Parse(xml);
            doc.ToString();

            File.WriteAllText(filePath, doc.ToString());
        }
        catch (Exception)
        {
            // Handle and throw if fatal exception here; don't just ignore them
        }
    }

    private static void DeleteFolderTypeNode(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        try
        {
            for (int i = 0; i < lines.Length; i++)
            {
                //check for filename
                if (lines[i].Contains("<sv:value>mgnl:folder</sv:value>"))
                {
                    for (int j = i; j <= i + 10; j++)
                    {
                        lines[j] = "";
                    }

                    File.WriteAllLines(filePath, lines);
                }


            }
        }
        catch (Exception)
        {

            
        }

        
    }
}



