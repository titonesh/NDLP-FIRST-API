using System;
using System.Collections.Generic;

namespace NDLP_FIRST_API.Models.DTOs
{
    public class PagedResponseDto<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / (PageSize > 0 ? PageSize : 1));
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;

        public PagedResponseDto(IEnumerable<T> items, int pageNumber, int pageSize, int totalRecords)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalRecords = totalRecords;
        }
    }
}
