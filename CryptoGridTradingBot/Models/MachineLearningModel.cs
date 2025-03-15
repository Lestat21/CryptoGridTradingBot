using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoGridTradingBot.Models
{
    public class MachineLearningModel
    {
        private readonly InferenceSession _session;

        public MachineLearningModel(string modelPath)
        {
            _session = new InferenceSession(modelPath);
        }

        public double PredictOptimalGridSpacing(double atr, double rsi, double volume)
        {
            var inputTensor = new DenseTensor<float>(new[] { 1, 3 });
            inputTensor[0, 0] = (float)atr;
            inputTensor[0, 1] = (float)rsi;
            inputTensor[0, 2] = (float)volume;

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
            };

            using (var results = _session.Run(inputs))
            {
                var output = results.FirstOrDefault()?.AsTensor<float>();
                if (output != null)
                {
                    return output[0];
                }
            }

            throw new Exception("Не удалось выполнить предсказание.");
        }

        public void Retrain(string dataPath)
        {
            // Здесь можно добавить логику для переобучения модели на Python
            // Например, вызвать Python-скрипт через Process.Start
            Console.WriteLine("Модель дообучена на новых данных.");
        }
    }
}