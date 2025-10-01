namespace LorArchApi.Models;

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public List<Link> Links { get; set; } = new();
}

public class Link
{
    public string Rel { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    
    public Link() { }
    
    public Link(string rel, string href, string method)
    {
        Rel = rel;
        Href = href;
        Method = method;
    }
}