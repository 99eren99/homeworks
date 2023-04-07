// 
// Genethic Algorithm:
// 1) Randomly initialize populations p
// 2) Determine fitness of population
// 3) Until convergence or reach max generation repeat:
//       a) Select parents by proportional selection(roulette wheel)
//       b) Crossover and generate new population
//       c) Perform mutation on new population
//       d) Calculate fitness for new population
//       e) Select samples which are going to survive by elitism(elitist replacement) 
// 
namespace Namespace {

    using System;
    using System.Collections;
    using System.Linq;
    using Permutations;
    using System.Diagnostics;

    public static class Program {
        
        static Random random_generator=new Random();
        public static void Main() {
            int n_cities = 8;
            int population_size = 100;
            double elite_ratio = 0.3;
            int n_child = Convert.ToInt32(population_size / elite_ratio);
            int chromosome_len = n_cities;
            double mutation_ratio = 0.2;
            int max_generation = 20;
            int [][] adjacency_matrix = create_adjacency_matrix(n_cities);
            int optimal_fitness = find_optimal_solution(adjacency_matrix);
            Console.WriteLine("Adjacency matrix:");
            printTwoDArray(adjacency_matrix);
            Console.WriteLine("\nThe optimal fitness(the shortest route distance):"+optimal_fitness.ToString()+ "\n\n");
            
            foreach (var i in Enumerable.Range(0, 3)) {
                Stopwatch stopwatch = new Stopwatch();  
                stopwatch.Start();  
                Console.WriteLine($"**********************************************\n{i+1}.trial started...\n");
                int[][] population = generate_initial_population(population_size, chromosome_len);
                int[] fitnesses = compute_fitnesses(population,adjacency_matrix);
                int best_fitness = fitnesses.Min();
                int arg_min = Array.IndexOf(fitnesses,best_fitness);
                int generation_count = 1;
                string route_string=string.Join(",", population[arg_min]);
                double avarage_distance=Queryable.Average(fitnesses.AsQueryable());
                Console.WriteLine($"Generation:{generation_count}\nThe best route:{route_string}"
                    +$"\nThe shortest distance:{best_fitness}\n"
                    +$"Avarage distance of generation:{avarage_distance}\n");
                while (generation_count < max_generation && best_fitness != optimal_fitness) {
                    fitnesses = compute_fitnesses(population,adjacency_matrix);
                    double[] proportions = roulette_wheel(fitnesses);
                    population = breed(population, proportions, n_child);
                    mutation(ref population, mutation_ratio);
                    fitnesses = compute_fitnesses(population,adjacency_matrix);
                    var tuple = elitism(population, fitnesses, elite_ratio);
                    population = tuple.Item1;
                    fitnesses = tuple.Item2;
                    best_fitness = fitnesses.Min();
                    arg_min = Array.IndexOf(fitnesses,best_fitness);
                    generation_count += 1;
                    route_string=string.Join(",", population[arg_min]);
                    avarage_distance=Queryable.Average(fitnesses.AsQueryable());
                    Console.WriteLine($"Generation:{generation_count}\nThe best route:{route_string}"
                        +$"\nThe shortest distance:{best_fitness}\n"
                        +$"Avarage distance of generation:{avarage_distance}\n");
                }
                stopwatch.Stop();
                Console.WriteLine("Algorithm terminated...\nTotal time passed(ms):", 
                    (stopwatch.ElapsedMilliseconds));
            }
            Console.ReadKey();
        }

        static void printTwoDArray(int[][] matrix)  
        {    
            for (int i = 0; i < matrix.Length; i++)  
            {  
                for (int j = 0; j < matrix.Length; j++)  
                {  
                        Console.Write($"{matrix[i][j]:00} ");
                }  
                Console.WriteLine();  
            }  
        }  

        public static int[] create_1d_random_int_array(int len,int low=0,int high=30){
            int[] array= new int[len];
            for (int i =0;i<len;i++){
                array[i]=random_generator.Next(low,high);
            }
            return array;
        }

        public static int[][] create_2d_random_int_array(int x_len,int y_len,int low=0,int high=30){
            int[][] array= new int[x_len][];
            for (int i =0;i<x_len;i++){
                array[i]=new int[y_len];
                for (int j=0;j<y_len;j++){
                    array[i][j]=random_generator.Next(low,high);
                }
            }
            return array;
        }

        public static int[][] create_adjacency_matrix(int n_cities) {
            int[][] adjcaceny_matrix= create_2d_random_int_array(n_cities,n_cities);
            //set diagonal to zero
            for (int i =0;i<n_cities;i++){
                adjcaceny_matrix[i][i]=0;
            }
            //make array symmetric
            for (int i =0;i<n_cities;i++){
                for (int j =i+1;j<n_cities;j++){
                    adjcaceny_matrix[j][i]=adjcaceny_matrix[i][j];
                }
            }
            return adjcaceny_matrix;
        }
        
        public static int find_optimal_solution(int[][] adjacency_matrix) {
            //finds all solutions by brute force and returns optimal
            int[] cities=Enumerable.Range(0, adjacency_matrix.GetLength(0)).ToArray(); 
            int[][] all_solutions = Permutation.get_permutations(cities).OfType<int[]>().ToArray();
            int[] fitnesses = compute_fitnesses(all_solutions,adjacency_matrix).OfType<int>().ToArray();
            int optimal_fitness = fitnesses.Min();
            return optimal_fitness;
        }

        public static int fitness_function(int[] route,int[][] adjacency_matrix) {
            //computes fitness of a route
            var total_distance = 0;
            foreach (var i in Enumerable.Range(0, route.Length - 1)) {
                total_distance += adjacency_matrix[route[i]][route[i + 1]];
            }
            return total_distance;
        }
        
        public static int[] compute_fitnesses(int[][] population,int[][] adjacency_matrix) {
            ArrayList fitnesses = new ArrayList();
            for (int i=0;i<population.GetLength(0);i++) {
                fitnesses.Add(fitness_function(population[i],adjacency_matrix));
            }
            return fitnesses.OfType<int>().ToArray();
        }

        public static int[][] generate_initial_population(int population_size = 100, int chromosome_len = 8) {
            //get random initial population
            ArrayList population = new ArrayList();
            int[] default_route=Enumerable.Range(0, chromosome_len).ToArray();
            int[] shuffled_route=new int[chromosome_len];
            foreach (var i in Enumerable.Range(0, population_size)) {
                shuffled_route = default_route.OrderBy(x => random_generator.Next()).ToArray();//shuffles route to get a random route
                population.Add(shuffled_route);
            }
            return population.OfType<int[]>().ToArray();
        }
        
        public static Tuple<int[][],int[]> elitism(int[][] population, int[] fitnesses, double elite_ratio = 0.3) {
            //go on with most promising chromosomes

            // Sorting population by its fitnesses
            Array.Sort(fitnesses,population);

            int elites_count = Convert.ToInt32(elite_ratio * population.Length);
            int[][] elites = population[0..elites_count];
            int[] elites_fitnesses = fitnesses[0..elites_count];
            return Tuple.Create(elites, elites_fitnesses);
        }
        
        public static void mutation(ref int[][] population , double mutation_ratio = 0.2) {
            int chromosome_len = population[0].Length;
            int n_times = Convert.ToInt32(chromosome_len * mutation_ratio);
            foreach (var i in Enumerable.Range(0, n_times)) {
                //pick chromosome randomly and swap genes randomly
                int chromosome_index=random_generator.Next(population.Length);
                int[] random_chromosome = population[chromosome_index];
                int random_indice1 = random_generator.Next(chromosome_len);
                int random_indice2 = random_generator.Next(chromosome_len);
                //ensure indices are not the same
                while(random_indice1==random_indice2){
                    random_indice2 = random_generator.Next(chromosome_len);
                }
                //swap genes
                int temp_value = random_chromosome[random_indice1];
                random_chromosome[random_indice1] = random_chromosome[random_indice2];
                random_chromosome[random_indice2] = temp_value;
                population[chromosome_index]=random_chromosome;
            }
        }
        
        public static double[] roulette_wheel(int[] fitnesses) {
            //this method weights selection(selecting partners for parents) probability of chromosomes wrt their fitnesses
            double[] weights = fitnesses.Select(t => (1/(double)t)).ToArray();
            double total_weight = weights.Sum();
            double[] proportions = weights.Select(t => t/total_weight).ToArray();
            return proportions;
        }
        
        public static int[] two_point_crossover(int[] chromosome1, int[] chromosome2) {
            //generates new chromosomes by crossover
            int chromosome_lenght = chromosome1.Length;
            //get crossover indices
            int crossover_indice1 = random_generator.Next(chromosome_lenght);
            int crossover_indice2 = random_generator.Next(chromosome_lenght);
            //ensure indices are not the same
            while(crossover_indice1==crossover_indice2){crossover_indice2 = random_generator.Next(chromosome_lenght);}

            int starting_index=Math.Min(crossover_indice1,crossover_indice2);
            int ending_index=Math.Max(crossover_indice1,crossover_indice2);
            int[] child = new int[chromosome_lenght];
            var chromosome1_legacy = chromosome1[starting_index..ending_index];

            List<int> chromosome2_legacy = chromosome2.OfType<int>().ToList();
            //remove genes of legacy 2 from legacy 1 to prevent repeatitive genes
            foreach (var i in Enumerable.Range(0, (ending_index - starting_index))) {
                chromosome2_legacy.Remove(chromosome1_legacy[i]);
            }
            //crossover
            foreach (var i in Enumerable.Range(0, starting_index)) {
                child[i] = chromosome2_legacy[i];
            }
            foreach (var i in Enumerable.Range(starting_index, (ending_index-starting_index))) {
                child[i] = chromosome1_legacy[i-starting_index];
            }
            foreach (var i in Enumerable.Range(ending_index, (chromosome_lenght-ending_index))) {
                child[i] = chromosome2_legacy[i-(ending_index-starting_index)];
            }
            return child;
        }
        
        public static int[][] breed(int[][] parents, double[] proportions, int n_child) {
            //picks partners for crossover wrt roulette probabilities and applies crossover to obtain next generation
            int[][] next_generation = new int[n_child][];
            foreach (var i in Enumerable.Range(0, n_child)) {
                //get partner indices
                int partner_indice1 = pick_by_probability(proportions);
                int partner_indice2 = pick_by_probability(proportions);
                //ensure indices are not the same
                while(partner_indice1==partner_indice2){partner_indice2 = pick_by_probability(proportions);;}

                int[] parent1 = parents[partner_indice1];
                int[] parent2 = parents[partner_indice2];
                next_generation[i]=(two_point_crossover(parent1, parent2));
            }
            return next_generation;
        }

        public static int pick_by_probability(double[] probabilities){
            double p = random_generator.NextDouble();
            double cumulativeProbability = 0.0;
            int index=0;
            foreach (double probability in probabilities) {
                cumulativeProbability += probability;
                if (p <= cumulativeProbability) {
                    break;
                }
                index++;
            }
            return index;
        }
    }
}
