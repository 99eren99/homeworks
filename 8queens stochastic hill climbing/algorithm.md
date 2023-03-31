#Putting queens\
-1.For column in matrix:\
  -1.1 Compute cost of all queen positions at -column-\
  -1.2 Pick a move randomly from optimal moves\
  
#Stochastic hill climbing\
-2.While cost!=0:\
  -2.1 Compute all possible moves\
  -2.2 If stucked in local optima restart randomly, else pick a move randomly from optimal moves\
