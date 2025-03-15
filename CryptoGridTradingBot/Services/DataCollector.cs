using Binance.Net;
using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects.Models.Spot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoGridTradingBot.Services
{
    public class DataCollector
    {
        private readonly BinanceClient _client;

        public DataCollector(BinanceClient client)
        {
            _client = client;
        }

        // Метод для получения текущей цены
        public async Task<double> GetPriceAsync(string symbol)
        {
            var tickerResult = await _client.SpotApi.ExchangeData.GetPriceAsync(symbol);
            if (!tickerResult.Success)
                throw new Exception($"Ошибка при получении цены: {tickerResult.Error.Message}");
            return (double)tickerResult.Data.Price;
        }

        // Метод для получения исторических данных (свечей)
        public async Task<List<IBinanceKline>> GetKlinesAsync(string symbol, KlineInterval interval, int limit)
        {
            var klinesResult = await _client.SpotApi.ExchangeData.GetKlinesAsync(symbol, interval, limit: limit);
            if (!klinesResult.Success)
                throw new Exception($"Ошибка при получении данных: {klinesResult.Error.Message}");
            return klinesResult.Data.ToList();
        }

        // Метод для расчета ATR (Average True Range)
        public async Task<double> CalculateATRAsync(string symbol, KlineInterval interval, int period)
        {
            var klines = await GetKlinesAsync(symbol, interval, period + 1);
            double sumTR = 0;

            for (int i = 1; i < klines.Count; i++)
            {
                double high = (double)klines[i].HighPrice;
                double low = (double)klines[i].LowPrice;
                double prevClose = (double)klines[i - 1].ClosePrice;

                double tr = Math.Max(high - low,
                    Math.Max(Math.Abs(high - prevClose),
                             Math.Abs(low - prevClose)));
                sumTR += tr;
            }

            return sumTR / period;
        }
        // Расчёт RSI (Relative Strength Index)
        public async Task<double> CalculateRSIAsync(string symbol, KlineInterval interval, int period)
        {
            var klines = await GetKlinesAsync(symbol, interval, period + 1);
            var closes = klines.Select(k => (double)k.ClosePrice).ToArray();

            // Реализация RSI вручную
            double[] gains = new double[closes.Length - 1];
            double[] losses = new double[closes.Length - 1];

            for (int i = 1; i < closes.Length; i++)
            {
                double change = closes[i] - closes[i - 1];
                gains[i - 1] = Math.Max(change, 0);
                losses[i - 1] = Math.Max(-change, 0);
            }

            double avgGain = gains.Take(period).Average();
            double avgLoss = losses.Take(period).Average();

            for (int i = period; i < gains.Length; i++)
            {
                avgGain = (avgGain * (period - 1) + gains[i]) / period;
                avgLoss = (avgLoss * (period - 1) + losses[i]) / period;
            }

            double rs = avgGain / avgLoss;
            double rsi = 100 - (100 / (1 + rs));

            return rsi;
        }

        // Получение объёма
        public async Task<double> GetVolumeAsync(string symbol)
        {
            var tickerResult = await _client.SpotApi.ExchangeData.GetTickerAsync(symbol);
            if (!tickerResult.Success)
                throw new Exception($"Ошибка при получении объёма: {tickerResult.Error.Message}");
            return (double)tickerResult.Data.Volume;
        }
    }
}