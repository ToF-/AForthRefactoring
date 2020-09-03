
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
3 5 CALC-FRAME-SCORE SCORE @ 8 ?S 0 ?S 
}T

T{ 
." Spare frame counts the next frame score twice " CR
RESET-SCORE
6 4 CALC-FRAME-SCORE SCORE @ 10 ?S DUP 1 ?S
3 2 CALC-FRAME-SCORE SCORE @ 18 ?S 0 ?S
}T
 
BYE
