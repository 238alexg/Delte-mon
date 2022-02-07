using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts.Controls
{
    public enum InputValue
    {
        MoveEast,
        MoveNorth,
        MoveWest,
        MoveSouth,

        AButton,
        BButton
    }

    public enum InputState
    {
        Down,
        Pressed,
        Up
    }

    public interface IInputGenerator
    {
        bool TryGetInputEvents(out Dictionary<InputValue, InputState> inputEvents);
    }

    public interface IInputConsumer
    {
        bool ConsumeInputEvents(Dictionary<InputValue, InputState> inputEvents);
    }

    public static class InputEventBroadcaster
    {
        private static List<IInputGenerator> InputGenerators = new List<IInputGenerator>();
        private static List<IInputConsumer> InputConsumers = new List<IInputConsumer>();
        public static Dictionary<InputValue, InputState> InputState = new Dictionary<InputValue, InputState>();

        public static void RegisterInputGenerator(IInputGenerator inputGenerator)
        {
            InputGenerators.Add(inputGenerator);
        }

        public static void RegisterInputConsumer(IInputConsumer inputConsumer)
        {
            InputConsumers.Add(inputConsumer);
        }

        public static void BroadcastInputEvents()
        {
            InputState.Clear();

            foreach(var generator in InputGenerators)
            {
                if (generator.TryGetInputEvents(out var inputEvents))
                {
                    foreach (var inputEvent in inputEvents)
                    {
                        if (InputState.ContainsKey(inputEvent.Key))
                        {
                            Debug.LogError($"Input State already has input set for: {inputEvent.Key}");
                        }
                        else
                        {
                            InputState.Add(inputEvent.Key, inputEvent.Value);
                        }
                    }
                }
            }

            foreach(var consumer in InputConsumers)
            {
                if (consumer.ConsumeInputEvents(InputState))
                {
                    // TODO: Should I break per input event? This is likely good enough for this game
                    // Consumer has consumed, stop sending this event
                    break;
                }
            }
        }
    }
}
