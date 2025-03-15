using System;
using System.Threading.Tasks;
using CryptoGridTradingBot.Services;
using CryptoGridTradingBot.Models;
using CryptoGridTradingBot.ML;
using Binance.Net.Enums;

class Program
{
    static async Task Main(string[] args)
    {
        // API Key и Secret Key для Testnet
        string apiKey = "541IJsLtaOajKRGY8NmoIZEqHgeo8BIV1lVtlUF9YYISOqjxDEVg6ja70qQC9FfQ";
        string apiSecret = "rVaYmVtCguKPhUXmIfsOvJvYetFUizZmhESIFmlpEoCNOHKBvNskCrCWcT9r0Ed1";

        var binanceClient = new BinanceAPIClient(apiKey, apiSecret).GetClient();
        var dataCollector = new DataCollector(binanceClient);
        var mlModel = new MachineLearningModel("Data/grid_model.onnx"); // Используем ONNX-модель
        var gridEngine = new GridTradingEngine(mlModel);
        var riskManager = new RiskManager(0.1); // Максимальная просадка 10%

        double initialBalance = 1000;
        double currentBalance = 1000;

        while (true)
        {
            try
            {
                Console.WriteLine("--- Начало новой итерации ---");

                // Шаг 1: Получение текущей цены
                double currentPrice = await dataCollector.GetPriceAsync("BTCUSDT");
                Console.WriteLine($"Текущая цена BTC/USDT: {currentPrice}");

                // Шаг 2: Получение волатильности (ATR)
                double atr = await dataCollector.CalculateATRAsync("BTCUSDT", KlineInterval.OneHour, 14);
                Console.WriteLine($"Волатильность (ATR): {atr}");

                // Шаг 3: Получение RSI
                double rsi = await dataCollector.CalculateRSIAsync("BTCUSDT", KlineInterval.OneHour, 14);
                Console.WriteLine($"RSI: {rsi}");

                // Шаг 4: Получение объёма
                double volume = await dataCollector.GetVolumeAsync("BTCUSDT");
                Console.WriteLine($"Объём: {volume}");

                // Шаг 5: Проверка рисков
                if (!riskManager.CheckRisk(currentBalance, initialBalance))
                {
                    Console.WriteLine("Превышена максимальная просадка. Торговля остановлена.");
                    break;
                }

                // Шаг 6: Использование LM-модели для предсказания оптимального расстояния
                Console.WriteLine("Использование LM-модели для предсказания оптимального расстояния...");
                float optimalSpacing = mlModel.PredictOptimalGridSpacing((float)atr, (float)rsi, (float)volume);
                Console.WriteLine($"Оптимальное расстояние между уровнями: {optimalSpacing}");

                // Шаг 7: Расчёт уровней сетки
                Console.WriteLine("Расчёт уровней сетки...");
                var gridLevels = gridEngine.CalculateGridLevels(currentPrice, atr, rsi, volume, 10);
                Console.WriteLine("Уровни сетки:");
                foreach (var level in gridLevels)
                {
                    Console.WriteLine($"Buy: {level.Buy}, Sell: {level.Sell}");
                }

                // Шаг 8: Сохранение результатов торговли для дообучения модели
                Console.WriteLine("Сохранение результатов торговли...");
                SaveTradingResults(atr, rsi, volume, gridLevels.First().Buy);

                // Шаг 9: Ожидание перед следующей итерацией
                Console.WriteLine("Ожидание 1 минуту...");
                await Task.Delay(60000); // 1 минута

                Console.WriteLine("--- Конец итерации ---\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }

    // Сохранение результатов торговли
    private static void SaveTradingResults(double atr, double rsi, double volume, double optimalSpacing)
    {
        using (var writer = new StreamWriter("Data/trading_results.csv", true))
        {
            writer.WriteLine($"{atr},{rsi},{volume},{optimalSpacing}");
        }
    }
}