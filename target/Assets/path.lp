#const n=5.
#const max=40.
#show space/1.



point((X*n..X*n+n-1, Y*n..Y*n+n-1)) :- tile((X, Y)). 
space((X, Y)) :- startpoint((X, Y)).


adj((Point1x, Point1y), (Point2x, Point2y)) :-
  space((Point1x, Point1y)),
  space((Point2x, Point2y)),
  |Point1x-Point2x| + |Point1y-Point2y| = 1.


  
3 { space(X): point(X)} max.


{startpath(X): startpoint(X)} = 1.
path(X) :- startpath(X).
path(X) :- adj(X,Y), path(Y).

:- startpoint(X), not path(X).
