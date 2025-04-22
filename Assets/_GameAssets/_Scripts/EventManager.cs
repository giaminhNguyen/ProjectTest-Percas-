using System;

namespace _GameAssets._Scripts
{
    public static class EventManager
    {
        public static Action<Node> onNodeSelected;
        public static Action       resetState;
    }
}