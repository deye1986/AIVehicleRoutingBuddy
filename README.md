# AIVehicleRoutingBuddy by David Ikin 2025

**Intelligent waypoint-based racing AI for Unity** - Create competitive opponents that handle any circuit with customizable driving styles.

**Updates** 
- AI Agent Brake System Integrated and passed initial limitation tests. Is still a WIP! WORKS ON BUMPS, CHICANE & HAIRPINS!!

**Pitfalls **
- IMPORTANT - ANGULAR DAMPING SHOULD BE SET TO 1.6 - 2.1. Reduce at own risk.

![pathFinder](https://github.com/user-attachments/assets/8d6d9115-333c-4ca4-a346-fb06583185d5)

- **Waypoint-Based Navigation** - Guide AI vehicles through custom race circuits
- **Smart Corner Detection** - Automatically detects and adjusts speed for sharp turns
-  **Brake Zone Markers** - Manual control over specific braking points
- **Configurable Driving Style** - Adjust aggression, speed, and handling characteristics
- **Stuck Recovery System** - Automatically reverses and recovers from obstacles
- **Smooth Steering** - Natural-looking steering with configurable smoothing and deadzones
- **Race Start Countdown** - Built-in delay system for synchronized starts
- **Debug Visualization** - Gizmos and debug lines for easy track setup

## Requirements

- Unity 2020.3 or higher
- Vehicle with Rigidbody component
- CarControl script (for handling motor/brake/steering input) // already included
- WheelCollider-based vehicle physics

## Quick Start and installation

Add the AICarController script to your AI car GameObject.

Ensure the car has a Rigidbody and a CarControl component.

Create a series of waypoints in the scene.

Assign the waypoints array in the AICarController inspector.

Optionally, add the Waypoint script to each waypoint to define brake zones and custom target points.

### 1. Setup Your Vehicle

Add the `AICarController` component to your vehicle GameObject. Ensure your vehicle has:
- Rigidbody component
- CarControl component
- WheelControl components on wheels

### 2. Create Waypoints

1. Create empty GameObjects along your race track
2. Add the `Waypoint` component to each
3. Assign them to the AI controller's `waypoints` array in order

```
Track Layout: Start → WP1 → WP2 → WP3 → ... → Finish (loops back to Start)
```

### 3. Configure Brake Zones (Optional) RECOMMENDED, 

Mark specific waypoints as brake zones for precise speed control:

1. Select a waypoint GameObject
2. In the Waypoint component, check `isBrakeZone`
3. Set `targetSpeedMultiplier` (0.5 = 50% speed)

~~Visual indicators:
~~- Green spheres = normal waypoints
~~- Red spheres = brake zone waypoints

## Configuration

### AI Driving Settings
<img width="454" height="610" alt="AIBuddy2" src="https://github.com/user-attachments/assets/5ad690a7-6dfc-4b78-a901-a206f81eb818" />

| Parameter | Default | Description |
|-----------|---------|-------------|
| `targetSpeed` | 15 | Maximum speed the AI will attempt to reach |
| `steeringPower` | 1.5 | Steering response strength |
| `throttleStrength` | 1.0 | Acceleration multiplier |
| `waypointReachDistance` | 5 | Distance to consider waypoint "reached" |

### Corner Detection

| Parameter | Default | Description |
|-----------|---------|-------------|
| `cornerLookahead` | 20 | How far ahead to check for corners |
| `sharpCornerAngle` | 45 | Angles above this trigger heavy braking (50% speed) |
| `moderateCornerAngle` | 25 | Angles above this trigger light braking (70% speed) |

### Steering Adjustments

| Parameter | Default | Description |
|-----------|---------|-------------|
| `steerSmoothingFactor` | 3.0 | Lower = smoother steering (less twitchy) |
| `maxSteeringAngleDampener` | 25 | Reduces steering sensitivity on straights |

### Race Start

| Parameter | Default | Description |
|-----------|---------|-------------|
| `startDelay` | 5.0 | Countdown time before AI starts driving |

### Stuck Recovery

| Parameter | Default | Description |
|-----------|---------|-------------|
| `stuckSpeedThreshold` | 1.0 | Speed below which vehicle is considered stuck |
| `stuckTimeThreshold` | 3.0 | Time stationary before recovery triggers |
| `reverseTime` | 2.5 | How long to reverse when stuck |

## How It Works

### Corner Detection Algorithm

The AI analyzes upcoming waypoints to calculate corner angles:

1. **Sharp corners (>45°)**: Reduces speed to 50% of target
2. **Moderate corners (>25°)**: Reduces speed to 70% of target
3. **Gentle curves (<25°)**: Maintains full speed

Speed reduction is gradual based on distance to corner for smooth deceleration.

~~### Brake Zones

~~Manual brake zones override automatic corner detection when stricter control is needed:

- Useful for complex sections (chicanes, hairpins)
- - Combines with corner detection (uses lower speed requirement)
- - Configurable per-waypoint speed targets

### Steering System

Multi-layer steering for natural behavior:

1. **Deadzone filtering** - Ignores micro-adjustments
2. **Angle dampening** - Reduces sensitivity on straights
3. **Smooth interpolation** - Gradual steering changes
4. **Power scaling** - More aggressive in tight corners

## Debug Tools

<img width="453" height="486" alt="AIBuddy" src="https://github.com/user-attachments/assets/08057cc0-38bf-4c71-bedf-ef8e66ee5892" />

### Visual Gizmos

- **Yellow lines** - Connect waypoints to show track layout
- **Green** - Current AI path 
- - **Red line** - (red = brake zone active)
- **Blue ray** - Vehicle forward direction
- **Spheres at waypoints** - Green (normal) / (Red (brake zones) not yet implemented coming soon)

### Debug Properties

Access current AI state via public properties:

```csharp
public bool IsOvertaking { get; }      // Currently overtaking (reserved for future use)
public bool IsRecovering { get; }      // Currently in stuck recovery mode
public float CurrentSpeed { get; }      // Current velocity magnitude
public float CurrentThrottleInput { get; } // Current throttle input value
```

## Tuning Tips

### For Faster, More Aggressive AI

- Increase `targetSpeed`
- Increase `steeringPower` (1.8-2.0)
- Increase `throttleStrength` (1.2-1.5)
- Reduce `cornerLookahead` (15-18)

### For Smoother, More Realistic AI

- Increase `steerSmoothingFactor` (4-6)
- Increase `cornerLookahead` (25-30)
- Use more brake zone waypoints for precise control (coming soon)
- Lower `steeringPower` (1.0-1.3)

### For Tight Technical Tracks

- Increase `sharpCornerAngle` threshold (50-60)
- Add brake zones before hairpins (coming soon)
- Reduce `targetSpeed`
- Increase `steerSmoothingFactor`

### For High-Speed Oval Tracks

- Increase `targetSpeed` significantly
- Reduce `steeringPower` (0.8-1.0)
- Increase `maxSteeringAngleDampener` (30-40)
- Minimal or no brake zones needed

## Integration with CarControl

The AI works with a `CarControl` component that must expose:

```csharp
public bool isAIControlled;
public float aiVerticalInput;    // -1 to 1 (throttle/brake)
public float aiHorizontalInput;  // -1 to 1 (steering)
public float steeringRange;      // Maximum steering angle
```

The CarControl script should check `isAIControlled` and use AI inputs instead of player inputs when true.

## Example Setup

```csharp
// On your AI vehicle GameObject:
// 1. Add AICarController component
// 2. Configure in Inspector:

Target Speed: 20
Steering Power: 1.5
Corner Lookahead: 20
Sharp Corner Angle: 45
Start Delay: 5

// 3. Assign waypoints array with your track waypoints
// 4. Mark brake zones on tight corners
// 5. Press Play and watch the AI race!
```

## Known Limitations

- Requires pre-placed waypoints (no dynamic pathfinding)
- No overtaking logic (reserved for future implementation)
- Best suited for circuit racing (not open-world navigation)
- Collision avoidance not implemented

## License

MIT License – free to use and modify in personal or commercial projects.

## Contributing and public involvement

- Overtaking behavior for multi-car races
- Dynamic difficulty adjustment
- Collision avoidance
- Adaptive learning from player racing line
- Support for non-circuit tracks (point-to-point)

## Credits

Developed for Unity racing game projects requiring competitive AI opponents. David Ikin 2025.
