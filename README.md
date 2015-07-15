# SuperCharacterController

Custom Character Controller for Unity. Fulfills all common character controller functions such as collision detection and pushback, slope limiting and collider ignoring. New features include the ability to rotate the controller to any angle, ground detection, ability to clamp the character to surfaces, and detailed collision response messages. All functions are fully exposed and editable. Also includes the SuperStateMachine, a finite state machine designed to interact with the controller.

See development blog of the project here: https://roystanross.wordpress.com/

## Installation

Open the project in Unity, demo scenes are included in the /Scenes folder.

## Usage

See the sample implementation of the controller in /Scripts/PlayerMachine.cs, which also demonstrates the SuperStateMachine.

## Contributing

1. Fork it
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request

## Credits

See /SuperCharacterController/README.txt for all contributor credits prior to the project being uploaded to GitHub.

## License

See LICENSE.
