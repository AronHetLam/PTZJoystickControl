# PTZ Joystick Control

PTZ Joystick Control simply lets you use any joystick or gamepad to control PTZ cameras via VISCA over IP. 🕹️🎮

## Features
  - Intuitive joystick interface.
  - Fully customizable button and axis mapping.
  - Control pan, tilt, zoom, and focus of your camera and more.
  - Supports VISCA over IP.
  - Easy to use and configure.
  - Tray icon indicating the selected camera.
  - Cross-platform compatible for Windows and macOs.
    - See Linux and RaspberryPi support further down.

## Installation
  1. Download the latest release of PTZ Joystick Control from the releases page.
  2. Double-click on the the downloaded installer to install the application.
  3. Double-click on the PTZ Joystick Control executable to launch the application.
    
## Usage
  1. Launch the application by double-clicking on the PTZ Joystick Control executable.
  2. Select a connected controller from the dropdown.
  3. Map your joystick inputs to camera commands.
  4. Add and configure your VISCA over IP compatible cameras.
  5. Use the joystick to control the movements of your camera.

## Linux and RaspberryPi support
The application also runs on Linux, and even a RaspberryPi, however I haven't got to releasing binaries for those yet.
On x86 and x64 Linux machienes you can build from source using ´dotnet build´, and it should be able to run.
For linux-ARM devices such as RaspberryPi you additionally need to compile libSDL2 yourself, and add it to the build output for it to run.

## License
This project is licensed under the GPL-3 License - see the LICENSE.txt file for details.

## Contributing
Contributions are always welcome! Please feel free to submit a pull request or open an issue if you encounter any problems with the software.

Thank you for using PTZ Joystick Control!
