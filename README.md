# Orbital Rocket Launch Grapher
A multi-stage rocket launch simulation and visualization tool that models rocket trajectories, velocity profiles, and orbital insertion physics.
![image](https://github.com/user-attachments/assets/62689b07-3800-4607-aac3-dfcee34b8b0c)

## Overview
The Orbital Rocket Launch Grapher is a desktop application that simulates rocket launches and provides real-time graphical visualization of various flight parameters. It uses a Python backend for physics calculations and a C# Windows Forms frontend for user interaction.
### The simulation models:

- Multi-stage rocket physics
- Gravitational effects
- Aerodynamic forces
- Mass changes during flight
- Orbital mechanics
- Stage separation dynamics

## Features

Two-Stage Rocket Simulation: Configure parameters for both first and second stages
### Real-time Visualization: Six different graphs showing:

- Earth Perspective (X/Y position)
- Velocity over time
- Satellite Mass over time
- Altitude profile
- Velocity X/Y components
- Aerodynamic Forces


## Configurable Parameters:

- Rocket mass (initial and per stage)
- Stage separation timing
- Engine ISP (Specific Impulse)
- Thrust values
- Thrust angle
- Start/end times

## Animation Controls:

- Start simulation
- View animation of the rocket trajectory
- Additional metrics display

## Technical Implementation
Architecture
The application uses a hybrid approach:

### Frontend: C# Windows Forms for the user interface and input handling
Backend: Python for the physics simulation and calculations
Data Visualization: Integrated plotting tools for real-time data display

### Physics Modeling
The simulation accounts for:

- Gravitational forces (inverse square law)
- Aerodynamic drag based on altitude and velocity
- Mass reduction from propellant consumption
- Thrust vectoring effects
- Stage separation dynamics
- Orbital mechanics for trajectory calculation

### Known Limitations

- Cannot process negative values or blank input boxes
- There may be a slight lag when the simulation begins due to calculation overhead
- Rocket final mass must be greater than 0 to prevent simulation errors

### Setup and Usage

Clone the repository
Install required dependencies:
.NET Framework for C# components

### Example Usage
The application comes pre-configured with parameters for a Falcon 9-like rocket, but users can modify:

- Stage masses
- Engine efficiency (ISP)
- Thrust profiles
- Separation timing
- Flight duration

### Future Development

- Support for more complex rocket configurations
- Additional environmental factors (wind, atmospheric variations)
- 3D visualization options
- Export capabilities for simulation data
- Trajectory optimization tools
