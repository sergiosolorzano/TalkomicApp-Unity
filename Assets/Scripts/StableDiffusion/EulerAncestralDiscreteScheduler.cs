﻿using Microsoft.ML.OnnxRuntime.Tensors;
using NumSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StableDiffusion
{
    public class EulerAncestralDiscreteScheduler : SchedulerBase
    {
        private readonly string _predictionType;
        public override float InitNoiseSigma { get; set; }
        public int num_inference_steps;
        public override List<int> Timesteps { get; set; }
        public override Tensor<float> Sigmas { get; set; }

        public EulerAncestralDiscreteScheduler(
            int num_train_timesteps = 1000,
            float beta_start = 0.00085f,
            float beta_end = 0.012f,
            string beta_schedule = "scaled_linear",
            List<float> trained_betas = null,
            string prediction_type = "epsilon"
        ) : base(num_train_timesteps)
        {
            var alphas = new List<float>();
            var betas = new List<float>();
            _predictionType = prediction_type;

            if (trained_betas != null)
            {
                betas = trained_betas;
            }
            else if (beta_schedule == "linear")
            {
                betas = Enumerable.Range(0, num_train_timesteps).Select(i => beta_start + (beta_end - beta_start) * i / (num_train_timesteps - 1)).ToList();
            }
            else if (beta_schedule == "scaled_linear")
            {
                var start = (float)Math.Sqrt(beta_start);
                var end = (float)Math.Sqrt(beta_end);
                betas = np.linspace(start, end, num_train_timesteps).ToArray<float>().Select(x => x * x).ToList();
            }
            else
            {
                throw new Exception("beta_schedule must be one of 'linear' or 'scaled_linear'");
            }

            alphas = betas.Select(beta => 1 - beta).ToList();

            this._alphasCumulativeProducts = alphas.Select((alpha, i) => alphas.Take(i + 1).Aggregate((a, b) => a * b)).ToList();
            // Create sigmas as a list and reverse it
            var sigmas = _alphasCumulativeProducts.Select(alpha_prod => Math.Sqrt((1 - alpha_prod) / alpha_prod)).Reverse().ToList();

            // standard deviation of the initial noise distrubution
            this.InitNoiseSigma = (float)sigmas.Max();
        }

        public override int[] SetTimesteps(int num_inference_steps)
        {
            double start = 0;
            double stop = _numTrainTimesteps - 1;
            double[] timesteps = np.linspace(start, stop, num_inference_steps).ToArray<double>();

            this.Timesteps = timesteps.Select(x => (int)x).Reverse().ToList();

            var sigmas = _alphasCumulativeProducts.Select(alpha_prod => Math.Sqrt((1 - alpha_prod) / alpha_prod)).Reverse().ToList();
            var range = np.arange((double)0, (double)(sigmas.Count)).ToArray<double>();
            sigmas = Interpolate(timesteps, range, sigmas).ToList();
            this.InitNoiseSigma = (float)sigmas.Max();
            this.Sigmas = new DenseTensor<float>(sigmas.Count());
            for (int i = 0; i < sigmas.Count(); i++)
            {
                this.Sigmas[i] = (float)sigmas[i];
            }

            return this.Timesteps.ToArray();
        }

        public override DenseTensor<float> Step(Tensor<float> modelOutput,
               int timestep,
               Tensor<float> sample,
               int order = 4
            )
        {

            if (!this.is_scale_input_called)
            {
                UnityEngine.Debug.Log(
                    "The `scale_model_input` function should be called before `step` to ensure correct denoising. " +
                    "See `StableDiffusionPipeline` for a usage example."
                );
            }

            var a1 = sample.ToArray();

            int stepIndex = this.Timesteps.IndexOf((int)timestep);
            var sigma = this.Sigmas[stepIndex];

            // 1. compute predicted original sample (x_0) from sigma-scaled predicted noise
            Tensor<float> predOriginalSample = null;
            if (this._predictionType == "epsilon")
            {
                //  pred_original_sample = sample - sigma * model_output
                predOriginalSample = TensorHelper.SubtractTensors(sample, TensorHelper.MultipleTensorByFloat(modelOutput, sigma));
                var a = predOriginalSample.ToArray();
            }
            else if (this._predictionType == "v_prediction")
            {
                // * c_out + input * c_skip
                //predOriginalSample = modelOutput * (-sigma / Math.Pow(sigma * sigma + 1, 0.5)) + (sample / (sigma * sigma + 1));
                throw new NotImplementedException($"prediction_type not implemented yet: {_predictionType}");
            }
            else if (this._predictionType == "sample")
            {
                throw new NotImplementedException($"prediction_type not implemented yet: {_predictionType}");
            }
            else
            {
                throw new ArgumentException(
                    $"prediction_type given as {this._predictionType} must be one of `epsilon`, or `v_prediction`"
                );
            }

            float sigmaFrom = this.Sigmas[stepIndex];
            float sigmaTo = this.Sigmas[stepIndex + 1];

            var sigmaFromLessSigmaTo = (MathF.Pow(sigmaFrom, 2) - MathF.Pow(sigmaTo, 2));
            var sigmaUpResult = (MathF.Pow(sigmaTo, 2) * sigmaFromLessSigmaTo) / MathF.Pow(sigmaFrom, 2);
            var sigmaUp = sigmaUpResult < 0 ? -MathF.Pow(MathF.Abs(sigmaUpResult), 0.5f) : MathF.Pow(sigmaUpResult, 0.5f);

            var sigmaDownResult = (MathF.Pow(sigmaTo, 2) - MathF.Pow(sigmaUp, 2));
            var sigmaDown = sigmaDownResult < 0 ? -MathF.Pow(MathF.Abs(sigmaDownResult), 0.5f) : MathF.Pow(sigmaDownResult, 0.5f);

            // 2. Convert to an ODE derivative
            var sampleMinusPredOriginalSample = TensorHelper.SubtractTensors(sample, predOriginalSample);
            var b = sampleMinusPredOriginalSample.ToArray();
            DenseTensor<float> derivative = TensorHelper.DivideTensorByFloat(sampleMinusPredOriginalSample.ToArray(), sigma, predOriginalSample.Dimensions.ToArray());// (sample - predOriginalSample) / sigma;
            var c = derivative.ToArray();
            float dt = sigmaDown - sigma;

            DenseTensor<float> prevSample = TensorHelper.AddTensors(sample, TensorHelper.MultipleTensorByFloat(derivative, dt));// sample + derivative * dt;
            var d= prevSample.ToArray();

            //var noise = generator == null ? np.random.randn(modelOutput.shape) : np.random.RandomState(generator).randn(modelOutput.shape);
            var noise = TensorHelper.GetRandomTensor(prevSample.Dimensions);

            var e = noise.ToArray();

            var noiseSigmaUpProduct = TensorHelper.MultipleTensorByFloat(noise, sigmaUp);
            prevSample = TensorHelper.AddTensors(prevSample, noiseSigmaUpProduct);// prevSample + noise * sigmaUp;

            var f = prevSample.ToArray();

            return prevSample;
        }
    }
}