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
            // Загрузка ONNX-модели
            _session = new InferenceSession(modelPath);
        }

        public float PredictOptimalGridSpacing(float atr, float rsi, float volume)
        {
            // Подготовка входных данных
            var inputTensor = new DenseTensor<float>(new[] { 1, 3 }); // 1 строка, 3 признака
            inputTensor[0, 0] = atr;
            inputTensor[0, 1] = rsi;
            inputTensor[0, 2] = volume;

            // Создание входных данных для модели
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
            };

            // Выполнение предсказания
            using (var results = _session.Run(inputs))
            {
                var output = results.FirstOrDefault()?.AsTensor<float>();
                if (output != null)
                {
                    return output[0]; // Возвращаем первое значение из вывода модели
                }
            }

            throw new Exception("Не удалось выполнить предсказание.");
        }
    }
}