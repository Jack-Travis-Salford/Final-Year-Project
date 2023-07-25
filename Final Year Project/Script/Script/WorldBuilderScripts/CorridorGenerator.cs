using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script
{
    public class CorridorGenerator
    {
        //gridData[newCorridorPos[0],newCorridorPos[1]].RoomType = GeneratorGlobalVals.ROOM;
        private const int NORTH = GeneratorGlobalVals.NORTH;
        private const int EAST = GeneratorGlobalVals.EAST;
        private const int SOUTH = GeneratorGlobalVals.SOUTH;
        private const int WEST = GeneratorGlobalVals.WEST;
        private const int UP = 1;
        private const int LEFT = 2;
        private const int RIGHT = 3;
        private readonly bool debuggingOn = false;
        private readonly string fileName = "CorridorGenerationDecisions.txt";
        private StreamWriter sw;

        public void GenerateCorridors(ref GridSegmentData[,] gridData, ref List<Room> rooms)
        {
            sw = File.CreateText(fileName);
            var corridorWidth = GeneratorGlobalVals.Instance.GetCorridorWidthTiles();
            /* for (int i = 0; i < rooms.Count; i++) // rooms.Count
             {
                 
                 Room room = rooms[i];
                 
                 //GameObject testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                 //testObject.transform.position = new Vector3(room.StartPos[0]+0.5f, 1f, room.StartPos[1]+0.5f);
                 //testObject.transform.localScale = new Vector3(.5f, .5f, .5f);
                 //testObject.name = "Room " + i;
                 sw.WriteLine("Room " + i + " Position [" + room.StartPos[0] + "," + room.StartPos[1] + "]");
                 foreach (Door door in room.Doors)
                 {
                     Stack<CorridorRecursiveCallData> recursiveCallStack = new Stack<CorridorRecursiveCallData>();
                     sw.Write("NEW DOOR. Offset:" + door.Offset + " Wall:");
                     switch (door.Wall)
                     {
                         case NORTH:
                             sw.Write("NORTH\n");
                             StartNorthCorridor(ref gridData,room, door,corridorWidth, ref recursiveCallStack);
                             break;
                         case SOUTH: 
                             sw.Write("SOUTH\n");
                             StartSouthCorridor(ref gridData,room, door,corridorWidth, ref recursiveCallStack);
                             break;
                         case EAST:
                             sw.WriteLine("EAST\n");
                             StartEastCorridor(ref gridData,room, door,corridorWidth, ref recursiveCallStack);
                             break;
                         case WEST:
                             sw.WriteLine("WEST\n");
                             StartWestCorridor(ref gridData,room, door,corridorWidth, ref recursiveCallStack);
                             break;
                     }
 
                     while (recursiveCallStack.Count > 0)
                     {
                         HandleNextCall(ref recursiveCallStack, ref gridData);
                     }
                 }
             }*/


            foreach (var room in rooms)
            foreach (var door in room.Doors)
            {
                var recursiveCallStack = new Stack<CorridorRecursiveCallData>();
                switch (door.Wall)
                {
                    case NORTH:
                        StartNorthCorridor(ref gridData, room, door, corridorWidth, ref recursiveCallStack);
                        break;
                    case SOUTH:
                        StartSouthCorridor(ref gridData, room, door, corridorWidth, ref recursiveCallStack);
                        break;
                    case EAST:
                        StartEastCorridor(ref gridData, room, door, corridorWidth, ref recursiveCallStack);
                        break;
                    case WEST:
                        StartWestCorridor(ref gridData, room, door, corridorWidth, ref recursiveCallStack);
                        break;
                }

                while (recursiveCallStack.Count > 0) HandleNextCall(ref recursiveCallStack, ref gridData);
            }

            sw.Close();
            //DEBUGGING
            /*gridData[150, 200].RoomType = GeneratorGlobalVals.CORRIDOR;

            
            
            int[] startPoint = new[] { 150,200 };
            CorridorPathingDecider cpd = new CorridorPathingDecider();
            HandleNorthCorridor(ref gridData,startPoint, corridorWidth,cpd);*/
        }

        private void HandleNextCall(ref Stack<CorridorRecursiveCallData> recursiveCallData,
            ref GridSegmentData[,] gridData)
        {
            var data = recursiveCallData.Pop();
            switch (data.PathingOption)
            {
                case NORTH:
                    HandleNorthCorridor(ref gridData, data.NewCorridorPos, data.CorridorWidth, data.PathingDecider,
                        ref recursiveCallData);
                    break;
                case SOUTH:
                    HandleSouthCorridor(ref gridData, data.NewCorridorPos, data.CorridorWidth, data.PathingDecider,
                        ref recursiveCallData);
                    break;
                case EAST:
                    HandleEastCorridor(ref gridData, data.NewCorridorPos, data.CorridorWidth, data.PathingDecider,
                        ref recursiveCallData);
                    break;
                case WEST:
                    HandleWestCorridor(ref gridData, data.NewCorridorPos, data.CorridorWidth, data.PathingDecider,
                        ref recursiveCallData);
                    break;
                default:
                    Debug.Log("Error: No matches for next call");
                    break;
            }
        }

        /**
         * Checks to see if door is already connected to a corridor. If so, nothing needs to be done
         * 
         * Checks to see if door is partially connected to a corridor. If so, connects door to corridor
         * 
         * Checks to see if door is not connected to any corridor. If so, picks a valid position to begin
         * corridor generation
         */
        private void StartNorthCorridor(ref GridSegmentData[,] gridData, Room room, Door door, int corridorWidth,
            ref Stack<CorridorRecursiveCallData> recursiveCallStack)
        {
            int[] corridorStartPos;

            int[] doorStartPos = { room.StartPos[0], room.StartPos[1] + door.Offset };
            var doorwayWidth = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            //Check for a blocking/complete connection to existing corridor
            var corridorTilesFound = 0;
            for (var i = 0; i < doorwayWidth; i++)
                if (gridData[doorStartPos[0] - 1, doorStartPos[1] + i].RoomType == GeneratorGlobalVals.CORRIDOR)
                    corridorTilesFound++;

            if (corridorTilesFound == doorwayWidth) //Door is already connected
                //Debug.Log(doorStartPos[0] + "," + doorStartPos[1] + " is already connected");
                return;

            if (corridorTilesFound > 0) //Door is partially blocked
            {
                //Debug.Log(doorStartPos[0] + "," + doorStartPos[1] + " is blocked");
                //Make area of doorwayWidth * corridorWidth corridors
                //Claim corridor tiles
                for (var x = 1; x <= corridorWidth; x++)
                for (var z = 0; z < doorwayWidth; z++)
                    gridData[doorStartPos[0] - x, doorStartPos[1] + z].RoomType = GeneratorGlobalVals.CORRIDOR;
                //Remove any unwanted internals walls
                if (doorwayWidth > 1)
                    for (var x = 1; x <= corridorWidth; x++)
                    for (var z = 0; z < doorwayWidth - 1; z++)
                        gridData[doorStartPos[0] - x, doorStartPos[1] + z].SetEastWall(false);

                if (corridorWidth > 1)
                    for (var x = 2; x <= corridorWidth; x++)
                    for (var z = 0; z < doorwayWidth; z++)
                        gridData[doorStartPos[0] - x, doorStartPos[1] + z].SetSouthWall(false);
                //Remove/add external walls
                for (var x = 1; x <= corridorWidth; x++)
                {
                    if (gridData[doorStartPos[0] - x, doorStartPos[1] - 1].RoomType == GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] - x, doorStartPos[1] - 1].SetEastWall(false);
                    else
                        gridData[doorStartPos[0] - x, doorStartPos[1] - 1].SetEastWall(true);
                    if (doorStartPos[1] + doorwayWidth < gridData.GetLength(1) &&
                        gridData[doorStartPos[0] - x, doorStartPos[1] + doorwayWidth].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] - x, doorStartPos[1] + doorwayWidth - 1].SetEastWall(false);
                    else
                        gridData[doorStartPos[0] - x, doorStartPos[1] + doorwayWidth - 1].SetEastWall(true);
                }

                for (var y = 0; y < doorwayWidth; y++)
                    if (gridData[doorStartPos[0] - corridorWidth - 1, doorStartPos[1] + y].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] - corridorWidth - 1, doorStartPos[1] + y].SetSouthWall(false);
                    else
                        gridData[doorStartPos[0] - corridorWidth - 1, doorStartPos[1] + y].SetSouthWall(true);
                return;
            }

            //Door isn't blocked: Generate corridor
            if (corridorWidth == doorwayWidth) //If there is only one possible corridor position
            {
                var cpd = new CorridorPathingDecider();
                corridorStartPos = doorStartPos.Clone() as int[];
                CreateInitialNorthCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                HandleNorthCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }

            //Choose a corridor position, when theres potentially multiple options
            var minCorridorStartPos = Math.Max(1, doorStartPos[1] - (corridorWidth - doorwayWidth));

            var maxCorridorStartPos = doorStartPos[1];
            var maxCorridorEndPos = Math.Min(doorStartPos[1] + corridorWidth, gridData.GetLength(1));
            if (minCorridorStartPos == maxCorridorStartPos) //If there is only one possible corridor position
            {
                corridorStartPos = new[] { doorStartPos[0], minCorridorStartPos };
                var cpd = new CorridorPathingDecider();
                CreateInitialNorthCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                HandleNorthCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }

            var successiveValidTiles = 0;
            int chosenOption;
            var corridorOptions = new List<int>();
            for (var currentPos = minCorridorStartPos; currentPos < maxCorridorEndPos; currentPos++)
            {
                var isValidRow = true;
                for (var j = 1; j <= corridorWidth && isValidRow; j++)
                    if (gridData[doorStartPos[0] - j, currentPos].RoomType is not GeneratorGlobalVals.EMPTY)
                    {
                        isValidRow = false;
                        successiveValidTiles = 0;
                    }

                if (isValidRow) successiveValidTiles++;

                if (successiveValidTiles >= corridorWidth) corridorOptions.Add(currentPos - corridorWidth + 1);
            }

            if (corridorOptions.Count != 0)
            {
                chosenOption = Random.Range(0, corridorOptions.Count);
                corridorStartPos = new[] { doorStartPos[0], corridorOptions[chosenOption] };
                CreateInitialNorthCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                var cpd = new CorridorPathingDecider();
                HandleNorthCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }

            //Debug.Log("No valid corridor positions for door " + doorStartPos[0] + "," + doorStartPos[1]);
            //Pick a valid start position for corridor
            successiveValidTiles = 0;
            for (var currentPos = minCorridorStartPos; currentPos < maxCorridorEndPos; currentPos++)
            {
                var isValidRow = true;
                for (var j = 1; j <= corridorWidth && isValidRow; j++)
                    if (gridData[doorStartPos[0] - j, currentPos].RoomType is not (GeneratorGlobalVals.EMPTY
                        or GeneratorGlobalVals.CORRIDOR))
                    {
                        isValidRow = false;
                        successiveValidTiles = 0;
                    }

                if (isValidRow) successiveValidTiles++;

                if (successiveValidTiles >= corridorWidth) corridorOptions.Add(currentPos - corridorWidth + 1);
            }

            if (corridorOptions.Count == 0)
            {
                Debug.Log("Something went wrong: No valid options to begin corridor " + doorStartPos[0] + "," +
                          doorStartPos[1]);
                return;
            }

            chosenOption = corridorOptions[Random.Range(0, corridorOptions.Count)];
            corridorStartPos = new[] { doorStartPos[0], chosenOption };

            var connectionFound = false;
            var tilesPathedForward = 0;
            //Add initial walls
            for (var z = 0; z < corridorWidth; z++)
                if (gridData[corridorStartPos[0], corridorStartPos[1] + z].RoomType == GeneratorGlobalVals.EMPTY)
                    gridData[corridorStartPos[0] - 1, corridorStartPos[1] + z].SetSouthWall(true);

            //Path forward until a corridor is found
            while (!connectionFound && tilesPathedForward < corridorWidth)
            {
                for (var i = 0; i < corridorWidth; i++)
                    if (gridData[corridorStartPos[0] - 1, corridorStartPos[1] + i].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        connectionFound = true;
                if (!connectionFound)
                {
                    tilesPathedForward++;
                    corridorStartPos[0]--;
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorStartPos[0], corridorStartPos[1] + i].RoomType = GeneratorGlobalVals.CORRIDOR;
                    gridData[corridorStartPos[0], corridorStartPos[1] - 1].SetEastWall(true);
                    gridData[corridorStartPos[0], corridorStartPos[1] + corridorWidth - 1].SetEastWall(true);
                }
            }

            if (!connectionFound)
            {
                Debug.Log("Error: Couldn't find corridor connection for \"Blocked corridor\"" + doorStartPos[0] + "," +
                          doorStartPos[1]);
                return;
            }

            //Claim tiles for corridor
            for (var x = 1; x <= corridorWidth; x++)
            for (var z = 0; z < corridorWidth; z++)
                gridData[corridorStartPos[0] - x, corridorStartPos[1] + z].RoomType = GeneratorGlobalVals.CORRIDOR;
            //Remove any unwanted internals walls
            if (corridorWidth > 1)
            {
                for (var x = 1; x <= corridorWidth; x++)
                for (var z = 0; z < corridorWidth - 1; z++)
                    gridData[corridorStartPos[0] - x, corridorStartPos[1] + z].SetEastWall(false);

                var offset = tilesPathedForward == 0 ? 1 : 0;
                for (var x = 1 + offset; x <= corridorWidth; x++)
                for (var z = 0; z < corridorWidth; z++)
                    gridData[corridorStartPos[0] - x, corridorStartPos[1] + z].SetSouthWall(false);
            }

            //Remove/add external walls
            for (var x = 1; x <= corridorWidth; x++)
            {
                if (gridData[corridorStartPos[0] - x, corridorStartPos[1] - 1].RoomType == GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] - x, corridorStartPos[1] - 1].SetEastWall(false);
                else
                    gridData[corridorStartPos[0] - x, corridorStartPos[1] - 1].SetEastWall(true);
                if (corridorStartPos[1] + corridorWidth < gridData.GetLength(1) &&
                    gridData[corridorStartPos[0] - x, corridorStartPos[1] + corridorWidth].RoomType ==
                    GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] - x, corridorStartPos[1] + corridorWidth - 1].SetEastWall(false);
                else
                    gridData[corridorStartPos[0] - x, corridorStartPos[1] + corridorWidth - 1].SetEastWall(true);
            }

            for (var y = 0; y < corridorWidth; y++)
                if (gridData[corridorStartPos[0] - corridorWidth - 1, corridorStartPos[1] + y].RoomType ==
                    GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] - corridorWidth - 1, corridorStartPos[1] + y].SetSouthWall(false);
                else
                    gridData[corridorStartPos[0] - corridorWidth - 1, corridorStartPos[1] + y].SetSouthWall(true);
            //Debug.Log("A connection was made");
        }

        /**
         * Checks to see if door is already connected to a corridor. If so, nothing needs to be done
         * 
         * Checks to see if door is partially connected to a corridor. If so, connects door to corridor
         * 
         * Checks to see if door is not connected to any corridor. If so, picks a valid position to begin
         * corridor generation
         */
        private void StartSouthCorridor(ref GridSegmentData[,] gridData, Room room, Door door, int corridorWidth,
            ref Stack<CorridorRecursiveCallData> recursiveCallStack)
        {
            int[] corridorStartPos;
            int[] doorStartPos = { room.StartPos[0] + room.RoomSize[0] - 1, room.StartPos[1] + door.Offset };

            var doorwayWidth = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            //Check for a blocking/complete connection to existing corridor
            var corridorTilesFound = 0;
            for (var i = 0; i < doorwayWidth; i++)
                if (gridData[doorStartPos[0] + 1, doorStartPos[1] + i].RoomType == GeneratorGlobalVals.CORRIDOR)
                    corridorTilesFound++;
            if (corridorTilesFound == doorwayWidth) //Door is already connected
                //Debug.Log(doorStartPos[0] + "," +doorStartPos[1] + " is already connected");
                return;
            if (corridorTilesFound > 0) //Door is partially blocked
            {
                //Debug.Log(doorStartPos[0] + "," +doorStartPos[1] + " is blocked");
                //Make area of doorwayWidth * corridorWidth corridors
                //Claim corridor tiles
                for (var x = 1; x <= corridorWidth; x++)
                for (var z = 0; z < doorwayWidth; z++)
                    gridData[doorStartPos[0] + x, doorStartPos[1] + z].RoomType = GeneratorGlobalVals.CORRIDOR;
                //Remove any unwanted internals walls
                if (doorwayWidth > 1)
                    for (var x = 1; x <= corridorWidth; x++)
                    for (var z = 0; z < doorwayWidth - 1; z++)
                        gridData[doorStartPos[0] + x, doorStartPos[1] + z].SetEastWall(false);
                if (corridorWidth > 1)
                    for (var x = 1; x < corridorWidth; x++)
                    for (var z = 0; z < doorwayWidth; z++)
                        gridData[doorStartPos[0] + x, doorStartPos[1] + z].SetSouthWall(false);
                //Remove/add external walls
                for (var x = 1; x <= corridorWidth; x++)
                {
                    if (gridData[doorStartPos[0] + x, doorStartPos[1] - 1].RoomType == GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] + x, doorStartPos[1] - 1].SetEastWall(false);
                    else
                        gridData[doorStartPos[0] + x, doorStartPos[1] - 1].SetEastWall(true);
                    if (doorStartPos[1] + doorwayWidth < gridData.GetLength(1) &&
                        gridData[doorStartPos[0] + x, doorStartPos[1] + doorwayWidth].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] + x, doorStartPos[1] + doorwayWidth - 1].SetEastWall(false);
                    else
                        gridData[doorStartPos[0] + x, doorStartPos[1] + doorwayWidth - 1].SetEastWall(true);
                }

                for (var y = 0; y < doorwayWidth; y++)
                    if (doorStartPos[0] + corridorWidth + 1 < gridData.GetLength(0) &&
                        gridData[doorStartPos[0] + corridorWidth + 1, doorStartPos[1] + y].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] + corridorWidth, doorStartPos[1] + y].SetSouthWall(false);
                    else
                        gridData[doorStartPos[0] + corridorWidth, doorStartPos[1] + y].SetSouthWall(true);
                return;
            }

            //Door isn't blocked: Generate corridor
            if (corridorWidth == doorwayWidth) //If there is only one possible corridor position
            {
                var cpd = new CorridorPathingDecider();
                corridorStartPos = doorStartPos.Clone() as int[];
                CreateInitialSouthCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                HandleSouthCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }

            //Choose a corridor position, when theres potentially multiple options
            var minCorridorStartPos = Math.Max(1, doorStartPos[1] - (corridorWidth - doorwayWidth));
            var maxCorridorStartPos = doorStartPos[1];
            var maxCorridorEndPos = Math.Min(doorStartPos[1] + corridorWidth, gridData.GetLength(1));
            if (minCorridorStartPos == maxCorridorStartPos) //If there is only one possible corridor position
            {
                corridorStartPos = new[] { doorStartPos[0], minCorridorStartPos };
                var cpd = new CorridorPathingDecider();
                CreateInitialSouthCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                HandleSouthCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }

            var successiveValidTiles = 0;
            int chosenOption;
            var corridorOptions = new List<int>();
            for (var currentPos = minCorridorStartPos; currentPos < maxCorridorEndPos; currentPos++)
            {
                var isValidRow = true;
                for (var j = 1; j <= corridorWidth && isValidRow; j++)
                    if (gridData[doorStartPos[0] + j, currentPos].RoomType is not GeneratorGlobalVals.EMPTY)
                    {
                        isValidRow = false;
                        successiveValidTiles = 0;
                    }

                if (isValidRow) successiveValidTiles++;

                if (successiveValidTiles >= corridorWidth) corridorOptions.Add(currentPos - corridorWidth + 1);
            }

            if (corridorOptions.Count != 0)
            {
                chosenOption = Random.Range(0, corridorOptions.Count);
                corridorStartPos = new[] { doorStartPos[0], corridorOptions[chosenOption] };
                var cpd = new CorridorPathingDecider();
                CreateInitialSouthCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                HandleSouthCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }
            //In this case, there in no available space to start generating a corridor
            //Connecting to an existing corridor is the only option

            //Debug.Log("No valid corridor positions for door " + doorStartPos[0] + "," +doorStartPos[1]);

            //Pick a valid start position for corridor
            successiveValidTiles = 0;
            for (var currentPos = minCorridorStartPos; currentPos < maxCorridorEndPos; currentPos++)
            {
                var isValidRow = true;
                for (var j = 1; j <= corridorWidth && isValidRow; j++)
                    if (gridData[doorStartPos[0] + j, currentPos].RoomType is not (GeneratorGlobalVals.EMPTY
                        or GeneratorGlobalVals.CORRIDOR))
                    {
                        isValidRow = false;
                        successiveValidTiles = 0;
                    }

                if (isValidRow) successiveValidTiles++;

                if (successiveValidTiles >= corridorWidth) corridorOptions.Add(currentPos - corridorWidth + 1);
            }

            if (corridorOptions.Count == 0)
            {
                Debug.Log("Something went wrong: No valid options to begin corridor " + doorStartPos[0] + "," +
                          doorStartPos[1]);
                return;
            }

            chosenOption = corridorOptions[Random.Range(0, corridorOptions.Count)];
            corridorStartPos = new[] { doorStartPos[0], chosenOption };
            var connectionFound = false;
            var tilesPathedForward = 0;
            //Add initial walls
            for (var z = 0; z < corridorWidth; z++)
                if (gridData[corridorStartPos[0], corridorStartPos[1] + z].RoomType == GeneratorGlobalVals.EMPTY)
                    gridData[corridorStartPos[0], corridorStartPos[1] + z].SetSouthWall(true);
            //Path forward until a corridor is found
            while (!connectionFound && tilesPathedForward < corridorWidth)
            {
                for (var i = 0; i < corridorWidth; i++)
                    if (gridData[corridorStartPos[0] + 1, corridorStartPos[1] + i].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        connectionFound = true;
                if (!connectionFound)
                {
                    tilesPathedForward++;
                    corridorStartPos[0]++;
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorStartPos[0], corridorStartPos[1] + i].RoomType = GeneratorGlobalVals.CORRIDOR;
                    gridData[corridorStartPos[0], corridorStartPos[1] - 1].SetEastWall(true);
                    gridData[corridorStartPos[0], corridorStartPos[1] + corridorWidth - 1].SetEastWall(true);
                }
            }

            if (!connectionFound)
            {
                Debug.Log("Error: Couldn't find corridor connection for \"Blocked corridor\"" + doorStartPos[0] + "," +
                          doorStartPos[1]);
                return;
            }

            //Claim tiles for corridor
            for (var x = 1; x <= corridorWidth; x++)
            for (var z = 0; z < corridorWidth; z++)
                gridData[corridorStartPos[0] + x, corridorStartPos[1] + z].RoomType = GeneratorGlobalVals.CORRIDOR;
            //Remove any unwanted internals walls
            if (corridorWidth > 1)
            {
                for (var x = 1; x <= corridorWidth; x++)
                for (var z = 0; z < corridorWidth - 1; z++)
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] + z].SetEastWall(false);
                var offset = tilesPathedForward == 0 ? 1 : 0;
                for (var x = 0 + offset; x < corridorWidth; x++)
                for (var z = 0; z < corridorWidth; z++)
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] + z].SetSouthWall(false);
            }

            //Remove/add external walls
            for (var x = 1; x <= corridorWidth; x++)
            {
                if (gridData[corridorStartPos[0] + x, corridorStartPos[1] - 1].RoomType == GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] - 1].SetEastWall(false);
                else
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] - 1].SetEastWall(true);
                if (corridorStartPos[1] + corridorWidth < gridData.GetLength(1) &&
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] + corridorWidth].RoomType ==
                    GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] + corridorWidth - 1].SetEastWall(false);
                else
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] + corridorWidth - 1].SetEastWall(true);
            }

            for (var z = 0; z < corridorWidth; z++)
                if (doorStartPos[0] + corridorWidth + 1 < gridData.GetLength(0) &&
                    gridData[corridorStartPos[0] + corridorWidth + 1, corridorStartPos[1] + z].RoomType ==
                    GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] + corridorWidth, corridorStartPos[1] + z].SetSouthWall(false);
                else
                    gridData[corridorStartPos[0] + corridorWidth, corridorStartPos[1] + z].SetSouthWall(true);
        }

        /**
         * Checks to see if door is already connected to a corridor. If so, nothing needs to be done
         * 
         * Checks to see if door is partially connected to a corridor. If so, connects door to corridor
         * 
         * Checks to see if door is not connected to any corridor. If so, picks a valid position to begin
         * corridor generation
         */
        private void StartWestCorridor(ref GridSegmentData[,] gridData, Room room, Door door, int corridorWidth,
            ref Stack<CorridorRecursiveCallData> recursiveCallStack)
        {
            int[] corridorStartPos;
            int[] doorStartPos = { room.StartPos[0] + door.Offset, room.StartPos[1] };
            var doorwayWidth = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            //Check for a blocking/complete connection to existing corridor
            var corridorTilesFound = 0;
            for (var i = 0; i < doorwayWidth; i++)
                if (gridData[doorStartPos[0] + i, doorStartPos[1] - 1].RoomType == GeneratorGlobalVals.CORRIDOR)
                    corridorTilesFound++;
            if (corridorTilesFound == doorwayWidth) //Door is already connected
                //Debug.Log(doorStartPos[0] + "," +doorStartPos[1] + " is already connected");
                return;
            if (corridorTilesFound > 0) //Door is partially blocked
            {
                //Debug.Log(doorStartPos[0] + "," +doorStartPos[1] + " is blocked");
                //Make area of doorwayWidth * corridorWidth corridors
                //Claim corridor tiles
                for (var z = 1; z <= corridorWidth; z++)
                for (var x = 0; x < doorwayWidth; x++)
                    gridData[doorStartPos[0] + x, doorStartPos[1] - z].RoomType = GeneratorGlobalVals.CORRIDOR;
                //Remove any unwanted internals walls
                if (doorwayWidth > 1)
                    for (var z = 1; z <= corridorWidth; z++)
                    for (var x = 0; x < doorwayWidth - 1; x++)
                        gridData[doorStartPos[0] + x, doorStartPos[1] - z].SetSouthWall(false);
                if (corridorWidth > 1)
                    for (var z = 2; z <= corridorWidth; z++)
                    for (var x = 0; x < doorwayWidth; x++)
                        gridData[doorStartPos[0] + x, doorStartPos[1] - z].SetEastWall(false);
                //Remove/add external walls
                for (var z = 1; z <= corridorWidth; z++)
                {
                    if (gridData[doorStartPos[0] - 1, doorStartPos[1] - z].RoomType == GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] - 1, doorStartPos[1] - z].SetSouthWall(false);
                    else
                        gridData[doorStartPos[0] - 1, doorStartPos[1] - z].SetSouthWall(true);
                    if (doorStartPos[0] + doorwayWidth < gridData.GetLength(0) &&
                        gridData[doorStartPos[0] + doorwayWidth, doorStartPos[1] - z].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] + doorwayWidth - 1, doorStartPos[1] - z].SetSouthWall(false);
                    else
                        gridData[doorStartPos[0] + doorwayWidth - 1, doorStartPos[1] - z].SetSouthWall(true);
                }

                for (var x = 0; x < doorwayWidth; x++)
                    if (gridData[doorStartPos[0] + x, doorStartPos[1] - corridorWidth - 1].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] + x, doorStartPos[1] - corridorWidth - 1].SetEastWall(false);
                    else
                        gridData[doorStartPos[0] + x, doorStartPos[1] - corridorWidth - 1].SetEastWall(true);
                return;
            }

            //Door isn't blocked: Generate corridor
            if (corridorWidth == doorwayWidth) //If there is only one possible corridor position
            {
                var cpd = new CorridorPathingDecider();
                corridorStartPos = doorStartPos.Clone() as int[];
                CreateInitialWestCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                HandleWestCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }

            //Choose a corridor position, when theres potentially multiple options
            var minCorridorStartPos = Math.Max(1, doorStartPos[0] - (corridorWidth - doorwayWidth));

            var maxCorridorStartPos = doorStartPos[0];
            var maxCorridorEndPos = Math.Min(doorStartPos[0] + corridorWidth, gridData.GetLength(0));

            if (minCorridorStartPos == maxCorridorStartPos) //If there is only one possible corridor position
            {
                corridorStartPos = new[] { minCorridorStartPos, doorStartPos[1] };
                var cpd = new CorridorPathingDecider();
                CreateInitialWestCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                HandleWestCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }

            var successiveValidTiles = 0;
            int chosenOption;
            var corridorOptions = new List<int>();
            for (var currentPos = minCorridorStartPos; currentPos < maxCorridorEndPos; currentPos++)
            {
                var isValidRow = true;
                for (var j = 1; j <= corridorWidth && isValidRow; j++)
                    if (gridData[currentPos, doorStartPos[1] - j].RoomType is not GeneratorGlobalVals.EMPTY)
                    {
                        isValidRow = false;
                        successiveValidTiles = 0;
                    }

                if (isValidRow) successiveValidTiles++;

                if (successiveValidTiles >= corridorWidth) corridorOptions.Add(currentPos - corridorWidth + 1);
            }

            if (corridorOptions.Count != 0)
            {
                chosenOption = Random.Range(0, corridorOptions.Count);
                corridorStartPos = new[] { corridorOptions[chosenOption], doorStartPos[1] };
                var cpd = new CorridorPathingDecider();
                CreateInitialWestCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                HandleWestCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }

            //Debug.Log("No valid corridor positions for door " + doorStartPos[0] + "," +doorStartPos[1]);
            //Pick a valid start position for corridor
            successiveValidTiles = 0;
            for (var currentPos = minCorridorStartPos; currentPos < maxCorridorEndPos; currentPos++)
            {
                var isValidRow = true;
                for (var j = 1; j <= corridorWidth && isValidRow; j++)
                    if (gridData[currentPos, doorStartPos[1] - j].RoomType is not (GeneratorGlobalVals.EMPTY
                        or GeneratorGlobalVals.CORRIDOR))
                    {
                        isValidRow = false;
                        successiveValidTiles = 0;
                    }

                if (isValidRow) successiveValidTiles++;

                if (successiveValidTiles >= corridorWidth) corridorOptions.Add(currentPos - corridorWidth + 1);
            }

            if (corridorOptions.Count == 0)
            {
                Debug.Log("Something went wrong: No valid options to begin corridor " + doorStartPos[0] + "," +
                          doorStartPos[1]);
                return;
            }

            chosenOption = corridorOptions[Random.Range(0, corridorOptions.Count)];
            corridorStartPos = new[] { chosenOption, doorStartPos[1] };

            var connectionFound = false;
            var tilesPathedForward = 0;
            //Add initial walls
            for (var x = 0; x < corridorWidth; x++)
                if (gridData[corridorStartPos[0] - x, corridorStartPos[1]].RoomType == GeneratorGlobalVals.EMPTY)
                    gridData[corridorStartPos[0] - x, corridorStartPos[1]].SetEastWall(true);
            //Path forward until a corridor is found
            while (!connectionFound && tilesPathedForward < corridorWidth)
            {
                for (var i = 0; i < corridorWidth; i++)
                    if (gridData[corridorStartPos[0] + i, corridorStartPos[1] - 1].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        connectionFound = true;
                if (!connectionFound)
                {
                    tilesPathedForward++;
                    corridorStartPos[1]--;
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorStartPos[0] + i, corridorStartPos[1]].RoomType = GeneratorGlobalVals.CORRIDOR;
                    gridData[corridorStartPos[0] - 1, corridorStartPos[1]].SetSouthWall(true);
                    gridData[corridorStartPos[0] + corridorWidth - 1, corridorStartPos[1]].SetSouthWall(true);
                }
            }

            if (!connectionFound)
            {
                Debug.Log("Error: Couldn't find corridor connection for \"Blocked corridor\"" + doorStartPos[0] + "," +
                          doorStartPos[1]);
                return;
            }

            //Claim tiles for corridor
            for (var z = 1; z <= corridorWidth; z++)
            for (var x = 0; x < corridorWidth; x++)
                gridData[corridorStartPos[0] + x, corridorStartPos[1] - z].RoomType = GeneratorGlobalVals.CORRIDOR;
            //Remove any unwanted internals walls
            if (corridorWidth > 1)
            {
                for (var z = 1; z <= corridorWidth; z++)
                for (var x = 0; x < corridorWidth - 1; x++)
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] - z].SetSouthWall(false);
                var offset = tilesPathedForward == 0 ? 1 : 0;
                for (var z = 1 + offset; z <= corridorWidth; z++)
                for (var x = 0; x < corridorWidth; x++)
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] - z].SetEastWall(false);
            }

            //Remove/add external walls
            for (var z = 1; z <= corridorWidth; z++)
            {
                if (gridData[corridorStartPos[0] - 1, corridorStartPos[1] - z].RoomType == GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] - 1, corridorStartPos[1] - z].SetSouthWall(false);
                else
                    gridData[corridorStartPos[0] - 1, corridorStartPos[1] - z].SetSouthWall(true);
                if (corridorStartPos[0] + corridorWidth < gridData.GetLength(0) &&
                    gridData[corridorStartPos[0] + corridorWidth, corridorStartPos[1] - z].RoomType ==
                    GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] + corridorWidth - 1, corridorStartPos[1] - z].SetSouthWall(false);
                else
                    gridData[corridorStartPos[0] + corridorWidth - 1, corridorStartPos[1] - z].SetSouthWall(true);
            }

            for (var x = 0; x < corridorWidth; x++)
                if (gridData[corridorStartPos[0] + x, corridorStartPos[1] - corridorWidth - 1].RoomType ==
                    GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] - corridorWidth - 1].SetEastWall(false);
                else
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] - corridorWidth - 1].SetEastWall(true);
        }

        /**
         * Checks to see if door is already connected to a corridor. If so, nothing needs to be done
         * 
         * Checks to see if door is partially connected to a corridor. If so, connects door to corridor
         * 
         * Checks to see if door is not connected to any corridor. If so, picks a valid position to begin
         * corridor generation
         */
        private void StartEastCorridor(ref GridSegmentData[,] gridData, Room room, Door door, int corridorWidth,
            ref Stack<CorridorRecursiveCallData> recursiveCallStack)
        {
            int[] corridorStartPos;
            int[] doorStartPos = { room.StartPos[0] + door.Offset, room.StartPos[1] + room.RoomSize[1] - 1 };
            var doorwayWidth = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            //Check for a blocking/complete connection to existing corridor
            var corridorTilesFound = 0;
            for (var i = 0; i < doorwayWidth; i++)
                if (gridData[doorStartPos[0] + i, doorStartPos[1] + 1].RoomType == GeneratorGlobalVals.CORRIDOR)
                    corridorTilesFound++;
            if (corridorTilesFound == doorwayWidth) //Door is already connected
                //Debug.Log(doorStartPos[0] + "," +doorStartPos[1] + " is already connected");
                return;

            if (corridorTilesFound > 0) //Door is partially blocked
            {
                //Debug.Log(doorStartPos[0] + "," +doorStartPos[1] + " is blocked");
                //Make area of doorwayWidth * corridorWidth corridors
                //Claim corridor tiles
                for (var z = 1; z <= corridorWidth; z++)
                for (var x = 0; x < doorwayWidth; x++)
                    gridData[doorStartPos[0] + x, doorStartPos[1] + z].RoomType = GeneratorGlobalVals.CORRIDOR;
                //Remove any unwanted internals walls
                if (doorwayWidth > 1)
                    for (var z = 1; z <= corridorWidth; z++)
                    for (var x = 0; x < doorwayWidth - 1; x++)
                        gridData[doorStartPos[0] + x, doorStartPos[1] + z].SetSouthWall(false);
                if (corridorWidth > 1)
                    for (var z = 1; z < corridorWidth; z++)
                    for (var x = 0; x < doorwayWidth; x++)
                        gridData[doorStartPos[0] + x, doorStartPos[1] + z].SetEastWall(false);
                //Remove/add external walls
                for (var z = 1; z <= corridorWidth; z++)
                {
                    if (gridData[doorStartPos[0] - 1, doorStartPos[1] + z].RoomType == GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] - 1, doorStartPos[1] + z].SetSouthWall(false);
                    else
                        gridData[doorStartPos[0] - 1, doorStartPos[1] + z].SetSouthWall(true);
                    if (doorStartPos[0] + doorwayWidth < gridData.GetLength(0) &&
                        gridData[doorStartPos[0] + doorwayWidth, doorStartPos[1] + z].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] + doorwayWidth - 1, doorStartPos[1] + z].SetSouthWall(false);
                    else
                        gridData[doorStartPos[0] + doorwayWidth - 1, doorStartPos[1] + z].SetSouthWall(true);
                }

                for (var x = 0; x < doorwayWidth; x++)
                    if (doorStartPos[1] + corridorWidth + 1 < gridData.GetLength(1) &&
                        gridData[doorStartPos[0] + x, doorStartPos[1] + corridorWidth + 1].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        gridData[doorStartPos[0] + x, doorStartPos[1] + corridorWidth].SetEastWall(false);
                    else
                        gridData[doorStartPos[0] + x, doorStartPos[1] + corridorWidth].SetEastWall(true);
                return;
            }

            //Door isn't blocked: Generate corridor
            if (corridorWidth == doorwayWidth) //If there is only one possible corridor position
            {
                var cpd = new CorridorPathingDecider();
                corridorStartPos = doorStartPos.Clone() as int[];
                CreateInitialEastCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                HandleEastCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }

            //Choose a corridor position, when theres potentially multiple options
            var minCorridorStartPos = Math.Max(1, doorStartPos[0] - (corridorWidth - doorwayWidth));
            var maxCorridorStartPos = doorStartPos[0];
            var maxCorridorEndPos = Math.Min(doorStartPos[0] + corridorWidth, gridData.GetLength(0));

            if (minCorridorStartPos == maxCorridorStartPos) //If there is only one possible corridor position
            {
                corridorStartPos = new[] { minCorridorStartPos, doorStartPos[1] };
                var cpd = new CorridorPathingDecider();
                CreateInitialEastCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                HandleEastCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }

            var successiveValidTiles = 0;
            int chosenOption;
            var corridorOptions = new List<int>();
            for (var currentPos = minCorridorStartPos; currentPos < maxCorridorEndPos; currentPos++)
            {
                var isValidRow = true;
                for (var j = 1; j <= corridorWidth && isValidRow; j++)
                    if (gridData[currentPos, doorStartPos[1] + j].RoomType is not GeneratorGlobalVals.EMPTY)
                    {
                        isValidRow = false;
                        successiveValidTiles = 0;
                    }

                if (isValidRow) successiveValidTiles++;

                if (successiveValidTiles >= corridorWidth) corridorOptions.Add(currentPos - corridorWidth + 1);
            }

            if (corridorOptions.Count != 0)
            {
                chosenOption = Random.Range(0, corridorOptions.Count);
                corridorStartPos = new[] { corridorOptions[chosenOption], doorStartPos[1] };
                var cpd = new CorridorPathingDecider();
                CreateInitialEastCorridor(ref gridData, corridorWidth, ref corridorStartPos);
                HandleEastCorridor(ref gridData, corridorStartPos, corridorWidth, cpd, ref recursiveCallStack);
                return;
            }

            //In this case, there in no available space to start generating a corridor
            //Connecting to an existing corridor is the only option
            //Debug.Log("No valid corridor positions for door " + doorStartPos[0] + "," +doorStartPos[1]);
            //Pick a valid start position for corridor
            successiveValidTiles = 0;
            for (var currentPos = minCorridorStartPos; currentPos < maxCorridorEndPos; currentPos++)
            {
                var isValidRow = true;
                for (var j = 1; j <= corridorWidth && isValidRow; j++)
                    if (gridData[currentPos, doorStartPos[1] + j].RoomType is not (GeneratorGlobalVals.EMPTY
                        or GeneratorGlobalVals.CORRIDOR))
                    {
                        isValidRow = false;
                        successiveValidTiles = 0;
                    }

                if (isValidRow) successiveValidTiles++;

                if (successiveValidTiles >= corridorWidth) corridorOptions.Add(currentPos - corridorWidth + 1);
            }

            if (corridorOptions.Count == 0)
            {
                Debug.Log("Something went wrong: No valid options to begin corridor " + doorStartPos[0] + "," +
                          doorStartPos[1]);
                return;
            }

            chosenOption = corridorOptions[Random.Range(0, corridorOptions.Count)];
            corridorStartPos = new[] { chosenOption, doorStartPos[1] };

            var connectionFound = false;
            var tilesPathedForward = 0;
            //Add initial walls
            for (var x = 0; x < corridorWidth; x++)
                if (gridData[corridorStartPos[0] + x, corridorStartPos[1]].RoomType == GeneratorGlobalVals.EMPTY)
                    gridData[corridorStartPos[0] + x, corridorStartPos[1]].SetEastWall(true);
            //Path forward until a corridor is found
            while (!connectionFound && tilesPathedForward < corridorWidth)
            {
                for (var i = 0; i < corridorWidth; i++)
                    if (gridData[corridorStartPos[0] + i, corridorStartPos[1] + 1].RoomType ==
                        GeneratorGlobalVals.CORRIDOR)
                        connectionFound = true;
                if (!connectionFound)
                {
                    tilesPathedForward++;
                    corridorStartPos[1]++;
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorStartPos[0] + i, corridorStartPos[1]].RoomType = GeneratorGlobalVals.CORRIDOR;
                    gridData[corridorStartPos[0] - 1, corridorStartPos[1]].SetSouthWall(true);
                    gridData[corridorStartPos[0] + corridorWidth - 1, corridorStartPos[1]].SetSouthWall(true);
                }
            }

            if (!connectionFound)
            {
                Debug.Log("Error: Couldn't find corridor connection for \"Blocked corridor\"" + doorStartPos[0] + "," +
                          doorStartPos[1]);
                return;
            }

            //Claim tiles for corridor
            for (var z = 1; z <= corridorWidth; z++)
            for (var x = 0; x < corridorWidth; x++)
                gridData[corridorStartPos[0] + x, corridorStartPos[1] + z].RoomType = GeneratorGlobalVals.CORRIDOR;
            //Remove any unwanted internals walls
            if (corridorWidth > 1)
            {
                for (var z = 1; z <= corridorWidth; z++)
                for (var x = 0; x < corridorWidth - 1; x++)
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] + z].SetSouthWall(false);
                var offset = tilesPathedForward == 0 ? 1 : 0;
                for (var z = 0 + offset; z < corridorWidth; z++)
                for (var x = 0; x < corridorWidth; x++)
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] + z].SetEastWall(false);
            }

            //Remove/add external walls
            for (var z = 1; z <= corridorWidth; z++)
            {
                if (gridData[corridorStartPos[0] - 1, corridorStartPos[1] + z].RoomType == GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] - 1, corridorStartPos[1] + z].SetSouthWall(false);
                else
                    gridData[corridorStartPos[0] - 1, corridorStartPos[1] + z].SetSouthWall(true);
                if (corridorStartPos[0] + corridorWidth < gridData.GetLength(0) &&
                    gridData[corridorStartPos[0] + corridorWidth, corridorStartPos[1] + z].RoomType ==
                    GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] + corridorWidth - 1, corridorStartPos[1] + z].SetSouthWall(false);
                else
                    gridData[corridorStartPos[0] + corridorWidth - 1, corridorStartPos[1] + z].SetSouthWall(true);
            }

            for (var x = 0; x < corridorWidth; x++)
                if (doorStartPos[1] + corridorWidth + 1 < gridData.GetLength(1) &&
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] + corridorWidth + 1].RoomType ==
                    GeneratorGlobalVals.CORRIDOR)
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] + corridorWidth].SetEastWall(false);
                else
                    gridData[corridorStartPos[0] + x, corridorStartPos[1] + corridorWidth].SetEastWall(true);
        }

        /*
         * When a valid corridor position is picked for north doors, this function is called to create the initial corridor start
         * Makes the area of corridorWith * corridorWidth corridors, generates walls where necessary, and alters corridorStartPos
         * to the new value.
         */
        private void CreateInitialNorthCorridor(ref GridSegmentData[,] gridData, int corridorWidth,
            ref int[] corridorStartPos)
        {
            //Draw initial corridor
            //Make corridorWidth * corridorWidth corridors
            for (var i = 0; i < corridorWidth; i++)
            for (var j = 1; j <= corridorWidth; j++)
                gridData[corridorStartPos[0] - j, corridorStartPos[1] + i].RoomType =
                    GeneratorGlobalVals.CORRIDOR;

            //Add east,west,south walls
            for (var i = 1; i <= corridorWidth; i++)
            {
                gridData[corridorStartPos[0] - i, corridorStartPos[1] - 1].SetEastWall(true);
                gridData[corridorStartPos[0] - i, corridorStartPos[1] + corridorWidth - 1].SetEastWall(true);
                gridData[corridorStartPos[0] - 1, corridorStartPos[1] + i - 1].SetSouthWall(true);
            }

            corridorStartPos[0] -= corridorWidth;
        }

        /*
         * When a valid corridor position is picked for south doors, this function is called to create the initial corridor start
         * Makes the area of corridorWith * corridorWidth corridors, generates walls where necessary, and alters corridorStartPos
         * to the new value.
         */
        private void CreateInitialSouthCorridor(ref GridSegmentData[,] gridData, int corridorWidth,
            ref int[] corridorStartPos)
        {
            //Draw initial corridor
            //Make corridorWidth * corridorWidth corridors
            for (var i = 0; i < corridorWidth; i++)
            for (var j = 1; j <= corridorWidth; j++)
                gridData[corridorStartPos[0] + j, corridorStartPos[1] + i].RoomType =
                    GeneratorGlobalVals.CORRIDOR;

            //Add east,west,north walls
            for (var i = 1; i <= corridorWidth; i++)
            {
                gridData[corridorStartPos[0] + i, corridorStartPos[1] - 1].SetEastWall(true);
                gridData[corridorStartPos[0] + i, corridorStartPos[1] + corridorWidth - 1].SetEastWall(true);
                gridData[corridorStartPos[0], corridorStartPos[1] + i - 1].SetSouthWall(true);
            }

            corridorStartPos[0] += corridorWidth;
        }

        /*
         * When a valid corridor position is picked for west doors, this function is called to create the initial corridor start
         * Makes the area of corridorWith * corridorWidth corridors, generates walls where necessary, and alters corridorStartPos
         * to the new value.
         */
        private void CreateInitialWestCorridor(ref GridSegmentData[,] gridData, int corridorWidth,
            ref int[] corridorStartPos)
        {
            //Draw initial corridor
            //Make corridorWidth * corridorWidth corridors
            for (var i = 1; i <= corridorWidth; i++)
            for (var j = 0; j < corridorWidth; j++)
                gridData[corridorStartPos[0] + j, corridorStartPos[1] - i].RoomType =
                    GeneratorGlobalVals.CORRIDOR;

            //Add east,south,north walls
            for (var i = 1; i <= corridorWidth; i++)
            {
                gridData[corridorStartPos[0] - 1, corridorStartPos[1] - i].SetSouthWall(true);
                gridData[corridorStartPos[0] + corridorWidth - 1, corridorStartPos[1] - i].SetSouthWall(true);
                gridData[corridorStartPos[0] + i - 1, corridorStartPos[1] - 1].SetEastWall(true);
            }

            corridorStartPos[1] -= corridorWidth;
        }

        /*
        * When a valid corridor position is picked for east doors, this function is called to create the initial corridor start
        * Makes the area of corridorWith * corridorWidth corridors, generates walls where necessary, and alters corridorStartPos
        * to the new value.
        */
        private void CreateInitialEastCorridor(ref GridSegmentData[,] gridData, int corridorWidth,
            ref int[] corridorStartPos)
        {
            //Draw initial corridor
            //Make corridorWidth * corridorWidth corridors
            for (var i = 1; i <= corridorWidth; i++)
            for (var j = 0; j < corridorWidth; j++)
                gridData[corridorStartPos[0] + j, corridorStartPos[1] + i].RoomType =
                    GeneratorGlobalVals.CORRIDOR;

            //Add east,south,north walls
            for (var i = 1; i <= corridorWidth; i++)
            {
                gridData[corridorStartPos[0] - 1, corridorStartPos[1] + i].SetSouthWall(true);
                gridData[corridorStartPos[0] + corridorWidth - 1, corridorStartPos[1] + i].SetSouthWall(true);
                gridData[corridorStartPos[0] + i - 1, corridorStartPos[1]].SetEastWall(true);
            }

            corridorStartPos[1] += corridorWidth;
        }

        /*
         * Handles corridor generation towards x=0
         * Checks for valid options, chooses an option and delegates the next steps to whichever method is to handle it
         *
         * Valid options:
         * All tiles in a given direction are empty
         * All tiles in a given direction are corridors AND belong to the same corridor
         * The chosen position would fully connect to a doorway.
          */
        private void HandleNorthCorridor(ref GridSegmentData[,] gridData, int[] corridorPosition, int corridorWidth,
            CorridorPathingDecider pathingDecider, ref Stack<CorridorRecursiveCallData> recursiveCallStack)
        {
            var nextPathCalls = new List<int>();
            //bool[]'s {is valid choice, can path into empty, can path into corridor, can connect to door
            bool[] isForwardValid = { true, true, true, true };
            bool[] isLeftValid = { true, true, true, true };
            bool[] isRightValid = { true, true, true, true };
            //Check Forward Validity
            CheckPathingNorthValidity(ref gridData, corridorPosition, ref isForwardValid, corridorWidth);
            //Check left validity
            CheckPathingWestValidity(ref gridData, corridorPosition, ref isLeftValid, corridorWidth);
            //Check right validity
            var corridorEndPos = corridorPosition[1] + corridorWidth - 1;
            int[] checkingPos = { corridorPosition[0], corridorEndPos };
            CheckPathingEastValidity(ref gridData, checkingPos, ref isRightValid, corridorWidth);
            if (!isForwardValid[0] && !isLeftValid[0] && !isRightValid[0])
            {
                //Debug.Log("Pathing North: Corridor stopped generating- No valid next positions. " + corridorPosition[0] + ", " +
                //          corridorPosition[1]);
                for (var i = 0; i < corridorWidth; i++)
                    gridData[corridorPosition[0] - 1, corridorPosition[1] + i].SetSouthWall(true);
                return;
            }

            var nextCorridor = pathingDecider.DecideNextCorridor(isForwardValid[0], isLeftValid[0], isRightValid[0]);
            var nextCorridorHasUp = false;

            if (debuggingOn)
            {
                sw.Write("Position: [" + corridorPosition[0] + "," + corridorPosition[1] + "] Decision: ");
                //sw.Write(nextCorridor != GeneratorGlobalVals.CORRIDOR_END ? nextCorridor != GeneratorGlobalVals.STRAIGHT ? nextCorridor != GeneratorGlobalVals.LEFT_TURN ? nextCorridor != GeneratorGlobalVals.RIGHT_TURN ? nextCorridor != GeneratorGlobalVals.LEFT_UP_SPLIT ? nextCorridor != GeneratorGlobalVals.LEFT_RIGHT_SPLIT? nextCorridor != GeneratorGlobalVals.RIGHT_UP_SPLIT?  nextCorridor != GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT? "Unknown": "Left Up Right Split\n"  :"Right Up Split\n" : "Left Right Split\n" : "Left Up Split\n" : "Right Turn\n"  : "Left Turn\n" :  "Straight\n" :  "End Corridor\n");
                switch (nextCorridor)
                {
                    case GeneratorGlobalVals.CORRIDOR_END:
                        sw.Write("End Corridor\n");
                        break;
                    case GeneratorGlobalVals.STRAIGHT:
                        sw.Write("Straight\n");
                        break;
                    case GeneratorGlobalVals.LEFT_TURN:
                        sw.Write("Left Turn\n");
                        break;
                    case GeneratorGlobalVals.RIGHT_TURN:
                        sw.Write("Right Turn\n");
                        break;
                    case GeneratorGlobalVals.LEFT_UP_SPLIT:
                        sw.Write("Left Up Split\n");
                        break;
                    case GeneratorGlobalVals.LEFT_RIGHT_SPLIT:
                        sw.Write("Left Right Split\n");
                        break;
                    case GeneratorGlobalVals.RIGHT_UP_SPLIT:
                        sw.Write("Right Up Split\n");
                        break;
                    case GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT:
                        sw.Write("Left Up Right Split\n");
                        break;
                    default:
                        sw.Write("Unknown");
                        break;
                }

                sw.WriteLine("Can path left:" + isLeftValid[0] + " Into Empty:" + isLeftValid[1] + " Into Corridor:" +
                             isLeftValid[2] + " Into Doorway:" + isLeftValid[3]);
                sw.WriteLine("Can path forward:" + isForwardValid[0] + " Into Empty:" + isForwardValid[1] +
                             " Into Corridor:" + isForwardValid[2] + " Into Doorway:" + isForwardValid[3]);
                sw.WriteLine("Can path right:" + isRightValid[0] + " Into Empty:" + isRightValid[1] +
                             " Into Corridor:" + isRightValid[2] + " Into Doorway:" + isRightValid[3]);
                sw.WriteLine();
            }


            if (nextCorridor is GeneratorGlobalVals.STRAIGHT or GeneratorGlobalVals.LEFT_UP_SPLIT
                or GeneratorGlobalVals.RIGHT_UP_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                nextCorridorHasUp = true;
                //Draw up
                if (isForwardValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] - 1, corridorPosition[1] + i].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                    gridData[corridorPosition[0] - 1, corridorPosition[1] - 1].SetEastWall(true);
                    gridData[corridorPosition[0] - 1, corridorPosition[1] + corridorWidth - 1].SetEastWall(true);
                    nextPathCalls.Add(UP);
                }
                else if (isForwardValid[2])
                {
                    //Debug.Log("North, Pathing north: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] - 1, corridorPosition[1] + i].SetSouthWall(false);
                }
                else if (isForwardValid[3])
                {
                    //Debug.Log("North, Pathing north: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] - 1, corridorPosition[1] + i].SetSouthWall(true);
                }
            }

            if (!nextCorridorHasUp)
                for (var i = 0; i < corridorWidth; i++)
                    gridData[corridorPosition[0] - 1, corridorPosition[1] + i].SetSouthWall(true);

            if (nextCorridor is GeneratorGlobalVals.LEFT_TURN or GeneratorGlobalVals.LEFT_UP_SPLIT
                or GeneratorGlobalVals.LEFT_RIGHT_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                //Draw left
                if (isLeftValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                    {
                        gridData[corridorPosition[0] + i, corridorPosition[1] - 1].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                        gridData[corridorPosition[0] + i, corridorPosition[1] - 1].SetEastWall(false);
                    }

                    gridData[corridorPosition[0] - 1, corridorPosition[1] - 1].SetSouthWall(true);
                    gridData[corridorPosition[0] + corridorWidth - 1, corridorPosition[1] - 1].SetSouthWall(true);
                    nextPathCalls.Add(LEFT);
                }
                else if (isLeftValid[2])
                {
                    //Debug.Log("North, Pathing west: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + i, corridorPosition[1] - 1].SetEastWall(false);
                }
                else if (isLeftValid[3])
                {
                    //Debug.Log("North, Pathing west: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + i, corridorPosition[1] - 1].SetEastWall(true);
                }
            }

            if (nextCorridor is GeneratorGlobalVals.RIGHT_TURN or GeneratorGlobalVals.RIGHT_UP_SPLIT
                or GeneratorGlobalVals.LEFT_RIGHT_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                //Draw right
                if (isRightValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                    {
                        gridData[corridorPosition[0] + i, corridorPosition[1] + corridorWidth].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                        gridData[corridorPosition[0] + i, corridorPosition[1] + corridorWidth - 1].SetEastWall(false);
                    }

                    gridData[corridorPosition[0] - 1, corridorPosition[1] + corridorWidth].SetSouthWall(true);
                    gridData[corridorPosition[0] + corridorWidth - 1, corridorPosition[1] + corridorWidth]
                        .SetSouthWall(true);
                    nextPathCalls.Add(RIGHT);
                }
                else if (isRightValid[2])
                {
                    //Debug.Log("North, Pathing east: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + i, corridorPosition[1] + corridorWidth - 1].SetEastWall(false);
                }
                else if (isRightValid[3])
                {
                    //Debug.Log("North, Pathing east: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + i, corridorPosition[1] + corridorWidth - 1].SetEastWall(true);
                }
            }

            nextPathCalls.Reverse();
            foreach (var call in nextPathCalls)
            {
                int[] newCorridorPos;
                CorridorRecursiveCallData data;
                CorridorPathingDecider newPathingDecider;
                switch (call)
                {
                    case UP:
                        newCorridorPos = new[] { corridorPosition[0] - 1, corridorPosition[1] };
                        //HandleNorthCorridor(ref gridData,newCorridorPos,corridorWidth,pathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, pathingDecider, NORTH);
                        recursiveCallStack.Push(data);
                        break;
                    case LEFT:
                        newCorridorPos = new[] { corridorPosition[0], corridorPosition[1] - 1 };
                        newPathingDecider = new CorridorPathingDecider();
                        //HandleWestCorridor(ref gridData,newCorridorPos,corridorWidth,newPathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, newPathingDecider, WEST);
                        recursiveCallStack.Push(data);
                        break;
                    case RIGHT:
                        newCorridorPos = new[] { corridorPosition[0], corridorPosition[1] + corridorWidth };
                        newPathingDecider = new CorridorPathingDecider();
                        //HandleEastCorridor(ref gridData,newCorridorPos,corridorWidth,newPathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, newPathingDecider, EAST);
                        recursiveCallStack.Push(data);
                        break;
                }
            }
        }

        /**
         * * Handles corridor generation towards increasing x value
         * * Checks for valid options, chooses an option and delegates the next steps to whichever method is to handle it
         * *
         * * Valid options:
         * * All tiles in a given direction are empty
         * * All tiles in a given direction are corridors AND belong to the same corridor
         * * The chosen position would fully connect to a doorway.
         */
        private void HandleSouthCorridor(ref GridSegmentData[,] gridData, int[] corridorPosition, int corridorWidth,
            CorridorPathingDecider pathingDecider, ref Stack<CorridorRecursiveCallData> recursiveCallStack)
        {
            var nextPathCalls = new List<int>();
            //bool[]'s {is valid choice, can path into empty, can path into corridor, can connect to door
            bool[] isForwardValid = { true, true, true, true };
            bool[] isLeftValid = { true, true, true, true };
            bool[] isRightValid = { true, true, true, true };
            //Check Forward Validity

            CheckPathingSouthValidity(ref gridData, corridorPosition, ref isForwardValid, corridorWidth);
            //Check right validity
            int[] corridorStartPos = { corridorPosition[0] - corridorWidth + 1, corridorPosition[1] };

            CheckPathingWestValidity(ref gridData, corridorStartPos, ref isRightValid, corridorWidth);
            //Check left validity
            corridorStartPos[1] = corridorPosition[1] + corridorWidth - 1;
            CheckPathingEastValidity(ref gridData, corridorStartPos, ref isLeftValid, corridorWidth);
            if (!isForwardValid[0] && !isLeftValid[0] && !isRightValid[0])
            {
                //Debug.Log("Pathing South: Corridor stopped generating- No valid next positions. " + corridorPosition[0] + ", " +
                //          corridorPosition[1]);
                for (var i = 0; i < corridorWidth; i++)
                    gridData[corridorPosition[0], corridorPosition[1] + i].SetSouthWall(true);
                return;
            }

            var nextCorridor = pathingDecider.DecideNextCorridor(isForwardValid[0], isLeftValid[0], isRightValid[0]);
            var nextCorridorHasUp = false;

            if (debuggingOn)
            {
                sw.Write("Position: [" + corridorPosition[0] + "," + corridorPosition[1] + "] Decision: ");
                switch (nextCorridor)
                {
                    case GeneratorGlobalVals.CORRIDOR_END:
                        sw.Write("End Corridor\n");
                        break;
                    case GeneratorGlobalVals.STRAIGHT:
                        sw.Write("Straight\n");
                        break;
                    case GeneratorGlobalVals.LEFT_TURN:
                        sw.Write("Left Turn\n");
                        break;
                    case GeneratorGlobalVals.RIGHT_TURN:
                        sw.Write("Right Turn\n");
                        break;
                    case GeneratorGlobalVals.LEFT_UP_SPLIT:
                        sw.Write("Left Up Split\n");
                        break;
                    case GeneratorGlobalVals.LEFT_RIGHT_SPLIT:
                        sw.Write("Left Right Split\n");
                        break;
                    case GeneratorGlobalVals.RIGHT_UP_SPLIT:
                        sw.Write("Right Up Split\n");
                        break;
                    case GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT:
                        sw.Write("Left Up Right Split\n");
                        break;
                    default:
                        sw.Write("Unknown");
                        break;
                }

                sw.WriteLine("Can path left:" + isLeftValid[0] + " Into Empty:" + isLeftValid[1] + " Into Corridor:" +
                             isLeftValid[2] + " Into Doorway:" + isLeftValid[3]);
                sw.WriteLine("Can path forward:" + isForwardValid[0] + " Into Empty:" + isForwardValid[1] +
                             " Into Corridor:" + isForwardValid[2] + " Into Doorway:" + isForwardValid[3]);
                sw.WriteLine("Can path right:" + isRightValid[0] + " Into Empty:" + isRightValid[1] +
                             " Into Corridor:" + isRightValid[2] + " Into Doorway:" + isRightValid[3]);
                sw.WriteLine();
            }


            if (nextCorridor is GeneratorGlobalVals.STRAIGHT or GeneratorGlobalVals.LEFT_UP_SPLIT
                or GeneratorGlobalVals.RIGHT_UP_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                nextCorridorHasUp = true;
                //Draw up
                if (isForwardValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + 1, corridorPosition[1] + i].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                    gridData[corridorPosition[0] + 1, corridorPosition[1] - 1].SetEastWall(true);
                    gridData[corridorPosition[0] + 1, corridorPosition[1] + corridorWidth - 1].SetEastWall(true);
                    nextPathCalls.Add(UP);
                }
                else if (isForwardValid[2])
                {
                    //Debug.Log("South, Pathing south: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0], corridorPosition[1] + i].SetSouthWall(false);
                }
                else if (isForwardValid[3])
                {
                    //Debug.Log("South, Pathing south: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0], corridorPosition[1] + i].SetSouthWall(true);
                }
            }

            if (!nextCorridorHasUp)
                for (var i = 0; i < corridorWidth; i++)
                    gridData[corridorPosition[0], corridorPosition[1] + i].SetSouthWall(true);
            if (nextCorridor is GeneratorGlobalVals.LEFT_TURN or GeneratorGlobalVals.LEFT_UP_SPLIT
                or GeneratorGlobalVals.LEFT_RIGHT_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                //Draw left
                if (isLeftValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                    {
                        gridData[corridorPosition[0] - i, corridorPosition[1] + corridorWidth].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                        gridData[corridorPosition[0] - i, corridorPosition[1] + corridorWidth - 1].SetEastWall(false);
                    }

                    gridData[corridorPosition[0], corridorPosition[1] + corridorWidth].SetSouthWall(true);
                    gridData[corridorPosition[0] - corridorWidth, corridorPosition[1] + corridorWidth]
                        .SetSouthWall(true);
                    nextPathCalls.Add(LEFT);
                }
                else if (isLeftValid[2])
                {
                    //Debug.Log("South, Pathing east: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] - i, corridorPosition[1] + corridorWidth - 1].SetEastWall(false);
                }
                else if (isLeftValid[3])
                {
                    //Debug.Log("South, Pathing east: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] - i, corridorPosition[1] + corridorWidth - 1].SetEastWall(true);
                }
            }

            if (nextCorridor is GeneratorGlobalVals.RIGHT_TURN or GeneratorGlobalVals.RIGHT_UP_SPLIT
                or GeneratorGlobalVals.LEFT_RIGHT_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                //Draw right
                if (isRightValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                    {
                        gridData[corridorPosition[0] - i, corridorPosition[1] - 1].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                        gridData[corridorPosition[0] - i, corridorPosition[1] - 1].SetEastWall(false);
                    }

                    gridData[corridorPosition[0], corridorPosition[1] - 1].SetSouthWall(true);
                    gridData[corridorPosition[0] - corridorWidth, corridorPosition[1] - 1].SetSouthWall(true);
                    nextPathCalls.Add(RIGHT);
                }
                else if (isRightValid[2])
                {
                    //Debug.Log("South, Pathing west: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] - i, corridorPosition[1] - 1].SetEastWall(false);
                }
                else if (isRightValid[3])
                {
                    //Debug.Log("South, Pathing west: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] - i, corridorPosition[1] - 1].SetEastWall(true);
                }
            }

            nextPathCalls.Reverse();
            foreach (var call in nextPathCalls)
            {
                int[] newCorridorPos;
                CorridorRecursiveCallData data;
                CorridorPathingDecider newPathingDecider;
                switch (call)
                {
                    case UP:
                        newCorridorPos = new[] { corridorPosition[0] + 1, corridorPosition[1] };
                        //HandleSouthCorridor(ref gridData,newCorridorPos,corridorWidth,pathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, pathingDecider, SOUTH);
                        recursiveCallStack.Push(data);
                        break;
                    case LEFT:
                        newCorridorPos = new[]
                            { corridorPosition[0] - corridorWidth + 1, corridorPosition[1] + corridorWidth };
                        newPathingDecider = new CorridorPathingDecider();
                        //HandleEastCorridor(ref gridData,newCorridorPos,corridorWidth,newPathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, newPathingDecider, EAST);
                        recursiveCallStack.Push(data);
                        break;
                    case RIGHT:
                        newCorridorPos = new[] { corridorPosition[0] - corridorWidth + 1, corridorPosition[1] - 1 };
                        newPathingDecider = new CorridorPathingDecider();
                        //HandleWestCorridor(ref gridData,newCorridorPos,corridorWidth,newPathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, newPathingDecider, WEST);
                        recursiveCallStack.Push(data);
                        break;
                }
            }
        }

        /**
         * * Handles corridor generation towards towards increasing z value
         * * Checks for valid options, chooses an option and delegates the next steps to whichever method is to handle it
         * *
         * * Valid options:
         * * All tiles in a given direction are empty
         * * All tiles in a given direction are corridors AND belong to the same corridor
         * * The chosen position would fully connect to a doorway.
         */
        private void HandleEastCorridor(ref GridSegmentData[,] gridData, int[] corridorPosition, int corridorWidth,
            CorridorPathingDecider pathingDecider, ref Stack<CorridorRecursiveCallData> recursiveCallStack)
        {
            var nextPathCalls = new List<int>();
            //bool[]'s {is valid choice, can path into empty, can path into corridor, can connect to door
            bool[] isForwardValid = { true, true, true, true };
            bool[] isLeftValid = { true, true, true, true };
            bool[] isRightValid = { true, true, true, true };
            //Check Forward Validity
            CheckPathingEastValidity(ref gridData, corridorPosition, ref isForwardValid, corridorWidth);
            //Check Left Validity
            int[] corridorStartPos = { corridorPosition[0], corridorPosition[1] - corridorWidth + 1 };
            CheckPathingNorthValidity(ref gridData, corridorStartPos, ref isLeftValid, corridorWidth);
            //Check Right Validity
            corridorStartPos[0] = corridorPosition[0] + corridorWidth - 1;
            CheckPathingSouthValidity(ref gridData, corridorStartPos, ref isRightValid, corridorWidth);
            if (!isForwardValid[0] && !isLeftValid[0] && !isRightValid[0])
            {
                //Debug.Log("Pathing East: Corridor stopped generating- No valid next positions. " + corridorPosition[0] + ", " +
                //          corridorPosition[1]);
                //No options, end corridor
                for (var i = 0; i < corridorWidth; i++)
                    gridData[corridorPosition[0] + i, corridorPosition[1]].SetEastWall(true);
                return;
            }

            var nextCorridor = pathingDecider.DecideNextCorridor(isForwardValid[0], isLeftValid[0], isRightValid[0]);
            var nextCorridorHasUp = false;
            if (debuggingOn)
            {
                sw.Write("Position: [" + corridorPosition[0] + "," + corridorPosition[1] + "] Decision: ");
                switch (nextCorridor)
                {
                    case GeneratorGlobalVals.CORRIDOR_END:
                        sw.Write("End Corridor\n");
                        break;
                    case GeneratorGlobalVals.STRAIGHT:
                        sw.Write("Straight\n");
                        break;
                    case GeneratorGlobalVals.LEFT_TURN:
                        sw.Write("Left Turn\n");
                        break;
                    case GeneratorGlobalVals.RIGHT_TURN:
                        sw.Write("Right Turn\n");
                        break;
                    case GeneratorGlobalVals.LEFT_UP_SPLIT:
                        sw.Write("Left Up Split\n");
                        break;
                    case GeneratorGlobalVals.LEFT_RIGHT_SPLIT:
                        sw.Write("Left Right Split\n");
                        break;
                    case GeneratorGlobalVals.RIGHT_UP_SPLIT:
                        sw.Write("Right Up Split\n");
                        break;
                    case GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT:
                        sw.Write("Left Up Right Split\n");
                        break;
                    default:
                        sw.Write("Unknown");
                        break;
                }

                sw.WriteLine("Can path left:" + isLeftValid[0] + " Into Empty:" + isLeftValid[1] + " Into Corridor:" +
                             isLeftValid[2] + " Into Doorway:" + isLeftValid[3]);
                sw.WriteLine("Can path forward:" + isForwardValid[0] + " Into Empty:" + isForwardValid[1] +
                             " Into Corridor:" + isForwardValid[2] + " Into Doorway:" + isForwardValid[3]);
                sw.WriteLine("Can path right:" + isRightValid[0] + " Into Empty:" + isRightValid[1] +
                             " Into Corridor:" + isRightValid[2] + " Into Doorway:" + isRightValid[3]);
                sw.WriteLine();
            }

            if (nextCorridor is GeneratorGlobalVals.STRAIGHT or GeneratorGlobalVals.LEFT_UP_SPLIT
                or GeneratorGlobalVals.RIGHT_UP_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                nextCorridorHasUp = true;
                //Draw up
                if (isForwardValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + i, corridorPosition[1] + 1].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                    gridData[corridorPosition[0] - 1, corridorPosition[1] + 1].SetSouthWall(true);
                    gridData[corridorPosition[0] + corridorWidth - 1, corridorPosition[1] + 1].SetSouthWall(true);
                    nextPathCalls.Add(UP);
                }
                else if (isForwardValid[2])
                {
                    //Debug.Log("East, Pathing east: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + i, corridorPosition[1]].SetEastWall(false);
                }
                else if (isForwardValid[3])
                {
                    //Debug.Log("East, Pathing east: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + i, corridorPosition[1]].SetEastWall(true);
                }
            }

            if (!nextCorridorHasUp)
                for (var i = 0; i < corridorWidth; i++)
                    gridData[corridorPosition[0] + i, corridorPosition[1]].SetEastWall(true);
            if (nextCorridor is GeneratorGlobalVals.LEFT_TURN or GeneratorGlobalVals.LEFT_UP_SPLIT
                or GeneratorGlobalVals.LEFT_RIGHT_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                //Draw left
                if (isLeftValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                    {
                        gridData[corridorPosition[0] - 1, corridorPosition[1] - i].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                        gridData[corridorPosition[0] - 1, corridorPosition[1] - i].SetSouthWall(false);
                    }

                    gridData[corridorPosition[0] - 1, corridorPosition[1]].SetEastWall(true);
                    gridData[corridorPosition[0] - 1, corridorPosition[1] - corridorWidth].SetEastWall(true);
                    nextPathCalls.Add(LEFT);
                }
                else if (isLeftValid[2])
                {
                    //Debug.Log("East, Pathing north: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] - 1, corridorPosition[1] - i].SetSouthWall(false);
                }
                else if (isLeftValid[3])
                {
                    //Debug.Log("East, Pathing north: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] - 1, corridorPosition[1] - i].SetSouthWall(true);
                }
            }

            if (nextCorridor is GeneratorGlobalVals.RIGHT_TURN or GeneratorGlobalVals.RIGHT_UP_SPLIT
                or GeneratorGlobalVals.LEFT_RIGHT_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                //Draw right
                if (isRightValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                    {
                        gridData[corridorPosition[0] + corridorWidth, corridorPosition[1] - i].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                        gridData[corridorPosition[0] + corridorWidth - 1, corridorPosition[1] - i].SetSouthWall(false);
                    }

                    gridData[corridorPosition[0] + corridorWidth, corridorPosition[1]].SetEastWall(true);
                    gridData[corridorPosition[0] + corridorWidth, corridorPosition[1] - corridorWidth]
                        .SetEastWall(true);
                    nextPathCalls.Add(RIGHT);
                }
                else if (isRightValid[2])
                {
                    //Debug.Log("East, Pathing south: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + corridorWidth - 1, corridorPosition[1] - i].SetSouthWall(false);
                }
                else if (isRightValid[3])
                {
                    //Debug.Log("East, Pathing south: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + corridorWidth - 1, corridorPosition[1] - i].SetSouthWall(true);
                }
            }

            nextPathCalls.Reverse();
            foreach (var call in nextPathCalls)
            {
                int[] newCorridorPos;
                CorridorRecursiveCallData data;
                CorridorPathingDecider newPathingDecider;
                switch (call)
                {
                    case UP:
                        newCorridorPos = new[] { corridorPosition[0], corridorPosition[1] + 1 };
                        //HandleEastCorridor(ref gridData,newCorridorPos,corridorWidth,pathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, pathingDecider, EAST);
                        recursiveCallStack.Push(data);
                        break;
                    case LEFT:
                        newCorridorPos = new[] { corridorPosition[0] - 1, corridorPosition[1] - corridorWidth + 1 };
                        newPathingDecider = new CorridorPathingDecider();
                        //HandleNorthCorridor(ref gridData,newCorridorPos,corridorWidth,newPathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, newPathingDecider, NORTH);
                        recursiveCallStack.Push(data);
                        break;
                    case RIGHT:
                        newCorridorPos = new[]
                            { corridorPosition[0] + corridorWidth, corridorPosition[1] - corridorWidth + 1 };
                        newPathingDecider = new CorridorPathingDecider();
                        //HandleSouthCorridor(ref gridData,newCorridorPos,corridorWidth,newPathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, newPathingDecider, SOUTH);
                        recursiveCallStack.Push(data);
                        break;
                }
            }
        }

        /**
         * * Handles corridor generation towards z=0
         * * Checks for valid options, chooses an option and delegates the next steps to whichever method is to handle it
         * *
         * * Valid options:
         * * All tiles in a given direction are empty
         * * All tiles in a given direction are corridors AND belong to the same corridor
         * * The chosen position would fully connect to a doorway.
         */
        private void HandleWestCorridor(ref GridSegmentData[,] gridData, int[] corridorPosition, int corridorWidth,
            CorridorPathingDecider pathingDecider, ref Stack<CorridorRecursiveCallData> recursiveCallStack)
        {
            var nextPathCalls = new List<int>();
            //bool[]'s {is valid choice, can path into empty, can path into corridor, can connect to door
            bool[] isForwardValid = { true, true, true, true };
            bool[] isLeftValid = { true, true, true, true };
            bool[] isRightValid = { true, true, true, true };
            //Check Forward Validity
            CheckPathingWestValidity(ref gridData, corridorPosition, ref isForwardValid, corridorWidth);
            //Check Right Validity
            CheckPathingNorthValidity(ref gridData, corridorPosition, ref isRightValid, corridorWidth);
            //Check left validity
            int[] corridorStartPos = { corridorPosition[0] + corridorWidth - 1, corridorPosition[1] };
            CheckPathingSouthValidity(ref gridData, corridorStartPos, ref isLeftValid, corridorWidth);

            if (!isForwardValid[0] && !isLeftValid[0] && !isRightValid[0])
            {
                //Debug.Log("Pathing West: Corridor stopped generating- No valid next positions. " + corridorPosition[0] + ", " +
                //          corridorPosition[1]);
                //No options, end corridor
                for (var i = 0; i < corridorWidth; i++)
                    gridData[corridorPosition[0] + i, corridorPosition[1] - 1].SetEastWall(true);
                return;
            }

            var nextCorridor = pathingDecider.DecideNextCorridor(isForwardValid[0], isLeftValid[0], isRightValid[0]);
            var nextCorridorHasUp = false;
            if (debuggingOn)
            {
                sw.Write("Position: [" + corridorPosition[0] + "," + corridorPosition[1] + "] Decision: ");
                switch (nextCorridor)
                {
                    case GeneratorGlobalVals.CORRIDOR_END:
                        sw.Write("End Corridor\n");
                        break;
                    case GeneratorGlobalVals.STRAIGHT:
                        sw.Write("Straight\n");
                        break;
                    case GeneratorGlobalVals.LEFT_TURN:
                        sw.Write("Left Turn\n");
                        break;
                    case GeneratorGlobalVals.RIGHT_TURN:
                        sw.Write("Right Turn\n");
                        break;
                    case GeneratorGlobalVals.LEFT_UP_SPLIT:
                        sw.Write("Left Up Split\n");
                        break;
                    case GeneratorGlobalVals.LEFT_RIGHT_SPLIT:
                        sw.Write("Left Right Split\n");
                        break;
                    case GeneratorGlobalVals.RIGHT_UP_SPLIT:
                        sw.Write("Right Up Split\n");
                        break;
                    case GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT:
                        sw.Write("Left Up Right Split\n");
                        break;
                    default:
                        sw.Write("Unknown");
                        break;
                }

                sw.WriteLine("Can path left:" + isLeftValid[0] + " Into Empty:" + isLeftValid[1] + " Into Corridor:" +
                             isLeftValid[2] + " Into Doorway:" + isLeftValid[3]);
                sw.WriteLine("Can path forward:" + isForwardValid[0] + " Into Empty:" + isForwardValid[1] +
                             " Into Corridor:" + isForwardValid[2] + " Into Doorway:" + isForwardValid[3]);
                sw.WriteLine("Can path right:" + isRightValid[0] + " Into Empty:" + isRightValid[1] +
                             " Into Corridor:" + isRightValid[2] + " Into Doorway:" + isRightValid[3]);
                sw.WriteLine();
            }

            if (nextCorridor is GeneratorGlobalVals.STRAIGHT or GeneratorGlobalVals.LEFT_UP_SPLIT
                or GeneratorGlobalVals.RIGHT_UP_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                nextCorridorHasUp = true;
                //Draw up
                if (isForwardValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + i, corridorPosition[1] - 1].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                    gridData[corridorPosition[0] - 1, corridorPosition[1] - 1].SetSouthWall(true);
                    gridData[corridorPosition[0] + corridorWidth - 1, corridorPosition[1] - 1].SetSouthWall(true);
                    nextPathCalls.Add(UP);
                }
                else if (isForwardValid[2])
                {
                    //Debug.Log("West, Pathing west: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + i, corridorPosition[1] - 1].SetEastWall(false);
                }
                else if (isForwardValid[3])
                {
                    //Debug.Log("West, Pathing west: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + i, corridorPosition[1] - 1].SetEastWall(true);
                }
            }

            if (!nextCorridorHasUp)
                for (var i = 0; i < corridorWidth; i++)
                    gridData[corridorPosition[0] + i, corridorPosition[1] - 1].SetEastWall(true);
            if (nextCorridor is GeneratorGlobalVals.LEFT_TURN or GeneratorGlobalVals.LEFT_UP_SPLIT
                or GeneratorGlobalVals.LEFT_RIGHT_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                //Draw left
                if (isLeftValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                    {
                        gridData[corridorPosition[0] + corridorWidth, corridorPosition[1] + i].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                        gridData[corridorPosition[0] + corridorWidth - 1, corridorPosition[1] + i].SetSouthWall(false);
                    }

                    gridData[corridorPosition[0] + corridorWidth, corridorPosition[1] - 1].SetEastWall(true);
                    gridData[corridorPosition[0] + corridorWidth, corridorPosition[1] + corridorWidth - 1]
                        .SetEastWall(true);
                    nextPathCalls.Add(LEFT);
                }
                else if (isLeftValid[2])
                {
                    //Debug.Log("West, Pathing north: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + corridorWidth - 1, corridorPosition[1] + i].SetSouthWall(false);
                }
                else if (isLeftValid[3])
                {
                    //Debug.Log("West, Pathing north: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] + corridorWidth - 1, corridorPosition[1] + i].SetSouthWall(true);
                }
            }

            if (nextCorridor is GeneratorGlobalVals.RIGHT_TURN or GeneratorGlobalVals.RIGHT_UP_SPLIT
                or GeneratorGlobalVals.LEFT_RIGHT_SPLIT or GeneratorGlobalVals.LEFT_UP_RIGHT_SPLIT)
            {
                //Draw right
                if (isRightValid[1])
                {
                    for (var i = 0; i < corridorWidth; i++)
                    {
                        gridData[corridorPosition[0] - 1, corridorPosition[1] + i].RoomType =
                            GeneratorGlobalVals.CORRIDOR;
                        gridData[corridorPosition[0] - 1, corridorPosition[1] + i].SetSouthWall(false);
                    }

                    gridData[corridorPosition[0] - 1, corridorPosition[1] - 1].SetEastWall(true);
                    gridData[corridorPosition[0] - 1, corridorPosition[1] + corridorWidth - 1].SetEastWall(true);
                    nextPathCalls.Add(RIGHT);
                }
                else if (isRightValid[2])
                {
                    //Debug.Log("West, Pathing South: Connection to corridor - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] - 1, corridorPosition[1] + i].SetSouthWall(false);
                }
                else if (isRightValid[3])
                {
                    //Debug.Log("West, Pathing South: Connection to door - Case not yet checked. Occured @"  +corridorPosition[0] + "," + corridorPosition[1]);
                    for (var i = 0; i < corridorWidth; i++)
                        gridData[corridorPosition[0] - 1, corridorPosition[1] + i].SetSouthWall(true);
                }
            }

            nextPathCalls.Reverse();
            foreach (var call in nextPathCalls)
            {
                int[] newCorridorPos;
                CorridorRecursiveCallData data;
                CorridorPathingDecider newPathingDecider;
                switch (call)
                {
                    case UP:
                        newCorridorPos = new[] { corridorPosition[0], corridorPosition[1] - 1 };
                        //HandleWestCorridor(ref gridData,newCorridorPos,corridorWidth,pathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, pathingDecider, WEST);
                        recursiveCallStack.Push(data);
                        break;
                    case LEFT:
                        newCorridorPos = new[] { corridorPosition[0] + corridorWidth, corridorPosition[1] };
                        newPathingDecider = new CorridorPathingDecider();
                        //HandleSouthCorridor(ref gridData,newCorridorPos,corridorWidth,newPathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, newPathingDecider, SOUTH);
                        recursiveCallStack.Push(data);
                        break;
                    case RIGHT:
                        newCorridorPos = new[] { corridorPosition[0] - 1, corridorPosition[1] };
                        newPathingDecider = new CorridorPathingDecider();
                        //HandleNorthCorridor(ref gridData,newCorridorPos,corridorWidth,newPathingDecider);
                        data = new CorridorRecursiveCallData(newCorridorPos, corridorWidth, newPathingDecider, NORTH);
                        recursiveCallStack.Push(data);
                        break;
                }
            }
        }

        /*
         * Checks to see if pathing north from passed starting pos would be valid or not
         * Modifies isOptionValid with the results.
         */
        private void CheckPathingNorthValidity(ref GridSegmentData[,] gridData, int[] checkingStartPos,
            ref bool[] isOptionValid, int corridorWidth)
        {
            var consecutiveDoorwayTiles = 0;
            var doorwayTiles = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            isOptionValid[0] = !(checkingStartPos[0] < 2);
            /*if (isOptionValid[0])
            {
                for (int i = 0; i < corridorWidth && isOptionValid[0]; i++)
                {
                    if (gridData[checkingStartPos[0], checkingStartPos[1]+i].RoomType != GeneratorGlobalVals.CORRIDOR)
                    {
                        isOptionValid[0] = false;
                    }
                    //Check for blocking walls? - Possibly needed, although don't think case will ever come up. Investigate
                }
            }*/
            if (isOptionValid[0])
            {
                for (var i = 0; i < corridorWidth && consecutiveDoorwayTiles < doorwayTiles; i++)
                {
                    if (isOptionValid[1] && gridData[checkingStartPos[0] - 1, checkingStartPos[1] + i].RoomType !=
                        GeneratorGlobalVals.EMPTY) isOptionValid[1] = false;
                    if (isOptionValid[2] && gridData[checkingStartPos[0] - 1, checkingStartPos[1] + i].RoomType !=
                        GeneratorGlobalVals.CORRIDOR) isOptionValid[2] = false;
                    //Can path into door?
                    //Check if door 
                    if (isOptionValid[3])
                    {
                        if (gridData[checkingStartPos[0] - 1, checkingStartPos[1] + i].SouthWallIsDoorway)
                            consecutiveDoorwayTiles++;
                        else
                            consecutiveDoorwayTiles = 0;

                        /*if (corridorWidth - i <= doorwayTiles - consecutiveDoorwayTiles)
                        {
                            isOptionValid[3] = false;
                        }*/
                    }
                    //isOptionValid[0] = isOptionValid[1] || isOptionValid[2] || isOptionValid[3];
                }

                if (consecutiveDoorwayTiles != doorwayTiles) isOptionValid[3] = false;
                isOptionValid[0] = isOptionValid[1] || isOptionValid[2] || isOptionValid[3];
            }
            else
            {
                isOptionValid[1] = false;
                isOptionValid[2] = false;
                isOptionValid[3] = false;
            }
            //Debug.Log("NORTH PATHING\n Has Valid option: " + isForwardValid[0] + "\nPathing into empty: " + isForwardValid[1] + "\nPathing into Corridor " + isForwardValid[2] + "\nPathing to doorway:" + isForwardValid[3]);
        }

        /*
        * Checks to see if pathing south from passed starting pos would be valid or not
        * Modifies isOptionValid with the results.
        */
        private void CheckPathingSouthValidity(ref GridSegmentData[,] gridData, int[] checkingStartPos,
            ref bool[] isOptionValid, int corridorWidth)
        {
            var consecutiveDoorwayTiles = 0;
            var doorwayTiles = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            isOptionValid[0] = checkingStartPos[0] < gridData.GetLength(0) - 1;
            /*if (isOptionValid[0])
            {
                for (int i = 0; i < corridorWidth && isOptionValid[0]; i++)
                {
                    if (gridData[checkingStartPos[0], checkingStartPos[1]+i].RoomType != GeneratorGlobalVals.CORRIDOR)
                    {
                        isOptionValid[0] = false;
                    }
                    //Check for blocking walls? - Possibly needed, although don't think case will ever come up. Investigate
                }
            }*/
            if (isOptionValid[0])
            {
                for (var i = 0; i < corridorWidth && consecutiveDoorwayTiles < doorwayTiles; i++)
                {
                    if (isOptionValid[1] && gridData[checkingStartPos[0] + 1, checkingStartPos[1] + i].RoomType !=
                        GeneratorGlobalVals.EMPTY) isOptionValid[1] = false;
                    if (isOptionValid[2] && gridData[checkingStartPos[0] + 1, checkingStartPos[1] + i].RoomType !=
                        GeneratorGlobalVals.CORRIDOR) isOptionValid[2] = false;
                    //Can path into door?
                    //Check if door 
                    if (isOptionValid[3])
                    {
                        if (gridData[checkingStartPos[0], checkingStartPos[1] + i].SouthWallIsDoorway)
                            consecutiveDoorwayTiles++;
                        else
                            consecutiveDoorwayTiles = 0;

                        /*if (corridorWidth - i <= doorwayTiles - consecutiveDoorwayTiles)
                        {
                            isOptionValid[3] = false;
                        }*/
                    }
                    //isOptionValid[0] = isOptionValid[1] || isOptionValid[2] || isOptionValid[3];
                }

                if (consecutiveDoorwayTiles != doorwayTiles) isOptionValid[3] = false;
                isOptionValid[0] = isOptionValid[1] || isOptionValid[2] || isOptionValid[3];
            }
            else
            {
                isOptionValid[1] = false;
                isOptionValid[2] = false;
                isOptionValid[3] = false;
            }
            //Debug.Log("NORTH PATHING\n Has Valid option: " + isForwardValid[0] + "\nPathing into empty: " + isForwardValid[1] + "\nPathing into Corridor " + isForwardValid[2] + "\nPathing to doorway:" + isForwardValid[3]);
        }

        /*
        * Checks to see if pathing east from passed starting pos would be valid or not
        * Modifies isOptionValid with the results.
        */
        private void CheckPathingEastValidity(ref GridSegmentData[,] gridData, int[] checkingStartPos,
            ref bool[] isOptionValid, int corridorWidth)
        {
            var consecutiveDoorwayTiles = 0;
            var doorwayTiles = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            isOptionValid[0] = checkingStartPos[1] < gridData.GetLength(1) - 1;
            consecutiveDoorwayTiles = 0;
            /*if (isOptionValid[0])
            {
                for (int i = 0; i < corridorWidth && isOptionValid[0]; i++)
                {
                    if (gridData[checkingStartPos[0] + i, checkingStartPos[1]].RoomType != GeneratorGlobalVals.CORRIDOR)
                    {
                        isOptionValid[0] = false;
                    }
                    //Check for isSouthWall? - Possibly needed, although don't think case will ever come up. Investigate
                }
            }*/
            if (isOptionValid[0])
            {
                for (var i = 0; i < corridorWidth && consecutiveDoorwayTiles < doorwayTiles; i++)
                {
                    if (isOptionValid[1] && gridData[checkingStartPos[0] + i, checkingStartPos[1] + 1].RoomType !=
                        GeneratorGlobalVals.EMPTY) isOptionValid[1] = false;
                    if (isOptionValid[2] && gridData[checkingStartPos[0] + i, checkingStartPos[1] + 1].RoomType !=
                        GeneratorGlobalVals.CORRIDOR) isOptionValid[2] = false;
                    //Can path into door?
                    //Check if door 
                    if (isOptionValid[3])
                    {
                        if (gridData[checkingStartPos[0] + i, checkingStartPos[1]].EastWallIsDoorway)
                            consecutiveDoorwayTiles++;
                        else
                            consecutiveDoorwayTiles = 0;

                        /*if (corridorWidth - i <= doorwayTiles - consecutiveDoorwayTiles)
                        {
                            isOptionValid[3] = false;
                        }*/
                    }
                    //isOptionValid[0] = isOptionValid[1] || isOptionValid[2] || isOptionValid[3];
                }

                if (consecutiveDoorwayTiles != doorwayTiles) isOptionValid[3] = false;
                isOptionValid[0] = isOptionValid[1] || isOptionValid[2] || isOptionValid[3];
            }
            else
            {
                isOptionValid[1] = false;
                isOptionValid[2] = false;
                isOptionValid[3] = false;
            }
            //Debug.Log("NORTH PATHING\nRight\n Has Valid option: " + isRightValid[0] + "\nPathing into empty: " + isRightValid[1] + "\nPathing into Corridor " + isRightValid[2] + "\nPathing to doorway:" + isRightValid[3]);
        }

        /*
        * Checks to see if pathing west from passed starting pos would be valid or not
        * Modifies isOptionValid with the results.
        */
        private void CheckPathingWestValidity(ref GridSegmentData[,] gridData, int[] checkingStartPos,
            ref bool[] isOptionValid, int corridorWidth)
        {
            var consecutiveDoorwayTiles = 0;
            var doorwayTiles = GeneratorGlobalVals.Instance.GetDoorwayWidthTiles();
            isOptionValid[0] = !(checkingStartPos[1] < 2);
            //isOptionValid[0] = !(checkingStartPos[0] < 2); 
            consecutiveDoorwayTiles = 0;
            /*if (isOptionValid[0])
            { 
                for (int i = 0; i < corridorWidth && isOptionValid[0]; i++)
                {
                    try
                    {
                        if (gridData[checkingStartPos[0] + i, checkingStartPos[1]].RoomType != GeneratorGlobalVals.CORRIDOR)
                        {
                            isOptionValid[0] = false;
                        }
                    }
                    catch (Exception e)
                    {
                        sw.Close();
                        throw;
                    }
                    
                    //Check for isSouthWall? - Possibly needed, although don't think case will ever come up. Investigate
                }
            }*/
            if (isOptionValid[0])
            {
                for (var i = 0; i < corridorWidth && consecutiveDoorwayTiles < doorwayTiles; i++)
                {
                    if (isOptionValid[1] && gridData[checkingStartPos[0] + i, checkingStartPos[1] - 1].RoomType !=
                        GeneratorGlobalVals.EMPTY) isOptionValid[1] = false;
                    if (isOptionValid[2] && gridData[checkingStartPos[0] + i, checkingStartPos[1] - 1].RoomType !=
                        GeneratorGlobalVals.CORRIDOR) isOptionValid[2] = false;

                    //Can path into door?
                    //Check if door 
                    if (isOptionValid[3])
                    {
                        if (gridData[checkingStartPos[0] + i, checkingStartPos[1] - 1].EastWallIsDoorway)
                            consecutiveDoorwayTiles++;
                        else
                            consecutiveDoorwayTiles = 0;

                        /*if (corridorWidth - i <= doorwayTiles - consecutiveDoorwayTiles)
                        {
                            isOptionValid[3] = false;
                        }*/
                    }
                    //isOptionValid[0] = isOptionValid[1] || isOptionValid[2] || isOptionValid[3];
                }

                if (consecutiveDoorwayTiles != doorwayTiles) isOptionValid[3] = false;
                isOptionValid[0] = isOptionValid[1] || isOptionValid[2] || isOptionValid[3];
            }
            else
            {
                isOptionValid[1] = false;
                isOptionValid[2] = false;
                isOptionValid[3] = false;
            }
            //Debug.Log("WEST PATHING\n Has Valid option: " + isLeftValid[0] + "\nPathing into empty: " + isLeftValid[1] + "\nPathing into Corridor " + isLeftValid[2] + "\nPathing to doorway:" + isLeftValid[3]);
        }
    }
}