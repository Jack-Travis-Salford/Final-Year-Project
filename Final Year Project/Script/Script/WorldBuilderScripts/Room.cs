using System.Collections.Generic;
using Script.Rooms;
using Script.Rooms.RoomObjects;
using UnityEngine;

public class Room
{
    //Holds info about door positions
    public List<Door> Doors;
    public bool isDestroyable;

    public bool isDestroyed;

    //The max doors a room can have (Allows for force place doors until the max is reached)
    public int MaxDoors;

    //How many tiles the room is [x,z]
    public int[] RoomSize;

    //Grid pos of top left tile of room [x,z]
    public int[] StartPos;

    //How many doors the room wants to have
    public int TargetDoors;

    public Room(int[] roomSize, int[] startPos)
    {
        RoomSize = roomSize;
        StartPos = startPos;
        Doors = new List<Door>();
        isDestroyable = true;
        var wallSections = RoomSize[0] + RoomSize[0] + RoomSize[1] + RoomSize[1];
        var maxTargetDoors = 1 + wallSections / 25;
        TargetDoors = Random.Range(1, maxTargetDoors + 1);
        MaxDoors = 1 + wallSections / 20;
        isDestroyed = false;
    }

    public void AddDoor(Door door)
    {
        Doors.Add(door);
    }

    public void Decorate(RoomOptions chosenRoomData, GameObject objectHolder)
    {
        var tileWidth = GeneratorGlobalVals.Instance.GetTileDimension();
        var wallDepth = GeneratorGlobalVals.Instance.GetWallDepth();
        var roomHeight = GeneratorGlobalVals.Instance.GetRoomHeight();
        float doorWidth = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
        var northWallObjects = new List<RoomObject>();
        var southWallObjects = new List<RoomObject>();
        var eastWallObjects = new List<RoomObject>();
        var westWallObjects = new List<RoomObject>();


        foreach (var roomObject in chosenRoomData.objectsToAdd)
            if (roomObject.IsWallMounted)
                for (var i = 0; i < roomObject.WantedQuantity; i++)
                {
                    var rand = Random.Range(0, 4);
                    switch (rand)
                    {
                        case GeneratorGlobalVals.NORTH:
                            northWallObjects.Add(roomObject);
                            break;
                        case GeneratorGlobalVals.EAST:
                            eastWallObjects.Add(roomObject);
                            break;
                        case GeneratorGlobalVals.SOUTH:
                            southWallObjects.Add(roomObject);
                            break;
                        case GeneratorGlobalVals.WEST:
                            westWallObjects.Add(roomObject);
                            break;
                    }
                }

        if (northWallObjects.Count != 0)
        {
            var middlePos = (StartPos[1] + 0.5f * RoomSize[1]) * tileWidth;
            var hasObstruction = false;
            foreach (var door in Doors)
                if (door.Wall == GeneratorGlobalVals.NORTH)
                {
                    var doorStartPos = (StartPos[1] + door.Offset) * tileWidth;
                    if (middlePos > doorStartPos && middlePos < doorStartPos + doorWidth + 1) hasObstruction = true;
                }

            if (!hasObstruction)
            {
                var wallMountedGameObject = WorldObjects.Instance.GetObject(northWallObjects[0].ObjectRef);
                wallMountedGameObject.transform.position =
                    new Vector3(StartPos[0] * tileWidth + 0.5f * wallDepth,
                        roomHeight * 0.5f - 0.5f * wallMountedGameObject.transform.localScale.y, middlePos);
                wallMountedGameObject.transform.rotation = Quaternion.Euler(0, 180, 12);
                wallMountedGameObject.transform.parent = objectHolder.transform;
            }
        }

        if (southWallObjects.Count != 0)
        {
            var middlePos = (StartPos[1] + 0.5f * RoomSize[1]) * tileWidth;
            var hasObstruction = false;
            foreach (var door in Doors)
                if (door.Wall == GeneratorGlobalVals.SOUTH)
                {
                    var doorStartPos = (StartPos[1] + door.Offset) * tileWidth;
                    if (middlePos > doorStartPos && middlePos < doorStartPos + doorWidth + 1) hasObstruction = true;
                }

            if (!hasObstruction)
            {
                var wallMountedGameObject = WorldObjects.Instance.GetObject(southWallObjects[0].ObjectRef);
                wallMountedGameObject.transform.position =
                    new Vector3((StartPos[0] + RoomSize[0]) * tileWidth - 0.5f * wallDepth,
                        roomHeight * 0.5f - 0.5f * wallMountedGameObject.transform.localScale.y, middlePos);
                wallMountedGameObject.transform.rotation = Quaternion.Euler(0, 0, 12);
                wallMountedGameObject.transform.parent = objectHolder.transform;
            }
        }

        if (eastWallObjects.Count != 0)
        {
            var middlePos = (StartPos[0] + 0.5f * RoomSize[0]) * tileWidth;
            var hasObstruction = false;
            foreach (var door in Doors)
                if (door.Wall == GeneratorGlobalVals.EAST)
                {
                    var doorStartPos = (StartPos[0] + door.Offset) * tileWidth;
                    if (middlePos > doorStartPos && middlePos < doorStartPos + doorWidth + 1) hasObstruction = true;
                }

            if (!hasObstruction)
            {
                var wallMountedGameObject = WorldObjects.Instance.GetObject(eastWallObjects[0].ObjectRef);
                wallMountedGameObject.transform.position =
                    new Vector3(middlePos, roomHeight * 0.5f - 0.5f * wallMountedGameObject.transform.localScale.y,
                        (StartPos[1] + RoomSize[1]) * tileWidth - 0.5f * wallDepth);
                wallMountedGameObject.transform.rotation = Quaternion.Euler(0, 270, 12);
                wallMountedGameObject.transform.parent = objectHolder.transform;
            }
        }

        if (westWallObjects.Count != 0)
        {
            var middlePos = (StartPos[0] + 0.5f * RoomSize[0]) * tileWidth;
            var hasObstruction = false;
            foreach (var door in Doors)
                if (door.Wall == GeneratorGlobalVals.WEST)
                {
                    var doorStartPos = (StartPos[0] + door.Offset) * tileWidth;
                    if (middlePos > doorStartPos && middlePos < doorStartPos + doorWidth + 1) hasObstruction = true;
                }

            if (!hasObstruction)
            {
                var wallMountedGameObject = WorldObjects.Instance.GetObject(westWallObjects[0].ObjectRef);
                wallMountedGameObject.transform.position =
                    new Vector3(middlePos, roomHeight * 0.5f - 0.5f * wallMountedGameObject.transform.localScale.y,
                        StartPos[1] * tileWidth + 0.5f * wallDepth);
                wallMountedGameObject.transform.rotation = Quaternion.Euler(0, 90, 12);
                wallMountedGameObject.transform.parent = objectHolder.transform;
            }
        }

        var centreItemPlaced = false;
        for (var i = 0; i < chosenRoomData.objectsToAdd.Count && !centreItemPlaced; i++)
        {
            var option = chosenRoomData.objectsToAdd[i];
            if (!option.IsWallMounted && !option.IsPlacedAgainstWall)
            {
                var centreItem = WorldObjects.Instance.GetObject(option.ObjectRef);
                var itemPosition = new Vector3((StartPos[0] + 0.5f * RoomSize[0]) * tileWidth,
                    GeneratorGlobalVals.Instance.GetFloorDepth() + 0.001f,
                    (StartPos[1] + 0.5f * RoomSize[1]) * tileWidth);
                if (option.ObjectRef == WorldObjects.SPAWN_POINT)
                    GeneratorGlobalVals.Instance.SetWorldSpawn(itemPosition);

                centreItem.transform.position = itemPosition;
                centreItemPlaced = true;
                centreItem.transform.parent = objectHolder.transform;
            }
        }

        var roomWantsChests = false;
        var wantedChestQnty = 0;
        for (var i = 0; i < chosenRoomData.objectsToAdd.Count && !roomWantsChests; i++)
            if (chosenRoomData.objectsToAdd[i].ObjectRef == WorldObjects.CHEST)
            {
                roomWantsChests = true;
                wantedChestQnty += chosenRoomData.objectsToAdd[i].WantedQuantity;
            }

        if (roomWantsChests)
        {
            var northWallSpace = new bool[RoomSize[1]];
            var southWallSpace = new bool[RoomSize[1]];
            var eastWallSpace = new bool[RoomSize[0]];
            var westWallSpace = new bool[RoomSize[0]];
            bool[] wallsHaveVaildPositions = { false, false, false, false };
            var tilesNeededPerChest = Mathf.CeilToInt(1.5f / tileWidth);
            var consecutiveFreeTiles = 0;
            for (var i = 0; i < northWallSpace.Length; i++)
            {
                northWallSpace[i] = true;
                southWallSpace[i] = true;
            }

            for (var i = 0; i < eastWallSpace.Length; i++)
            {
                eastWallSpace[i] = true;
                westWallSpace[i] = true;
            }

            foreach (var door in Doors)
                switch (door.Wall)
                {
                    case GeneratorGlobalVals.NORTH:
                        for (var i = door.Offset; i < door.Offset + doorWidth; i++) northWallSpace[i] = false;
                        break;
                    case GeneratorGlobalVals.SOUTH:
                        for (var i = door.Offset; i < door.Offset + doorWidth; i++) southWallSpace[i] = false;
                        break;
                    case GeneratorGlobalVals.EAST:
                        for (var i = door.Offset; i < door.Offset + doorWidth; i++) eastWallSpace[i] = false;
                        break;
                    case GeneratorGlobalVals.WEST:
                        for (var i = door.Offset; i < door.Offset + doorWidth; i++) westWallSpace[i] = false;
                        break;
                }

            for (var i = 0; i < northWallSpace.Length && !wallsHaveVaildPositions[GeneratorGlobalVals.NORTH]; i++)
            {
                if (northWallSpace[i])
                    consecutiveFreeTiles++;
                else
                    consecutiveFreeTiles = 0;
                if (consecutiveFreeTiles == tilesNeededPerChest)
                    wallsHaveVaildPositions[GeneratorGlobalVals.NORTH] = true;
            }

            consecutiveFreeTiles = 0;
            for (var i = 0; i < southWallSpace.Length && !wallsHaveVaildPositions[GeneratorGlobalVals.SOUTH]; i++)
            {
                if (southWallSpace[i])
                    consecutiveFreeTiles++;
                else
                    consecutiveFreeTiles = 0;

                if (consecutiveFreeTiles == tilesNeededPerChest)
                    wallsHaveVaildPositions[GeneratorGlobalVals.SOUTH] = true;
            }

            consecutiveFreeTiles = 0;
            for (var i = 0; i < eastWallSpace.Length && !wallsHaveVaildPositions[GeneratorGlobalVals.EAST]; i++)
            {
                if (eastWallSpace[i])
                    consecutiveFreeTiles++;
                else
                    consecutiveFreeTiles = 0;

                if (consecutiveFreeTiles == tilesNeededPerChest)
                    wallsHaveVaildPositions[GeneratorGlobalVals.EAST] = true;
            }

            consecutiveFreeTiles = 0;
            for (var i = 0; i < westWallSpace.Length && !wallsHaveVaildPositions[GeneratorGlobalVals.WEST]; i++)
            {
                if (westWallSpace[i])
                    consecutiveFreeTiles++;
                else
                    consecutiveFreeTiles = 0;

                if (consecutiveFreeTiles == tilesNeededPerChest)
                    wallsHaveVaildPositions[GeneratorGlobalVals.WEST] = true;
            }

            var optionsExhausted = false;
            while (wantedChestQnty > 0 && !optionsExhausted)
            {
                consecutiveFreeTiles = 0;
                int chosenPosition;
                GameObject chest;
                var offsetOptions = new List<int>();
                switch (Random.Range(0, 4))
                {
                    case GeneratorGlobalVals.NORTH:
                        if (!wallsHaveVaildPositions[GeneratorGlobalVals.NORTH]) break;
                        for (var i = 0; i < northWallSpace.Length; i++)
                        {
                            if (northWallSpace[i])
                                consecutiveFreeTiles++;
                            else
                                consecutiveFreeTiles = 0;

                            if (consecutiveFreeTiles >= tilesNeededPerChest)
                                offsetOptions.Add(i - tilesNeededPerChest + 1);
                        }

                        if (offsetOptions.Count == 0) break;

                        chosenPosition = offsetOptions[Random.Range(0, offsetOptions.Count)];
                        //DRAW OBJ
                        chest = WorldObjects.Instance.GetObject(WorldObjects.CHEST);
                        chest.transform.position = new Vector3(StartPos[0] * tileWidth + wallDepth + 0.3f,
                            GeneratorGlobalVals.Instance.GetFloorDepth(),
                            (StartPos[1] + chosenPosition) * tileWidth + 0.7f);
                        chest.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                        chest.transform.parent = objectHolder.transform;
                        for (var i = 0; i < tilesNeededPerChest; i++) northWallSpace[chosenPosition + i] = false;

                        wantedChestQnty -= 1;
                        consecutiveFreeTiles = 0;
                        wallsHaveVaildPositions[GeneratorGlobalVals.NORTH] = false;
                        for (var i = 0;
                             i < northWallSpace.Length && !wallsHaveVaildPositions[GeneratorGlobalVals.NORTH];
                             i++)
                        {
                            if (northWallSpace[i])
                                consecutiveFreeTiles++;
                            else
                                consecutiveFreeTiles = 0;

                            if (consecutiveFreeTiles == tilesNeededPerChest)
                                wallsHaveVaildPositions[GeneratorGlobalVals.NORTH] = true;
                        }

                        break;
                    case GeneratorGlobalVals.SOUTH:
                        if (!wallsHaveVaildPositions[GeneratorGlobalVals.SOUTH]) break;
                        for (var i = 0; i < southWallSpace.Length; i++)
                        {
                            if (southWallSpace[i])
                                consecutiveFreeTiles++;
                            else
                                consecutiveFreeTiles = 0;

                            if (consecutiveFreeTiles >= tilesNeededPerChest)
                                offsetOptions.Add(i - tilesNeededPerChest + 1);
                        }

                        if (offsetOptions.Count == 0) break;

                        chosenPosition = offsetOptions[Random.Range(0, offsetOptions.Count)];
                        //DRAW OBJ
                        chest = WorldObjects.Instance.GetObject(WorldObjects.CHEST);
                        chest.transform.position = new Vector3(
                            (StartPos[0] + RoomSize[0]) * tileWidth - wallDepth - 0.3f,
                            GeneratorGlobalVals.Instance.GetFloorDepth(),
                            (StartPos[1] + chosenPosition) * tileWidth + 0.7f);
                        chest.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                        chest.transform.parent = objectHolder.transform;
                        for (var i = 0; i < tilesNeededPerChest; i++) southWallSpace[chosenPosition + i] = false;
                        wantedChestQnty -= 1;
                        consecutiveFreeTiles = 0;
                        wallsHaveVaildPositions[GeneratorGlobalVals.SOUTH] = false;
                        for (var i = 0;
                             i < southWallSpace.Length && !wallsHaveVaildPositions[GeneratorGlobalVals.SOUTH];
                             i++)
                        {
                            if (southWallSpace[i])
                                consecutiveFreeTiles++;
                            else
                                consecutiveFreeTiles = 0;

                            if (consecutiveFreeTiles == tilesNeededPerChest)
                                wallsHaveVaildPositions[GeneratorGlobalVals.SOUTH] = true;
                        }

                        break;
                    case GeneratorGlobalVals.WEST:
                        if (!wallsHaveVaildPositions[GeneratorGlobalVals.WEST]) break;
                        for (var i = 0; i < westWallSpace.Length; i++)
                        {
                            if (westWallSpace[i])
                                consecutiveFreeTiles++;
                            else
                                consecutiveFreeTiles = 0;

                            if (consecutiveFreeTiles >= tilesNeededPerChest)
                                offsetOptions.Add(i - tilesNeededPerChest + 1);
                        }

                        if (offsetOptions.Count == 0) break;

                        chosenPosition = offsetOptions[Random.Range(0, offsetOptions.Count)];
                        //DRAW OBJ
                        chest = WorldObjects.Instance.GetObject(WorldObjects.CHEST);
                        chest.transform.position = new Vector3((StartPos[0] + chosenPosition) * tileWidth + 0.7f,
                            GeneratorGlobalVals.Instance.GetFloorDepth(), StartPos[1] * tileWidth + wallDepth + 0.3f);
                        chest.transform.parent = objectHolder.transform;
                        for (var i = 0; i < tilesNeededPerChest; i++) westWallSpace[chosenPosition + i] = false;

                        wantedChestQnty -= 1;
                        consecutiveFreeTiles = 0;
                        wallsHaveVaildPositions[GeneratorGlobalVals.WEST] = false;
                        for (var i = 0;
                             i < westWallSpace.Length && !wallsHaveVaildPositions[GeneratorGlobalVals.WEST];
                             i++)
                        {
                            if (westWallSpace[i])
                                consecutiveFreeTiles++;
                            else
                                consecutiveFreeTiles = 0;

                            if (consecutiveFreeTiles == tilesNeededPerChest)
                                wallsHaveVaildPositions[GeneratorGlobalVals.WEST] = true;
                        }

                        break;
                    case GeneratorGlobalVals.EAST:
                        if (!wallsHaveVaildPositions[GeneratorGlobalVals.EAST]) break;
                        for (var i = 0; i < eastWallSpace.Length; i++)
                        {
                            if (eastWallSpace[i])
                                consecutiveFreeTiles++;
                            else
                                consecutiveFreeTiles = 0;

                            if (consecutiveFreeTiles >= tilesNeededPerChest)
                                offsetOptions.Add(i - tilesNeededPerChest + 1);
                        }

                        if (offsetOptions.Count == 0) break;

                        chosenPosition = offsetOptions[Random.Range(0, offsetOptions.Count)];
                        //DRAW OBJ
                        chest = WorldObjects.Instance.GetObject(WorldObjects.CHEST);
                        chest.transform.position = new Vector3((StartPos[0] + chosenPosition) * tileWidth + 0.7f,
                            GeneratorGlobalVals.Instance.GetFloorDepth(),
                            (StartPos[1] + RoomSize[1]) * tileWidth - wallDepth - 0.3f);
                        chest.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                        chest.transform.parent = objectHolder.transform;
                        for (var i = 0; i < tilesNeededPerChest; i++) eastWallSpace[chosenPosition + i] = false;

                        wantedChestQnty -= 1;
                        consecutiveFreeTiles = 0;
                        wallsHaveVaildPositions[GeneratorGlobalVals.EAST] = false;
                        for (var i = 0;
                             i < eastWallSpace.Length && !wallsHaveVaildPositions[GeneratorGlobalVals.EAST];
                             i++)
                        {
                            if (eastWallSpace[i])
                                consecutiveFreeTiles++;
                            else
                                consecutiveFreeTiles = 0;
                            if (consecutiveFreeTiles == tilesNeededPerChest)
                                wallsHaveVaildPositions[GeneratorGlobalVals.EAST] = true;
                        }

                        break;
                }

                optionsExhausted = true;
                foreach (var isOptionValid in wallsHaveVaildPositions)
                    if (isOptionValid)
                    {
                        optionsExhausted = false;
                        break;
                    }

                optionsExhausted = !wallsHaveVaildPositions[GeneratorGlobalVals.NORTH] ||
                                   !wallsHaveVaildPositions[GeneratorGlobalVals.SOUTH];
            }
        }
    }
}