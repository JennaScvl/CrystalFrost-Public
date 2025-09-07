# Crystal Frost

Crystal Frost is an experimental, open-source Second Life and OpenSim viewer built on the Unity 2021.3.6f1 LTS engine and the LibreMetaverse library. This project aims to provide a modern and extensible platform for interacting with virtual worlds.

## 🚀 Current Status

This project is currently in an experimental phase. While many core functionalities are in place, it is still under active development, and some features may be incomplete or subject to change.

## ✨ Features

- **Unity-Based:** Leverages the power and flexibility of the Unity engine for rendering and physics.
- **LibreMetaverse:** Utilizes the LibreMetaverse library for communication with Second Life and OpenSim grids.
- **Extensible Architecture:** Designed with a modular architecture to facilitate contributions and new features.

## 🛠️ Getting Started

### Prerequisites

- **Unity Hub**
- **Unity 2021.3.6f1 LTS** with the following modules:
  - Windows Build Support (IL2CPP)
  - Mac Build Support (Mono)
  - Linux Build Support (IL2CPP)

### Setup

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/your-username/crystal-frost.git
   ```

2. **Open in Unity:**
   - Open Unity Hub and click **Add**.
   - Navigate to the cloned repository's root folder and select it.
   - The project will open in the Unity Editor.

3. **Restore Packages:**
   - Unity should automatically restore the required packages. If not, open the **Package Manager** (`Window > Package Manager`) and ensure all dependencies are resolved.

## 🕹️ Usage

1. **Open the Login Scene:**
   - In the Unity Editor, navigate to the `Assets/Scenes` folder.
   - Open the `Login.unity` scene.

2. **Enter Credentials:**
   - Run the scene by pressing the **Play** button.
   - Enter your Second Life or OpenSim credentials in the login UI.

3. **Connect to the Grid:**
   - Click the **Login** button to connect to the virtual world.

## 🏛️ Project Architecture

The project is organized into several key directories:

- **`Assets/CFEngine`:** Contains the core engine logic, including asset management, networking, and world state management.
- **`Assets/Scripts`:** Houses various gameplay scripts, UI controllers, and other high-level functionalities.
- **`Assets/ECS`:** Includes experimental code related to the Entity Component System (ECS) architecture.
- **`Assets/Editor`:** Contains custom Unity Editor scripts and tools to aid in development.
- **`Assets/Joystick Pack`:** A third-party asset for implementing on-screen joysticks.

## 🤝 Contributing

We welcome contributions from the community! If you're interested in helping, please contact **Berry Bunny** in Second Life or **.Kallisti** on Discord to be added as a contributor.

When contributing, please ensure that your code adheres to the existing style and documentation standards. All new public classes, methods, and properties should be fully documented with XML docstrings.

## 📄 License

All code written specifically for Crystal Frost is publicly available under the **GPLv3** license, unless otherwise specified. This project also incorporates code licensed under the **BSD license** and other proprietary licenses. Please see the `LICENSE` file for more details.
