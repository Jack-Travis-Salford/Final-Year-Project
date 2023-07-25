using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script
{
    public class RoomGenerator
    {
        public void GenerateRooms(ref GridSegmentData[,] gridData, ref List<Room> rooms)
        {
            var roomBounds = GeneratorGlobalVals.Instance.GetRoomSizeTilesBounds();


            var gridSize = GeneratorGlobalVals.Instance.GetNoTiles();
            var percentToBeRooms = GeneratorGlobalVals.Instance.GetMapPercentToBeRooms();
            float totalTiles = (gridData.GetLength(0) - 1) * (gridData.GetLength(1) - 1);
            float currentRoomTiles = 0;
            var generateRooms = true;
            var totalRooms = 0;

            while (generateRooms)
            {
                var roomSize = new int[2];
                switch (totalRooms)
                {
                    case >= 2:
                        for (var i = 0; i < 2; i++) roomSize[i] = Random.Range(roomBounds[i, 0], roomBounds[i, 1] + 1);
                        break;
                    case 1:
                        roomSize = GeneratorGlobalVals.Instance.GetEndRoomSizeTilesBounds();
                        break;
                    case 0:
                        roomSize = GeneratorGlobalVals.Instance.GetStartRoomSizeTilesBounds();
                        break;
                }

                //Pick pos in grid
                var startPos = new int[2];
                var posIsValid = false;
                var attempts = 0;
                var minRoomSizeX = GeneratorGlobalVals.Instance.GetRoomSizeTilesBounds()[0, 0];
                var minRoomSizeZ = GeneratorGlobalVals.Instance.GetRoomSizeTilesBounds()[1, 0];
                while (!posIsValid && attempts <= 1000)
                {
                    for (var i = 0; i < 2; i++) startPos[i] = Random.Range(1, gridSize[i] - roomSize[i] + 1);

                    attempts++;
                    posIsValid = true;
                    //Make sure room can be placed on chosen tiles
                    //Incrementer not in for, should be While?
                    for (var x = startPos[0]; x < startPos[0] + roomSize[0] && posIsValid;)
                    {
                        for (var z = startPos[1]; z < startPos[1] + roomSize[1] && posIsValid;)
                        {
                            if (gridData[x, z].RoomType != -1) posIsValid = false;
                            //Inner For statement incrementer
                            if (z != startPos[1] + roomSize[1] - 1)
                                z = Mathf.Min(z + minRoomSizeZ, startPos[1] + roomSize[1] - 1);
                            else
                                z++;
                        }

                        //Outer For statement incrementer
                        if (x != startPos[0] + roomSize[0] - 1)
                            x = Mathf.Min(x + minRoomSizeX, startPos[0] + roomSize[0] - 1);
                        else
                            x++;
                    }
                }

                if (posIsValid)
                {
                    totalRooms++;
                    var rd = new Room(roomSize, startPos);
                    rooms.Add(rd);
                    //Alter gridData to create walls for room
                    for (var x = 0; x < roomSize[1]; x++)
                    {
                        gridData[startPos[0] - 1, x + startPos[1]].SetSouthWall(true);
                        gridData[startPos[0] + roomSize[0] - 1, x + startPos[1]].SetSouthWall(true);
                    }

                    for (var x = 0; x < roomSize[0]; x++)
                    {
                        gridData[x + startPos[0], startPos[1] - 1].SetEastWall(true);
                        gridData[x + startPos[0], startPos[1] - 1 + roomSize[1]].SetEastWall(true);
                    }


                    for (var x = startPos[0]; x < startPos[0] + roomSize[0]; x++)
                    for (var z = startPos[1]; z < startPos[1] + roomSize[1]; z++)
                    {
                        gridData[x, z].RoomType = GeneratorGlobalVals.ROOM;
                        gridData[x, z].Room = rd;
                    }


                    currentRoomTiles += roomSize[0] * roomSize[1];
                    if (currentRoomTiles / totalTiles >= percentToBeRooms && totalRooms >= 2) generateRooms = false;
                }
                else if (totalRooms >= 2)
                {
                    //If failed placement, but not yet at target percent to be rooms, decrease max room size
                    if (roomBounds[0, 0] == roomBounds[0, 1] && roomBounds[1, 0] == roomBounds[1, 1])
                    {
                        generateRooms = false;
                    }
                    else
                    {
                        roomBounds[0, 1] = Math.Max(roomBounds[0, 0], roomBounds[0, 1] - 1);
                        roomBounds[1, 1] = Math.Max(roomBounds[1, 0], roomBounds[1, 1] - 1);
                    }
                }
            }
        }
    }
}