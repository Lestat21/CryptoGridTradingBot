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
        var orderManager = new OrderManager(binanceClient);

        double initialBalance = 1000;
        double currentBalance = 1000;
        var testPair = "DOGEUSDT";

        while (true)
        {
            try
            {
                Console.WriteLine("--- Начало новой итерации ---");

                // Шаг 1: Получение данных
                double currentPrice = await dataCollector.GetPriceAsync(testPair);
                Console.WriteLine($"Текущая цена {testPair}: {currentPrice}");

                double atr = await dataCollector.CalculateATRAsync(testPair, KlineInterval.OneHour, 14);
                Console.WriteLine($"Волатильность (ATR): {atr}");

                double rsi = await dataCollector.CalculateRSIAsync(testPair, KlineInterval.OneHour, 14);
                Console.WriteLine($"RSI: {rsi}");

                double volume = await dataCollector.GetVolumeAsync(testPair);
                Console.WriteLine($"Объём: {volume}");

                // Шаг 2: Предсказание оптимального расстояния
                double optimalSpacing = mlModel.PredictOptimalGridSpacing(atr, rsi, volume);
                Console.WriteLine($"Оптимальное расстояние между уровнями: {optimalSpacing}");

                // Шаг 3: Расчёт уровней сетки
                var gridLevels = gridEngine.CalculateGridLevels(currentPrice, atr, rsi, volume, 10);
                Console.WriteLine("Уровни сетки:");
                foreach (var level in gridLevels)
                {
                    Console.WriteLine($"Buy: {level.Buy}, Sell: {level.Sell}");
                }

                // Шаг 4: Перестановка ордеров
                await orderManager.CancelAndReplaceOrders(testPair, gridLevels);

                // Шаг 5: Фиксация сделок
                var tradeResult = await orderManager.CheckTrades(testPair);
                if (tradeResult != null)
                {
                    dataCollector.SaveTradeResult(tradeResult);
                }

                // Шаг 6: Дообучение модели (например, раз в день)
                if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0) // Каждый день в полночь
                {
                    mlModel.Retrain("Data/trading_results.csv");
                }

                // Ожидание перед следующей итерацией
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
}