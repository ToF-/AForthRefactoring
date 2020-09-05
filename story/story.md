The following is refactoring sequence of some Forth code that Tom Ayerst and I wrote recently, as part of a *bowling score kata* pair programming session.

## Initial State
These are the tests for our program so far. Each test puts initial values on the stack with `RESET-SCORE`, then puts some throws and call `CURRENT-SCORE`, checking that the expected results are on the stack.
```Forth
REQUIRE ffl/tst.fs
REQUIRE bowling.fs
\ normal
\ x + y < 10 - std score
\ x + y = 10 - spare 
\ x     = 10 - strike

PAGE    \ Clears the screen
: CHECK ( x,y -- x ) OVER ?S ;
T{
." Average frame counts both throws for scoring " CR
RESET-SCORE 
3 5 CURRENT-SCORE 8 ?S 0 ?S 
}T

T{ 
." Spare frame counts the next frame score twice " CR
RESET-SCORE
6 4 CURRENT-SCORE 10 CHECK  
3 2 CURRENT-SCORE 18 ?S 0 ?S
}T
``` 
And here is the `CURRENT-SCORE` word definition:
```Forth
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
``` 
This word calculates the current score given a bonus switch (0 or 1), the previous score, and the two throws the player just rolled.
While being pretty complicated, its tasks amounts to 

- collect the bonus possibly earned on previous frame,
- calculate the new bonus in case the two throws constitute a *spare*
- add the frame score to the global score

This definition needs refactoring: it does a lot of juggling on the stack, uses the return stack, and is difficult to understand.

## Extract Method #1
Our first simplification step is to extract a specific word to collect the bonus previously earned. If the bonus *swich* is 1, multiplying it by the first throw will create the bonus score.

```Forth
: COLLECT-BONUS ( score,switch,throw1 -- score )
  * + ;

: CURRENT-SCORE ( switch, score, throw1, throw2 -- switch, score ) 
  >R >R          \ switch, score             - R throw2, throw1
  SWAP           \ score, switch
  R@             \ score, switch, throw1   - R throw2, throw1
  COLLECT-BONUS  \ score
  R> R>          \ new-score, throw1, throw2  
  OVER OVER      \ new-score, throw1, throw2, throw1, throw2
  + 10 = NEGATE  \ new-score, throw1, throw2, (0|1)
  -ROT           \ new-score, (0|1), throw1, throw2
  +              \ new-score, (0|1), frame-score
  ROT            \ (0|1), frame-score, new-score
  +              \ (0|1), frame-total  
  ;
``` 
## Extract Method #2
Then we extract another word to calculate the bonus for the next frame. To be clear, at this point our bowling score program detecs only spare bonus (if the player knocked down all the pins, leave a 1 on the stack, else leave a 0). We know that the program will have to be adapted to detect strike. We want its design to be as simple as possible for its current features. This way it will easier to adapt it for the next feature.

```Forth
: COLLECT-BONUS ( score,switch,throw1 -- score )
  * + ;

: CALC-BONUS ( throw1,throw2 -- switch )
  + 10 = 
  IF 1 ELSE 0 THEN ;

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
``` 
## Extract Method #3
The last *extracted* word is merely adding the throws points to the score. We hesitated about this one, wondering just adding points to the score was worthy of a word definition. We decided that yes, since the definition, small as it is, make the program clearer and easier to change in the future.
```Forth
: COLLECT-BONUS ( score,switch,throw1 -- score )
  * + ;

: CALC-BONUS ( throw1,throw2 -- switch )
  + 10 = 
  IF 1 ELSE 0 THEN ;

: ADD-THROWS ( score, throw1, throw2 -- score )
  + + ;

: CURRENT-SCORE ( switch, score, throw1, throw2 -- switch, score ) 
  >R >R          \ switch, score             - R throw2, throw1
  SWAP           \ score, switch
  R@             \ score, switch, throw1   - R throw2, throw1
  COLLECT-BONUS  \ score
  2R@            \ score', throw1, throw2  
  CALC-BONUS     \ score', switch
  SWAP           \ switch, score'
  R> R>          \ switch, score', throw1, throw2
  ADD-THROWS     \ switch, score'
  ;
```
## Using a variable
One reason why the code is so complicated in the first place is that we sought to use the stack as our storage area for the whole program, which led us to a word using 4 parameters. Using a variable for the global score will lead us to a simpler code. That way, `COLLECT-BONUS` and `ADD-THROWS` update the `SCORE` variable and don't require returning any value on the stack.

```Forth
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
```
Removing the score from the stack allows for less juggling, but we have to return it at the end of the word since our tests are expecting a score value on the stack.

## Code layout
```Forth
: CURRENT-SCORE ( switch, throw1, throw2 -- switch, score ) 
  SWAP 2>R          \ save throws, throw2 first, on return stack
  R@ COLLECT-BONUS  
  2R@ CALC-BONUS    
  2R> ADD-THROWS     
  SCORE @
  ;
```
A little change in the code layout make things clearer. Ideally we want to see only the "domain logic" steps of calculating a bowling score, and leave the stack manipulation words "in the background".

## Query to Command
Our tests used to expect the calculated score on the stack after `CURRENT-SCORE` is called. Since the score is now stored in a variable we can change this. Also we rename the word from `CURRENT-SCORE` which indicates a query, to `CALC-FRAME-SCORE` which indicates a command, while being more precise. 

```Forth
T{
." Average frame counts both throws for scoring " CR
RESET-SCORE 
3 5 CALC-FRAME-SCORE SCORE @ 8 ?S 0 ?S 
}T

T{ 
." Spare frame counts the next frame score twice " CR
RESET-SCORE
6 4 CALC-FRAME-SCORE SCORE @ 10 ?S DUP 1 ?S
3 2 CALC-FRAME-SCORE SCORE @ 18 ?S 0 ?S
}
```
```Forth
: CALC-FRAME-SCORE ( switch, throw1, throw2 -- switch ) 
  SWAP 2>R          \ save throws, throw2 first, on return stack
  R@ COLLECT-BONUS  
  2R@ CALC-BONUS    
  2R> ADD-THROWS ;
```
## Avoiding use of the Return Stack
The definition is now in a better shape, but it is still complicated. The definition uses the Return Stack as a way to temporarily discard some parameters. It is a Forth code smell. Instead of using two stacks, we should simplify the use of the main (Data) Stack.

```Forth
: CALC-FRAME-SCORE ( switch, throw1, throw2 -- switch ) 
  -ROT 2DUP COLLECT-BONUS 
  NIP  2DUP CALC-BONUS
  -ROT ADD-THROWS ;
```
With ondy 3 parameters on the stack, we don't need to use the return stack. The stack manipulation words here do an auxiliary job of arranging the parameters for a correct execution of the 3 steps. Compare this definition with the one we started with: it is thinner (it uses 9 words instead of 18), simpler as we don't use a secondary stack anymore, and clearer as the domain logic elements are more visible. 

Refactoring done!

