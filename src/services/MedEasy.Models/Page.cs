namespace MedEasy.Models;
using System.Collections.Generic;

public class PageModel<T> where T : notnull
{
    public int Page { get; set; }

    public int Total { get; set; }

    public int Count { get; set; }

    public int PageSize { get; set; }

    public IEnumerable<T> Items { get; set; }
}
