using System.Collections.Generic;

namespace CryptoGridTradingBot.Services
{
    public class GridTradingEngine
    {
        private readonly Models.MachineLearningModel _mlModel;

        public GridTradingEngine(Models.MachineLearningModel mlModel)
        {
            _mlModel = mlModel;
        }

        public List<(double Buy, double Sell)> CalculateGridLevels(double currentPrice, double atr, double rsi, double volume, int levels)
        {
            float optimalSpacing = (float)_mlModel.PredictOptimalGridSpacing((float)atr, (float)rsi, (float)volume);
            var gridLevels = new List<(double Buy, double Sell)>();

            for (int i = 1; i <= levels; i++)
            {
                double buyPrice = currentPrice - i * optimalSpacing;
                double sellPrice = currentPrice + i * optimalSpacing;
                gridLevels.Add((buyPrice, sellPrice));
            }

            return gridLevels;
        }
    }
}