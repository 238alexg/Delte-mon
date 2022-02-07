using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Controls
{
    public class PCInput : IInputGenerator
    {
		private readonly Dictionary<InputValue, InputState> InputEvents = new Dictionary<InputValue, InputState>();
		private bool AnyInputLastFrame = false;

        public bool TryGetInputEvents(out Dictionary<InputValue, InputState> inputEvents)
        {
			InputEvents.Clear();

			bool anyInput = false;

			if (TryGetMovementInput(out var movementInput, out var state))
            {
				anyInput = true;
				InputEvents.Add(movementInput, state);
			}

			if (TryGetInputStateOfKey(KeyCode.Return, out var aButtonState) ||
				TryGetInputStateOfKey(KeyCode.Space, out aButtonState))
            {
				anyInput = true;
				InputEvents.Add(InputValue.AButton, aButtonState);
			}

			if (TryGetInputStateOfKey(KeyCode.LeftShift, out var bButtonState) ||
				TryGetInputStateOfKey(KeyCode.RightShift, out bButtonState))
			{
				anyInput = true;
				InputEvents.Add(InputValue.BButton, bButtonState);
			}

			bool anyInputOrInputChange = AnyInputLastFrame || anyInput;
			AnyInputLastFrame = anyInput;

			inputEvents = InputEvents;
			return anyInputOrInputChange;
		}

		private bool TryGetMovementInput(out InputValue inputEvent, out InputState state)
        {
			inputEvent = default;
			if (TryGetInputStateOfKey(KeyCode.W, out state))
			{
				inputEvent = InputValue.MoveNorth;
			}
			else if (TryGetInputStateOfKey(KeyCode.A, out state))
			{
				inputEvent = InputValue.MoveWest;
			}
			else if (TryGetInputStateOfKey(KeyCode.S, out state))
			{
				inputEvent = InputValue.MoveSouth;
			}
			else if (TryGetInputStateOfKey(KeyCode.D, out state))
			{
				inputEvent = InputValue.MoveEast;
			}
			else
            {
				return false;
            }

			return true;
		}

		private bool TryGetInputStateOfKey(KeyCode key, out InputState state)
        {
			if (Input.GetKeyDown(key))
            {
				state = InputState.Down;
            }
			else if (Input.GetKeyUp(key))
            {
				state = InputState.Up;
            }
			else if (Input.GetKey(key))
            {
				state = InputState.Pressed;
            }
			else
            {
				state = default;
				return false;
            }

			return true;
        }
    }
}

