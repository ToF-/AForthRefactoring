VARIABLE SCORE

: RESET-SCORE ( -- switch )
  0  
  0 SCORE ! ;

: COLLECT-BONUS ( switch,throw1 -- )
  * SCORE +! ; 

: CALC-BONUS ( throw1,throw2 -- switch )
  + 10 = 
  IF 1 ELSE 0 THEN ;

: ADD-THROWS ( score, throw1, throw2 -- )
  + SCORE +! ;

\ given the current bonus switch and score, and two rolls,
\ calculate the current score. Use a score variable

: CURRENT-SCORE ( switch, throw1, throw2 -- switch, score ) 
  >R >R          \ switch,            - R throw2, throw1
  R@             \  switch, throw1   - R throw2, throw1
  COLLECT-BONUS  \ 
  2R@            \ throw1, throw2  
  CALC-BONUS     \ switch
  R> R>          \ switch, throw1, throw2
  ADD-THROWS     \ switch
  SCORE @
  ;