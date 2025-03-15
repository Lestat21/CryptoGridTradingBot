using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.IO;
using System.Linq;

namespace CryptoGridTradingBot.ML
{
    public class ModelTrainer
    {
        private readonly MLContext _mlContext;
        private IDataView _trainingData;
        private ITransformer _model;

        public ModelTrainer()
        {
            _mlContext = new MLContext();
        }

        // Загрузка данных из CSV
        public void LoadData(string dataPath)
        {
            _trainingData = _mlContext.Data.LoadFromTextFile<TradingData>(dataPath, hasHeader: true, separatorChar: ',');
        }

        // Обучение модели
        public void TrainModel()
        {
            var pipeline = _mlContext.Transforms.Concatenate("Features", nameof(TradingData.ATR), nameof(TradingData.RSI), nameof(TradingData.Volume))
                .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: "OptimalGridSpacing", maximumNumberOfIterations: 100));

            _model = pipeline.Fit(_trainingData);
        }

        // Сохранение модели
        public void SaveModel(string modelPath)
        {
            _mlContext.Model.Save(_model, _trainingData.Schema, modelPath);
        }

        // Дообучение модели на новых данных
        public void RetrainModel(string newDataPath)
        {
            var newData = _mlContext.Data.LoadFromTextFile<TradingData>(newDataPath, hasHeader: true, separatorChar: ',');

            // Создаем новый конвейер на основе существующей модели
            var pipeline = _mlContext.Transforms.Concatenate("Features", nameof(TradingData.ATR), nameof(TradingData.RSI), nameof(TradingData.Volume))
                .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: "OptimalGridSpacing", maximumNumberOfIterations: 100));

            // Дообучаем модель на новых данных
            var retrainedModel = pipeline.Fit(newData);
            _model = retrainedModel;
        }

        // Модель данных для обучения
        public class TradingData
        {
            public float ATR { get; set; }
            public float RSI { get; set; }
            public float Volume { get; set; }
            public float OptimalGridSpacing { get; set; }
        }
    }
}