using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using Geodesy;

namespace Struthio
{
    class Genetic
    {
        public Genetic(Image image)
        {
            float minRollError = -30f;
            float maxRollError = 30f;

            float minPitchError = -30f;
            float maxPitchError = 30f;

            float minYawError = -25f;
            float maxYawError = 25f;

            float minAltError = -30f;
            float maxAltError = 30f;

            float minUtmEastError = -10f;
            float maxUtmEastError = 10;

            float minUtmWestError = -10f;
            float maxUtmWestError = 10;

            var chromosome = new FloatingPointChromosome(
                new double[] { minRollError, minPitchError, minYawError, minAltError, minUtmEastError, minUtmWestError },
                new double[] { maxRollError, maxPitchError, maxYawError, maxAltError, maxUtmEastError, maxUtmWestError },
                new int[] { 64, 64, 64, 64, 64, 64 },
                new int[] { 5, 5, 5, 5, 5, 5 });

            var population = new Population(15000, 200000, chromosome);

            var fitness = new FuncFitness((c) =>
            {
                var fc = c as FloatingPointChromosome;

                var values = fc.ToFloatingPoints();
                var v = new VariableErrors(values);

                image.georeferenceAllDatapoints(v);

                return 1 / image.calcAverageDistanceError();
            });

            var selection = new RouletteWheelSelection();
            var crossover = new ThreeParentCrossover();
            var mutation = new UniformMutation();
            var termination = new FitnessStagnationTermination(200);

            var ga = new GeneticAlgorithm(
                population,
                fitness,
                selection,
                crossover,
                mutation);

            ga.Termination = termination;

            var latestFitness = 0.0;

            Console.WriteLine(
                        "Initial Error {0,2}", image.calcAverageDistanceError());
            VariableErrors solved = null;
            int solvedGenNum = 0;
            ga.GenerationRan += (sender, e) =>
            {
                var bestChromosome = ga.BestChromosome as FloatingPointChromosome;
                var bestFitness = bestChromosome.Fitness.Value;

                if (bestFitness != latestFitness)
                {
                    latestFitness = bestFitness;
                    var phenotype = bestChromosome.ToFloatingPoints();
                    solved = new VariableErrors(phenotype);
                    solvedGenNum = ga.GenerationsNumber;
                    Console.WriteLine(
                        "Generation {0,2}: {1} = {2}",
                        solvedGenNum,
                        solved,
                        1/bestFitness
                    );
                }
            };

            ga.Start();

            image.solvedErrors = solved;
            image.solvedGenNum = solvedGenNum;
            Console.WriteLine("Completed Genetic Algorithm");
        }


    }
}
