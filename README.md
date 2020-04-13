# ![Logo](UWPMiniGames/Assets/Small.png) UWP Mini Games
Simple Universal Windows Platform Games

Implemented in C#

Project is based on State pattern (consisting of 4 states, switching between states is performed in button clicks):
1. Game Selection (Home Screen, Game Selection - All available games)
2. Game Menu (Concrete Game's Menu - New Game or Continue)
3. Game (Playing game)
4. Pause (Game Paused)

![Logo](StateDiagram.png)
![Logo](State.png)
![Logo](States.png)

Games are implemented as Strategy Pattern

![Logo](IGame.png)

Uses both Keyboard and mouse (touchcsreen) inputs

## Snake
- Uses only Key input (WASD or Arrows)

![Logo](Snake.png)

## Tetris
- Uses only Key input (A,D,R, Left and Right Arrow)

![Logo](Tetris.png)

## Tic-Tac-Toe
- For "AI" was used MinMax algorithm
- Uses only moude / touch input

![Logo](Tic-Tac-Toe.png)


## Changelog
### 1.0.0.0
- Created 4 UI states
- Tic-Tac-Toe Game
- Snake Game
### 1.1.0.0
- Added Tetris Game
- Refactoring of Snake Game
 
