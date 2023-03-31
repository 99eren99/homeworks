import numpy as np
import time
import pandas as pd
import tabulate

def main():
    stats=pd.DataFrame(columns=["Move Count","Random Restart Count","Time Passed(seconds)"])
    for i in range(9):
        print(f"********{i+1}.EPISODE STARTED********")
        time_passed=time.time()
        state=np.zeros(shape=(8,8))
        current_cost=0
        
        #put queens column by column
        for target_column_index in range(8):
            current_cost=put_queen(state,target_column_index)
        print("ALL QUEENS PLACED")
        print(tabulate.tabulate(state, tablefmt="grid"))

        move_count=8
        random_restart_count=0
        #apply stochastic hill climbing 
        while current_cost!=0:
            state,current_cost, random_restart_flag=change_queen_position(state,current_cost)
            if random_restart_flag:
                random_restart_count+=1
            move_count+=1
            
        print("TERMINAL STATE")
        print(tabulate.tabulate(state, tablefmt="grid"),"\n\n")
        
        time_passed=time.time()-time_passed
        stats.loc[len(stats.index),:]=[move_count,random_restart_count,time_passed]
    
    stats=stats.reset_index().rename(columns={'index': 'Episode'})
    stats.Episode+=1
    print("*********************************** SUMMARY ***********************************\n",
          tabulate.tabulate(stats, tablefmt='grid',headers=stats.columns, showindex=False))

def put_queen(state,target_column_index):
    costs=np.empty(shape=8)
    for row_index in range(8):
        candidate_state=np.copy(state)
        candidate_state[row_index,target_column_index]=1
        costs[row_index]=compute_cost(candidate_state)
    
    #get random move with smaller cost than current cost among candidates
    min_cost=costs.min()
    candidate_row_indices=np.where(costs==min_cost)[0]
    row_index=np.random.choice(candidate_row_indices,1)
    state[row_index,target_column_index]=1
    return costs.min()
    
def change_queen_position(state,current_cost):
    cost_matrix=np.empty(shape=(8,8))
    for column in range(8):
        for row in range(8):
            if state[row,column]==1:
                cost_matrix[row,column]=99
                continue
            candidate_state=np.copy(state)
            candidate_state[:,column]=0
            candidate_state[row,column]=1
            cost_matrix[row,column]=compute_cost(candidate_state)

    random_restart_flag=0
    min_cost = np.amin(cost_matrix)
    if min_cost==current_cost:
        random_restart_flag=1
        state=random_restart()
        min_cost=compute_cost(state)
    else:
        min_indices = np.asarray(np.where(cost_matrix == min_cost))
        stochastic_optimal_choice=np.random.choice(len(min_indices[0]),1)
        stochastic_optimal_choice=min_indices[:,stochastic_optimal_choice]
        state[:,stochastic_optimal_choice[1][0]]=0
        state[stochastic_optimal_choice[0][0]][stochastic_optimal_choice[1][0]]=1
    return state,min_cost,random_restart_flag
    
def random_restart():
    state=np.zeros(shape=(8,8))
    for column in range(8):
        random_row=np.random.randint(0,8)
        state[random_row,column]=1
    return state

def compute_cost(candidate_state):
    #row wise costs
    row_costs=candidate_state.sum(axis=1)
    func = lambda t: t if t==0 else t-1
    vectorized_func = np.vectorize(func)
    row_costs=vectorized_func(row_costs).sum()

    #left diagonals costs
    left_diagonals_costs=0
    diagonals_cost=candidate_state.diagonal().sum()
    if diagonals_cost>0:
        diagonals_cost-=1
    left_diagonals_costs+=diagonals_cost
    for i in range(1,7):
        sub_matrix=candidate_state[i:,:-i]
        diagonals_cost=sub_matrix.diagonal().sum()
        if diagonals_cost>0:
            diagonals_cost-=1
        left_diagonals_costs+=diagonals_cost
    for i in range(1,7):
        sub_matrix=candidate_state[:-i,i:]
        diagonals_cost=sub_matrix.diagonal().sum()
        if diagonals_cost>0:
            diagonals_cost-=1
        left_diagonals_costs+=diagonals_cost

    #right diagonals costs
    right_diagonals_costs=0
    mirror_candidate_state=np.fliplr(candidate_state)
    diagonals_cost=mirror_candidate_state.diagonal().sum()
    if diagonals_cost>0:
        diagonals_cost-=1
    right_diagonals_costs+=diagonals_cost
    for i in range(1,7):
        sub_matrix=mirror_candidate_state[i:,:-i]
        diagonals_cost=sub_matrix.diagonal().sum()
        if diagonals_cost>0:
            diagonals_cost-=1
        right_diagonals_costs+=diagonals_cost
    for i in range(1,7):
        sub_matrix=mirror_candidate_state[:-i,i:]
        diagonals_cost=sub_matrix.diagonal().sum()
        if diagonals_cost>0:
            diagonals_cost-=1
        right_diagonals_costs+=diagonals_cost

    return right_diagonals_costs+left_diagonals_costs+row_costs

main()
input()