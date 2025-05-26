using MarketViewer.Clients.Interfaces;
using MarketViewer.Contracts.Requests.Management.Strategy;
using MarketViewer.Contracts.Requests.Management.Trade;
using Microsoft.Extensions.Logging;

namespace MarketViewer.Clients.Examples;

/// <summary>
/// Example class demonstrating how to use the MarketViewer clients
/// </summary>
public class ClientUsageExample(
    IStrategyClient strategyClient,
    ITradeClient tradeClient,
    IUserClient userClient,
    ILogger<ClientUsageExample> logger)
{
    private readonly IStrategyClient _strategyClient = strategyClient;
    private readonly ITradeClient _tradeClient = tradeClient;
    private readonly IUserClient _userClient = userClient;
    private readonly ILogger<ClientUsageExample> _logger = logger;

    /// <summary>
    /// Example of working with strategies
    /// </summary>
    public async Task StrategyExampleAsync()
    {
        try
        {
            // Set authentication token
            _strategyClient.SetAuthorizationToken("your-jwt-token");

            // Create a new strategy
            var createRequest = new StrategyPutRequest
            {
                Name = "Example Strategy",
                Enabled = true,
                Public = false
                // Add other required properties based on your StrategyPutRequest model
            };

            var createdStrategy = await _strategyClient.CreateAsync(createRequest);
            if (createdStrategy != null)
            {
                _logger.LogInformation("Created strategy with ID: {StrategyId}", createdStrategy.Id);

                // Get the strategy
                var retrievedStrategy = await _strategyClient.GetAsync(createdStrategy.Id);
                
                // Update the strategy
                createRequest.Name = "Updated Strategy Name";
                var updatedStrategy = await _strategyClient.UpdateAsync(createdStrategy.Id, createRequest);

                // List strategies
                var listRequest = new StrategyListRequest();
                var strategies = await _strategyClient.ListAsync(listRequest);
                _logger.LogInformation("Found {Count} strategies", strategies?.Count() ?? 0);

                // Delete the strategy
                var deleteSuccess = await _strategyClient.DeleteAsync(createdStrategy.Id);
                _logger.LogInformation("Strategy deletion success: {Success}", deleteSuccess);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in strategy example");
        }
    }

    /// <summary>
    /// Example of working with trades
    /// </summary>
    public async Task TradeExampleAsync()
    {
        try
        {
            // Set authentication token
            _tradeClient.SetAuthorizationToken("your-jwt-token");

            // Open a trade
            var openRequest = new TradeOpenRequest
            {
                StrategyId = "strategy-id",
                Ticker = "AAPL",
                Shares = 100,
                OpenedAt = DateTimeOffset.Now.ToString(),
                EntryPrice = 150.00f,
                EntryPosition = 15000.00f
                // Add other required properties
            };

            var openSuccess = await _tradeClient.OpenAsync(openRequest);
            if (openSuccess)
            {
                _logger.LogInformation("Trade opened successfully");

                // List trades for a user
                var listRequest = new TradeListRequest
                {
                    User = "user-id"
                };
                var trades = await _tradeClient.ListAsync(listRequest);
                _logger.LogInformation("Found {Count} trades", trades?.Count() ?? 0);

                // Close a trade (assuming we have a trade ID)
                if (trades?.Any() == true)
                {
                    var tradeToClose = trades.First();
                    var closeRequest = new TradeCloseRequest
                    {
                        ClosedAt = DateTimeOffset.Now.ToString(),
                        ClosePrice = 155.00f,
                        ClosePosition = 15500.00f,
                        Profit = 500.00f
                    };

                    var closeSuccess = await _tradeClient.CloseAsync(tradeToClose.Id, closeRequest);
                    _logger.LogInformation("Trade closure success: {Success}", closeSuccess);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in trade example");
        }
    }

    /// <summary>
    /// Example of working with users
    /// </summary>
    public async Task UserExampleAsync()
    {
        try
        {
            // Get a specific user
            var user = await _userClient.GetAsync("user-id");
            if (user != null)
            {
                _logger.LogInformation("Retrieved user: {UserId}", user.Id);
            }

            // List all users
            var users = await _userClient.ListAsync();
            _logger.LogInformation("Found {Count} users", users?.Count() ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in user example");
        }
    }

    /// <summary>
    /// Example of clearing authentication tokens
    /// </summary>
    public void ClearAuthenticationExample()
    {
        _strategyClient.ClearAuthorizationToken();
        _tradeClient.ClearAuthorizationToken();
        _userClient.ClearAuthorizationToken();
        
        _logger.LogInformation("Cleared authentication tokens for all clients");
    }
} 