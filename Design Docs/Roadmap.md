# Crystal Frost Development Roadmap

This document outlines the development roadmap for the Crystal Frost viewer, based on the feature set of the Firestorm viewer. It provides a prioritized list of features to be implemented, along with their current status.

## High Priority

These are the core features required for a basic, usable virtual world experience.

*   **3D World Navigation & Camera**
    *   **Description:** The complete system for moving the avatar and controlling the camera.
    *   **Status:** **Implemented.**
        *   Keyboard movement (WASD) is implemented.
        *   Mouselook camera control is implemented.
        *   Basic animations (walk, fly, stand, jump) are implemented.
        *   **To Do:** "Always Run" mode, advanced camera controls, further animation refinements.

*   **Communication UI**
    *   **Description:** The user interface for local chat, group chat, and instant messaging.
    *   **Status:** **Partially Implemented.**
        *   Backend logic is present in `libremetaverse`.
        *   A very basic UI exists (`ChatWindowUI.cs`).
        *   **To Do:** A complete redesign and implementation of the chat and IM UI floaters to be usable and feature-rich. This is a major UI task.

*   **Inventory UI**
    *   **Description:** The user interface for viewing and managing the avatar's inventory.
    *   **Status:** **Not Implemented.**
        *   Backend logic is handled by `libremetaverse`.
        *   **To Do:** A complete UI for displaying the inventory folder structure, item lists, and allowing for basic interaction (wearing/attaching items).

## Medium Priority

These features are important for a full-featured experience but are not as critical as the high-priority items.

*   **Avatar Appearance Editor**
    *   **Description:** The user interface for customizing the avatar's shape, skin, clothing, and attachments.
    *   **Status:** **Not Implemented.**
        *   Backend logic is handled by `libremetaverse`.
        *   **To Do:** A complete UI for editing the avatar's appearance. This is a very large and complex feature.

*   **Friends & Contacts UI**
    *   **Description:** The user interface for managing friends and contacts.
    *   **Status:** **Partially Implemented.**
        *   Backend logic is present in `libremetaverse`.
        *   A basic contact list exists.
        *   **To Do:** A full-featured UI for adding friends, seeing their online status, and initiating conversations.

*   **Basic Object Manipulation (Build/Edit)**
    *   **Description:** The ability to create and edit basic prims in-world.
    *   **Status:** **Not Implemented.**
        *   Backend logic is handled by `libremetaverse`.
        *   **To Do:** A UI for the build/edit tools.

## Low Priority

These features are desirable but can be implemented after the core functionality is in place.

*   **Voice Chat Integration**
    *   **Description:** Integrating the Vivox voice chat system.
    *   **Status:** **Not Implemented.**
        *   `libremetaverse` has a Vivox module.
        *   **To Do:** Implement the UI and client-side logic to manage voice chat connections.

*   **RLVa Support**
    *   **Description:** Implementing support for the Restrained Love Viewer extensions.
    *   **Status:** **Not Implemented.**
        *   `libremetaverse` has an RLV module.
        *   **To Do:** Implement the client-side logic to handle RLV commands.

*   **Client-Side Animation Overrider (AO)**
    *   **Description:** A built-in system for managing and playing custom avatar animations.
    *   **Status:** **Not Implemented.**

*   **Advanced Firestorm Features**
    *   **Description:** Implementing other Firestorm-specific features like Radar, Area Search, and Contact Sets.
    *   **Status:** **Not Implemented.**

## Frost Light (Advanced Rendering)

This section is for tracking the implementation of advanced rendering features that will distinguish Crystal Frost.

*   **Physically Based Rendering (PBR) Materials:**
    *   **Status:** Partially implemented in the core rendering engine. Needs to be fully integrated and exposed in the UI.
*   **Advanced Lighting & Shadows:**
    *   **Status:** Core deferred rendering pipeline is in place. Advanced features like high-quality dynamic shadows and global illumination are yet to be implemented.
*   **Customizable Post-Processing Effects:**
    *   **Status:** Basic effects are present (Glow, FXAA). A more extensive and user-configurable post-processing stack is needed.
