using Binance.Net;
using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;

namespace CryptoGridTradingBot.Services
{
    public class BinanceAPIClient
    {
        private readonly BinanceClient _client;

        public BinanceAPIClient(string apiKey, string apiSecret)
        {
            // Настройка клиента для работы с Testnet
            var options = new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials(apiKey, apiSecret)
            };

            // Указываем адрес Testnet для Spot API
            options.SpotApiOptions.BaseAddress = "https://testnet.binance.vision";

            _client = new BinanceClient(options);
        }

        public BinanceClient GetClient()
        {
            return _client;
        }
    }
}