using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGeneticLearning
{
    public static class Extensions
    {
        public static double NextDouble(this Random random, double min, double max)
        {
            return random.NextDouble() * (max - min) + min;
        }
    }
    class Generation
    {

        public Individual[] population;
        public Generation(int popcount)
        {
            population = new Individual[popcount];
        }
        public void InitializePopulation(int[] NeuronsPerLayer, int Inputs, Random random)
        {
            for (int i = 0; i < population.Length; i++)
            {
                population[i] = new Individual(NeuronsPerLayer, Inputs, random);
            }
        }
        void Crossover(NeuralNetwork winner, NeuralNetwork loser, Random random)
        {
            for (int i = 0; i < winner.layers.Length; i++)
            {
                //References to the Layers
                Layer winLayer = winner.layers[i];
                Layer childLayer = loser.layers[i];

                int cutPoint = random.Next(winLayer.Neurons.Length); //calculate a cut point for the layer
                bool flip = random.Next(2) == 0; //randomly decide which side of the cut point will come from winner

                //Either copy from 0->cutPoint or cutPoint->Neurons.Length from the winner based on the flip variable
                for (int j = (flip ? 0 : cutPoint); j < (flip ? cutPoint : winLayer.Neurons.Length); j++)
                {
                    //References to the Neurons
                    Neuron winNeuron = winLayer.Neurons[j];
                    Neuron childNeuron = childLayer.Neurons[j];

                    //Copy the winners Weights and Bias into the loser/child neuron
                    for (int k = 0; k < winNeuron.dendrites.Length; k++)
                    {
                        childNeuron.dendrites[0].Weight = winNeuron.dendrites[k].Weight;
                    }
                    childNeuron.bias = winNeuron.bias;
                }
            }

        }
        void Mutate(NeuralNetwork net, Random random, double mutationRate)
        {
            foreach (Layer layer in net.layers)
            {
                foreach (Neuron neuron in layer.Neurons)
                {
                    //Mutate the Weights
                    for (int i = 0; i < neuron.dendrites.Length; i++)
                    {
                        if (random.NextDouble() < mutationRate)
                        {
                            if (random.Next(2) == 0)
                            {
                                neuron.dendrites[i].Weight *= (random.NextDouble() + 0.5); //scale weight
                            }
                            else
                            {
                                neuron.dendrites[i].Weight *= -1; //flip sign
                            }
                        }
                    }

                    //Mutate the Bias
                    if (random.NextDouble() < mutationRate)
                    {
                        if (random.Next(2) == 0)
                        {
                            neuron.bias *= (random.NextDouble() + 0.5); //scale weight
                        }
                        else
                        {
                            neuron.bias *= -1; //flip sign
                        }
                    }
                }
            }
        }

        public void FitnessTest(double[] fitness)
        {
            for (int i = 0; i < population.Length; i++)
            {
                population[i].fitness = fitness[i];
            }

        }
        //Trains Generation and returns best Individual
        public Individual GenerationTraining(Random random, double mrate)
        {
           // FitnessTest(fitness);
            Array.Sort(population, (a, b) => b.fitness.CompareTo(a.fitness));
            //splits the population between top 10% and the rest
            ;
            int start = (int)(population.Length * .1);
            int end = (int)(population.Length * .9);
            // crosses over the midlle 80% with the top 10%
            for (int i = start; i < end; i++)
            {
                Crossover(population[random.Next(start)].network, population[i].network, random);
                Mutate(population[i].network, random, mrate);
            }
            ;
            //Randomizes the worst 10%
            for (int i = end; i < population.Length; i++)
            {
                population[i].network.Randomize(random);
            }
            return population[0];
        }
    }
    class Individual
    {
        public double fitness;
        public NeuralNetwork network;
        public static double Sigmoid(double x)
        {
            return 1 / (1 + Math.Pow(Math.E, -x));
        }
        ErrorFunction errorfunc = new ErrorFunction((input, output) => (Math.Pow(input - output, 2)), (input, output) => (2 * (input - output)));
        ActivationFunction acfunc = new ActivationFunction((input) => (Math.Tanh(input)), (input) => (0));
        public Individual(int[] neuronperlayer, int inputsize, Random random)
        {
            network = new NeuralNetwork(acfunc, errorfunc, inputsize, neuronperlayer);
            network.Randomize(random);

        }
    }
    class Dendrite
    {
        public double weightupdate { get; set; }
        public Neuron Previous { get; }
        public Neuron Next { get; }
        public double Weight { get; set; }

        public Dendrite(Neuron previous, Neuron next, double weight)
        {
            Previous = previous;
            Next = next;
            Weight = weight;
        }
        public void ApplyUpdate()
        {
            Weight += weightupdate;
            weightupdate = 0;
        }
        public double Compute()
        {
            return Previous.Output * Weight;
        }
    }
    class Neuron
    {
        public double bias;
        public double biasupdate;
        public double delta { get; set; }

        public Dendrite[] dendrites;
        public double Output { get; set; }
        public double Input { get; private set; }
        public ActivationFunction Activation { get; set; }

        public Neuron(ActivationFunction activation, Neuron[] previousNeurons)
        {
            Activation = activation;
            dendrites = new Dendrite[previousNeurons.Length];
            for (int i = 0; i < previousNeurons.Length; i++) dendrites[i] = new Dendrite(previousNeurons[i], null, 0);
        }

        public void Randomize(Random random)
        {
            bias = random.NextDouble(-7, 7);

            foreach (var i in dendrites)
            {
                i.Weight = random.NextDouble(-7, 7);

            }

        }
        public void ApplyUpdate()
        {
            foreach (var i in dendrites) i.ApplyUpdate();
            bias += biasupdate;
            biasupdate = 0;
        }
        public void backprop(double learningrate)
        {
            double biasderivative = Activation.Derivative(Input) * delta;
            delta = 0;
            biasupdate += biasderivative * -learningrate;
            foreach (var i in dendrites)
            {
                i.weightupdate += -learningrate * biasderivative * i.Previous.Output;
                ;
            }
        }
        public double Compute(int layerindex)
        {
            Output = 0;
            Input = bias;
            foreach (var i in dendrites)
            {
                Input += i.Compute();
            }
            Output = Activation.Function(Input);
            return Output;
        }
    }
    class Layer
    {
        public Neuron[] Neurons { get; set; }
        public double[] Outputs { get; set; }

        public Layer(ActivationFunction activation, int neuronCount, Layer previousLayer)
        {
            Outputs = new double[neuronCount];
            Neurons = new Neuron[neuronCount];
            for (int i = 0; i < Neurons.Length; i++) Neurons[i] = new Neuron(activation, previousLayer.Neurons);
        }
        public void Randomize(Random random)
        {
            foreach (var i in Neurons) i.Randomize(random);
        }
        public void ApplyUpdate()
        {
            foreach (var i in Neurons) i.ApplyUpdate();

        }
        public void backprop(double learningrate)
        {
            foreach (var i in Neurons) i.backprop(learningrate);
        }
        public double[] Compute(int layerindex)
        {

            for (int i = 0; i < Outputs.Length; i++) Outputs[i] = Neurons[i].Compute(layerindex);
            return Outputs;
        }
    }
    class NeuralNetwork
    {
        public Layer[] layers;
        ErrorFunction errorFunc;
        Layer Inputlayer;

        public NeuralNetwork(ActivationFunction activation, ErrorFunction ErrorFunc, int inputsize,
        params int[] neuronsPerLayer)
        {
            Inputlayer = new Layer(activation, inputsize, new Layer(activation, 0, null));
            errorFunc = ErrorFunc;
            layers = new Layer[neuronsPerLayer.Length];
            for (int i = 0; i < neuronsPerLayer.Length; i++)
            {
                if (i >= 1) layers[i] = new Layer(activation, neuronsPerLayer[i], layers[i - 1]);
                else
                {
                    layers[i] = new Layer(activation, neuronsPerLayer[i], Inputlayer);
                }
            }
        }
        public void Randomize(Random random)
        {
            foreach (var i in layers) i.Randomize(random);
        }
        public void ApplyUpdate()
        {
            foreach (var i in layers) i.ApplyUpdate();
        }
        public void backprop(double learningrate, double[] desiredoutputs)
        {
            for (int i = layers.Length - 1; i >= 0; i--)
            {
                if (i == layers.Length - 1)
                {
                    for (int k = 0; k < layers[i].Neurons.Length; k++)
                    {
                        layers[i].Neurons[k].delta += errorFunc.Derivative(layers[i].Neurons[k].Compute(0), desiredoutputs[k]);
                    }
                }
                else
                {
                    for (int k = 0; k < layers[i].Neurons.Length; k++)
                    {
                        foreach (var j in layers[i + 1].Neurons)
                        {
                            foreach (var l in j.dendrites)
                            {
                                layers[i].Neurons[k].delta += layers[i].Neurons[k].Activation.Derivative(layers[i].Neurons[k].Input) * j.delta * l.Weight;
                            }

                        }

                    }
                }
            }
            for (int i = layers.Length - 1; i >= 0; i--) layers[i].backprop(learningrate);
        }
        public double Train(double[][] inputs, double[][] desiredoutputs, double learningrate)
        {
            double averageerror = 0;
            for (int i = 0; i < inputs.Length; i++)
            {
                averageerror += GetError(inputs[i], desiredoutputs[i]);
                backprop(learningrate, desiredoutputs[i]);
            }
            ApplyUpdate();
            averageerror /= inputs.Length;
            return averageerror;
        }
        public double[] Compute(double[] inputs)
        {
            Inputlayer.Outputs = inputs;
            // Inputlayer.Neurons = new Neuron[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                // Inputlayer.Neurons[i] = new Neuron(null, new Neuron[0]);
                Inputlayer.Neurons[i].Output = inputs[i];

            }
            ;
            for (int i = 0; i < layers.Length; i++)
            {
                if (i + 1 != layers.Length) layers[i].Compute(layers.Length - (i + 1));
                else return layers[i].Compute(layers.Length - (i + 1));
            }
            return null;
        }
        public double GetError(double[] inputs, double[] desiredOutputs)
        {
            double error = 0;
            double[] outputs = Compute(inputs);
            for (int i = 0; i < desiredOutputs.Length; i++) error += errorFunc.Function(outputs[i], desiredOutputs[i]);
            return error;
        }
    }
    class ActivationFunction
    {
        Func<double, double> function;
        Func<double, double> derivative;
        public ActivationFunction(Func<double, double> Function, Func<double, double> Derivative)
        {
            if (Function == null || Derivative == null)
            {
                throw new NullReferenceException();
            }
            function = Function;
            derivative = Derivative;
        }

        public double Function(double input)
        {
            return function(input);
        }

        public double Derivative(double input)
        {
            return derivative(input);
        }
    }
    class ErrorFunction
    {
        Func<double, double, double> function;
        Func<double, double, double> derivative;
        public ErrorFunction(Func<double, double, double> Function, Func<double, double, double> Derivative)
        {
            if (Function == null || Derivative == null)
            {
                throw new NullReferenceException();
            }
            function = Function;
            derivative = Derivative;
        }

        public double Function(double output, double desiredOutput)
        {
            return function(output, desiredOutput);
        }
        public double Derivative(double output, double desiredOutput)
        {
            return derivative(output, desiredOutput);
        }
    }
}
