using MarketViewer.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketViewer.Api.Controllers.Management;

[ApiController]
[Route("api/user")]
public class UserController(UserRepository repository, ILogger<UserController> logger) : Controller
{

    //[HttpGet]
    //[Route("{userId}")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status404NotFound)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> Read([FromRoute] string userId)
    //{
    //    try
    //    {
    //        var orders = await repository.GetTradesByUser(userId);

    //        return Ok(new GetOrdersResponse
    //        {
    //            Orders = orders
    //        });
    //    }
    //    catch (Exception e)
    //    {
    //        logger.LogError("Exception: {message}", e.Message);
    //        return StatusCode(StatusCodes.Status500InternalServerError, new GetOrdersResponse
    //        {
    //            Orders = []
    //        });
    //    }
    //}

    //[HttpGet]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //public async Task<IActionResult> List()
    //{
    //    try
    //    {
    //        var orders = await repository.ListUsers("asdf");

    //        return Ok(orders);
    //    }
    //    catch (Exception e)
    //    {
    //        logger.LogError("Exception: {message}", e.Message);
    //        return StatusCode(StatusCodes.Status500InternalServerError, new GetOrdersResponse
    //        {
    //            Orders = []
    //        });
    //    }
    //}
}
