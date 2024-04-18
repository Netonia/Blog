namespace Blog.Services;

public class PostService : IPostService
{
    private readonly HttpClient _client;
    private const string _basePostPath = "assets/posts/documents/";

    public PostService(HttpClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<string>> GetAllPostNamesAsync()
    {
        var response = await _client.GetFromJsonAsync<JsonElement>("assets/posts/post-list.json");
        var postList = new List<string>();
        foreach (var post in response.GetProperty("posts").EnumerateArray())
        {
            postList.Add(post.GetString());
        }

        return postList;
    }

    public async Task<Post> GetPostAsync(string filename)
    {
        // Retrieve and convert to string the file from the specified path
        var fileString = await _client.GetStringAsync(_basePostPath + filename + ".md");

        var post = await GetPostMetadataAsync(filename);

        var pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();

        post.Body = Markdown.ToHtml(fileString, pipeline);

        return post;
    }

    public async Task<Post> GetPostMetadataAsync(string filename)
    {
        // Retrieve and convert to string the file from the specified path
        var byteArray = await _client.GetByteArrayAsync(_basePostPath + filename + ".md");
        var fileString = Encoding.Latin1.GetString(byteArray, 0, byteArray.Length);

        var pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .Build();

        var document = Markdown.Parse(fileString, pipeline);

        var yamlLines = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault()
            .Lines.ToString();

        var yamlDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        var post = yamlDeserializer.Deserialize<Post>(yamlLines);
        return post;
    }
}
