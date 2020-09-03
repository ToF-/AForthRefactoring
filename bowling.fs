: RESET-SCORE ( -- switch, score )
  0 0 ;

: COLLECT-BONUS ( score,switch,throw1 -- score )
  * + ;

: CALC-BONUS ( throw1,throw2 -- switch )
  + 10 = 
  IF 1 ELSE 0 THEN ;

\ given the current bonus switch and score, and two rolls,
\ calculate the current score. All the data is on the stack

: CURRENT-SCORE ( switch, score, throw1, throw2 -- switch, score ) 
  >R >R          \ switch, score             - R throw2, throw1
  SWAP           \ score, switch
  R@             \ score, switch, throw1   - R throw2, throw1
  COLLECT-BONUS  \ score
  2R@            \ score', throw1, throw2  
  CALC-BONUS     \ score', switch
  R> R>          \ score', switch, throw1, throw2
  +              \ score', switch, frame-score
  ROT            \ switch, frame-score, score'
  +              \ switch, score'  
  ;