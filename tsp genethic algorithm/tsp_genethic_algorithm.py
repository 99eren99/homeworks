"""
Genethic Algorithm:
1) Randomly initialize populations p
2) Determine fitness of population
3) Until convergence or reach max generation repeat:
      a) Select parents by proportional selection(roulette wheel)
      b) Crossover and generate new population
      c) Perform mutation on new population
      d) Calculate fitness for new population
      e) Select samples which are going to survive by elitism(elitist replacement) 
"""

import numpy as np
import time
import tabulate
from itertools import permutations
from statistics import mean

def main():

    global adjacency_matrix
    n_cities=8
    adjacency_matrix=create_adjacency_matrix(n_cities)
    optimal_fitness=find_optimal_solution(adjacency_matrix)#simply brute force
    print("Adjacency matrix:\n",tabulate.tabulate(adjacency_matrix, tablefmt='grid', showindex=False),
        "\nThe optimal fitness(the shortest route distance):",optimal_fitness,"\n\n")
    
    population_size=100
    elite_ratio=0.3
    n_child=int(population_size/elite_ratio)
    chromosome_len=n_cities
    mutation_ratio=0.2
    max_generation=20

    for i in range(3):
        print(f"**********************************************\n{i+1}.trial started...\n")
        start_time=int(time.time()*1000.0)#start time as miliseconds
        population=generate_initial_population(population_size,chromosome_len)
        fitnesses=compute_fitnesses(population)
        best_fitness=min(fitnesses)
        arg_min=np.asarray(fitnesses).argmin()
        generation_count=1
        print(f"Generation:{generation_count}\nThe best route:{population[arg_min]}\nThe shortest distance:{best_fitness}\n"+
              f"Avarage distance of generation:{mean(fitnesses)}\n")
        
        while generation_count<max_generation and best_fitness!=optimal_fitness:
            fitnesses=compute_fitnesses(population)
            proportions=roulette_wheel(fitnesses)
            population=breed(population,proportions,n_child)
            mutation(population,mutation_ratio)
            fitnesses=compute_fitnesses(population)
            population,fitnesses=elitism(population,fitnesses,elite_ratio)
            best_fitness=min(fitnesses)
            arg_min=np.asarray(fitnesses).argmin()
            generation_count+=1
            print(f"Generation:{generation_count}\nThe best route:{population[arg_min]}\nThe shortest distance:{best_fitness}\n"+
              f"Avarage distance of generation:{mean(fitnesses)}\n")
            
        print("Algorithm terminated...\nTotal time passed(ms):", (int(time.time()*1000.0)-start_time))

def create_adjacency_matrix(n_cities:int):
    adjacency_matrix=np.random.randint(30, size=(n_cities,n_cities))
    #set self distances to zero
    np.fill_diagonal(adjacency_matrix, 0)
    #set upper triangle to lower triangle for obtanining equal mutual distances
    for i in range(n_cities):#traverse rows
        for j in range(i+1,n_cities):#traverse columns
            adjacency_matrix[j,i]=adjacency_matrix[i,j]
    return adjacency_matrix
    
def find_optimal_solution(adjacency_matrix):
    #finds all solutions by brute force and returns optimal 
    all_solutions=set(permutations(range(adjacency_matrix.shape[0])))
    fitnesses=compute_fitnesses(all_solutions)
    optimal_fitness=min(fitnesses)
    return optimal_fitness

def generate_initial_population(population_size:int,chromosome_len:int):
    #get random initial population
    population=[]
    route=np.arange(chromosome_len)
    
    for i in range(population_size):
        np.random.shuffle(route)
        population.append(route.copy())
    
    return population

def fitness_function(route):
    #computes fitness of a route
    total_distance=0
    for i in range(len(route)-1):
        total_distance+=adjacency_matrix[route[i],route[i+1]]
    return total_distance

def compute_fitnesses(population):
    fitnesses=[]
    for chromosome in population:
        fitnesses.append(fitness_function(chromosome))
    return fitnesses

def elitism(population,fitnesses,elite_ratio:float):
    #go on with most promising chromosomes

    # Sorting population based on fitnesses
    population = [val for (_, val) in sorted(zip(fitnesses, population), key=lambda x: x[0])]

    fitnesses = sorted(fitnesses)
    elites = []
    elites_count = int(elite_ratio*len(population))
    elites=population[:elites_count]
    elites_fitnesses=fitnesses[:elites_count]
    return elites, elites_fitnesses
    
def mutation(population:list,mutation_ratio:float):
    chromosome_len=len(population[0])
    n_times=int(chromosome_len*mutation_ratio)
    for i in range(n_times):
        #pick chromosome randomly and swap genes randomly
        random_chromosome=population[np.random.randint(len(population),size=1)[0]]
        random_indices=np.random.choice(range(chromosome_len),2,replace=False)
        temp_value=random_chromosome[random_indices[0]]
        random_chromosome[random_indices[0]]=random_chromosome[random_indices[1]]
        random_chromosome[random_indices[1]]=temp_value
        
def roulette_wheel(fitnesses):
    #this method weights selection(selecting partners for parents) probability of chromosomes wrt their fitnesses
    weights=1/np.asarray(fitnesses)
    total_weight=weights.sum()
    proportions=weights/total_weight
    return proportions
    
def two_point_crossover(chromosome1,chromosome2):
    #generates new chromosomes by crossover
    chromosome_lenght=len(chromosome1)
    crossover_indices=np.random.choice(chromosome_lenght,2,replace=False)
    starting_index=crossover_indices.min()
    ending_index=crossover_indices.max()

    child=np.arange(chromosome_lenght)

    chromosome1_legacy=list(chromosome1[starting_index:ending_index])
    chromosome2_legacy=list(chromosome2)
    #remove repeatitive genes
    for i in range(ending_index-starting_index):
        chromosome2_legacy.remove(chromosome1_legacy[i])

    for i in range(starting_index):
        child[i]=chromosome2_legacy.pop(0)
    for i in range(starting_index,ending_index):
        child[i]=chromosome1_legacy.pop(0)
    for i in range(ending_index,len(chromosome1)):
        child[i]=chromosome2_legacy.pop(0)
        
    return child
    
def breed(parents,proportions,n_child):
    #picks partners for crossover wrt roulette probabilities and applies crossover to obtain next generation
    next_generation=[]
    next_generation.extend(parents)
    for i in range(n_child):
        parent_indices=np.random.choice(len(parents),2,replace=False,p=proportions)
        parent1=parents[parent_indices[0]]
        parent2=parents[parent_indices[1]]
        next_generation.append(two_point_crossover(parent1,parent2))
    return next_generation
    
main()
input()
    

