using NWP_DB_Migration.Article;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static System.Net.Mime.MediaTypeNames;


List<string[]> articleList = new List<string[]>();
articleList.Add(new[] { "28,197" });

var fileName = @"C:\temp\stories.nwp.2025.9.yaml";

//1.
//ProcessAuthors(fileName, "author:");

//2.
//GenerateInsertSql(articleList, fileName);


//3.
//Extract fetured image
ExtractFeaturedImage();

void CreatePostInsertSql(Post post){

    int PostID = 2406;
    int ImageId = 2405;


    string WP_Post_Article_InsertSql = $"INSERT INTO `wp_posts` ( `ID`,`post_author`, `post_date`, `post_date_gmt`, `post_content`, `post_title`, `post_excerpt`, `post_status`, `comment_status`, `ping_status`, `post_password`, `post_name`, `to_ping`, `pinged`, `post_modified`, `post_modified_gmt`, `post_content_filtered`, `post_parent`, `guid`, `menu_order`, `post_type`, `post_mime_type`, `comment_count`) " +
                          $"VALUES({PostID} ,'{getPostAuthorID(post.author)}', '{formatDateTime(post.created)}', '{formatDateTime(post.created)}', '{getPostContent(post)}', '{post.title}', '{post.caption}', '{getPostStatus(post)}', 'open', 'open', '', '{post.title.Replace(" ", "-")}', '', '', '{formatDateTime(post.lastmodified)}', '{formatDateTime(post.lastmodified)}', '', 0, 'https://newswatchplus-staging.azurewebsites.net/?p=', 0, 'post', '', 0);";


   
    string WP_PostMeta = $"INSERT INTO `wp_postmeta` ( `post_id`, `meta_key`, `meta_value`) VALUES( {PostID}, '_thumbnail_id', '{GetPostMetaValue(post.imagesource)}');";

}


void GenerateInsertSql(List<string[]> articleList, string fileName)
{
    foreach (var range in articleList)
    {
        // Split the string by comma and convert each part to an integer
        int[] intArray = range.FirstOrDefault().Split(',')
                        .Select(s => int.Parse(s.Trim())) // Trim to handle potential whitespace
                        .ToArray();

        int startLine = intArray[0];
        int endLine = intArray[1];

        List<string> articles = GetArticles(fileName, startLine, endLine);
        string yml = string.Join("\n", articles);

        //start parsing
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
            .Build();


        //yml contains a string containing your YAML
        var p = deserializer.Deserialize<Post>(yml);
        if (p != null)
        {
            CreatePostInsertSql(p);
        }
    }
}

int GetPostMetaValue(string imagesource)
{
    return 2412;
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
    AuthorsList AuthorsList = new AuthorsList();
    var ID = AuthorsList.GetAuthors().FirstOrDefault(s => s.Author == name).ID;
    return ID;
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

    //var resourceDirectory = @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Assets\nwp\2024";
    //var resourceDirectory = @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Assets\nwp\2024";

    // Get all subdirectories in the specified path
    //string[] directories = Directory.GetDirectories(resourceDirectory);
    string[] directories = { @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Assets\nwp\2024\5",
                             @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Assets\nwp\2024\6",
                             @"C:\Users\jervi\Desktop\Newswatchplus\db migration\NWP\NWP\Assets\nwp\2024\7"};

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
            for (int i = 0; i < lines.Length; i++)
            { 
                if (lines[i].Contains("        <sv:property sv:name=\"jcr:uuid\" sv:type=\"String\">"))
                {
                    
                    var imageSource = lines[i+1].Replace("<sv:value>", "").Replace("</sv:value>", "").Trim();

                    // Use File.ReadLines to read lines lazily and efficiently
                    var matchingLines = lines.Select((line, index) => new { LineText = line, LineNumber = index + 1 })
                                            .Where(item => item.LineText.Contains(imageSource));

                    string ImageFileName = string.Empty;
                    if (matchingLines.Any())
                    {
                        var startLine = matchingLines.FirstOrDefault().LineNumber;

                        int skipCount = startLine - 1;
                        int takeCount = 70;

                        // Skip the lines before the start of the range, then take the specified number of lines.
                        var ImageRange = File.ReadLines(filePath)
                                   .Skip(skipCount)
                                   .Take(takeCount)
                                   .ToList();

                        bool IsFound = false;

                        foreach (var item in ImageRange)
                        {
                            if (IsFound)
                            {
                                //convert base 64 string to Jpg
                                Base64StringToJpeg(item.Replace("<sv:value>", "").Replace("</sv:value>", ""), imageSource, subpath);
                                break;
                            }
                            else if (item.Contains("<sv:property sv:name=\"jcr:data\" sv:type=\"Binary\">"))
                            {
                                IsFound = true;
                                continue;
                            }
                        }
                    }
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
