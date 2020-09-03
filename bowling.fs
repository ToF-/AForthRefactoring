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
  -ROT 2DUP COLLECT-BONUS 
  NIP  2DUP CALC-BONUS
  -ROT ADD-THROWS ;