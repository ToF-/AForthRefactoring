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

: CALC-FRAME-SCORE ( switch, throw1, throw2 -- switch ) 
  SWAP 2>R          \ save throws, throw2 first, on return stack
  R@ COLLECT-BONUS  
  2R@ CALC-BONUS    
  2R> ADD-THROWS ;