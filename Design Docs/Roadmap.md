# Crystal Frost Development Roadmap

This document outlines the development roadmap for the Crystal Frost viewer. It is based on a feature analysis of the Firestorm viewer, with consideration for the capabilities of the underlying Libremetaverse library.

The features are grouped into logical categories and prioritized to guide development from a basic, functional viewer to a full-featured client.

## Prioritization Key

*   **High**: Essential for basic viewer functionality. The viewer is not usable without these.
*   **Medium**: Standard features expected in a modern viewer. These are required for a good user experience.
*   **Low**: Advanced, quality-of-life, or niche features. These can be implemented after a solid baseline is established.

---

## 1. Core Infrastructure & Engine

These are the fundamental systems of the viewer. Most are already in place in Crystal Frost but are listed here for completeness.

| Feature                          | Priority | Libremetaverse Role / Notes                                                                                             |
| -------------------------------- | :------: | ----------------------------------------------------------------------------------------------------------------------- |
| Application Core & Main Loop     | **High** | **Done.** Crystal Frost has an `EngineBehavior` and `BackgroundWorker` system. Libremetaverse handles the network loop. |
| Grid Support (SL, OpenSim)       | **High** | **Done.** Libremetaverse's `GridClient` handles this. Crystal Frost has a `GridClientFactory`.                           |
| Configuration & Settings System  | **High** | **Done.** Crystal Frost uses `Microsoft.Extensions.Configuration`. A UI for settings is needed (see UI section).          |
| Error Handling & Logging         | **High** | **Done.** Crystal Frost has a `GlobalExceptionHandler` and `ILogger` integration.                                       |
| Networking & Message Handling    | **High** | **Done.** Libremetaverse handles the entire message protocol. Crystal Frost has message handlers (`HandleObjectUpdate`). |
| Asset & Cache System             | **High** | **Done.** Crystal Frost has `AssetManager`, `TextureManager`, etc., with background workers and caching.                |

## 2. World Interaction

Features related to navigating and interacting with the virtual world.

| Feature                           | Priority | Libremetaverse Role / Notes                                                               |
| --------------------------------- | :------: | ----------------------------------------------------------------------------------------- |
| 3D World Navigation & Camera      | **High** | LMV provides agent position updates. Unity's Input System and Cinemachine can be used for implementation. |
| Object Selection & Highlighting   | **High** | LMV provides object data. Raycasting and highlighting must be implemented in Unity.       |
| Object Manipulation (Move, Rotate, Scale) | **Medium** | LMV sends object updates. The UI and in-world gizmos need to be built in Unity.     |
| Terraforming Tools                | **Low**    | LMV has messages for terrain modification. A dedicated UI and terrain tools are needed.   |

## 3. Graphics & Rendering

Visual fidelity and effects.

| Feature                          | Priority | Libremetaverse Role / Notes                                                                                             |
| -------------------------------- | :------: | ----------------------------------------------------------------------------------------------------------------------- |
| Primitive & Avatar Rendering     | **High** | **Partially Done.** LMV's `Meshmerizer` provides the mesh data. Crystal Frost renders it. Needs ongoing improvements.    |
| Deferred Rendering Pipeline      | **Medium** | This is a Unity rendering pipeline choice (URP/HDRP). Firestorm's is custom. Can be implemented or improved.         |
| Advanced Lighting (Windlight)    | **Medium** | LMV provides sky/water settings. Unity's lighting system (Directional Lights, Volumetric Clouds) needs to be configured. |
| Shadows (Dynamic & SSAO)         | **Medium** | This is a standard feature in Unity's render pipelines. Needs to be enabled and configured.                           |
| Post-Processing Effects (DoF, Glow) | **Medium** | Standard in Unity's Post-Processing Stack. Needs to be integrated and configured.                                   |
| Physically Based Rendering (PBR) | **Low**    | Unity's standard shaders are PBR. LMV provides PBR material parameters. Needs full integration.                         |

## 4. Avatar & Appearance

Features for customizing the user's avatar.

| Feature                           | Priority | Libremetaverse Role / Notes                                                                                             |
| --------------------------------- | :------: | ----------------------------------------------------------------------------------------------------------------------- |
| Wearables System (Attachments)    | **High** | LMV handles attachment data. The logic to attach/detach GameObjects in Unity needs to be built.                         |
| Basic Appearance (Baked Textures) | **High** | LMV provides the composite texture UUIDs. The `TextureManager` in CF handles downloading them.                          |
| Advanced Appearance Editor        | **Medium** | This is a major UI project. LMV provides the necessary messages to change avatar shape and textures.                  |
| Outfit Management                 | **Medium** | A client-side feature. The UI for saving/loading outfits needs to be built. LMV provides inventory functions.         |
| Client-Side Animation Overrider (AO) | **Low**    | A client-side feature. Requires a UI and a system to override avatar animations played via LMV.                     |

## 5. Communication & Social

Tools for users to interact with each other.

| Feature                           | Priority | Libremetaverse Role / Notes                                                                                             |
| --------------------------------- | :------: | ----------------------------------------------------------------------------------------------------------------------- |
| Local & Group Chat                | **High** | LMV handles sending/receiving chat messages. A robust, scrollable chat UI is a high priority.                           |
| Instant Messaging (IM)            | **High** | LMV handles sending/receiving IMs. A tabbed IM window UI is needed.                                                       |
| Friends & Contacts List           | **Medium** | LMV manages the friends list. A UI to display and interact with the list is needed.                                     |
| Blocklist (Mute) Manager          | **Medium** | LMV provides muting functionality. A UI to manage the blocklist is needed.                                              |
| Radar                             | **Low**    | Client-side feature. Needs to read avatar data from the `World` state and display it in a dedicated UI.               |
| Contact Sets                      | **Low**    | Client-side feature for organizing contacts. Requires its own data model and UI.                                        |

## 6. Audio

Sound effects, music, and voice.

| Feature                           | Priority | Libremetaverse Role / Notes                                                                                             |
| --------------------------------- | :------: | ----------------------------------------------------------------------------------------------------------------------- |
| 3D Positional Audio               | **High** | LMV provides sound UUIDs and positions. Unity's `AudioSource` component can handle playback. `AssetManager` needs to fetch sounds. |
| UI Sound Effects                  | **Medium** | Client-side feature. A simple audio manager to play sounds on UI events.                                                |
| Streaming Music Player            | **Medium** | LMV provides the parcel music URL. A Unity component to stream and play the audio is needed.                            |
| Voice Chat (Vivox)                | **Medium** | LMV has a `LibreMetaverse.Voice.Vivox` module. This needs to be integrated, and a UI for voice controls is required. |

## 7. Inventory

Managing the user's virtual items.

| Feature                           | Priority | Libremetaverse Role / Notes                                                                                             |
| --------------------------------- | :------: | ----------------------------------------------------------------------------------------------------------------------- |
| Hierarchical Inventory UI         | **High** | LMV provides the full inventory structure. A robust, performant UI to display and manage it is a major project.         |
| Inventory Search & Filtering      | **Medium** | Client-side feature building on top of the inventory UI.                                                                |
| Object/Outfit Import & Export     | **Low**    | Complex feature involving file formats. Can be considered much later.                                                   |

## 8. Building & Creating

Tools for in-world content creation.

| Feature                           | Priority | Libremetaverse Role / Notes                                                                                             |
| --------------------------------- | :------: | ----------------------------------------------------------------------------------------------------------------------- |
| In-World Object Creation (Prims)  | **Medium** | LMV has messages to create objects. A UI for selecting prim types and parameters is needed.                             |
| Advanced Build Tools              | **Low**    | Firestorm-specific enhancements. Can be added incrementally after basic building is functional.                         |
| Area Search                       | **Low**    | Client-side feature to search for objects within a given radius.                                                        |

## 9. Scripting (LSL)

Features related to the Linden Scripting Language.

| Feature                           | Priority | Libremetaverse Role / Notes                                                                                             |
| --------------------------------- | :------: | ----------------------------------------------------------------------------------------------------------------------- |
| Script Execution                  | **Medium** | LSL is executed server-side. The client needs to listen for script-driven effects (e.g., `llSay`, object movement). |
| LSL Script Editor                 | **Medium** | A UI for editing script assets from inventory. Should include syntax highlighting.                                      |
| LSL Preprocessor & Tools          | **Low**    | Libremetaverse has an `LslTools` project. This should be used instead of a custom implementation.                       |
| Restrained Love Viewer (RLVa)     | **Low**    | Libremetaverse has an `RLV` module. This is a large, advanced feature set that requires significant client-side logic and UI work. |

## 10. User Interface (General)

General UI features that are not tied to a specific system.

| Feature                           | Priority | Libremetaverse Role / Notes                                                                                             |
| --------------------------------- | :------: | ----------------------------------------------------------------------------------------------------------------------- |
| Skinning and Theming Engine       | **Low**    | A system to allow users to change the look and feel of the UI. Unity's UI system can be leveraged for this.          |
| Advanced Preferences Window       | **Low**    | A comprehensive UI to expose all the configurable settings.                                                             |
| Notification System               | **Medium** | A UI element to display notifications to the user (e.g., new IM, money received).                                       |
