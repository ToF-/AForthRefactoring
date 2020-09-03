: RESET-SCORE ( -- switch, score )
  0 0 ;

\ given the current bonus switch and score, and two rolls,
\ calculate the current score. All the data is on the stack
: CURRENT-SCORE ( switch, score, throw1, throw2 -- switch, score ) 
  >R >R          \ switch, score             - R throw2, throw1
  SWAP           \ score, switch
  R@             \ score, switch+1, throw1   - R throw2, throw1
  *              \ score, throw1*bonus 
  +              \ new-score
  R> R>          \ new-score, throw1, throw2  
  OVER OVER      \ new-score, throw1, throw2, throw1, throw2
  + 10 = NEGATE  \ new-score, throw1, throw2, (0|1)
  -ROT           \ new-score, (0|1), throw1, throw2
  +              \ new-score, (0|1), frame-score
  ROT            \ (0|1), frame-score, new-score
  +              \ (0|1), frame-total  
  ;