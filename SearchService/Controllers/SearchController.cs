using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> SearchItems([FromQuery] SearchParams searchParams)
        {
            try
            {
                var query = DB.PagedSearch<Item, Item>();

                // Apply search term
                if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
                {
                    query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
                }

                // Apply sorting
                query = searchParams.OrderBy switch
                {
                    "make" => query.Sort(u => u.Ascending(a => a.Make)),
                    "new" => query.Sort(u => u.Descending(a => a.CreatedAt)),
                    _ => query.Sort(u => u.Ascending(a => a.AuctionEnd)),
                };

                // Apply filtering
                query = searchParams.FilterBy switch
                {
                    "finished" => query.Match(u => u.AuctionEnd < DateTime.UtcNow),
                    "endingsoon" => query.Match(u => u.AuctionEnd < DateTime.UtcNow.AddHours(6) && u.AuctionEnd > DateTime.UtcNow),
                    _ => query.Match(u => u.AuctionEnd > DateTime.UtcNow),
                };

                // Apply additional filters
                if (!string.IsNullOrEmpty(searchParams.Seller))
                {
                    query.Match(u => u.Seller == searchParams.Seller);
                }
                if (!string.IsNullOrEmpty(searchParams.Winner))
                {
                    query.Match(u => u.Winner == searchParams.Winner);
                }

                // Apply pagination
                query.PageNumber(searchParams.PageNumber);
                query.PageSize(searchParams.PageSize);

                // Execute the query
                var result = await query.ExecuteAsync();

                // Return the results with items
                return Ok(new
                {
                    results = result.Results, // This should be the list of items
                    pageCount = result.PageCount,
                    totalCount = result.TotalCount
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return StatusCode(500, "An error occurred while searching for items.");
            }
        }
       /* public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
        {
            var query = DB.PagedSearch<Item, Item>();


            if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }
            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(u => u.Ascending(a => a.Make)),
                "new" => query.Sort(u => u.Descending(a => a.CreatedAt)),
                _ => query.Sort(u => u.Ascending(a => a.AuctionEnd)),
            };

            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(u => u.AuctionEnd < DateTime.UtcNow),
                "endingsoon" => query.Match(u => u.AuctionEnd < DateTime.UtcNow.AddHours(6)
                && u.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(u => u.AuctionEnd > DateTime.UtcNow)
            };

            if (!string.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(u => u.Seller == searchParams.Seller);
            }
            if (!string.IsNullOrEmpty(searchParams.Winner))
            {
                query.Match(u => u.Winner == searchParams.Winner);
            }

            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);
            var result = await query.ExecuteAsync();

            return Ok(new
            {
                results = result,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
        }*/
    }
}
