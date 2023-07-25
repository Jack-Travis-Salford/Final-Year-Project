using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script
{
    public class DoorGenerator
    {
        private const int NORTH = GeneratorGlobalVals.NORTH;
        private const int EAST = GeneratorGlobalVals.EAST;
        private const int SOUTH = GeneratorGlobalVals.SOUTH;
        private const int WEST = GeneratorGlobalVals.WEST;

        public void GenerateDoors(ref GridSegmentData[,] gridData, ref List<Room> rooms)
        {
            //DEBUG: Draw cube in room that im investigating
            /*for (int i = 0; i < 10; i++)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                int[] roomStartPos = rooms[i].StartPos;
                go.transform.position = new Vector3((roomStartPos[0] * GeneratorGlobalVals.Instance.GetTileDimension()) + 2,
                    0f, (roomStartPos[1] * GeneratorGlobalVals.Instance.GetTileDimension()) + 2);
                switch (i%3)
                {
                    case 0:
                        go.GetComponent<MeshRenderer>().material.color = Color.red;
                        break;
                    case 1:
                        go.GetComponent<MeshRenderer>().material.color = Color.yellow;
                        break;
                    case 2:
                        go.GetComponent<MeshRenderer>().material.color = Color.green;
                        break;

                }

                go.name = "Cube " + (i + 1);

            }*/

            //DEBUG END

            var corridorWidth = GeneratorGlobalVals.Instance.GetCorridorWidthTiles();
            //Find which walls are valid for a door (corridor wont path into out of bounds area
            foreach (var roomData in rooms)
            {
                var doorwayOptions = new List<Door>();
                if (roomData.StartPos[1] > corridorWidth * 2)
                    CheckValidDoorPosWest(roomData, doorwayOptions, ref gridData);

                if (roomData.StartPos[1] + roomData.RoomSize[1] < gridData.GetLength(1) - corridorWidth * 2)
                    CheckValidDoorPosEast(roomData, doorwayOptions, ref gridData);

                if (roomData.StartPos[0] > corridorWidth * 2)
                    CheckValidDoorPosNorth(roomData, doorwayOptions, ref gridData);

                if (roomData.StartPos[0] + roomData.RoomSize[0] < gridData.GetLength(0) - corridorWidth * 2)
                    CheckValidDoorPosSouth(roomData, doorwayOptions, ref gridData);

                if (doorwayOptions.Count == 0)
                {
                    RoomDestroyer(roomData.StartPos, ref gridData);
                    continue;
                }

                while (doorwayOptions.Count != 0 && roomData.Doors.Count < roomData.TargetDoors)
                {
                    var chosenDoorPos = Random.Range(0, doorwayOptions.Count);
                    PlaceDoorway(roomData, doorwayOptions[chosenDoorPos], ref gridData);
                    doorwayOptions.RemoveAt(chosenDoorPos);
                }
            }
        }

        /*
         * Checks for valid positions a door could be placed on the South wall of a room.
         * Valid  options are added to the referenced list provided as an argument
         */
        private void CheckValidDoorPosSouth(Room roomData, List<Door> validPositionsArray,
            ref GridSegmentData[,] gridData)
        {
            var southValidPositionsArray = new List<Door>();
            var corridorWidth = GeneratorGlobalVals.Instance.GetCorridorWidthTiles();
            var doorwayWidth = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            int[] wallStart = { roomData.StartPos[0] + roomData.RoomSize[0] - 1, roomData.StartPos[1] };
            //Keeps track of which positions are valid for a door for the given room
            var validDoorPos = new bool[roomData.RoomSize[1] - doorwayWidth + 1];
            for (var i = 0; i < validDoorPos.Length; i++) validDoorPos[i] = false;
            //Absolute min & max a corridor can fit at
            var absCorridorStartPos = math.max(roomData.StartPos[1] + (doorwayWidth - corridorWidth), 1);
            var absCorridorMaxPos =
                math.min(roomData.StartPos[1] + roomData.RoomSize[1] - (doorwayWidth - corridorWidth) - 1,
                    gridData.GetLength(1) - 1);
            //Relative min & max a corridor can fit at
            var minCorridorStartPos = absCorridorStartPos - roomData.StartPos[1];
            var maxCorridorEndPos = absCorridorMaxPos - roomData.StartPos[1];
            //Make sure relative positioning fits on grid
            var successiveValidCorridorSpots = 0;
            for (var currentPos = minCorridorStartPos; currentPos <= maxCorridorEndPos; currentPos++)
            {
                var canFitCorridor = true;
                for (var i = 1; i <= corridorWidth && canFitCorridor; i++)
                    //If not empty or corridor
                    if (gridData[wallStart[0] + i, wallStart[1] + currentPos].RoomType is not GeneratorGlobalVals.EMPTY
                        or GeneratorGlobalVals.CORRIDOR)
                    {
                        canFitCorridor = false;
                        successiveValidCorridorSpots = 0;
                    }

                if (canFitCorridor) successiveValidCorridorSpots++;

                if (successiveValidCorridorSpots == corridorWidth)
                {
                    var minValid = Math.Max(currentPos - corridorWidth + 1, 0);
                    var maxValid = Mathf.Min(currentPos + 1 - doorwayWidth, roomData.RoomSize[1] - 1,
                        validDoorPos.Length - 1);
                    for (var i = minValid; i <= maxValid; i++) validDoorPos[i] = true;
                }
                else if (successiveValidCorridorSpots > corridorWidth &&
                         currentPos + 1 - doorwayWidth < validDoorPos.Length)
                {
                    validDoorPos[currentPos + 1 - doorwayWidth] = true;
                }
            }

            for (var i = 0; i < validDoorPos.Length; i++)
                if (validDoorPos[i])
                    validPositionsArray.Add(new Door(i, SOUTH));
        }

        /*
         * Checks for valid positions a door could be placed on the North wall of a room.
         * Valid  options are added to the referenced list provided as an argument
         */
        private void CheckValidDoorPosNorth(Room roomData, List<Door> validPositionsArray,
            ref GridSegmentData[,] gridData)
        {
            var corridorWidth = GeneratorGlobalVals.Instance.GetCorridorWidthTiles();
            var doorwayWidth = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            var wallStart = roomData.StartPos;
            //Keeps track of which positions are valid for a door for the given room
            var validDoorPos = new bool[roomData.RoomSize[1] - doorwayWidth + 1];
            for (var i = 0; i < validDoorPos.Length; i++) validDoorPos[i] = false;

            //Absolute min & max a corridor can fit at
            var absCorridorStartPos = math.max(roomData.StartPos[1] + (doorwayWidth - corridorWidth), 1);
            var absCorridorMaxPos =
                math.min(roomData.StartPos[1] + roomData.RoomSize[1] - (doorwayWidth - corridorWidth) - 1,
                    gridData.GetLength(1) - 1);
            //Relative min & max a corridor can fit at
            var minCorridorStartPos = absCorridorStartPos - roomData.StartPos[1];
            var maxCorridorEndPos = absCorridorMaxPos - roomData.StartPos[1];


            //Relative min & max a corridor can fit at
            //int minCorridorStartPos = doorwayWidth-corridorWidth;
            //int maxCorridorEndPos = roomData.RoomSize[1] - minCorridorStartPos-1;
            var successiveValidCorridorSpots = 0;
            for (var currentPos = minCorridorStartPos; currentPos <= maxCorridorEndPos; currentPos++)
            {
                var canFitCorridor = true;
                for (var i = 1; i <= corridorWidth && canFitCorridor; i++)
                    //If not empty or corridor
                    if (gridData[wallStart[0] - i, wallStart[1] + currentPos].RoomType is not GeneratorGlobalVals.EMPTY
                        or GeneratorGlobalVals.CORRIDOR)
                    {
                        canFitCorridor = false;
                        successiveValidCorridorSpots = 0;
                    }

                if (canFitCorridor) successiveValidCorridorSpots++;

                if (successiveValidCorridorSpots == corridorWidth)
                {
                    var minValid = Math.Max(currentPos - corridorWidth + 1, 0);
                    var maxValid = Mathf.Min(currentPos + 1 - doorwayWidth, roomData.RoomSize[1] - 1,
                        validDoorPos.Length - 1);


                    for (var i = minValid; i <= maxValid; i++) validDoorPos[i] = true;
                }
                else if (successiveValidCorridorSpots > corridorWidth &&
                         currentPos + 1 - doorwayWidth < validDoorPos.Length)
                {
                    validDoorPos[currentPos + 1 - doorwayWidth] = true;
                }
            }

            for (var i = 0; i < validDoorPos.Length; i++)
                if (validDoorPos[i])
                    validPositionsArray.Add(new Door(i, NORTH));
        }

        /*
        * Checks for valid positions a door could be placed on the West wall of a room.
        * Valid  options are added to the referenced list provided as an argument
        */
        private void CheckValidDoorPosEast(Room roomData, List<Door> validPositionsArray,
            ref GridSegmentData[,] gridData)
        {
            var corridorWidth = GeneratorGlobalVals.Instance.GetCorridorWidthTiles();
            var doorwayWidth = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            int[] wallStart = { roomData.StartPos[0], roomData.StartPos[1] + roomData.RoomSize[1] - 1 };
            //Keeps track of which positions are valid for a door for the given room
            var validDoorPos = new bool[roomData.RoomSize[0] - doorwayWidth + 1];
            for (var i = 0; i < validDoorPos.Length; i++) validDoorPos[i] = false;
            //Absolute min & max a corridor can fit at
            var absCorridorStartPos = math.max(roomData.StartPos[0] + (doorwayWidth - corridorWidth), 1);
            var absCorridorMaxPos =
                math.min(roomData.StartPos[0] + roomData.RoomSize[0] - (doorwayWidth - corridorWidth) - 1,
                    gridData.GetLength(0) - 1);
            //Relative min & max a corridor can fit at
            var minCorridorStartPos = absCorridorStartPos - roomData.StartPos[0];
            var maxCorridorEndPos = absCorridorMaxPos - roomData.StartPos[0];
            //Make sure relative positioning fits on grid
            var successiveValidCorridorSpots = 0;
            for (var currentPos = minCorridorStartPos; currentPos <= maxCorridorEndPos; currentPos++)
            {
                var canFitCorridor = true;
                for (var i = 1; i <= corridorWidth && canFitCorridor; i++)
                    //If not empty or corridor
                    if (gridData[wallStart[0] + currentPos, wallStart[1] + i].RoomType is not GeneratorGlobalVals.EMPTY
                        or GeneratorGlobalVals.CORRIDOR)
                    {
                        canFitCorridor = false;
                        successiveValidCorridorSpots = 0;
                    }

                if (canFitCorridor) successiveValidCorridorSpots++;

                if (successiveValidCorridorSpots == corridorWidth)
                {
                    var minValid = Math.Max(currentPos - corridorWidth + 1, 0);
                    var maxValid = Mathf.Min(currentPos + 1 - doorwayWidth, roomData.RoomSize[0] - 1,
                        validDoorPos.Length - 1);
                    for (var i = minValid; i <= maxValid; i++) validDoorPos[i] = true;
                }
                else if (successiveValidCorridorSpots > corridorWidth &&
                         currentPos + 1 - doorwayWidth < validDoorPos.Length)
                {
                    validDoorPos[currentPos + 1 - doorwayWidth] = true;
                }
            }

            for (var i = 0; i < validDoorPos.Length; i++)
                if (validDoorPos[i])
                    validPositionsArray.Add(new Door(i, EAST));
        }

        /*
        * Checks for valid positions a door could be placed on the West wall of a room.
        * Valid  options are added to the referenced list provided as an argument
        */
        private void CheckValidDoorPosWest(Room roomData, List<Door> validPositionsArray,
            ref GridSegmentData[,] gridData)
        {
            var corridorWidth = GeneratorGlobalVals.Instance.GetCorridorWidthTiles();
            var doorwayWidth = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            var wallStart = roomData.StartPos;
            //Keeps track of which positions are valid for a door for the given room
            var validDoorPos = new bool[roomData.RoomSize[0] - doorwayWidth + 1];
            for (var i = 0; i < validDoorPos.Length; i++) validDoorPos[i] = false;

            //Absolute min & max a corridor can fit at
            var absCorridorStartPos = math.max(roomData.StartPos[0] + (doorwayWidth - corridorWidth), 1);
            var absCorridorMaxPos =
                math.min(roomData.StartPos[0] + roomData.RoomSize[0] - (doorwayWidth - corridorWidth) - 1,
                    gridData.GetLength(0) - 1);
            //Relative min & max a corridor can fit at
            var minCorridorStartPos = absCorridorStartPos - roomData.StartPos[0];
            var maxCorridorEndPos = absCorridorMaxPos - roomData.StartPos[0];
            //Make sure relative positioning fits on grid

            var successiveValidCorridorSpots = 0;
            for (var currentPos = minCorridorStartPos; currentPos <= maxCorridorEndPos; currentPos++)
            {
                var canFitCorridor = true;
                for (var i = 1; i <= corridorWidth && canFitCorridor; i++)
                    //If not empty or corridor
                    if (gridData[wallStart[0] + currentPos, wallStart[1] - i].RoomType is not GeneratorGlobalVals.EMPTY
                        or GeneratorGlobalVals.CORRIDOR)
                    {
                        canFitCorridor = false;
                        successiveValidCorridorSpots = 0;
                    }

                if (canFitCorridor) successiveValidCorridorSpots++;

                if (successiveValidCorridorSpots == corridorWidth)
                {
                    var minValid = Math.Max(currentPos - corridorWidth + 1, 0);
                    var maxValid = Mathf.Min(currentPos + 1 - doorwayWidth, roomData.RoomSize[0] - 1,
                        validDoorPos.Length - 1);
                    for (var i = minValid; i <= maxValid; i++) validDoorPos[i] = true;
                }
                else if (successiveValidCorridorSpots > corridorWidth &&
                         currentPos + 1 - doorwayWidth < validDoorPos.Length)
                {
                    validDoorPos[currentPos + 1 - doorwayWidth] = true;
                }
            }

            for (var i = 0; i < validDoorPos.GetLength(0); i++)
                if (validDoorPos[i])
                    validPositionsArray.Add(new Door(i, WEST));
        }

        /**
          * Attempts to place doorway in chosen place
          */
        private bool PlaceDoorway(Room room, Door newDoor, ref GridSegmentData[,] gridData)
        {
            var doorwayWidth = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            var doorStartPos = new int[2];
            switch (newDoor.Wall)
            {
                case NORTH:
                case SOUTH:
                    if (newDoor.Offset + doorwayWidth > room.RoomSize[1]) return false;
                    foreach (var door in room.Doors)
                    {
                        if (door.Wall != newDoor.Wall) continue;
                        for (var i = door.Offset - doorwayWidth; i <= door.Offset + doorwayWidth; i++)
                            if (newDoor.Offset == i)
                                return false;
                    }

                    doorStartPos[1] = room.StartPos[1] + newDoor.Offset;
                    switch (newDoor.Wall)
                    {
                        case NORTH:
                            doorStartPos[0] = room.StartPos[0] - 1;
                            break;
                        case SOUTH:
                            doorStartPos[0] = room.StartPos[0] + room.RoomSize[0] - 1;
                            break;
                    }

                    for (var i = 0; i < doorwayWidth; i++)
                        gridData[doorStartPos[0], doorStartPos[1] + i].SetSouthWallIsDoorway(true);
                    room.AddDoor(newDoor);
                    return true;
                case EAST:
                case WEST:
                    if (newDoor.Offset + doorwayWidth > room.RoomSize[0]) return false;
                    foreach (var door in room.Doors)
                    {
                        if (door.Wall != newDoor.Wall) continue;
                        for (var i = door.Offset - doorwayWidth; i <= door.Offset + doorwayWidth; i++)
                            if (newDoor.Offset == i)
                                return false;
                    }

                    doorStartPos[0] = room.StartPos[0] + newDoor.Offset;
                    switch (newDoor.Wall)
                    {
                        case EAST:
                            doorStartPos[1] = room.StartPos[1] + room.RoomSize[1] - 1;
                            break;
                        case WEST:
                            doorStartPos[1] = room.StartPos[1] - 1;
                            break;
                    }

                    for (var i = 0; i < doorwayWidth; i++)
                        gridData[doorStartPos[0] + i, doorStartPos[1]].SetEastWallIsDoorway(true);
                    room.AddDoor(newDoor);
                    return true;
            }

            return false;
        }

        private void RoomDestroyer(int[] pointInRoom, ref GridSegmentData[,] gridData)
        {
            if (gridData[pointInRoom[0], pointInRoom[1]].RoomType != 1) return;
            var room = gridData[pointInRoom[0], pointInRoom[1]].Room;
            if (!room.isDestroyable)
            {
                Debug.Log("Can not destroy room: Room is indestructible");
                return;
            }

            var roomSize = room.RoomSize;
            var startPos = room.StartPos;
            //Delete exterior walls
            //Make it no longer a room

            for (var x = 0; x < roomSize[1]; x++)
            {
                if (gridData[startPos[0] - 1, x + startPos[1]].RoomType == -1)
                    gridData[startPos[0] - 1, x + startPos[1]].SetSouthWall(false);


                if (startPos[0] + roomSize[0] >= gridData.GetLength(0) ||
                    gridData[startPos[0] + roomSize[0], x + startPos[1]].RoomType == -1)
                    gridData[startPos[0] + roomSize[0] - 1, x + startPos[1]].SetSouthWall(false);
            }

            for (var x = 0; x < roomSize[0]; x++)
            {
                if (gridData[x + startPos[0], startPos[1] - 1].RoomType == -1)
                    gridData[x + startPos[0], startPos[1] - 1].SetEastWall(false);
                if (startPos[1] + roomSize[1] >= gridData.GetLength(1) ||
                    gridData[x + startPos[0], startPos[1] + roomSize[1]].RoomType == -1)
                    gridData[x + startPos[0], startPos[1] - 1 + roomSize[1]].SetEastWall(false);
            }

            for (var x = startPos[0]; x < startPos[0] + roomSize[0]; x++)
            for (var z = startPos[1]; z < startPos[1] + roomSize[1]; z++)
            {
                gridData[x, z].RoomType = -1;
                gridData[x, z].Room = null;
            }

            room.isDestroyed = true;
            //Debug.Log("Room destroyed");
        }
    }
}