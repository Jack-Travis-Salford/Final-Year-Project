using System.Collections.Generic;
using UnityEngine;

namespace Script.Rooms
{
    public class RoomOptionDecider
    {
        private const int EMPTY = 0;
        private const int EXIT = 1;
        private const int SM_CHEST_ROOM = 2;
        private const int LRG_CHEST_ROOM = 3;
        private readonly float _emptyChance;
        private readonly float _exitChance;
        private readonly float _lrgChestRoomChance;
        private readonly float _smChestRoomChance;
        private readonly float _totalChance;
        private readonly List<KeyValuePair<int, float>> options;

        public RoomOptionDecider()
        {
            options = new List<KeyValuePair<int, float>>();
            _emptyChance = WorldObjects.Instance.emptyChance;
            _smChestRoomChance = WorldObjects.Instance.smChestRoomChance;
            _lrgChestRoomChance = WorldObjects.Instance.lrgChestRoomChance;
            _exitChance = WorldObjects.Instance.exitChance;
            options.Add(new KeyValuePair<int, float>(EMPTY, _emptyChance));
            options.Add(new KeyValuePair<int, float>(SM_CHEST_ROOM, _smChestRoomChance));
            options.Add(new KeyValuePair<int, float>(LRG_CHEST_ROOM, _lrgChestRoomChance));
            options.Add(new KeyValuePair<int, float>(EXIT, _exitChance));
            _totalChance = 0;
            foreach (var chance in options) _totalChance += chance.Value;
        }

        public RoomOptions DecideRoom()
        {
            var chosenNum = Random.Range(0, _totalChance);
            float currentTotal = 0;
            var chosenOptionFound = false;
            var chosenOption = 0;
            for (var i = 0; i < options.Count && !chosenOptionFound; i++)
            {
                currentTotal += options[i].Value;
                if (currentTotal >= chosenNum)
                {
                    chosenOption = options[i].Key;
                    chosenOptionFound = true;
                }
            }

            switch (chosenOption)
            {
                case EMPTY:
                    return new Empty();
                case EXIT:
                    return new Exit();
                case SM_CHEST_ROOM:
                    return new SmChestRoom();
                case LRG_CHEST_ROOM:
                    return new LrgChestRoom();
            }

            return new Empty();
        }
    }
}