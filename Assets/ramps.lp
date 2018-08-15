#const ramps=1.
#show ramp/3.

%How many ramps are there?
{ ramp(X, Y, Z): space(X), space(Y), space(Z), twoapart(X, Z), adj(X, Y), adj(Y, Z)  } = ramps.

%What are ramps like?
twoapart((Point1x, Point1y), (Point2x, Point2y)) :-
  point((Point1x, Point1y)),
  point((Point2x, Point2y)),
  Point1y==Point2y,
  |Point1x-Point2x| == 2.
  
twoapart((Point1x, Point1y), (Point2x, Point2y)) :-
  point((Point1x, Point1y)),
  point((Point2x, Point2y)),
  Point1x==Point2x,
  |Point1y-Point2y| == 2.  
  
:- ramp(X,Y,Z), startpoint(Y).